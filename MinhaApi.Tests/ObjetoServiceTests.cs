using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MinhaApi.Data;
using MinhaApi.Dtos;
using MinhaApi.Exceptions;
using MinhaApi.Models;
using MinhaApi.Services;

namespace MinhaApi.Tests;

public class ObjetoServiceTests
{
    private static (AppDbContext context, ObjetoService service) CriarService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var service = new ObjetoService(context, NullLogger<ObjetoService>.Instance);

        return (context, service);
    }

    [Fact]
    public async Task CriarAsync_PersisteEGeraId()
    {
        var (_, service) = CriarService();

        var criado = await service.CriarAsync(new ObjetoCreateDto
        {
            Nome = "Cadeira",
            Descricao = "Gamer"
        });

        Assert.True(criado.Id > 0);
        Assert.Equal("Cadeira", criado.Nome);
        Assert.Equal("Gamer", criado.Descricao);
    }

    [Fact]
    public async Task BuscarPorIdAsync_Existente_RetornaDto()
    {
        var (context, service) = CriarService();
        context.Objetos.Add(new Objeto { Nome = "Mesa", Descricao = "Escritório" });
        await context.SaveChangesAsync();

        var resultado = await service.BuscarPorIdAsync(1);

        Assert.Equal(1, resultado.Id);
        Assert.Equal("Mesa", resultado.Nome);
    }

    [Fact]
    public async Task BuscarPorIdAsync_Inexistente_LancaNotFound()
    {
        var (_, service) = CriarService();

        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => service.BuscarPorIdAsync(999));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task ListarAsync_PaginaERetornaTotal()
    {
        var (context, service) = CriarService();
        context.Objetos.AddRange(
            new Objeto { Nome = "A" },
            new Objeto { Nome = "B" },
            new Objeto { Nome = "C" });
        await context.SaveChangesAsync();

        var pagina1 = await service.ListarAsync(1, 2);
        var pagina2 = await service.ListarAsync(2, 2);

        Assert.Equal(3, pagina1.TotalCount);
        Assert.Equal(2, pagina1.Items.Count);
        Assert.Single(pagina2.Items);
        Assert.Equal(1, pagina1.Page);
        Assert.Equal(2, pagina1.PageSize);
    }

    [Fact]
    public async Task ListarAsync_PaginaInvalida_NormalizaParaPrimeiraPagina()
    {
        var (_, service) = CriarService();

        var resultado = await service.ListarAsync(0, 0);

        Assert.Equal(1, resultado.Page);
        Assert.Equal(10, resultado.PageSize);
    }

    [Fact]
    public async Task AtualizarAsync_Existente_AlteraCampos()
    {
        var (_, service) = CriarService();
        var criado = await service.CriarAsync(new ObjetoCreateDto { Nome = "Antigo" });

        var atualizado = await service.AtualizarAsync(criado.Id, new ObjetoUpdateDto
        {
            Nome = "Novo",
            Descricao = "Atualizado"
        });

        Assert.Equal("Novo", atualizado.Nome);
        Assert.Equal("Atualizado", atualizado.Descricao);
    }

    [Fact]
    public async Task AtualizarAsync_Inexistente_LancaNotFound()
    {
        var (_, service) = CriarService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.AtualizarAsync(999, new ObjetoUpdateDto { Nome = "X" }));
    }

    [Fact]
    public async Task RemoverAsync_Existente_Remove()
    {
        var (_, service) = CriarService();
        var criado = await service.CriarAsync(new ObjetoCreateDto { Nome = "Temp" });

        await service.RemoverAsync(criado.Id);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.BuscarPorIdAsync(criado.Id));
    }

    [Fact]
    public async Task RemoverAsync_Inexistente_LancaNotFound()
    {
        var (_, service) = CriarService();

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.RemoverAsync(999));
    }
}
