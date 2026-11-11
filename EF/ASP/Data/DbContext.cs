using ASP.Models;
using Microsoft.EntityFrameworkCore;

namespace ASP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Tasks { get; set; }
}
