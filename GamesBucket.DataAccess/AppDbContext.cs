using GamesBucket.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GamesBucket.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Game> Games { get; set; }
        public DbSet<Genres> Genres { get; set; }
        public DbSet<Movies> Movies { get; set; }
        public DbSet<Screenshots> Screenshots { get; set; }
        public DbSet<SteamLibrary> SteamLibraries { get; set; }
    }
}