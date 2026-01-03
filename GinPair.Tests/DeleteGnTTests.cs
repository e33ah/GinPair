namespace GinPair.Tests;

public class DeleteGnTTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public DeleteGnTTests() {
        mockContext = CreateInMemoryGinPairDbContext();
        mockController = CreateController(mockContext);
    }

    [Fact]
    public void DeleteGin_ShouldReturnSuccess_WhenGinIsDeleted() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        SeedGins(mockContext, [gin]);
        var data = BuildJsonElement(new { ginId = "1" });
        string ginToBeDeleted = $"{gin.Distillery} {gin.GinName}";

        var sut = mockController;
        var result = sut.DeleteGin(data);

        AssertApiResponse(result, BsColor.Success, $"{ginToBeDeleted}\" gin was removed");
        var deletedGin = mockContext.Gins.Find(1);
        deletedGin.ShouldBeNull();
    }

    [Fact]
    public void DeleteGin_ShouldReturnWarning_WhenGinIdIsInvalid() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { ginId = "0" });

        var sut = mockController;
        var result = sut.DeleteGin(data);

        AssertApiResponse(result, BsColor.Warning, "Please select a gin to delete");
    }

    [Fact]
    public void DeleteGin_ShouldReturnWarning_WhenGinIdNotFound() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { ginId = "2" });

        var sut = mockController;
        var result = sut.DeleteGin(data);

        AssertApiResponse(result, BsColor.Warning, "Gin not found");
    }

    [Fact]
    public void DeleteTonic_ShouldReturnSuccess_WhenTonicIsDeleted() {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedTonics(mockContext, [tonic]);
        var data = BuildJsonElement(new { tonicId = "1" });
        string tonicToBeDeleted = $"{tonic.TonicBrand} {tonic.TonicFlavour}";

        var sut = mockController;
        var result = sut.DeleteTonic(data);

        AssertApiResponse(result, BsColor.Success, $"{tonicToBeDeleted}\" tonic was removed");
        var deletedTonic = mockContext.Tonics.Find(1);
        deletedTonic.ShouldBeNull();
    }

    [Fact]
    public void DeleteTonic_ShouldReturnWarning_WhenTonicIdIsInvalid() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { tonicId = "0" });

        var sut = mockController;
        var result = sut.DeleteTonic(data);

        AssertApiResponse(result, BsColor.Warning, "Please select a tonic to delete");
    }

    [Fact]
    public void DeleteTonic_ShouldReturnWarning_WhenTonicIdNotFound() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { tonicId = "2" });

        var sut = mockController;
        var result = sut.DeleteTonic(data);

        AssertApiResponse(result, BsColor.Warning, "Tonic not found");
    }

    [Fact]
    public void DeletePairing_ShouldReturnSuccess_WhenPairingIsDeleted() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "Test Gin", "Test Distillery");
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        var pairing = CreatePairing(gin, tonic);
        SeedGins(mockContext, [gin]);
        SeedTonics(mockContext, [tonic]);
        SeedPairings(mockContext, [pairing]);
        var data = BuildJsonElement(new { pairingId = "1" });
        string ginToBeDeleted = $"{gin.Distillery} {gin.GinName}";
        string tonicToBeDeleted = $"{tonic.TonicBrand} {tonic.TonicFlavour}";
        string pairingToBeDeleted = $"\"{ginToBeDeleted} gin and {tonicToBeDeleted} tonic\"";

        var sut = mockController;
        var result = sut.DeletePairing(data);

        AssertApiResponse(result, BsColor.Success, $"{pairingToBeDeleted} pairing was removed");
        var deletedPairing = mockContext.Pairings.Find(pairing.PairingId);
        deletedPairing.ShouldBeNull();
    }

    [Fact]
    public void DeletePairing_ShouldReturnWarning_WhenPairingIdIsInvalid() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { pairingId = "0" });

        var sut = mockController;
        var result = sut.DeletePairing(data);

        AssertApiResponse(result, BsColor.Warning, "Please select a pairing to delete");
    }

    [Fact]
    public void DeletePairing_ShouldReturnWarning_WhenPairingIdNotFound() {
        ResetDatabase(mockContext);
        var data = BuildJsonElement(new { pairingId = "2" });

        var sut = mockController;
        var result = sut.DeletePairing(data);

        AssertApiResponse(result, BsColor.Warning, "Pairing not found");
    }

    [Fact]
    public void DeletePairing_ShouldReturnWarning_WhenGinOrTonicNotFound() {
        ResetDatabase(mockContext);

        var pairing = new Pairing { PairingId = 1, GinId = 999, TonicId = 999 };
        mockContext.Pairings.Add(pairing);
        mockContext.SaveChanges();

        var sut = mockController;

        var data = BuildJsonElement(new { pairingId = "1" });

        var result = sut.DeletePairing(data);

        AssertApiResponse(result, BsColor.Danger, "An Error occurred: Gin or Tonic not found. Not able to delete pairing");
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, false)]
    public void IsGinOrTonicNull_WorksCorrectly(bool isGinNull, bool isTonicNull, bool expectedResult) {
        var gin = isGinNull ? null : new Gin();
        var tonic = isTonicNull ? null : new Tonic();

        bool result = GinApiController.IsGinOrTonicNull(gin, tonic);

        result.ShouldBe(expectedResult);
    }
}
