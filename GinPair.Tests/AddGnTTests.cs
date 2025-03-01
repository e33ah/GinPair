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
    public void AddGin_ReturnsOk_WhenGinIsAddedSuccessfully() {
        string jsonData = JsonSerializer.Serialize(new { ginName = "Test Gin", distillery = "Test Distillery", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddGin(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Success &&
                response.StatusMessage == $"✅ Success! \"Test Distillery Test Gin\" gin was added!");
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsEmpty() {
        string jsonData = JsonSerializer.Serialize(new { ginName = "", distillery = "", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddGin(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Warning &&
                response.StatusMessage == "Please provide the name of the Distillery and Gin.");
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsPresent() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        mockContext.Gins.Add(gin);
        mockContext.SaveChanges();
        string jsonData = JsonSerializer.Serialize(new { ginName = "Test Gin", distillery = "Test Distillery", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddGin(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Danger &&
                response.StatusMessage == "Sorry this gin cannot be added as it is already part of our collection!");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsAddedSuccessfully() {
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddTonic(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Success &&
                response.StatusMessage == "✅ Success! \"Test Brand Test Flavour\" tonic was added!");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsEmpty() {
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "", tonicFlavour = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddTonic(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Warning &&
                response.StatusMessage == "Please provide the tonic brand and flavour/name.");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsPresent() {
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        mockContext.Tonics.Add(tonic);
        mockContext.SaveChanges();
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var result = mockController.AddTonic(data);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Danger &&
                response.StatusMessage == "Sorry this tonic cannot be added as it is already part of our collection!");
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

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Success &&
                response.StatusMessage == "✅ Success! \"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic were paired!");
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

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Warning &&
                response.StatusMessage == "Please select the gin and tonic to pair");
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

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Should().Match<ApiResponse>(response =>
                response.BsColor == BsColor.Danger &&
                response.StatusMessage == "\"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic are already paired!");
    }
}
