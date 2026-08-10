namespace GinPair.Tests;

public class PairingServiceTests {
    private readonly GinPairDbContext mockContext;
    private readonly PairingService pairingService;

    public PairingServiceTests() {
        mockContext = CreateInMemoryGinPairDbContext();
        var ginServiceLogger = new NullLogger<GinService>();
        var tonicServiceLogger = new NullLogger<TonicService>();
        var pairingServiceLogger = new NullLogger<PairingService>();

        var ginService = new GinService(mockContext, ginServiceLogger);
        var tonicService = new TonicService(mockContext, tonicServiceLogger);
        pairingService = new PairingService(mockContext, ginService, tonicService, pairingServiceLogger);
    }

    [Fact]
    public async Task GetRandomPairingForGinAsync_ReturnsFailure_WithMessage_WhenGinNotFound() {
        ResetDatabase(mockContext);

        var result = await pairingService.GetRandomPairingForGinAsync(999);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Gin not found");
        result.BsColor.ShouldBe(BsColor.Danger);
    }

    [Fact]
    public async Task GetRandomPairingForGinAsync_ReturnsSuccess_WithWarning_WhenNoPairingsExist() {
        ResetDatabase(mockContext);
        string ginName = "Lonely";
        string distillery = "Solo";
        SeedGins(mockContext, [CreateGin(10, ginName, distillery)]);

        var result = await pairingService.GetRandomPairingForGinAsync(10);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.BsColor.ShouldBe(BsColor.Warning);
        result.Data.StatusMessage.ShouldContain($"Sorry, there is no pairing available for \"{distillery} {ginName}\"");
    }

    [Fact]
    public async Task GetRandomPairingForGinAsync_ReturnsSuccess_WithPairing_WhenPairingsExist() {
        ResetDatabase(mockContext);
        int ginId = 5;
        string ginName = "Alpha";
        string distillery = "Beta";
        int tonicId = 7;
        string brand = "BrandX";
        string flavour = "Citrus";

        var gin = CreateGin(ginId, ginName, distillery);
        var tonic = CreateTonic(tonicId, brand, flavour);
        var pairing = CreatePairing(gin, tonic);

        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);

        var result = await pairingService.GetRandomPairingForGinAsync(ginId);

        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBe("Pairing found");
        result.Data.ShouldNotBeNull();
        result.Data.BsColor.ShouldBe(BsColor.Primary);
        result.Data.StatusMessage.ShouldContain($"{distillery} {ginName}");
        result.Data.StatusMessage.ShouldContain($"{brand} {flavour}");
    }

    [Fact]
    public async Task AddPairingAsync_ReturnsWarning_WhenGinIsNull() {
        ResetDatabase(mockContext);
        int validTonicId = 1;
        var tonic = CreateTonic(validTonicId, "ValidBrand", "ValidFlavour");
        SeedTonics(mockContext, [tonic]);

        var result = await pairingService.AddPairingAsync(999, validTonicId);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Please select the gin and tonic to pair");
        result.BsColor.ShouldBe(BsColor.Warning);
    }

    [Fact]
    public async Task AddPairingAsync_ReturnsWarning_WhenTonicIsNull() {
        ResetDatabase(mockContext);
        int validGinId = 1;
        var gin = CreateGin(validGinId, "ValidGin", "ValidDistillery");
        SeedGins(mockContext, [gin]);

        var result = await pairingService.AddPairingAsync(validGinId, 999);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Please select the gin and tonic to pair");
        result.BsColor.ShouldBe(BsColor.Warning);
    }

    [Fact]
    public async Task AddPairingAsync_ReturnsWarning_WhenBothGinAndTonicAreNull() {
        ResetDatabase(mockContext);

        var result = await pairingService.AddPairingAsync(999, 888);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("Please select the gin and tonic to pair");
        result.BsColor.ShouldBe(BsColor.Warning);
    }

    [Fact]
    public async Task PairingExistsAsync_ReturnsTrue_WhenBothGinIdAndTonicIdMatch() {
        ResetDatabase(mockContext);
        int ginId = 1;
        int tonicId = 1;
        var gin = CreateGin(ginId, "TestGin", "TestDistillery");
        var tonic = CreateTonic(tonicId, "TestBrand", "TestFlavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);

        bool result = await pairingService.PairingExistsAsync(ginId, tonicId);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task PairingExistsAsync_ReturnsFalse_WhenOnlyGinIdMatches() {
        ResetDatabase(mockContext);
        int matchingGinId = 1;
        int matchingTonicId = 1;
        int differentTonicId = 2;
        var gin = CreateGin(matchingGinId, "TestGin", "TestDistillery");
        var tonic = CreateTonic(matchingTonicId, "TestBrand", "TestFlavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);

        bool result = await pairingService.PairingExistsAsync(matchingGinId, differentTonicId);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task PairingExistsAsync_ReturnsFalse_WhenOnlyTonicIdMatches() {
        ResetDatabase(mockContext);
        int matchingGinId = 1;
        int differentGinId = 2;
        int matchingTonicId = 1;
        var gin = CreateGin(matchingGinId, "TestGin", "TestDistillery");
        var tonic = CreateTonic(matchingTonicId, "TestBrand", "TestFlavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);

        bool result = await pairingService.PairingExistsAsync(differentGinId, matchingTonicId);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task PairingExistsAsync_ReturnsFalse_WhenNeitherIdMatches() {
        ResetDatabase(mockContext);
        int existingGinId = 1;
        int existingTonicId = 1;
        var gin = CreateGin(existingGinId, "TestGin", "TestDistillery");
        var tonic = CreateTonic(existingTonicId, "TestBrand", "TestFlavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);

        bool result = await pairingService.PairingExistsAsync(999, 888);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task AddPairingAsync_StoresCorrectGinIdAndTonicId_InDatabase() {
        ResetDatabase(mockContext);
        int ginId = 42;
        int tonicId = 99;
        var gin = CreateGin(ginId, "TestGin", "TestDistillery");
        var tonic = CreateTonic(tonicId, "TestBrand", "TestFlavour");
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);

        var result = await pairingService.AddPairingAsync(ginId, tonicId);

        result.IsSuccess.ShouldBeTrue();
        var storedPairing = mockContext.Pairings.FirstOrDefault(p => p.GinId == ginId && p.TonicId == tonicId);
        storedPairing.ShouldNotBeNull();
        storedPairing.GinId.ShouldBe(ginId);
        storedPairing.TonicId.ShouldBe(tonicId);
    }
}
