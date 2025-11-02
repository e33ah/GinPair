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
    public void AddGin_ReturnsOk_WhenGinIsPresent() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        SeedGins(mockContext, [gin]);
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
        var data = BuildJsonElement(new { ginName, distillery, description = "desc" });
        
        var sut = mockController;
        var result = sut.AddGin(data);

        AssertApiResponse(result, BsColor.Warning, "Please provide the name of the Distillery and Gin.");
    }

    [Fact]
    public void IsGinPresent_ReturnsTrue_WhenGinExists() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "TestGin", "TestDistillery");
        SeedGins(mockContext, [gin]);

        var sut = mockController;
        bool result = sut.IsGinPresent("TestGin", "TestDistillery");

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsGinPresent_ReturnsFalse_WhenGinNotExists() {
        ResetDatabase(mockContext);

        var sut = mockController;
        bool result = sut.IsGinPresent("NonExistentGin", "NonExistentDistillery");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void IsGinPresent_ReturnsFalse_WhenGinNameOrDistilleryIsNull(bool isGinNameNull, bool isDistilleryNull) {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "TestGin", "SomeDistillery");
        SeedGins(mockContext, [gin]);

        if (isGinNameNull) {
            gin.GinName = null;
        }
        if (isDistilleryNull) {
            gin.Distillery = null;
        }
        mockContext.SaveChanges();
        var sut = mockController;
        bool result = sut.IsGinPresent("TestGin", "SomeDistillery");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("TestGin", "OtherDistillery")]
    [InlineData("OtherGin", "SomeDistillery")]
    public void IsGinPresent_ReturnsFalse_WhenGinNameOrDistilleryNotMatch(string ginNameInput, string distilleryInput) {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, ginNameInput, distilleryInput);
        SeedGins(mockContext, [gin]);

        var sut = mockController;
        bool result = sut.IsGinPresent("TestGin", "SomeDistillery");

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsAddedSuccessfully() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });

        var sut = mockController;
        var result = sut.AddTonic(data);

        AssertApiResponse(result, BsColor.Success, "✅ Success! \"Test Brand Test Flavour\" tonic was added!");
    }

    [Theory]
    [InlineData("newBrand", "")]
    [InlineData("", "newFlav")]
    [InlineData(null, "newFlavour")]
    [InlineData("newBrand", null)]
    [InlineData(null, null)]
    public void AddTonic_ReturnsOk_WhenTonicIsNullOrEmpty(string? tonicBrand, string? tonicFlavour) {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { tonicBrand, tonicFlavour });

        var sut = mockController;
        var result = sut.AddTonic(data);

        AssertApiResponse(result, BsColor.Warning, "Please provide the tonic brand and flavour/name.");
    }

    [Fact]
    public void AddTonic_ReturnsOk_WhenTonicIsPresent() {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedTonics(mockContext, [tonic]);
        var data = BuildJsonElement(new { tonicBrand = "Test Brand", tonicFlavour = "Test Flavour" });

        var sut = mockController;
        var result = sut.AddTonic(data);

        AssertApiResponse(result, BsColor.Danger, "Sorry this tonic cannot be added as it is already part of our collection!");
    }

    [Fact]
    public void IsTonicPresent_ReturnsTrue_WhenTonicExists() {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedTonics(mockContext, [tonic]);

        var sut = mockController;
        bool result = sut.IsTonicPresent("Test Brand", "Test Flavour");

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsTonicPresent_ReturnsFalse_WhenTonicNotExists() {
        ResetDatabase(mockContext);

        var sut = mockController;
        bool result = sut.IsTonicPresent("NonExistentBrand", "NonExistentFlavour");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void IsTonicPresent_ReturnsFalse_WhenTonicNameOrFlavourIsNull(bool isTonicBrandNull, bool isFlavourNull) {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, "TestBrand", "TestFlavour");
        SeedTonics(mockContext, [tonic]);

        if (isTonicBrandNull) {
            tonic.TonicBrand = null;
        }
        if (isFlavourNull) {
            tonic.TonicFlavour = null;
        }
        mockContext.SaveChanges();
        var sut = mockController;
        bool result = sut.IsTonicPresent("TestBrand", "TestFlavour");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("OtherBrand", "TestFlavour")]
    [InlineData("TestBrand", "OtherFlavour")]
    public void IsTonicPresent_ReturnsFalse_WhenTonicNameOrFlavourNotMatch(string tonicBrandInput, string tonicFlavourInput) {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, tonicBrandInput, tonicFlavourInput);
        SeedTonics(mockContext, [tonic]);

        var sut = mockController;
        bool result = sut.IsTonicPresent("TestBrand", "TestFlavour");

        result.ShouldBeFalse();
    }

    [Fact]
    public void AddPairing_ReturnsOk_WhenPairingIsAddedSuccessfully() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);

        var data = BuildJsonElement(new { ginId = "1", tonicId = "1" });

        var sut = mockController;
        var result = sut.AddPairing(data);

        AssertApiResponse(result, BsColor.Success, "✅ Success! \"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic were paired!");
    }

    [Theory]
    [InlineData("1", "")]
    [InlineData("", "1")]
    [InlineData("1", "1")]
    [InlineData(null, "1")]
    [InlineData("1", null)]
    [InlineData(null, null)]
    public void AddPairing_ReturnsOk_WhenGinOrTonicIsNullOrEmpty(string? ginId, string? tonicId) {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { ginId, tonicId });

        var sut = mockController;
        var result = sut.AddPairing(data);

        AssertApiResponse(result, BsColor.Warning, "Please select the gin and tonic to pair");
    }

    [Theory]
    [InlineData("999", "1")]
    [InlineData("1", "999")]
    public void AddPairing_ReturnsWarning_WhenGinOrTonicNotFound(string ginIdInput, string tonicIdInput) {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "This gin is here");
        var tonic = CreateTonic(1, "This tonic is here");
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);

        var data = BuildJsonElement(new { ginId = ginIdInput, tonicId = tonicIdInput });

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
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);
        var data = BuildJsonElement(new { ginId = "1", tonicId = "1" });

        var sut = mockController;
        var result = sut.AddPairing(data);

        AssertApiResponse(result, BsColor.Danger, "\"Test Distillery Test Gin\" gin and \"Test Brand Test Flavour\" tonic are already paired!");
    }

    [Fact]
    public void IsPairingPresent_ReturnsTrue_WhenPairingExists() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "TestGin", "TestDistillery");
        var tonic = CreateTonic(1, "TestBrand", "TestFlavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);

        var sut = mockController;
        bool result = sut.IsPairingPresent(1, 1);

        result.ShouldBeTrue();
    }

    [Fact]
    public void IsPairingPresent_ReturnsFalse_WhenPairingNotExists() {
        ResetDatabase(mockContext);

        var sut = mockController;
        bool result = sut.IsPairingPresent(999, 888);

        result.ShouldBeFalse();
    }


    [Theory]
    [InlineData(2, 1)]
    [InlineData(1, 2)]
    public void IsPairingPresent_ReturnsFalse_WhenIdsNotMatch(int ginIdInput, int tonicIdInput) {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "PairMeGin");
        var tonic = CreateTonic(1, "PairMeTonic");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings (mockContext, [pairing]);

        var sut = mockController;
        bool result = sut.IsPairingPresent(ginIdInput, tonicIdInput);

        result.ShouldBeFalse();
    }
}
