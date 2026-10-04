# MinhaApi

Primeira API em **ASP.NET Core (.NET 10)** com CRUD de `Objeto`
(`id`, `nome`, `descricao`), PostgreSQL via Docker, Entity Framework Core,
Swagger e tratamento global de erros padronizado (RFC 7807 / `ProblemDetails`).

## Tecnologias

| Item | Versão / Detalhe |
|---|---|
| .NET | 10 (`net10.0`, `Nullable` + `ImplicitUsings` habilitados) |
| ASP.NET Core | Web API com Controllers (`AddControllers` + `MapControllers`) |
| Banco | PostgreSQL 16 via `docker-compose.yml` |
| ORM | EF Core + `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3` |
| Migrations | EF Core (`dotnet ef`, pasta `Migrations/`) |
| Docs | Swashbuckle (`AddSwaggerGen`, `/swagger`) |
| Erros | `AddProblemDetails` + `IExceptionHandler` (`ProblemDetails`) |

## Estrutura do projeto

```
MinhaApi/
├── Program.cs                 # Composição: DI, EF, Swagger, erros, pipeline
├── appsettings.json           # ConnectionStrings:DefaultConnection + Logging
├── appsettings.Development.json
├── docker-compose.yml         # Postgres 16 (db minhaapi, porta 5432, volume + healthcheck)
├── MinhaApi.http              # Chamadas de teste (weatherforecast legado)
├── .gitignore                 # Template oficial `dotnet new gitignore` (ignora bin/, obj/, .env...)
│
├── Controllers/               # Camada HTTP: rotas, status codes, sem regra de negócio
│   ├── HelloController.cs     # GET /api/hello → "Hello World" (exemplo DI simples)
│   └── ObjetosController.cs   # CRUD /api/objetos (paginado, DTOs, CancellationToken)
│
├── Services/                  # Camada de negócio, injetada via interface (AddScoped)
│   ├── HelloService.cs        # IHelloService / HelloService
│   └── ObjetoService.cs       # IObjetoService / ObjetoService (EF Core + ILogger)
│
├── Models/                    # Entidades de domínio (mapeadas ao banco)
│   └── Objeto.cs              # Id [Key, Identity], Nome [Required, MaxLength(100)], Descricao [MaxLength(500)]
│
├── Dtos/                      # Contratos da API — o Id nunca entra no POST/PUT
│   ├── ObjetoCreateDto.cs     # POST: nome, descricao
│   ├── ObjetoUpdateDto.cs     # PUT: nome, descricao (id vem da rota)
│   ├── ObjetoResponseDto.cs   # Respostas: id, nome, descricao
│   └── PagedResponse.cs       # { items, totalCount, page, pageSize }
│
├── Data/
│   └── AppDbContext.cs        # DbContext (DbSet<Objeto>), configurado com UseNpgsql
│
├── Exceptions/
│   └── NotFoundException.cs   # NotFoundException.Para("Objeto", id)
│
├── Middleware/
│   └── GlobalExceptionHandler.cs  # IExceptionHandler → exceção vira ProblemDetails
│
└── Migrations/                # InitialCreate (tabela Objetos)
```

## Padrões aplicados

1. **Controller → Service → EF Core.** Controller só traduz HTTP (`Ok`, `CreatedAtAction`,
   `NoContent`); regra de negócio e acesso a dados ficam no Service (`Services/ObjetoService.cs`).
2. **Inversão de dependência.** Controllers e `AppDbContext` recebem dependências por
   construtor (`IObjetoService`, `ILogger<T>`, `DbContextOptions`), registradas em
   `Program.cs` (`AddScoped`, `AddDbContext`).
3. **DTOs de entrada/saída.** `POST/PUT` recebem `Create/UpdateDto` (sem `Id` — o banco gera
   via `Identity`); respostas devolvem `ObjetoResponseDto`. Evita over-posting e desacopla
   o modelo de domínio do contrato HTTP.
4. **Validação declarativa.** `[Required]`, `[MaxLength]` nos DTOs + `[ApiController]`
   → erro de validação vira `400 ProblemDetails` automaticamente.
5. **Async + `CancellationToken`.** Todos os acessos usam `ToListAsync`, `FindAsync`,
   `SaveChangesAsync`, `CountAsync` com o token propagado do controller ao EF.
6. **Paginação no listar.** `GET /api/objetos?page=1&pageSize=10` (`OrderBy(Id) + Skip/Take`,
   `pageSize` limitado a 1–100, `AsNoTracking` na leitura).
7. **REST + Swagger documentado.** `GET`, `GET {id}`, `POST → 201 CreatedAtAction`,
   `PUT → 200`, `DELETE → 204`, `404` quando inexistente; cada ação tem
   `[ProducesResponseType]` e aparece no Swagger.
8. **Erros centralizados (`ProblemDetails`).** O Service lança `NotFoundException`; o
   `GlobalExceptionHandler` converte em resposta padrão (ver tabela abaixo). `500`
   loga `Error` e omite detalhes fora de Development; `4xx` logam `Warning`.
9. **Configuração por ambiente.** Conexão via `GetConnectionString("DefaultConnection")`,
   sobrescrevível sem mudar código por env
   (`ConnectionStrings__DefaultConnection`) ou User Secrets em produção.
10. **Logs estruturados.** `ILogger` com templates (`"Objeto criado com Id {Id}"`).

## Endpoints

| Método | Rota | Corpo | Resposta |
|---|---|---|---|
| GET | `/api/hello` | — | `200 "Hello World"` |
| GET | `/api/objetos?page=1&pageSize=10` | — | `200 PagedResponse<ObjetoResponseDto>` |
| GET | `/api/objetos/{id}` | — | `200 ObjetoResponseDto` ou `404` |
| POST | `/api/objetos` | `{"nome":"Cadeira","descricao":"Gamer"}` | `201 ObjetoResponseDto` (com `id` gerado) ou `400` |
| PUT | `/api/objetos/{id}` | `{"nome":"Novo","descricao":"..."}` | `200` ou `404/400` |
| DELETE | `/api/objetos/{id}` | — | `204` ou `404` |
| GET | `/swagger` | — | Swagger UI (Development) |

Exemplo paginado:

```json
GET /api/objetos?page=1&pageSize=2
{
  "items": [{ "id": 1, "nome": "Cadeira", "descricao": "Gamer" }],
  "totalCount": 3, "page": 1, "pageSize": 2
}
```

## Tratamento de erros

| Exceção | Status | `title` |
|---|---|---|
| `NotFoundException` | 404 | `Recurso não encontrado` |
| `BadRequestException` | 400 | `Requisição inválida` |
| `ConflictException` / `DbUpdateException` | 409 | `Conflito ao persistir os dados` |
| `HttpRequestException` (falha na ViaCEP) | 502 | `Falha na integração externa` |
| demais | 500 | `Erro interno do servidor` |

Erros de domínio herdam de `AppException` (cada tipo carrega seu `StatusCode`/`Title`),
então um novo tipo de erro = uma nova classe em `Exceptions/`, sem alterar o handler (OCP).

Exemplos reais:

```json
// GET /api/objetos/999999 → 404
{"title":"Recurso não encontrado","status":404,
 "detail":"Objeto com Id 999999 não foi encontrado.","instance":"/api/objetos/999999"}

// POST {"nome":""} → 400 (validação automática do [ApiController])
{"title":"One or more validation errors occurred.","status":400,
 "errors":{"Nome":["The Nome field is required."]}}
```

## Como rodar

Pré-requisitos: .NET 10 SDK, Docker + Compose.

```bash
cd MinhaApi

# 1. Banco
sudo docker compose up -d        # ou: docker compose up -d (se usuário está no grupo docker)

# 2. Schema (a migration InitialCreate já existe)
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update

# 3. API
dotnet run
```

Acesse:

- API: `http://localhost:5062/api/objetos`
- Swagger: `http://localhost:5062/swagger`
- Swagger JSON: `http://localhost:5062/swagger/v1/swagger.json`

Teste rápido:

```bash
curl -X POST http://localhost:5062/api/objetos \
  -H "Content-Type: application/json" \
  -d '{"nome":"Cadeira","descricao":"Gamer"}'
# → {"id":1,"nome":"Cadeira","descricao":"Gamer"}

curl "http://localhost:5062/api/objetos?page=1&pageSize=10"
```

Nova migration (se mudar o modelo):

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

## Observações

- A senha do Postgres está fixa no `appsettings.json` por ser ambiente local; em
  produção use variável de ambiente ou User Secrets.
- `bin/` e `obj/` foram removidos do índice do git (`git rm -r --cached`) e são
  ignorados pelo `.gitignore`.
