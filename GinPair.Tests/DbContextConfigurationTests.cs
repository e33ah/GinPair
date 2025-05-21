using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GinPair.Tests;

public class DbContextConfigurationTests
{
    [Theory]
    [InlineData("Postgres", "Npgsql.EntityFrameworkCore.PostgreSQL")]
    [InlineData("SqlServer", "Microsoft.EntityFrameworkCore.SqlServer")]
    public void Should_Use_Correct_Database_Provider(string dbProvider, string expectedProviderName)
    {
        var fakeConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
               { "DatabaseProvider", dbProvider },
               { "ConnectionStrings:PostgresConnection", "FakeConnectionString" },
               { "ConnectionStrings:SqlServerConnection", "FakeConnectionString" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddConfiguredDbContext(fakeConfig);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<GinPairDbContext>>();

        var relationalExtension = options.Extensions
            .OfType<RelationalOptionsExtension>()
            .FirstOrDefault();

        using var scope = new AssertionScope();
        relationalExtension.Should().NotBeNull();
        relationalExtension!.GetType().Assembly.FullName.Should().StartWith(expectedProviderName);

    }
}
