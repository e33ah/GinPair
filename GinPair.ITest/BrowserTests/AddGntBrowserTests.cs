using GinPair.ITest.Setup;

namespace GinPair.ITest.BrowserTests;

[Collection(BrowserAppCollection.COLLECTION_NAME)]
public sealed class AddGntBrowserTests(BrowserAppFixture fixture) : BrowserTestBase(fixture) {
    [Fact]
    public async Task AddGin_ShowsWarning_WhenRequiredFieldsAreMissing() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddGinSection);
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit gin" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addGinResult");
            result.ShouldContain("Please provide the name of the Distillery and Gin.");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddTonic_ShowsWarning_WhenRequiredFieldsAreMissing() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddTonicSection);
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit tonic" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addTonicResult");
            result.ShouldContain("Please provide the tonic brand and flavour/name.");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddPairing_ShowsWarning_WhenSelectionsAreMissing() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddPairingSection);
            await WaitForDropdownOptionsAsync(sut, "#ginDropDown");
            await WaitForDropdownOptionsAsync(sut, "#tonicDropDown");
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit pairing" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addPairingResult");
            result.ShouldContain("Please select the gin and tonic to pair");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddGin_ShowsFailure_WhenGinAlreadyExists() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddGinSection);
            await sut.GetByLabel("Distillery / Brand: *").FillAsync(scenario.Distillery);
            await sut.GetByLabel("Gin Name / Flavour: *").FillAsync(scenario.GinName);
            await sut.GetByLabel("Short Description:").FillAsync(scenario.Description);
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit gin" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addGinResult");
            result.ShouldContain("Sorry this gin cannot be added as it is already part of our collection!");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddTonic_ShowsFailure_WhenTonicAlreadyExists() {
        var scenario = CreateScenarioData();
        _ = await AddTonicViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddTonicSection);
            await sut.GetByLabel("Tonic Brand: *").FillAsync(scenario.TonicBrand);
            await sut.GetByLabel("Tonic Flavour: *").FillAsync(scenario.TonicFlavour);
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit tonic" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addTonicResult");
            result.ShouldContain("Sorry this tonic cannot be added as it is already part of our collection!");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddPairing_ShowsFailure_WhenPairingAlreadyExists() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        _ = await AddTonicViaApiAsync(scenario);
        _ = await AddPairingViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddPairingSection);
            await WaitForDropdownOptionAsync(sut, "#ginDropDown", scenario.GinLabel);
            await WaitForDropdownOptionAsync(sut, "#tonicDropDown", scenario.TonicLabel);
            await sut.GetByLabel("Gin to pair:").SelectOptionAsync(new SelectOptionValue { Label = scenario.GinLabel });
            await sut.GetByLabel("Tonic to pair:").SelectOptionAsync(new SelectOptionValue { Label = scenario.TonicLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit pairing" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addPairingResult");
            result.ShouldContain($"\"{scenario.GinLabel}\" gin and \"{scenario.TonicLabel}\" tonic are already paired!");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddGin_ShowsSuccess_WhenValidDetailsAreSubmitted() {
        var scenario = CreateScenarioData();
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddGinSection);
            await WaitForDropdownOptionsAsync(sut, "#ginDropDown");
            await sut.GetByLabel("Distillery / Brand: *").FillAsync(scenario.Distillery);
            await sut.GetByLabel("Gin Name / Flavour: *").FillAsync(scenario.GinName);
            await sut.GetByLabel("Short Description:").FillAsync(scenario.Description);
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit gin" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addGinResult");
            result.ShouldContain($"\"{scenario.GinLabel}\" gin was added!");
            await WaitForDropdownOptionAsync(sut, "#ginDropDown", scenario.GinLabel);
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddTonic_ShowsSuccess_WhenValidDetailsAreSubmitted() {
        var scenario = CreateScenarioData();
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddTonicSection);
            await WaitForDropdownOptionsAsync(sut, "#tonicDropDown");
            await sut.GetByLabel("Tonic Brand: *").FillAsync(scenario.TonicBrand);
            await sut.GetByLabel("Tonic Flavour: *").FillAsync(scenario.TonicFlavour);
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit tonic" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addTonicResult");
            result.ShouldContain($"\"{scenario.TonicLabel}\" tonic was added!");
            await WaitForDropdownOptionAsync(sut, "#tonicDropDown", scenario.TonicLabel);
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task AddPairing_ShowsSuccess_WhenValidSelectionsAreSubmitted() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        _ = await AddTonicViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await OpenSectionAsync(sut, AddPairingSection);
            await WaitForDropdownOptionAsync(sut, "#ginDropDown", scenario.GinLabel);
            await WaitForDropdownOptionAsync(sut, "#tonicDropDown", scenario.TonicLabel);
            await sut.GetByLabel("Gin to pair:").SelectOptionAsync(new SelectOptionValue { Label = scenario.GinLabel });
            await sut.GetByLabel("Tonic to pair:").SelectOptionAsync(new SelectOptionValue { Label = scenario.TonicLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Submit pairing" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#addPairingResult");
            result.ShouldContain($"\"{scenario.GinLabel}\" gin and \"{scenario.TonicLabel}\" tonic were paired!");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }
}
