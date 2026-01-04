#pragma warning disable CA1873 // CA1873 (performance rule) is suppressed in this test file because the flagged pattern occurs only in logging test assertions and does not impact production performance.

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

    private static Mock<ILogger<GinService>> CreateMockGinServiceLogger() {
        var mockLogger = new Mock<ILogger<GinService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mockLogger;
    }

    private static Mock<ILogger<TonicService>> CreateMockTonicServiceLogger() {
        var mockLogger = new Mock<ILogger<TonicService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mockLogger;
    }

    private static Mock<ILogger<PairingService>> CreateMockPairingServiceLogger() {
        var mockLogger = new Mock<ILogger<PairingService>>();
        mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return mockLogger;
    }

    private static GinApiController CreateSut(
        Mock<ILogger<GinApiController>> mockControllerLogger,
        GinPairDbContext context,
        out Mock<ILogger<GinService>> mockGinServiceLogger,
        out Mock<ILogger<TonicService>> mockTonicServiceLogger,
        out Mock<ILogger<PairingService>> mockPairingServiceLogger) {

        mockGinServiceLogger = CreateMockGinServiceLogger();
        mockTonicServiceLogger = CreateMockTonicServiceLogger();
        mockPairingServiceLogger = CreateMockPairingServiceLogger();

        var ginService = new GinService(context, mockGinServiceLogger.Object);
        var tonicService = new TonicService(context, mockTonicServiceLogger.Object);
        var pairingService = new PairingService(context, ginService, tonicService, mockPairingServiceLogger.Object);
        var dbInitState = new DatabaseInitializationState { IsDatabaseReady = true };

        return new GinApiController(ginService, tonicService, pairingService, dbInitState, mockControllerLogger.Object);
    }

    private static void VerifyLogContains<T>(Mock<ILogger<T>> mockLogger, LogLevel level, params string[] expectedContent) {
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
    public async Task AddGin_LogsInformation_WhenGinAddedSuccessfully() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var data = BuildJsonElement(new {
            ginName = TESTGINNAME,
            distillery = TESTDISTILLERY,
            description = "Test"
        });

        var result = await sut.AddGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockGinServiceLogger, LogLevel.Information, "Gin added successfully");
    }

    [Fact]
    public async Task AddTonic_LogsInformation_WhenTonicAddedSuccessfully() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out var mockTonicServiceLogger, out _);

        var data = BuildJsonElement(new {
            tonicBrand = TESTBRAND,
            tonicFlavour = TESTFLAVOUR
        });

        var result = await sut.AddTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockTonicServiceLogger, LogLevel.Information, "Tonic added successfully");
    }

    [Fact]
    public async Task AddPairing_LogsStructuredData_WhenPairingAddedSuccessfully() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);

        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var data = BuildJsonElement(new {
            ginId = "1",
            tonicId = "1"
        });

        var result = await sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Information,
            "Pairing added successfully", TESTDISTILLERY, TESTBRAND);
    }

    [Fact]
    public async Task AddPairing_LogsWarning_WhenGinOrTonicNotFoundInDatabase() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);
        var data = BuildJsonElement(new { ginId = "999", tonicId = "888" });

        var result = await sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Warning,
            "AddPairing validation failed: gin or tonic not found",
            "GinId: 999",
            "TonicId: 888");
    }

    [Fact]
    public async Task DeleteGin_LogsInformation_WhenGinDeletedSuccessfully() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var data = BuildJsonElement(new { ginId = "1" });

        var result = await sut.DeleteGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockGinServiceLogger, LogLevel.Information,
            "Gin deleted successfully", TESTDISTILLERY, TESTGINNAME);
    }

    [Fact]
    public async Task DeleteTonic_LogsInformation_WhenTonicDeletedSuccessfully() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedTonics(context, [CreateTonic(1, TESTBRAND, TESTFLAVOUR)]);
        var sut = CreateSut(mockControllerLogger, context, out _, out var mockTonicServiceLogger, out _);

        var data = BuildJsonElement(new { tonicId = "1" });

        var result = await sut.DeleteTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockTonicServiceLogger, LogLevel.Information,
            "Tonic deleted successfully", TESTBRAND, TESTFLAVOUR);
    }

    [Fact]
    public async Task DeletePairing_LogsInformation_WhenPairingDeletedSuccessfully() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);
        SeedPairings(context, [CreatePairing(gin, tonic)]);

        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var data = BuildJsonElement(new { pairingId = "1" });

        var result = await sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Information,
            "Pairing deleted successfully", TESTDISTILLERY, TESTBRAND);
    }

    [Fact]
    public async Task MatchGin_LogsWarning_WhenDatabaseIsEmpty() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out _);

        var result = await sut.MatchGin("test");

        result.ShouldBeOfType<BadRequestResult>();
        VerifyLogContains(mockControllerLogger, LogLevel.Warning, "MatchGin called but database contains no gins");
    }

    [Fact]
    public async Task MatchGin_LogsInformation_WhenSearchingForGin() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var result = await sut.MatchGin("test");

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockGinServiceLogger, LogLevel.Information, "Searching for gin with partial match", "test");
    }

    [Fact]
    public async Task MatchGin_LogsInformation_WhenNoResultsFound() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var result = await sut.MatchGin("non-existent");

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockGinServiceLogger, LogLevel.Information, "No results found for search term", "non-existent");
    }

    [Fact]
    public async Task MatchGin_LogsInformation_WhenResultsFound() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var result = await sut.MatchGin("test");

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockGinServiceLogger, LogLevel.Information, "Found", "gin(s) matching search term", "test");
    }

    [Fact]
    public async Task GetPairingByGinId_LogsWarning_WhenGinNotFound() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var result = await sut.GetPairingByGinId(999);

        result.ShouldBeOfType<BadRequestResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Warning, "GetPairingByGinId called with invalid GinId", "999");
    }

    [Fact]
    public async Task GetPairingByGinId_LogsInformation_WhenNoPairingsFound() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var result = await sut.GetPairingByGinId(1);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Information, "No pairings found for gin", TESTDISTILLERY, TESTGINNAME);
    }

    [Fact]
    public async Task GetPairingByGinId_LogsInformation_WhenPairingSuggested() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);
        SeedPairings(context, [CreatePairing(gin, tonic)]);

        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var result = await sut.GetPairingByGinId(1);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Information, "Pairing suggested for", TESTDISTILLERY, TESTBRAND);
    }

    [Theory]
    [InlineData(null, "TestDistillery", "GinName: (null)", "Distillery: TestDistillery")]
    [InlineData("TestGin", null, "GinName: TestGin", "Distillery: (null)")]
    [InlineData(null, null, "GinName: (null)", "Distillery: (null)")]
    [InlineData("", "", "GinName: ", "Distillery: ")]
    public async Task AddGin_LogsWarningWithNullOrEmptyValues_WhenGinNameOrDistilleryIsMissing(string? ginName, string? distillery, string expectedGinNameLog, string expectedDistilleryLog) {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out _);

        var data = BuildJsonElement(new {
            ginName,
            distillery,
            description = "Test"
        });

        var result = await sut.AddGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockControllerLogger, LogLevel.Warning, "AddGin validation failed: missing required fields", expectedGinNameLog, expectedDistilleryLog);
    }

    [Fact]
    public async Task AddGin_LogsWarning_WhenDuplicateGin() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedGins(context, [CreateGin(1, TESTGINNAME, TESTDISTILLERY)]);
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var data = BuildJsonElement(new {
            ginName = TESTGINNAME,
            distillery = TESTDISTILLERY,
            description = "Test"
        });

        var result = await sut.AddGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockGinServiceLogger, LogLevel.Warning, "AddGin validation failed: duplicate gin", TESTDISTILLERY, TESTGINNAME);
    }

    [Theory]
    [InlineData(null, "TestFlavour", "TonicBrand: (null)", "TonicFlavour: TestFlavour")]
    [InlineData("TestBrand", null, "TonicBrand: TestBrand", "TonicFlavour: (null)")]
    [InlineData(null, null, "TonicBrand: (null)", "TonicFlavour: (null)")]
    [InlineData("", "", "TonicBrand: ", "TonicFlavour: ")]
    public async Task AddTonic_LogsWarningWithNullOrEmptyValues_WhenTonicBrandOrFlavourIsMissing(string? tonicBrand, string? tonicFlavour, string expectedBrandLog, string expectedFlavourLog) {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out _);

        var data = BuildJsonElement(new {
            tonicBrand,
            tonicFlavour
        });

        var result = await sut.AddTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockControllerLogger, LogLevel.Warning, "AddTonic validation failed: missing required fields", expectedBrandLog, expectedFlavourLog);
    }

    [Fact]
    public async Task AddTonic_LogsWarning_WhenDuplicateTonic() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        SeedTonics(context, [CreateTonic(1, TESTBRAND, TESTFLAVOUR)]);
        var sut = CreateSut(mockControllerLogger, context, out _, out var mockTonicServiceLogger, out _);

        var data = BuildJsonElement(new {
            tonicBrand = TESTBRAND,
            tonicFlavour = TESTFLAVOUR
        });

        var result = await sut.AddTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockTonicServiceLogger, LogLevel.Warning, "AddTonic validation failed: duplicate tonic", TESTBRAND, TESTFLAVOUR);
    }

    [Theory]
    [InlineData(null, "1", "GinId: (null)", "TonicId: 1")]
    [InlineData("1", null, "GinId: 1", "TonicId: (null)")]
    [InlineData(null, null, "GinId: (null)", "TonicId: (null)")]
    [InlineData("", "", "GinId: ", "TonicId: ")]
    public async Task AddPairing_LogsWarningWithNullOrEmptyValues_WhenGinIdOrTonicIdIsMissing(string? ginId, string? tonicId, string expectedGinIdLog, string expectedTonicIdLog) {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out _);

        var data = BuildJsonElement(new {
            ginId,
            tonicId
        });

        var result = await sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockControllerLogger, LogLevel.Warning, "AddPairing validation failed: missing required fields", expectedGinIdLog, expectedTonicIdLog);
    }

    [Fact]
    public async Task AddPairing_LogsWarning_WhenDuplicatePairing() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var gin = CreateGin(1, TESTGINNAME, TESTDISTILLERY);
        var tonic = CreateTonic(1, TESTBRAND, TESTFLAVOUR);
        SeedGins(context, [gin]);
        SeedTonics(context, [tonic]);
        SeedPairings(context, [CreatePairing(gin, tonic)]);

        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var data = BuildJsonElement(new {
            ginId = "1",
            tonicId = "1"
        });

        var result = await sut.AddPairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Warning, "AddPairing validation failed: duplicate pairing", TESTDISTILLERY, TESTBRAND);
    }

    [Theory]
    [InlineData("", "DeleteGin validation failed: invalid GinId")]
    [InlineData(null, "DeleteGin validation failed: invalid GinId", "(null)")]
    [InlineData("0", "DeleteGin validation failed: invalid GinId", "0")]
    [InlineData("999", "DeleteGin validation failed: gin not found", "999")]
    public async Task DeleteGin_LogsWarning_WhenInvalidOrNullGinId(string? ginId, params string[] expectedContent) {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out var mockGinServiceLogger, out _, out _);

        var data = BuildJsonElement(new { ginId });

        var result = await sut.DeleteGin(data);

        result.ShouldBeOfType<OkObjectResult>();
        if (ginId == "999") {
            VerifyLogContains(mockGinServiceLogger, LogLevel.Warning, expectedContent);
        } else {
            VerifyLogContains(mockControllerLogger, LogLevel.Warning, expectedContent);
        }
    }

    [Theory]
    [InlineData("", "DeleteTonic validation failed: invalid TonicId")]
    [InlineData(null, "DeleteTonic validation failed: invalid TonicId", "(null)")]
    [InlineData("0", "DeleteTonic validation failed: invalid TonicId", "0")]
    [InlineData("999", "DeleteTonic validation failed: tonic not found", "999")]
    public async Task DeleteTonic_LogsWarning_WhenInvalidOrNullTonicId(string? tonicId, params string[] expectedContent) {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out var mockTonicServiceLogger, out _);

        var data = BuildJsonElement(new { tonicId });

        var result = await sut.DeleteTonic(data);

        result.ShouldBeOfType<OkObjectResult>();
        if (tonicId == "999") {
            VerifyLogContains(mockTonicServiceLogger, LogLevel.Warning, expectedContent);
        } else {
            VerifyLogContains(mockControllerLogger, LogLevel.Warning, expectedContent);
        }
    }

    [Theory]
    [InlineData("", "DeletePairing validation failed: invalid PairingId")]
    [InlineData(null, "DeletePairing validation failed: invalid PairingId", "(null)")]
    [InlineData("0", "DeletePairing validation failed: invalid PairingId", "0")]
    public async Task DeletePairing_LogsWarning_WhenInvalidOrNullPairingId(string? pairingId, params string[] expectedContent) {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out _);

        var data = BuildJsonElement(new { pairingId });

        var result = await sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockControllerLogger, LogLevel.Warning, expectedContent);
    }

    [Fact]
    public async Task DeletePairing_LogsWarning_WhenPairingNotFound() {
        var mockControllerLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();
        var sut = CreateSut(mockControllerLogger, context, out _, out _, out var mockPairingServiceLogger);

        var data = BuildJsonElement(new { pairingId = "999" });

        var result = await sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Warning, "DeletePairing validation failed: pairing not found", "999");
    }

    [Fact]
    public async Task DeletePairing_LogsWarning_WhenGinOrTonicNotFoundForPairing() {
        var mockLogger = CreateMockLogger();
        var context = CreateInMemoryGinPairDbContext();

        var pairing = new Pairing { PairingId = 1, GinId = 999, TonicId = 999 };
        context.Pairings.Add(pairing);
        context.SaveChanges();

        var sut = CreateSut(mockLogger, context, out _, out _, out var mockPairingServiceLogger);

        var data = BuildJsonElement(new { pairingId = "1" });

        var result = await sut.DeletePairing(data);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyLogContains(mockPairingServiceLogger, LogLevel.Warning, "DeletePairing validation failed: gin or tonic not found for pairing");
    }
}
