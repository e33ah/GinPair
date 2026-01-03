#pragma warning disable CA1873 // CA1873 (performance rule) is suppressed in this test file because the flagged pattern occurs only in logging test assertions and does not impact production performance.

using Microsoft.Extensions.Logging;
using Moq;

namespace GinPair.Tests;

public class LoggingTests {
    private const string TESTGINNAME = "TestGin";
    private const string TESTDISTILLERY = "TestDistillery";
    private const string TESTBRAND = "TestBrand";
    private const string TESTFLAVOUR = "TestFlavour";

    private static Mock<ILogger<GinApiController>> CreateMockLogger() {
        var mockLogger = new Mock<ILogger<GinApiController>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mockLogger;
    }

    private static GinApiController CreateSut(Mock<ILogger<GinApiController>> mockLogger, GinPairDbContext context) {
        var state = new DatabaseInitializationState { IsDatabaseReady = true };
        return new GinApiController(context, state, mockLogger.Object);
    }

    private static void VerifyLogContains(Mock<ILogger<GinApiController>> mockLogger, LogLevel level, params string[] expectedContent) {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => expectedContent.All(content => v.ToString()!.Contains(content))),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void AddGin_LogsInformation_WhenGinAddedSuccessfully() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            ginName = TESTGINNAME,
            distillery = TESTDISTILLERY,
            description = "Test"
        });

        var result = sut.AddGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "Gin added successfully");
    }

    [Fact]
    public void AddTonic_LogsInformation_WhenTonicAddedSuccessfully() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            tonicBrand = TESTBRAND,
            tonicFlavour = TESTFLAVOUR
        });

        var result = sut.AddTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "Tonic added successfully");
    }

    [Fact]
    public void AddPairing_LogsStructuredData_WhenPairingAddedSuccessfully() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);

        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            ginId = "1",
            tonicId = "1"
        });

        var result = sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information,
            "Pairing added successfully", TESTDISTILLERY, TESTBRAND);
    }

    [Fact]
    public void DeleteGin_LogsInformation_WhenGinDeletedSuccessfully() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { ginId = "1" });

        var result = sut.DeleteGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information,
            "Gin deleted successfully", TESTDISTILLERY, TESTGINNAME);
    }

    [Fact]
    public void DeleteTonic_LogsInformation_WhenTonicDeletedSuccessfully() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedTonics(context, [CreateTonic(1, TESTBRAND, TESTFLAVOUR)]);
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { tonicId = "1" });

        var result = sut.DeleteTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information,
            "Tonic deleted successfully", TESTBRAND, TESTFLAVOUR);
    }

    [Fact]
    public void DeletePairing_LogsInformation_WhenPairingDeletedSuccessfully() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);
        SeedPairings(context, [CreatePairing(gin, tonic)]);

        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { pairingId = "1" });

        var result = sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information,
            "Pairing deleted successfully", TESTDISTILLERY, TESTBRAND);
    }

    [Fact]
    public async Task MatchGin_LogsWarning_WhenDatabaseIsEmpty() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var result = await sut.MatchGin("test");

        result.ShouldBeOfType<BadRequestResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "MatchGin called but database contains no gins");
    }

    [Fact]
    public async Task MatchGin_LogsInformation_WhenSearchingForGin() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockLogger, context);

        var result = await sut.MatchGin("test");

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "Searching for gin with partial match", "test");
    }

    [Fact]
    public async Task MatchGin_LogsInformation_WhenNoResultsFound() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockLogger, context);

        var result = await sut.MatchGin("non-existent");

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "No results found for search term", "non-existent");
    }

    [Fact]
    public async Task MatchGin_LogsInformation_WhenResultsFound() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockLogger, context);

        var result = await sut.MatchGin("test");

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "Found", "gin(s) matching search term", "test");
    }

    [Fact]
    public async Task GetPairingByGinId_LogsWarning_WhenGinNotFound() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var result = await sut.GetPairingByGinId(999);

        result.ShouldBeOfType<BadRequestResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "GetPairingByGinId called with invalid GinId", "999");
    }

    [Fact]
    public async Task GetPairingByGinId_LogsInformation_WhenNoPairingsFound() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockLogger, context);

        var result = await sut.GetPairingByGinId(1);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "No pairings found for gin", TESTDISTILLERY, TESTGINNAME);
    }

    [Fact]
    public async Task GetPairingByGinId_LogsInformation_WhenPairingSuggested() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);
        SeedPairings(context, [CreatePairing(gin, tonic)]);

        var sut = CreateSut(mockLogger, context);

        var result = await sut.GetPairingByGinId(1);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Information, "Pairing suggested for", TESTDISTILLERY, TESTBRAND);
    }

    [Theory]
    [InlineData(null, "TestDistillery", "GinName: (null)", "Distillery: TestDistillery")]
    [InlineData("TestGin", null, "GinName: TestGin", "Distillery: (null)")]
    [InlineData(null, null, "GinName: (null)", "Distillery: (null)")]
    [InlineData("", "", "GinName: ", "Distillery: ")]
    public void AddGin_LogsWarningWithNullOrEmptyValues_WhenGinNameOrDistilleryIsMissing(string? ginName, string? distillery, string expectedGinNameLog, string expectedDistilleryLog) {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            ginName,
            distillery,
            description = "Test"
        });

        var result = sut.AddGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "AddGin validation failed: missing required fields", expectedGinNameLog, expectedDistilleryLog);
    }

    [Fact]
    public void AddGin_LogsWarning_WhenDuplicateGin() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            ginName = TESTGINNAME,
            distillery = TESTDISTILLERY,
            description = "Test"
        });

        var result = sut.AddGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "AddGin validation failed: duplicate gin", TESTDISTILLERY, TESTGINNAME);
    }

    [Theory]
    [InlineData(null, "TestFlavour", "TonicBrand: (null)", "TonicFlavour: TestFlavour")]
    [InlineData("TestBrand", null, "TonicBrand: TestBrand", "TonicFlavour: (null)")]
    [InlineData(null, null, "TonicBrand: (null)", "TonicFlavour: (null)")]
    [InlineData("", "", "TonicBrand: ", "TonicFlavour: ")]
    public void AddTonic_LogsWarningWithNullOrEmptyValues_WhenTonicBrandOrFlavourIsMissing(string? tonicBrand, string? tonicFlavour, string expectedBrandLog, string expectedFlavourLog) {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            tonicBrand,
            tonicFlavour
        });

        var result = sut.AddTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "AddTonic validation failed: missing required fields", expectedBrandLog, expectedFlavourLog);
    }

    [Fact]
    public void AddTonic_LogsWarning_WhenDuplicateTonic() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedTonics(context, [CreateTonic(1, TESTBRAND, TESTFLAVOUR)]);
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            tonicBrand = TESTBRAND,
            tonicFlavour = TESTFLAVOUR
        });

        var result = sut.AddTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "AddTonic validation failed: duplicate tonic", TESTBRAND, TESTFLAVOUR);
    }

    [Theory]
    [InlineData(null, "1", "GinId: (null)", "TonicId: 1")]
    [InlineData("1", null, "GinId: 1", "TonicId: (null)")]
    [InlineData(null, null, "GinId: (null)", "TonicId: (null)")]
    [InlineData("", "", "GinId: ", "TonicId: ")]
    public void AddPairing_LogsWarningWithNullOrEmptyValues_WhenGinIdOrTonicIdIsMissing(string? ginId, string? tonicId, string expectedGinIdLog, string expectedTonicIdLog) {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            ginId,
            tonicId
        });

        var result = sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "AddPairing validation failed: missing required fields", expectedGinIdLog, expectedTonicIdLog);
    }

    [Fact]
    public void AddPairing_LogsWarning_WhenDuplicatePairing() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);
        SeedPairings(context, [CreatePairing(gin, tonic)]);

        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new {
            ginId = "1",
            tonicId = "1"
        });

        var result = sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "AddPairing validation failed: duplicate pairing", TESTDISTILLERY, TESTBRAND);
    }

    [Theory]
    [InlineData("", "DeleteGin validation failed: invalid GinId")]
    [InlineData(null, "DeleteGin validation failed: invalid GinId", "(null)")]
    [InlineData("0", "DeleteGin validation failed: invalid GinId", "0")]
    [InlineData("999", "DeleteGin validation failed: gin not found", "999")]
    public void DeleteGin_LogsWarning_WhenInvalidOrNullGinId(string? ginId, params string[] expectedContent) {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { ginId });

        var result = sut.DeleteGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, expectedContent);
    }

    [Theory]
    [InlineData("", "DeleteTonic validation failed: invalid TonicId")]
    [InlineData(null, "DeleteTonic validation failed: invalid TonicId", "(null)")]
    [InlineData("0", "DeleteTonic validation failed: invalid TonicId", "0")]
    [InlineData("999", "DeleteTonic validation failed: tonic not found", "999")]
    public void DeleteTonic_LogsWarning_WhenInvalidOrNullTonicId(string? tonicId, params string[] expectedContent) {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { tonicId });

        var result = sut.DeleteTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, expectedContent);
    }

    [Theory]
    [InlineData("", "DeletePairing validation failed: invalid PairingId")]
    [InlineData(null, "DeletePairing validation failed: invalid PairingId", "(null)")]
    [InlineData("0", "DeletePairing validation failed: invalid PairingId", "0")]
    public void DeletePairing_LogsWarning_WhenInvalidOrNullPairingId(string? pairingId, params string[] expectedContent) {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { pairingId });

        var result = sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, expectedContent);
    }

    [Fact]
    public void DeletePairing_LogsWarning_WhenPairingNotFound() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { pairingId = "999" });

        var result = sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "DeletePairing validation failed: pairing not found", "999");
    }

    [Fact]
    public void DeletePairing_LogsWarning_WhenGinOrTonicNotFoundForPairing() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        
        var pairing = new Pairing { PairingId = 1, GinId = 999, TonicId = 999 };
        context.Pairings.Add(pairing);
        context.SaveChanges();
        
        var sut = CreateSut(mockLogger, context);

        var data = BuildJsonElement(new { pairingId = "1" });

        var result = sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockLogger, LogLevel.Warning, "DeletePairing validation failed: gin or tonic not found for pairing");
    }
}
