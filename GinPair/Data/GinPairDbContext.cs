namespace GinPair.Data;

public class GinPairDbContext(DbContextOptions<GinPairDbContext> options) : DbContext(options) {
    public DbSet<Gin> Gins { get; set; }
    public DbSet<Tonic> Tonics { get; set; }
    public DbSet<Pairing> Pairings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("gp_schema");
        SeedData.Seed(modelBuilder);
    }
}

