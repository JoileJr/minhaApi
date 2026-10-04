using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Dtos;
using MinhaApi.Models;

namespace MinhaApi.Services;

public interface IObjetoService
{
    Task<List<Objeto>> ListarAsync();
    Task<Objeto?> BuscarPorIdAsync(int id);
    Task<Objeto> CriarAsync(ObjetoCreateDto dto);
    Task<Objeto?> AtualizarAsync(int id, ObjetoUpdateDto dto);
    Task<bool> RemoverAsync(int id);
}

public class ObjetoService : IObjetoService
{
    private readonly AppDbContext _context;

    public ObjetoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Objeto>> ListarAsync()
    {
        return await _context.Objetos.AsNoTracking().ToListAsync();
    }

    public async Task<Objeto?> BuscarPorIdAsync(int id)
    {
        return await _context.Objetos.FindAsync(id);
    }

    public async Task<Objeto> CriarAsync(ObjetoCreateDto dto)
    {
        var objeto = new Objeto
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao
        };

        _context.Objetos.Add(objeto);
        await _context.SaveChangesAsync();
        return objeto;
    }

    public async Task<Objeto?> AtualizarAsync(int id, ObjetoUpdateDto dto)
    {
        var existente = await _context.Objetos.FindAsync(id);
        if (existente is null)
            return null;

        existente.Nome = dto.Nome;
        existente.Descricao = dto.Descricao;
        await _context.SaveChangesAsync();
        return existente;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var existente = await _context.Objetos.FindAsync(id);
        if (existente is null)
            return false;

        _context.Objetos.Remove(existente);
        await _context.SaveChangesAsync();
        return true;
    }
}
