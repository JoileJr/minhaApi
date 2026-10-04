using Microsoft.EntityFrameworkCore;
using MinhaApi.Models;

namespace MinhaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Objeto> Objetos => Set<Objeto>();
}
