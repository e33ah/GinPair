namespace GinPair.Tests;

public class DeleteGnTTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public DeleteGnTTests() {
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
    public void DeleteGin_ShouldReturnSuccess_WhenGinIsDeleted() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        mockContext.Gins.Add(gin);
        string json = JsonSerializer.Serialize(new { ginId = "1" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteGin(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;
        var deletedGin = mockContext.Gins.Find(1);

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);
        response.ShouldNotBeNull();
        response.BsColor.ToString().ShouldBe("Success");
        response.StatusMessage.ShouldContain("was removed");
        deletedGin.ShouldBeNull();
    }

    [Fact]
    public void DeleteGin_ShouldReturnWarning_WhenGinIdIsInvalid() {
        string json = JsonSerializer.Serialize(new { ginId = "0" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteGin(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);
        response.ShouldNotBeNull();
        response.BsColor.ToString().ShouldBe("Warning");
        response.StatusMessage.ShouldBe("Please select a gin to delete");
    }

    [Fact]
    public void DeleteTonic_ShouldReturnSuccess_WhenTonicIsDeleted() {
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        mockContext.Tonics.Add(tonic);
        string json = JsonSerializer.Serialize(new { tonicId = "1" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteTonic(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;
        var deletedTonic = mockContext.Tonics.Find(1);

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);
        response.ShouldNotBeNull();
        response.BsColor.ToString().ShouldBe("Success");
        response.StatusMessage.ShouldContain("was removed");
        deletedTonic.ShouldBeNull();
    }

    [Fact]
    public void DeleteTonic_ShouldReturnWarning_WhenTonicIdIsInvalid() {
        string json = JsonSerializer.Serialize(new { tonicId = "0" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteTonic(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);
        response.ShouldNotBeNull();
        response.BsColor.ToString().ShouldBe("Warning");
        response.StatusMessage.ShouldBe("Please select a tonic to delete");
    }

    [Fact]
    public void DeletePairing_ShouldReturnSuccess_WhenPairingIsDeleted() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        var tonic = new Tonic { TonicId = 1, TonicBrand = "Test Brand", TonicFlavour = "Test Flavour" };
        var pairing = new Pairing { PairingId = 1, PairedGin = gin, PairedTonic = tonic };
        mockContext.Gins.Add(gin);
        mockContext.Tonics.Add(tonic);
        mockContext.Pairings.Add(pairing);
        string json = JsonSerializer.Serialize(new { pairingId = "1" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeletePairing(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;
        var deletedPairing = mockContext.Pairings.Find(1);

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);
        response.ShouldNotBeNull();
        response.BsColor.ToString().ShouldBe("Success");
        response.StatusMessage.ShouldContain("was removed");
        deletedPairing.ShouldBeNull();

    }

    [Fact]
    public void DeletePairing_ShouldReturnWarning_WhenPairingIdIsInvalid() {
        string json = JsonSerializer.Serialize(new { pairingId = "0" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeletePairing(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;

        result.ShouldNotBeNull();
        result.StatusCode.ShouldBe(200);
        response.ShouldNotBeNull();
        response.BsColor.ToString().ShouldBe("Warning");
        response.StatusMessage.ShouldBe("Please select a pairing to delete");
    }
}
