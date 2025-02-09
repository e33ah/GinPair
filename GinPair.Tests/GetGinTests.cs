namespace GinPair.Tests;
public class GetGinTests
{
    private static DbContextOptions<GinPairDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<GinPairDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task MatchPartial_ReturnsOk()
    {
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        mockContext.Gins.RemoveRange(mockContext.Gins);
        mockContext.Gins.AddRange(new List<Gin>
        {
            new() {GinId = 1, GinName = "TestName1", Distillery = "TestDis1"},
            new() {GinId = 2, GinName = "TestName2", Distillery = "TestDis2"}
        });
        mockContext.SaveChanges();
        var controller = new GinApiController(mockContext);

        var result = await controller.MatchGin("Test");

        Assert.NotNull(result);
        Assert.IsType<OkObjectResult>(result);
    }
    [Fact]
    public async Task GetPairingById_ReturnsOk()
    {
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        mockContext.Pairings.AddRange(new List<Pairing>
        {
            new() {
                PairedGin = new Gin {GinId = 3, GinName = "TestName3", Distillery = "TestDis3"},
                PairedTonic = new Tonic {TonicId = 3, TonicBrand = "TestBrand3", TonicFlavour = "TestFl3"}
            }
        });
        mockContext.SaveChanges();
        var controller = new GinApiController(mockContext);

        var result = await controller.GetPairingByGinId(3);

        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse>(okResult.Value);
    }
    [Fact]
    public async Task GetPairingById_NoGin_Returns_Bad()
    {
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        mockContext.Pairings.AddRange(new List<Pairing>
        {
            new() {
                PairedGin = new Gin {GinId = 3, GinName = "TestName3", Distillery = "TestDis3"},
                PairedTonic = new Tonic {TonicId = 3, TonicBrand = "TestBrand3", TonicFlavour = "TestFl3"}
            }
        });
        mockContext.SaveChanges();
        var controller = new GinApiController(mockContext);

        var result = await controller.GetPairingByGinId(4);
        Assert.NotNull(result);
        Assert.IsType<BadRequestResult>(result);
    } 
}
