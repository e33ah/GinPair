namespace GinPair.Tests;
public class GetGinTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public GetGinTests() {
        var options = GetDbContextOptions();
        mockContext = new GinPairDbContext(options);
        var dbInitState = new DatabaseInitializationState { IsDatabaseReady = true };
        mockController = new GinApiController(mockContext, dbInitState);
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

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var ginList = okResult.Value.ShouldBeOfType<List<Gin>>();
        ginList.Count.ShouldBe(2);
        ginList.ShouldContain(g => g.GinName == "TestName1");
        ginList.ShouldContain(g => g.GinName == "TestName2");
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

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.StatusMessage.ShouldContain("Try pairing <b>TestDis3 TestName3</b> gin with");
        apiResponse.BsColor.ShouldBe(BsColor.Primary);
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

        result.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public void GetGinList_ReturnsOk_WithEmptyList_WhenNoGinsExist() {
        mockContext.Gins.RemoveRange(mockContext.Gins);
        mockContext.SaveChanges();

        var result = mockController.GetGinList();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var selectList = okResult.Value as IEnumerable<SelectListItem>;
        selectList.ShouldNotBeNull();
        selectList.ShouldBeEmpty();
    }
}
