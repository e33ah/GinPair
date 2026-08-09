using GinPair.ITest.Setup;

namespace GinPair.ITest.BrowserTests;

[Collection(BrowserAppCollection.COLLECTION_NAME)]
public sealed class SearchBrowserTests(BrowserAppFixture fixture) : BrowserTestBase(fixture) {
    private const string SEARCHRESULTSELECTOR = "#searchResult";

    [Fact]
    public async Task Index_ShowsSuggestedPairing_WhenSeededGinIsSelected() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/");
            await sut.GetByLabel("Search for a gin").FillAsync("Rhubarb");

            var firstSuggestion = sut.Locator(".ui-menu-item").First;
            await firstSuggestion.WaitForAsync();
            await firstSuggestion.ClickAsync();

            string selectedGinId = await sut.Locator("#ginSelectedId").InputValueAsync();
            selectedGinId.ShouldNotBeNullOrWhiteSpace();

            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.SEARCH }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, SEARCHRESULTSELECTOR);
            result.ShouldContain("Whitley Neill Rhubarb & Ginger");
            result.ShouldContain("FeverTree ElderFlower");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task Index_ShowsWarning_WhenSearchIsEmpty() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/");
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.SEARCH }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, SEARCHRESULTSELECTOR);
            result.ShouldContain("Enter the name of a gin and hit Search");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task Index_ShowsWarning_WhenGinIsUnknown() {
        string suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        string unknownGin = $"Unknown Gin {suffix}";
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/");
            await sut.GetByLabel("Search for a gin").FillAsync(unknownGin);
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.SEARCH }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, SEARCHRESULTSELECTOR);
            result.ShouldContain("No matching gin found.");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task Index_ShowsWarning_WhenGinHasNoPairing() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);

        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/");
            await sut.GetByLabel("Search for a gin").FillAsync(scenario.Distillery);

            var firstSuggestion = sut.Locator(".ui-menu-item").First;
            await firstSuggestion.WaitForAsync();
            await firstSuggestion.ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.SEARCH }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, SEARCHRESULTSELECTOR);
            result.ShouldContain("Sorry, there is no pairing available");
            result.ShouldContain(scenario.GinLabel);
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }
}
