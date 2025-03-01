namespace GinPair.Tests;

public class DeleteGnTTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public DeleteGnTTests() {
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
    public void DeleteGin_ShouldReturnSuccess_WhenGinIsDeleted() {
        var gin = new Gin { GinId = 1, GinName = "Test Gin", Distillery = "Test Distillery" };
        mockContext.Gins.Add(gin);
        string json = JsonSerializer.Serialize(new { ginId = "1" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteGin(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;
        var deletedGin = mockContext.Gins.Find(1);

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response.BsColor.ToString().Should().Be("Success");
        response.StatusMessage.Should().Contain("was removed");
        deletedGin.Should().BeNull();
    }

    [Fact]
    public void DeleteGin_ShouldReturnWarning_WhenGinIdIsInvalid() {
        string json = JsonSerializer.Serialize(new { ginId = "0" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteGin(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response.BsColor.ToString().Should().Be("Warning");
        response.StatusMessage.Should().Be("Please select a gin to delete");
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

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response.BsColor.ToString().Should().Be("Success");
        response.StatusMessage.Should().Contain("was removed");
        deletedTonic.Should().BeNull();
    }

    [Fact]
    public void DeleteTonic_ShouldReturnWarning_WhenTonicIdIsInvalid() {
        string json = JsonSerializer.Serialize(new { tonicId = "0" });
        var data = JsonDocument.Parse(json).RootElement;

        var result = mockController.DeleteTonic(data) as OkObjectResult;
        var response = result?.Value as ApiResponse;

        using var scope = new AssertionScope();
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        response.Should().NotBeNull();
        response.BsColor.ToString().Should().Be("Warning");
        response.StatusMessage.Should().Be("Please select a tonic to delete");
    }
}
