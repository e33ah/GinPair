namespace GinPair.Tests;

public static class TestHelper {
    public static GinPairDbContext CreateInMemoryGinPairDbContext() {
        var options = new DbContextOptionsBuilder<GinPairDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GinPairDbContext(options);
    }

    public static GinApiController CreateController(GinPairDbContext context) {
        var state = new DatabaseInitializationState { IsDatabaseReady = true };
        return new GinApiController(context, state);
    }

    public static void ResetDatabase(GinPairDbContext context) {
        context.Pairings.RemoveRange(context.Pairings);
        context.Tonics.RemoveRange(context.Tonics);
        context.Gins.RemoveRange(context.Gins);
        context.SaveChanges();
    }

    public static void SeedGins(GinPairDbContext context, IEnumerable<Gin> gins) {
        context.Gins.AddRange(gins);
        context.SaveChanges();
    }

    public static void SeedTonics(GinPairDbContext context, IEnumerable<Tonic> tonics) {
        context.Tonics.AddRange(tonics);
        context.SaveChanges();
    }

    public static void SeedPairings(GinPairDbContext context, IEnumerable<Pairing> pairings) {
        context.Pairings.AddRange(pairings);
        context.SaveChanges();
    }

    public static Gin CreateGin(int id, string name, string? distillery = null) {
        return new Gin { GinId = id, GinName = name, Distillery = distillery ?? name };
    }

    public static Tonic CreateTonic(int id, string brand, string? flavour = null) {
        return new Tonic { TonicId = id, TonicBrand = brand, TonicFlavour = flavour ?? brand };
    }

    public static Pairing CreatePairing(Gin gin, Tonic tonic) {
        return new Pairing { PairedGin = gin, PairedTonic = tonic };
    }

    public static T GetOkValue<T>(IActionResult result) {
        var ok = result.ShouldBeOfType<OkObjectResult>();
        object? value = ok.Value;
        value.ShouldNotBeNull();
        return value.ShouldBeOfType<T>();
    }

    public static List<string> GetSelectListTexts(IActionResult result) {
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var items = ok.Value as IEnumerable<SelectListItem>;
        items.ShouldNotBeNull();
        return items!.Select(i => i.Text).ToList();
    }

    public static ApiResponse GetApiResponse(IActionResult result) {
        return GetOkValue<ApiResponse>(result);
    }

    public static void AssertOkWithSelectListTexts(IActionResult result, params string[] expectedTexts) {
        var texts = GetSelectListTexts(result);
        texts.ShouldBe(expectedTexts.ToList());
    }

    public static void AssertApiResponse(IActionResult result, BsColor expectedColour, params string[] mustContain) {
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(Microsoft.AspNetCore.Http.StatusCodes.Status200OK);
        var api = ok.Value.ShouldBeOfType<ApiResponse>();
        api.BsColor.ShouldBe(expectedColour);
        foreach (string s in mustContain) {
            api.StatusMessage.ShouldContain(s);
        }
    }

    public static JsonElement BuildJsonElement(object payload) {
        string json = JsonSerializer.Serialize(payload);
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
