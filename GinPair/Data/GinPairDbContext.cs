using System.Xml;
using GinPair.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GinPair.Data
{
    public class GinPairDbContext : DbContext
    {
        public DbSet<Gin> Gins { get; set; }
        public DbSet<Tonic> Tonics { get; set; }
        public DbSet<Pairing> Pairings { get; set; }

        public GinPairDbContext(DbContextOptions<GinPairDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("gp_schema");

            modelBuilder.Entity<Gin>()
                .Property(e => e.GinName)
                .HasColumnType("varchar")
                .HasMaxLength(100);
            modelBuilder.Entity<Gin>()
                .Property(e => e.GinDescription)
                .HasColumnType("varchar")
                .HasMaxLength(500);
            modelBuilder.Entity<Gin>()
                .Property(e => e.Distillery)
                .HasColumnType("varchar")
                .HasMaxLength(100);

            modelBuilder.Entity<Tonic>()
                .Property(e => e.TonicBrand)
                .HasColumnType("varchar")
                .HasMaxLength(100);
            modelBuilder.Entity<Tonic>()
                .Property(e => e.TonicFlavour)
                .HasColumnType("varchar")
                .HasMaxLength(100);
            
            SeedData.Seed(modelBuilder);
        }
    }
}

