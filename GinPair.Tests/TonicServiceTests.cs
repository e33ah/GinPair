namespace GinPair.Tests;

public class TonicServiceTests {
    private readonly GinPairDbContext mockContext;
    private readonly TonicService tonicService;

    public TonicServiceTests() {
        mockContext = CreateInMemoryGinPairDbContext();
        var tonicServiceLogger = new NullLogger<TonicService>();
        tonicService = new TonicService(mockContext, tonicServiceLogger);
    }

    [Fact]
    public async Task TonicExistsAsync_ReturnsTrue_WhenTonicExists() {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, "Test Brand", "Test Flavour");
        SeedTonics(mockContext, [tonic]);

        bool result = await tonicService.TonicExistsAsync("Test Brand", "Test Flavour");

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task TonicExistsAsync_ReturnsFalse_WhenTonicNotExists() {
        ResetDatabase(mockContext);

        bool result = await tonicService.TonicExistsAsync("NonExistentBrand", "NonExistentFlavour");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task TonicExistsAsync_ReturnsFalse_WhenTonicNameOrFlavourIsNull(bool isTonicBrandNull, bool isFlavourNull) {
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

        bool result = await tonicService.TonicExistsAsync("TestBrand", "TestFlavour");

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("OtherBrand", "TestFlavour")]
    [InlineData("TestBrand", "OtherFlavour")]
    public async Task TonicExistsAsync_ReturnsFalse_WhenTonicNameOrFlavourNotMatch(string tonicBrandInput, string tonicFlavourInput) {
        ResetDatabase(mockContext);
        var tonic = CreateTonic(1, tonicBrandInput, tonicFlavourInput);
        SeedTonics(mockContext, [tonic]);

        bool result = await tonicService.TonicExistsAsync("TestBrand", "TestFlavour");

        result.ShouldBeFalse();
    }
}
