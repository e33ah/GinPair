using GinPair.ITest.Setup;

namespace GinPair.ITest;

public abstract partial class BrowserTestBase(BrowserAppFixture fixture) {
    protected const int DATABASEINITIALISATIONDELAYMS = 60000;
    private const int DEFAULTUITIMEOUTMS = 10000;
    protected static SectionDefinition AddGinSection { get; } = new(BrowserUi.Selectors.DISTILLERYINPUT, BrowserUi.Buttons.OPENADDGINSECTION);
    protected static SectionDefinition AddPairingSection { get; } = new(BrowserUi.Selectors.ADDPAIRINGBUTTON, BrowserUi.Buttons.OPENADDPAIRINGSECTION);
    protected static SectionDefinition AddTonicSection { get; } = new(BrowserUi.Selectors.TONICBRANDINPUT, BrowserUi.Buttons.OPENADDTONICSECTION);
    protected static SectionDefinition DeleteGinSection { get; } = new(BrowserUi.Selectors.DELETEGINBUTTON, BrowserUi.Buttons.OPENDELETEGINSECTION);
    protected static SectionDefinition DeletePairingSection { get; } = new(BrowserUi.Selectors.DELETEPAIRINGBUTTON, BrowserUi.Buttons.OPENDELETEPAIRINGSECTION);
    protected static SectionDefinition DeleteTonicSection { get; } = new(BrowserUi.Selectors.DELETETONICBUTTON, BrowserUi.Buttons.OPENDELETETONICSECTION);
    protected BrowserAppFixture Fixture { get; } = fixture;

    protected async Task<ApiResponse> AddGinViaApiAsync(TestScenarioData scenario) {
        using var client = Fixture.CreateApiClient();
        using var response = await client.PostAsJsonAsync("/api/ginapi/addGin", new {
            ginName = scenario.GinName,
            distillery = scenario.Distillery,
            description = scenario.Description
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        result.ShouldNotBeNull();
        return result;
    }

    protected async Task<ApiResponse> AddTonicViaApiAsync(TestScenarioData scenario) {
        using var client = Fixture.CreateApiClient();
        using var response = await client.PostAsJsonAsync("/api/ginapi/addTonic", new {
            tonicBrand = scenario.TonicBrand,
            tonicFlavour = scenario.TonicFlavour
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        result.ShouldNotBeNull();
        return result;
    }

    protected async Task<ApiResponse> AddPairingViaApiAsync(TestScenarioData scenario) {
        string ginId = await GetSelectOptionValueAsync("/api/ginapi/getGinList", scenario.GinLabel);
        string tonicId = await GetSelectOptionValueAsync("/api/ginapi/getTonicList", scenario.TonicLabel);

        using var client = Fixture.CreateApiClient();
        using var response = await client.PostAsJsonAsync("/api/ginapi/addPairing", new {
            ginId,
            tonicId
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        result.ShouldNotBeNull();
        return result;
    }

    protected static TestScenarioData CreateScenarioData() {
        string suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        TestScenarioData result = new(suffix);
        return result;
    }

    protected static async Task EnsureVisibleAsync(IPage page, string selector, string sectionButtonName) {
        bool isVisible = await page.Locator(selector).IsVisibleAsync();
        if (!isVisible) {
            await page.GetByRole(AriaRole.Button, new() { Name = sectionButtonName }).ClickAsync();
        }

        await page.Locator(selector).WaitForAsync(new LocatorWaitForOptions {
            State = WaitForSelectorState.Visible
        });
    }

    protected static async Task OpenSectionAsync(IPage page, SectionDefinition section) =>
        await EnsureVisibleAsync(page, section.Selector, section.ButtonName);

    protected static async Task WaitForDropdownOptionsAsync(IPage page, string selector) {
        var dropdown = page.Locator(selector);
        await dropdown.WaitForAsync(new LocatorWaitForOptions {
            State = WaitForSelectorState.Attached
        });

        await dropdown.Locator("option").First.WaitForAsync(new LocatorWaitForOptions {
            State = WaitForSelectorState.Attached,
            Timeout = DEFAULTUITIMEOUTMS
        });
    }

    protected static async Task WaitForDropdownOptionAsync(IPage page, string selector, string optionLabel) {
        await WaitForDropdownOptionsAsync(page, selector);

        await page
            .Locator(selector)
            .Locator("option", new() { HasTextString = optionLabel })
            .WaitForAsync(new LocatorWaitForOptions {
                State = WaitForSelectorState.Attached,
                Timeout = DEFAULTUITIMEOUTMS
            });
    }

    protected static async Task WaitForDropdownOptionToBeAbsentAsync(IPage page, string selector, string optionLabel) {
        await page
            .Locator(selector)
            .Locator("option", new() { HasTextString = optionLabel })
            .WaitForAsync(new LocatorWaitForOptions {
                State = WaitForSelectorState.Detached,
                Timeout = DEFAULTUITIMEOUTMS
            });
    }

    protected static async Task<string> WaitForResultTextAsync(IPage page, string selector) {
        var resultLocator = page.Locator(selector);
        await resultLocator.WaitForAsync(new LocatorWaitForOptions {
            State = WaitForSelectorState.Visible
        });

        await Assertions.Expect(resultLocator).ToContainTextAsync(
            HasContentRegex(),
            new LocatorAssertionsToContainTextOptions {
                Timeout = DEFAULTUITIMEOUTMS
            });

        string result = (await resultLocator.InnerTextAsync()).Trim();
        result.ShouldNotBeNullOrWhiteSpace();
        return result;
    }

    private async Task<string> GetSelectOptionValueAsync(string path, string label) {
        using var client = Fixture.CreateApiClient();
        var options = await client.GetFromJsonAsync<List<SelectOptionDto>>(path);
        options.ShouldNotBeNull();

        var option = options.SingleOrDefault(o => o.Text == label);
        option.ShouldNotBeNull();
        option.Value.ShouldNotBeNullOrWhiteSpace();
        return option.Value!;
    }

    private sealed record SelectOptionDto(string? Value, string? Text);

    protected sealed record SectionDefinition(string Selector, string ButtonName);

    protected static class BrowserUi {
        protected internal static class Buttons {
            public const string ADDGIN = "Submit gin";
            public const string ADDTONIC = "Submit tonic";
            public const string CONFIRMDELETEGIN = "Confirm remove gin";
            public const string CONFIRMDELETEPAIRING = "Confirm remove pairing";
            public const string CONFIRMDELETETONIC = "Confirm remove tonic";
            public const string OPENADDGINSECTION = "Add Gin";
            public const string OPENADDPAIRINGSECTION = "Add Pairing";
            public const string OPENADDTONICSECTION = "Add Tonic";
            public const string OPENDELETEGINSECTION = "Remove Gin";
            public const string OPENDELETEPAIRINGSECTION = "Remove Pairing";
            public const string OPENDELETETONICSECTION = "Remove Tonic";
            public const string REMOVESELECTEDGIN = "Remove selected gin";
            public const string REMOVESELECTEDPAIRING = "Remove selected pairing";
            public const string REMOVESELECTEDTONIC = "Remove selected tonic";
            public const string SUBMITPAIRING = "Submit pairing";
            public const string SEARCH = "Search";
        }

        protected internal static class Labels {
            public const string DESCRIPTION = "Short Description:";
            public const string DISTILLERY = "Distillery / Brand: *";
            public const string GINNAME = "Gin Name / Flavour: *";
            public const string GINTOPAIR = "Gin to pair:";
            public const string GINTOREMOVE = "Gin to remove:";
            public const string PAIRINGTOREMOVE = "Pairing to remove:";
            public const string TONICBRAND = "Tonic Brand: *";
            public const string TONICFLAVOUR = "Tonic Flavour: *";
            public const string TONICTOPAIR = "Tonic to pair:";
            public const string TONICTOREMOVE = "Tonic to remove:";
        }

        protected internal static class Selectors {
            public const string ADDPAIRINGBUTTON = "#addPairingButton";
            public const string ADDGINRESULT = "#addGinResult";
            public const string ADDPAIRINGRESULT = "#addPairingResult";
            public const string ADDTONICRESULT = "#addTonicResult";
            public const string DELETEGINBUTTON = "#ginDeleteButton";
            public const string DISTILLERYINPUT = "#distilleryInput";
            public const string GINDROPDOWN = "#ginDropDown";
            public const string DELETEGINRESULT = "#deleteGinResult";
            public const string DELETEPAIRINGBUTTON = "#pairingDeleteButton";
            public const string DELETEPAIRINGRESULT = "#deletePairingResult";
            public const string DELETETONICBUTTON = "#tonicDeleteButton";
            public const string DELETETONICRESULT = "#deleteTonicResult";
            public const string PAIRINGDROPDOWN = "#pairingDropDown";
            public const string TONICBRANDINPUT = "#tonicBrandInput";
            public const string TONICDROPDOWN = "#tonicDropDown";
        }
    }

    protected sealed class TestScenarioData(string suffix) {
        public string Description { get; } = $"Created by Playwright {suffix}";
        public string Distillery { get; } = $"Playwright Distillery {suffix}";
        public string GinName { get; } = $"Playwright Gin {suffix}";
        public string TonicBrand { get; } = $"Playwright Tonic {suffix}";
        public string TonicFlavour { get; } = $"Citrus {suffix}";
        public string GinLabel => $"{Distillery} {GinName}";
        public string PairingLabel => $"{GinLabel} gin and {TonicLabel} tonic";
        public string TonicLabel => $"{TonicBrand} {TonicFlavour}";
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"\S")]
    private static partial System.Text.RegularExpressions.Regex HasContentRegex();
}
