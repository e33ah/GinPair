namespace GinPair.Tests;

public class GetGinTests {
    private readonly GinPairDbContext mockContext;
    private readonly GinApiController mockController;

    public GetGinTests() {
        mockContext = CreateInMemoryGinPairDbContext();
        mockController = CreateController(mockContext);
    }
    public class MatchGinTests : GetGinTests {

        [Fact]
        public async Task MatchPartial_ReturnsOk() {
            ResetDatabase(mockContext);
            SeedGins(mockContext, [
                CreateGin(1, "TestName1", "TestDis1"),
            CreateGin(2, "TestName2", "TestDis2")
            ]);

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
            ResetDatabase(mockContext);
            SeedGins(mockContext, [
                CreateGin(1, "TestName1", "TestDis1"),
            CreateGin(2, "TestName2", "TestDis2")
            ]);

            var sut = mockController;
            var result = await sut.MatchGin(partial);

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var ginList = okResult.Value.ShouldBeOfType<List<Gin>>();
            ginList.Count.ShouldBe(expectedCount);
        }

        [Fact]
        public async Task MatchGin_ReturnsBadRequest_WhenNoGinsExist() {
            ResetDatabase(mockContext);

            var result = await mockController.MatchGin("anything");

            result.ShouldBeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task MatchGin_ReturnsResultsOrderedByDistilleryThenGinName() {
            ResetDatabase(mockContext);
            SeedGins(mockContext, [
                CreateGin(1, "Zulu", "Charlie"),
                CreateGin(2, "Alpha", "Charlie"),
                CreateGin(3, "Mike", "Bravo"),
                CreateGin(4, "Tango", "Alpha"),
                CreateGin(5, "Delta", "Alpha")
            ]);

            var sut = mockController;
            var result = await sut.MatchGin("a");

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var ginList = okResult.Value.ShouldBeOfType<List<Gin>>();
            ginList.Count.ShouldBe(5);
            ginList[0].Distillery.ShouldBe("Alpha");
            ginList[0].GinName.ShouldBe("Delta");
            ginList[1].Distillery.ShouldBe("Alpha");
            ginList[1].GinName.ShouldBe("Tango");
            ginList[2].Distillery.ShouldBe("Bravo");
            ginList[2].GinName.ShouldBe("Mike");
            ginList[3].Distillery.ShouldBe("Charlie");
            ginList[3].GinName.ShouldBe("Alpha");
            ginList[4].Distillery.ShouldBe("Charlie");
            ginList[4].GinName.ShouldBe("Zulu");
        }

        [Fact]
        public async Task MatchGin_LimitsResultsToTenItems_InCorrectOrder() {
            ResetDatabase(mockContext);
            var gins = new List<Gin>();
            for (int i = 1; i <= 15; i++) {
                gins.Add(CreateGin(i, $"Gin{i:D2}", $"Distillery{i:D2}"));
            }
            SeedGins(mockContext, gins);

            var sut = mockController;
            var result = await sut.MatchGin("Gin");

            var okResult = result.ShouldBeOfType<OkObjectResult>();
            var ginList = okResult.Value.ShouldBeOfType<List<Gin>>();
            ginList.Count.ShouldBe(10);
            ginList[0].Distillery.ShouldBe("Distillery01");
            ginList[9].Distillery.ShouldBe("Distillery10");
        }
    }

    [Fact]
    public void GetGinList_ReturnsOk_WithEmptyList_WhenNoGinsExist() {
        ResetDatabase(mockContext);

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
        ResetDatabase(mockContext);
        var gins = new List<Gin> {
            CreateGin(21, n1),
            CreateGin(22, n2),
            CreateGin(23, n3)
        };
        SeedGins(mockContext, gins);

        var sut = mockController;
        var result = sut.GetGinList();

        var texts = GetSelectListTexts(result);
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
        ResetDatabase(mockContext);
        var tonics = new List<Tonic> {
            CreateTonic(31, t1, t1),
            CreateTonic(32, t2, t2),
            CreateTonic(33, t3, t3)
        };
        SeedTonics(mockContext, tonics);

        var sut = mockController;
        var result = sut.GetTonicList();

        var texts = GetSelectListTexts(result);
        var expectedOrder = new List<string> { t1, t2, t3 }
            .OrderBy(x => x)
            .Select(x => $"{x} {x}")
            .ToList();
        texts.ShouldBe(expectedOrder);
    }

    [Fact]
    public async Task GetPairingById_ReturnsOk() {
        ResetDatabase(mockContext);
        string ginName = "gin2Pair";
        string distillery = "dis2Pair";
        var gin = CreateGin(3, ginName, distillery);
        var tonic = CreateTonic(3, "brand2Pair", "flavour2Pair");
        var pairing = CreatePairing(gin, tonic);
        SeedPairings(mockContext, [pairing]);

        var result = await mockController.GetPairingByGinId(3);

        AssertApiResponse(result, BsColor.Primary, $"Try pairing <b>{distillery} {ginName}</b> gin with");
    }

    [Fact]
    public async Task GetPairingById_WhenGinNotExist_Returns_Bad() {

        var result = await mockController.GetPairingByGinId(453);

        result.ShouldBeOfType<BadRequestResult>();
    }

    [Fact]
    public void GetPairingList_ReturnsItemsOrderedByTextAscending() {
        const string EXPECTEDALPHA = "Alpha Alpha gin and Alpha Alpha tonic";
        const string EXPECTEDMIKE = "Mike Mike gin and Mike Mike tonic";
        const string EXPECTEDTANGO = "Tango Tango gin and Tango Tango tonic";
        ResetDatabase(mockContext);

        var ginTango = CreateGin(101, "Tango");
        var ginAlpha = CreateGin(102, "Alpha");
        var ginMike = CreateGin(103, "Mike");
        var tonicTango = CreateTonic(201, "Tango");
        var tonicAlpha = CreateTonic(202, "Alpha");
        var tonicMike = CreateTonic(203, "Mike");
        var pairings = new List<Pairing> {
            CreatePairing(ginTango, tonicTango),
            CreatePairing(ginAlpha, tonicAlpha),
            CreatePairing(ginMike, tonicMike)
        };
        SeedGins(mockContext, [ginTango, ginAlpha, ginMike]);
        SeedTonics(mockContext, [tonicTango, tonicAlpha, tonicMike]);
        SeedPairings(mockContext, pairings);

        var sut = mockController;
        var result = sut.GetPairingList();

        AssertOkWithSelectListTexts(result,
            EXPECTEDALPHA,
            EXPECTEDMIKE,
            EXPECTEDTANGO);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void GetPairingList_ExcludesOrphanedPairings_WhenGinOrTonicDeleted(bool removeGin, bool removeTonic) {
        const string EXPECTEDALPHA = "Alpha Alpha gin and Alpha Alpha tonic";

        ResetDatabase(mockContext);
        var ginTango = CreateGin(101, "Tango");
        var ginAlpha = CreateGin(102, "Alpha");
        var tonicTango = CreateTonic(201, "Tango");
        var tonicAlpha = CreateTonic(202, "Alpha");
        var pairings = new List<Pairing> {
            CreatePairing(ginTango, tonicTango),
            CreatePairing(ginAlpha, tonicAlpha),
        };
        SeedGins(mockContext, [ginTango, ginAlpha]);
        SeedTonics(mockContext, [tonicTango, tonicAlpha]);
        SeedPairings(mockContext, pairings);

        if (removeGin) {
            mockContext.Gins.Remove(ginTango);
            mockContext.SaveChanges();
        }
        if (removeTonic) {
            mockContext.Tonics.Remove(tonicTango);
        }
        mockContext.SaveChanges();

        var sut = mockController;
        var result = sut.GetPairingList();

        var texts = GetSelectListTexts(result);
        texts.ShouldBe([EXPECTEDALPHA]);
    }

    [Fact]
    public async Task GetPairingByGinId_ReturnsWarning_WhenNoPairingsFound() {
        ResetDatabase(mockContext);
        string ginName = "Lonely";
        string distillery = "Solo";
        SeedGins(mockContext, [CreateGin(10, ginName, distillery)]);

        var sut = mockController;
        var result = await sut.GetPairingByGinId(10);

        AssertApiResponse(result, BsColor.Warning, $"Sorry, there is no pairing available for \"{distillery} {ginName}\"");
    }

    [Fact]
    public async Task GetPairingByGinId_ReturnsPrimary_WithMessage_WhenPairingExists() {
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

        var sut = mockController;
        var result = await sut.GetPairingByGinId(ginId);

        AssertApiResponse(result, BsColor.Primary,
            $"Try pairing <b>{distillery} {ginName}</b> gin with",
            $"a <b>{brand} {flavour}</b> tonic");
    }
}
