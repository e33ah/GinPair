namespace GinPair.Tests;
public class GetGinTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public GetGinTests() {
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
    private void ResetDatabase() {
        mockContext.Pairings.RemoveRange(mockContext.Pairings);
        mockContext.Tonics.RemoveRange(mockContext.Tonics);
        mockContext.Gins.RemoveRange(mockContext.Gins);
        mockContext.SaveChanges();
    }

    private void SeedDefaultGins() {
        ResetDatabase();
        var gins = new List<Gin> {
            new() { GinId = 1, GinName = "TestName1", Distillery = "TestDis1" },
            new() { GinId = 2, GinName = "TestName2", Distillery = "TestDis2" }
        };
        mockContext.Gins.AddRange(gins);
        mockContext.SaveChanges();
    }

    [Fact]
    public async Task MatchPartial_ReturnsOk() {
        SeedDefaultGins();

        var result = await mockController.MatchGin("Test");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var ginList = okResult.Value.ShouldBeOfType<List<Gin>>();
        ginList.Count.ShouldBe(2);
        ginList.ShouldContain(g => g.GinName == "TestName1");
        ginList.ShouldContain(g => g.GinName == "TestName2");
    }

    [Theory]
    [InlineData("test", 2)]
    [InlineData("TEST", 2)]
    [InlineData("tEsT", 2)]
    [InlineData("dis1", 1)]
    [InlineData("DIS2", 1)]
    public async Task MatchGin_ReturnsExpectedCount_IgnoringCase(string partial, int expectedCount) {
        SeedDefaultGins();

        var sut = mockController;
        var result = await sut.MatchGin(partial);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var ginList = okResult.Value.ShouldBeOfType<List<Gin>>();
        ginList.Count.ShouldBe(expectedCount);
    }

    [Fact]
    public async Task GetPairingById_ReturnsOk() {
        ResetDatabase();
        mockContext.Pairings.AddRange(new List<Pairing> {
            new() {
                PairedGin = new Gin {GinId = 3, GinName = "TestName3", Distillery = "TestDis3"},
                PairedTonic = new Tonic {TonicId = 3, TonicBrand = "TestBrand3", TonicFlavour = "TestFl3"}
            }
        });
        mockContext.SaveChanges();

        var result = await mockController.GetPairingByGinId(3);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.StatusMessage.ShouldContain("Try pairing <b>TestDis3 TestName3</b> gin with");
        apiResponse.BsColor.ShouldBe(BsColor.Primary);
    }

    [Fact]
    public async Task GetPairingById_NoGin_Returns_Bad() {
        SeedDefaultGins();

        var result = await mockController.GetPairingByGinId(4);

        result.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public void GetGinList_ReturnsOk_WithEmptyList_WhenNoGinsExist() {
        ResetDatabase();

        var result = mockController.GetGinList();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var selectList = okResult.Value as IEnumerable<SelectListItem>;
        selectList.ShouldNotBeNull();
        selectList.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Tango", "Alpha", "Mike")]
    [InlineData("Zulu", "Alpha", "Mike")]
    public void GetGinList_ReturnsItemsOrderedByTextAscending(string n1, string n2, string n3) {
        ResetDatabase();
        var gins = new List<Gin> {
            new() { GinId = 21, GinName = n1, Distillery = n1 },
            new() { GinId = 22, GinName = n2, Distillery = n2 },
            new() { GinId = 23, GinName = n3, Distillery = n3 }
        };
        mockContext.Gins.AddRange(gins);
        mockContext.SaveChanges();

        var sut = mockController;
        var result = sut.GetGinList();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var selectList = okResult.Value as IEnumerable<SelectListItem>;
        selectList.ShouldNotBeNull();
        var items = selectList!.ToList();
        var texts = items.Select(i => i.Text).ToList();

        var expectedOrder = new List<string> { n1, n2, n3 }
            .OrderBy(x => x)
            .Select(x => $"{x} {x}")
            .ToList();

        texts.ShouldBe(expectedOrder);
    }

    [Theory]
    [InlineData("Tango", "Alpha", "Mike")]
    [InlineData("Zulu", "Alpha", "Mike")]
    public void GetTonicList_ReturnsItemsOrderedByTextAscending(string t1, string t2, string t3) {
        ResetDatabase();
        var tonics = new List<Tonic> {
            new() { TonicId = 31, TonicBrand = t1, TonicFlavour = t1 },
            new() { TonicId = 32, TonicBrand = t2, TonicFlavour = t2 },
            new() { TonicId = 33, TonicBrand = t3, TonicFlavour = t3 }
        };
        mockContext.Tonics.AddRange(tonics);
        mockContext.SaveChanges();

        var sut = mockController;
        var result = sut.GetTonicList();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var selectList = okResult.Value as IEnumerable<SelectListItem>;
        selectList.ShouldNotBeNull();
        var items = selectList!.ToList();
        var texts = items.Select(i => i.Text).ToList();

        var expectedOrder = new List<string> { t1, t2, t3 }
            .OrderBy(x => x)
            .Select(x => $"{x} {x}")
            .ToList();

        texts.ShouldBe(expectedOrder);
    }

    [Fact]
    public void GetPairingList_ReturnsItemsOrderedByTextAscending() {
        ResetDatabase();

        var ginTango = new Gin { GinId = 101, GinName = "Tango", Distillery = "Tango" };
        var ginAlpha = new Gin { GinId = 102, GinName = "Alpha", Distillery = "Alpha" };
        var ginMike = new Gin { GinId = 103, GinName = "Mike", Distillery = "Mike" };
        var tonicTango = new Tonic { TonicId = 201, TonicBrand = "Tango", TonicFlavour = "Tango" };
        var tonicAlpha = new Tonic { TonicId = 202, TonicBrand = "Alpha", TonicFlavour = "Alpha" };
        var tonicMike = new Tonic { TonicId = 203, TonicBrand = "Mike", TonicFlavour = "Mike" };
        var pairingTango = new Pairing { PairedGin = ginTango, PairedTonic = tonicTango };
        var pairingAlpha = new Pairing { PairedGin = ginAlpha, PairedTonic = tonicAlpha };
        var pairingMike = new Pairing { PairedGin = ginMike, PairedTonic = tonicMike };

        mockContext.Gins.AddRange(ginTango, ginAlpha, ginMike);
        mockContext.Tonics.AddRange(tonicTango, tonicAlpha, tonicMike);
        mockContext.Pairings.AddRange(pairingTango, pairingAlpha, pairingMike);
        mockContext.SaveChanges();

        var sut = mockController;
        var result = sut.GetPairingList();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var selectList = okResult.Value as IEnumerable<SelectListItem>;
        selectList.ShouldNotBeNull();
        var items = selectList!.ToList();
        var texts = items.Select(i => i.Text).ToList();
        texts.ShouldBe([
            "Alpha Alpha gin and Alpha Alpha tonic",
            "Mike Mike gin and Mike Mike tonic",
            "Tango Tango gin and Tango Tango tonic",
        ]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetPairingList_ExcludesNullPairings(bool removeGin) {
        const string EXPECTEDALPHA = "Alpha Alpha gin and Alpha Alpha tonic";
        const string EXPECTEDMIKE = "Mike Mike gin and Mike Mike tonic";

        ResetDatabase();
        var ginTango = new Gin { GinId = 101, GinName = "Tango", Distillery = "Tango" };
        var ginAlpha = new Gin { GinId = 102, GinName = "Alpha", Distillery = "Alpha" };
        var ginMike = new Gin { GinId = 103, GinName = "Mike", Distillery = "Mike" };
        var tonicTango = new Tonic { TonicId = 201, TonicBrand = "Tango", TonicFlavour = "Tango" };
        var tonicAlpha = new Tonic { TonicId = 202, TonicBrand = "Alpha", TonicFlavour = "Alpha" };
        var tonicMike = new Tonic { TonicId = 203, TonicBrand = "Mike", TonicFlavour = "Mike" };
        var pairingTango = new Pairing { PairedGin = ginTango, PairedTonic = tonicTango };
        var pairingAlpha = new Pairing { PairedGin = ginAlpha, PairedTonic = tonicAlpha };
        var pairingMike = new Pairing { PairedGin = ginMike, PairedTonic = tonicMike };

        mockContext.Gins.AddRange(ginTango, ginAlpha, ginMike);
        mockContext.Tonics.AddRange(tonicTango, tonicAlpha, tonicMike);
        mockContext.Pairings.AddRange(pairingTango, pairingAlpha, pairingMike);
        mockContext.SaveChanges();

        if (removeGin) {
            mockContext.Gins.Remove(ginTango);
        } else {
            mockContext.Tonics.Remove(tonicTango);
        }
        mockContext.SaveChanges();

        var sut = mockController;
        var result = sut.GetPairingList();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var selectList = okResult.Value as IEnumerable<SelectListItem>;
        selectList.ShouldNotBeNull();
        var items = selectList!.ToList();
        items.Count.ShouldBe(2);

        var texts = items.Select(i => i.Text).ToList();
        texts.ShouldBe([EXPECTEDALPHA, EXPECTEDMIKE]);
    }

    [Fact]
    public async Task MatchGin_ReturnsBadRequest_WhenNoGinsExist() {
        mockContext.Gins.RemoveRange(mockContext.Gins);
        mockContext.SaveChanges();

        var result = await mockController.MatchGin("anything");

        result.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GetPairingByGinId_ReturnsWarning_WhenNoPairingsFound() {
        ResetDatabase();
        string ginName = "Lonely";
        string distillery = "Solo";
        mockContext.Gins.Add(new Gin { GinId = 10, GinName = ginName, Distillery = distillery });
        mockContext.SaveChanges();

        var sut = mockController;
        var result = await sut.GetPairingByGinId(10);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Warning);
        apiResponse.StatusMessage.ShouldContain($"Sorry, there is no pairing available for \"{distillery} {ginName}\"");
    }

    [Fact]
    public async Task GetPairingByGinId_ReturnsPrimary_WithMessage_WhenPairingExists() {
        ResetDatabase();

        int ginId = 5;
        string ginName = "Alpha";
        string distillery = "Beta";
        int tonicId = 7;
        string brand = "BrandX";
        string flavour = "Citrus";

        var gin = new Gin { GinId = ginId, GinName = ginName, Distillery = distillery };
        var tonic = new Tonic { TonicId = tonicId, TonicBrand = brand, TonicFlavour = flavour };
        var pairing = new Pairing { PairedGin = gin, PairedTonic = tonic };

        mockContext.Gins.Add(gin);
        mockContext.Tonics.Add(tonic);
        mockContext.Pairings.Add(pairing);
        mockContext.SaveChanges();

        var sut = mockController;
        var result = await sut.GetPairingByGinId(ginId);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var apiResponse = okResult.Value.ShouldBeOfType<ApiResponse>();
        apiResponse.BsColor.ShouldBe(BsColor.Primary);
        apiResponse.StatusMessage.ShouldContain($"Try pairing <b>{distillery} {ginName}</b> gin with");
        apiResponse.StatusMessage.ShouldContain($"a <b>{brand} {flavour}</b> tonic");
    }
}
