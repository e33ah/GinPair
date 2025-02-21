namespace GinPair.Data;

public class GinPairDbContext : DbContext {
    public DbSet<Gin> Gins { get; set; }
    public DbSet<Tonic> Tonics { get; set; }
    public DbSet<Pairing> Pairings { get; set; }

    public GinPairDbContext(DbContextOptions<GinPairDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("gp_schema");
        SeedData.Seed(modelBuilder);
    }
}

