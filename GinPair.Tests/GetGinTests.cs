namespace GinPair.Tests;
public class GetGinTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public GetGinTests() {
        var options = GetDbContextOptions();
        mockContext = new GinPairDbContext(options);
        mockController = new GinApiController(mockContext);
    }
    private static DbContextOptions<GinPairDbContext> GetDbContextOptions() {
        return new DbContextOptionsBuilder<GinPairDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task MatchPartial_ReturnsOk() {
        mockContext.Gins.RemoveRange(mockContext.Gins);
        mockContext.Gins.AddRange(new List<Gin>
        {
            new() {GinId = 1, GinName = "TestName1", Distillery = "TestDis1"},
            new() {GinId = 2, GinName = "TestName2", Distillery = "TestDis2"}
        });
        mockContext.SaveChanges();

        var result = await mockController.MatchGin("Test");

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<List<Gin>>()
            .Which.Should().HaveCount(2);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<List<Gin>>()
            .Which.Should().Contain(g => g.GinName == "TestName1")
            .And.Contain(g => g.GinName == "TestName2");

    }
    [Fact]
    public async Task GetPairingById_ReturnsOk() {
        mockContext.Pairings.RemoveRange(mockContext.Pairings);
        mockContext.Pairings.AddRange(new List<Pairing>
        {
            new() {
                PairedGin = new Gin {GinId = 3, GinName = "TestName3", Distillery = "TestDis3"},
                PairedTonic = new Tonic {TonicId = 3, TonicBrand = "TestBrand3", TonicFlavour = "TestFl3"}
            }
        });
        mockContext.SaveChanges();

        var result = await mockController.GetPairingByGinId(3);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.StatusMessage.Should().Contain("Try pairing <b>TestDis3 TestName3</b> gin with");
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.BsColor.Should().Be(BsColor.Primary);
    }

    [Fact]
    public async Task GetPairingById_NoGin_Returns_Bad() {
        mockContext.Pairings.RemoveRange(mockContext.Pairings);
        mockContext.Pairings.AddRange(new List<Pairing>
        {
            new() {
                PairedGin = new Gin {GinId = 3, GinName = "TestName3", Distillery = "TestDis3"},
                PairedTonic = new Tonic {TonicId = 3, TonicBrand = "TestBrand3", TonicFlavour = "TestFl3"}
            }
        });
        mockContext.SaveChanges();

        var result = await mockController.GetPairingByGinId(4);

        result.Should().BeOfType<BadRequestResult>();
    }
}
