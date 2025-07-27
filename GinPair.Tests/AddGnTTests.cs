namespace GinPair.Tests;

public class AddGnTTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public AddGnTTests() {
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
    public void AddGin_ReturnsOk_WhenGinIsAddedSuccessfully() {
        string jsonData = JsonSerializer.Serialize(new { ginName = "Test Gin", distillery = "Test Distillery", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddGin(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Success);
        apiResponse.StatusMessage.ShouldBe("✅ Success! \"Test Distillery Test Gin\" gin was added!");
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsEmpty() {
        string jsonData = JsonSerializer.Serialize(new { ginName = "", distillery = "", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddGin(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Warning);
        apiResponse.StatusMessage.ShouldBe("Please provide the name of the Distillery and Gin.");
    }

    [Fact]
    public void IsGinPresent_ReturnsTrue_WhenGinExists() {
        var gin = new Gin { GinName = "TestGin", Distillery = "TestDistillery" };
        mockContext.Gins.Add(gin);
        mockContext.SaveChanges();

        bool result = mockController.IsGinPresent("TestGin", "TestDistillery");

        result.ShouldBeTrue();
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsPresent() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        mockContext.Gins.Add(gin);
        mockContext.SaveChanges();
        string jsonData = JsonSerializer.Serialize(new { ginName = "Test Gin", distillery = "Test Distillery", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddGin(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Danger);
        apiResponse.StatusMessage.ShouldBe("Sorry this gin cannot be added as it is already part of our collection!");
    }

    [Theory]
    [InlineData(null, "Distillery")]
    [InlineData("GinName", null)]
    [InlineData("", "Distillery")]
    [InlineData("GinName", "")]
    public void AddGin_ReturnsOk_WhenNameOrDistilleryMissing(string? ginName, string? distillery) {
        string json = $@"{{ ""ginName"": ""{ginName}"", ""distillery"": ""{distillery}"", ""description"": ""desc"" }}";
        var doc = JsonDocument.Parse(json);

        var result = mockController.AddGin(doc.RootElement);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Warning);
        apiResponse.StatusMessage.ShouldContain("Please provide the name of the Distillery and Gin.");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsAddedSuccessfully() {
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddTonic(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Success);
        apiResponse.StatusMessage.ShouldBe("✅ Success! \"Test Brand Test Flavour\" tonic was added!");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsEmpty() {
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "", tonicFlavour = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddTonic(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Warning);
        apiResponse.StatusMessage.ShouldBe("Please provide the tonic brand and flavour/name.");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsPresent() {
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        mockContext.Tonics.Add(tonic);
        mockContext.SaveChanges();
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddTonic(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Danger);
        apiResponse.StatusMessage.ShouldBe("Sorry this tonic cannot be added as it is already part of our collection!");
    }

    [Fact]
    public void AddPairing_ReturnsOk_WhenPairingIsAddedSuccessfully() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        mockContext.Gins.Add(gin);
        mockContext.Tonics.Add(tonic);
        mockContext.SaveChanges();

        string jsonData = JsonSerializer.Serialize(new { ginId = "1", tonicId = "1" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddPairing(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Success);
        apiResponse.StatusMessage.ShouldBe("✅ Success! \"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic were paired!");
    }

    [Theory]
    [InlineData("1", "")]
    [InlineData("", "1")]
    [InlineData(null, "1")]
    [InlineData("1", null)]
    [InlineData("1", "1")]
    public void AddPairing_ReturnsOk_WhenGinOrTonicIsNullOrEmpty(string? ginId, string? tonicId) {
        string jsonData = JsonSerializer.Serialize(new { ginId, tonicId });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddPairing(data);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Warning);
        apiResponse.StatusMessage.ShouldBe("Please select the gin and tonic to pair");
    }

    [Fact]
    public void AddPairing_ReturnsOk_WhenGinOrTonicIsAlreadyPresent() {
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

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Danger);
        apiResponse.StatusMessage.ShouldBe("\"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic are already paired!");
    }
}
