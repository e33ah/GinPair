namespace GinPair.Tests;

public class AddGnTTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public AddGnTTests() {
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
    public void AddPairing_ReturnsOkResult_WhenPairingIsAddedSuccessfully() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        mockContext.Gins.Add(gin);
        mockContext.Tonics.Add(tonic);
        mockContext.SaveChanges();

        string jsonData = JsonSerializer.Serialize(new { ginId = "1", tonicId = "1" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddPairing(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Success &&
                response.StatusMessage == "\"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic were paired successfully!");
    }

    [Fact]
    public void AddPairing_ReturnsOkResult_WhenGinOrTonicIsEmpty() {
        string jsonData = JsonSerializer.Serialize(new { ginId = "", tonicId = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddPairing(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Warning &&
                response.StatusMessage == "Please select the gin and tonic to pair");
    }
    
    [Fact]
    public void AddPairing_ReturnsOkResult_WhenGinOrTonicIsPresent() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        var pairing = new Pairing { PairedGin = gin, PairedTonic = tonic };
        mockContext.Gins.Add(gin);
        mockContext.Tonics.Add(tonic);
        mockContext.Pairings.Add(pairing);
        mockContext.SaveChanges();

        string jsonData = JsonSerializer.Serialize(new { ginId = "1", tonicId = "1" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddPairing(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Danger &&
                response.StatusMessage == "\"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic are already paired!");
    }
}
