using System;
using GamesBucket.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GamesBucket.DataAccess
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Game> Games { get; set; }
        public DbSet<Genres> Genres { get; set; }
        public DbSet<Movies> Movies { get; set; }
        public DbSet<Screenshots> Screenshots { get; set; }
        public DbSet<SteamLibrary> SteamLibraries { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // setting a unique constraint
            modelBuilder.Entity<Genres>()
                .HasIndex(g => g.Name)
                .IsUnique();
            // .HasAlternateKey(c => c.Name)
            // .HasName("AlternateKey_GenreName");
            
            // cascade deletion of dependent entities
            modelBuilder.Entity<Game>()
                .HasMany(x => x.Screenshots)
                .WithOne()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Game>()
                .HasMany(x => x.Movies)
                .WithOne()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(x => x.Games)
                .WithOne()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // TODO: it doesnt save a new guid but stays null
            // modelBuilder.Entity<Game>().Property(x => x.GameId)
            //     .HasDefaultValue(Guid.NewGuid());
            
            base.OnModelCreating(modelBuilder);
        }
    }
}