using Microsoft.EntityFrameworkCore;
using tictac.Models;

namespace tictac.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
}
