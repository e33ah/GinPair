namespace GinPair.Tests;

public class GinServiceTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinService ginService;

    public GinServiceTests() {
        mockContext = CreateInMemoryGinPairDbContext();
        var ginServiceLogger = new NullLogger<GinService>();
        ginService = new GinService(mockContext, ginServiceLogger);
    }

    [Fact]
    public async Task GinExistsAsync_ReturnsTrue_WhenGinExists() {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, "TestGin", "TestDistillery");
        SeedGins(mockContext, [gin]);

        bool result = await ginService.GinExistsAsync("TestGin", "TestDistillery");

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task GinExistsAsync_ReturnsFalse_WhenGinNotExists() {
        ResetDatabase(mockContext);

        bool result = await ginService.GinExistsAsync("NonExistentGin", "NonExistentDistillery");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task GinExistsAsync_ReturnsFalse_WhenGinNameOrDistilleryIsNull(bool isGinNameNull, bool isDistilleryNull) {
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

        bool result = await ginService.GinExistsAsync("TestGin", "SomeDistillery");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("TestGin", "OtherDistillery")]
    [InlineData("OtherGin", "SomeDistillery")]
    public async Task GinExistsAsync_ReturnsFalse_WhenGinNameOrDistilleryNotMatch(string ginNameInput, string distilleryInput) {
        ResetDatabase(mockContext);
        var gin = CreateGin(1, ginNameInput, distilleryInput);
        SeedGins(mockContext, [gin]);

        bool result = await ginService.GinExistsAsync("TestGin", "SomeDistillery");

        result.ShouldBeFalse();
    }
}
