using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Dtos;
using MinhaApi.Exceptions;
using MinhaApi.Models;

namespace MinhaApi.Services;

public interface IObjetoService
{
    Task<PagedResponse<ObjetoResponseDto>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ObjetoResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ObjetoResponseDto> CriarAsync(ObjetoCreateDto dto, CancellationToken cancellationToken = default);
    Task<ObjetoResponseDto> AtualizarAsync(int id, ObjetoUpdateDto dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(int id, CancellationToken cancellationToken = default);
}

public class ObjetoService : IObjetoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ObjetoService> _logger;

    public ObjetoService(AppDbContext context, ILogger<ObjetoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static ObjetoResponseDto ToResponse(Objeto o) => new()
    {
        Id = o.Id,
        Nome = o.Nome,
        Descricao = o.Descricao
    };

    public async Task<PagedResponse<ObjetoResponseDto>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

        var total = await _context.Objetos.CountAsync(cancellationToken);

        var items = await _context.Objetos
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new ObjetoResponseDto
            {
                Id = o.Id,
                Nome = o.Nome,
                Descricao = o.Descricao
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ObjetoResponseDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ObjetoResponseDto> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var objeto = await _context.Objetos
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (objeto is null)
            throw NotFoundException.Para("Objeto", id);

        return ToResponse(objeto);
    }

    public async Task<ObjetoResponseDto> CriarAsync(ObjetoCreateDto dto, CancellationToken cancellationToken = default)
    {
        var objeto = new Objeto
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao
        };

        _context.Objetos.Add(objeto);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Objeto criado com Id {Id}", objeto.Id);

        return ToResponse(objeto);
    }

    public async Task<ObjetoResponseDto> AtualizarAsync(int id, ObjetoUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var existente = await _context.Objetos.FindAsync([id], cancellationToken);
        if (existente is null)
            throw NotFoundException.Para("Objeto", id);

        existente.Nome = dto.Nome;
        existente.Descricao = dto.Descricao;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Objeto atualizado com Id {Id}", id);

        return ToResponse(existente);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var existente = await _context.Objetos.FindAsync([id], cancellationToken);
        if (existente is null)
            throw NotFoundException.Para("Objeto", id);

        _context.Objetos.Remove(existente);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Objeto removido com Id {Id}", id);
    }
}
