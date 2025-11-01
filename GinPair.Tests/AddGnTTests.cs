namespace GinPair.Tests;

public class AddGnTTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public AddGnTTests() {
        mockContext = CreateInMemoryGinPairDbContext();
        mockController = CreateController(mockContext);
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsAddedSuccessfully() {
        ResetDatabase(mockContext);
        string jsonData = JsonSerializer.Serialize(new { ginName = "Test Gin", distillery = "Test Distillery", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddGin(data);

        AssertApiResponse(result, BsColor.Success, "✅ Success! \"Test Distillery Test Gin\" gin was added!");
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsEmpty() {
        ResetDatabase(mockContext);
        string jsonData = JsonSerializer.Serialize(new { ginName = "", distillery = "", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddGin(data);

        AssertApiResponse(result, BsColor.Warning, "Please provide the name of the Distillery and Gin.");
    }

    [Fact]
    public void IsGinPresent_ReturnsTrue_WhenGinExists() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "TestGin", "TestDistillery");
        SeedGins(mockContext, new[] { gin });

        bool result = mockController.IsGinPresent("TestGin", "TestDistillery");

        result.ShouldBeTrue();
    }

    [Fact]
    public void AddGin_ReturnsOk_WhenGinIsPresent() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        SeedGins(mockContext, new[] { gin });
        string jsonData = JsonSerializer.Serialize(new { ginName = "Test Gin", distillery = "Test Distillery", description = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddGin(data);

        AssertApiResponse(result, BsColor.Danger, "Sorry this gin cannot be added as it is already part of our collection!");
    }

    [Theory]
    [InlineData(null, "Distillery")]
    [InlineData("GinName", null)]
    [InlineData("", "Distillery")]
    [InlineData("GinName", "")]
    public void AddGin_ReturnsOk_WhenNameOrDistilleryMissing(string? ginName, string? distillery) {
        ResetDatabase(mockContext);
        string json = $@"{{ ""ginName"": ""{ginName}"", ""distillery"": ""{distillery}"", ""description"": ""desc"" }}";
        var doc = JsonDocument.Parse(json);

        var sut = mockController;
        var result = sut.AddGin(doc.RootElement);

        AssertApiResponse(result, BsColor.Warning, "Please provide the name of the Distillery and Gin.");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsAddedSuccessfully() {
        ResetDatabase(mockContext);
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddTonic(data);

        AssertApiResponse(result, BsColor.Success, "✅ Success! \"Test Brand Test Flavour\" tonic was added!");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsEmpty() {
        ResetDatabase(mockContext);
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "", tonicFlavour = "" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddTonic(data);

        AssertApiResponse(result, BsColor.Warning, "Please provide the tonic brand and flavour/name.");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsPresent() {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedTonics(mockContext, new[] { tonic });
        string jsonData = JsonSerializer.Serialize(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddTonic(data);

        AssertApiResponse(result, BsColor.Danger, "Sorry this tonic cannot be added as it is already part of our collection!");
    }

    [Fact]
    public void AddPairing_ReturnsOk_WhenPairingIsAddedSuccessfully() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedGins(mockContext, new[] { gin });
        SeedTonics(mockContext, new[] { tonic });

        string jsonData = JsonSerializer.Serialize(new { ginId = "1", tonicId = "1" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddPairing(data);

        AssertApiResponse(result, BsColor.Success, "✅ Success! \"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic were paired!");
    }

    [Theory]
    [InlineData("1", "")]
    [InlineData("", "1")]
    [InlineData(null, "1")]
    [InlineData("1", null)]
    [InlineData("1", "1")]
    public void AddPairing_ReturnsOk_WhenGinOrTonicIsNullOrEmpty(string? ginId, string? tonicId) {
        ResetDatabase(mockContext);
        string jsonData = JsonSerializer.Serialize(new { ginId, tonicId });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddPairing(data);

        AssertApiResponse(result, BsColor.Warning, "Please select the gin and tonic to pair");
    }

    [Fact]
    public void AddPairing_ReturnsOk_WhenGinOrTonicIsAlreadyPresent() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, new[] { gin });
        SeedTonics(mockContext, new[] { tonic });
        SeedPairings(mockContext, new[] { pairing });

        string jsonData = JsonSerializer.Serialize(new { ginId = "1", tonicId = "1" });
        var data = JsonDocument.Parse(jsonData).RootElement;

        var sut = mockController;
        var result = sut.AddPairing(data);

        AssertApiResponse(result, BsColor.Danger, "\"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic are already paired!");
    }
}
