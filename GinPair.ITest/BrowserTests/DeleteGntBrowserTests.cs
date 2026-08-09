using GinPair.ITest.Setup;

namespace GinPair.ITest.BrowserTests;

[Collection(BrowserAppCollection.COLLECTION_NAME)]
public sealed class DeleteGntBrowserTests(BrowserAppFixture fixture) : BrowserTestBase(fixture) {
    [Fact]
    public async Task DeleteGin_ShowsWarning_WhenNoGinIsSelected() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeleteGinSection);
            await WaitForDropdownOptionsAsync(sut, "#ginDropDown");
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected gin" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Confirm remove gin" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#deleteGinResult");
            result.ShouldContain("Please select a gin to delete");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeleteTonic_ShowsWarning_WhenNoTonicIsSelected() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeleteTonicSection);
            await WaitForDropdownOptionsAsync(sut, "#tonicDropDown");
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected tonic" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Confirm remove tonic" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#deleteTonicResult");
            result.ShouldContain("Please select a tonic to delete");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeletePairing_ShowsWarning_WhenNoPairingIsSelected() {
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeletePairingSection);
            await WaitForDropdownOptionsAsync(sut, "#pairingDropDown");
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected pairing" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Confirm remove pairing" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#deletePairingResult");
            result.ShouldContain("Please select a pairing to delete");
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeleteGin_DoesNotRemoveGin_WhenModalIsCancelled() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeleteGinSection);
            await WaitForDropdownOptionAsync(sut, "#ginDropDown", scenario.GinLabel);
            await sut.GetByLabel("Gin to remove:").SelectOptionAsync(new SelectOptionValue { Label = scenario.GinLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected gin" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Cancel remove gin" }).ClickAsync();
            await WaitForDropdownOptionAsync(sut, "#ginDropDown", scenario.GinLabel);

            string? result = await sut.Locator("#deleteGinResult").TextContentAsync();
            result.ShouldNotBeNull();
            (result ?? string.Empty).Trim().ShouldBeEmpty();
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }
    [Fact]
    public async Task DeleteTonic_DoesNotRemoveTonic_WhenModalIsCancelled() {
        var scenario = CreateScenarioData();
        _ = await AddTonicViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeleteTonicSection);
            await WaitForDropdownOptionAsync(sut, "#tonicDropDown", scenario.TonicLabel);
            await sut.GetByLabel("Tonic to remove:").SelectOptionAsync(new SelectOptionValue { Label = scenario.TonicLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected tonic" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Cancel remove tonic" }).ClickAsync();
            await WaitForDropdownOptionAsync(sut, "#tonicDropDown", scenario.TonicLabel);

            string? result = await sut.Locator("#deleteTonicResult").TextContentAsync();
            (result ?? string.Empty).Trim().ShouldBeEmpty();
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeletePairing_DoesNotRemovePairing_WhenModalIsCancelled() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        _ = await AddTonicViaApiAsync(scenario);
        _ = await AddPairingViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();
        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeletePairingSection);
            await WaitForDropdownOptionAsync(sut, "#pairingDropDown", scenario.PairingLabel);
            await sut.GetByLabel("Pairing to remove:").SelectOptionAsync(new SelectOptionValue { Label = scenario.PairingLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected pairing" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Cancel remove pairing" }).ClickAsync();
            await WaitForDropdownOptionAsync(sut, "#pairingDropDown", scenario.PairingLabel);
            string? result = await sut.Locator("#deletePairingResult").TextContentAsync();
            (result ?? string.Empty).Trim().ShouldBeEmpty();
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeletePairing_ShowsSuccess_WhenConfirmed() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        _ = await AddTonicViaApiAsync(scenario);
        _ = await AddPairingViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeletePairingSection);
            await WaitForDropdownOptionAsync(sut, "#pairingDropDown", scenario.PairingLabel);
            await sut.GetByLabel("Pairing to remove:").SelectOptionAsync(new SelectOptionValue { Label = scenario.PairingLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected pairing" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Confirm remove pairing" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#deletePairingResult");
            result.ShouldContain($"\"{scenario.PairingLabel}\" pairing was removed!");
            await WaitForDropdownOptionToBeAbsentAsync(sut, "#pairingDropDown", scenario.PairingLabel);
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeleteGin_ShowsSuccess_WhenConfirmed() {
        var scenario = CreateScenarioData();
        _ = await AddGinViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeleteGinSection);
            await WaitForDropdownOptionAsync(sut, "#ginDropDown", scenario.GinLabel);
            await sut.GetByLabel("Gin to remove:").SelectOptionAsync(new SelectOptionValue { Label = scenario.GinLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected gin" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Confirm remove gin" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#deleteGinResult");
            result.ShouldContain($"\"{scenario.GinLabel}\" gin was removed!");
            await WaitForDropdownOptionToBeAbsentAsync(sut, "#ginDropDown", scenario.GinLabel);
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    [Fact]
    public async Task DeleteTonic_ShowsSuccess_WhenConfirmed() {
        var scenario = CreateScenarioData();
        _ = await AddTonicViaApiAsync(scenario);
        var sut = await Fixture.CreatePageAsync();

        try {
            await sut.GotoAsync("/Home/DeleteGnt");
            await OpenSectionAsync(sut, DeleteTonicSection);
            await WaitForDropdownOptionAsync(sut, "#tonicDropDown", scenario.TonicLabel);
            await sut.GetByLabel("Tonic to remove:").SelectOptionAsync(new SelectOptionValue { Label = scenario.TonicLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = "Remove selected tonic" }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = "Confirm remove tonic" }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, "#deleteTonicResult");
            result.ShouldContain($"\"{scenario.TonicLabel}\" tonic was removed!");
            await WaitForDropdownOptionToBeAbsentAsync(sut, "#tonicDropDown", scenario.TonicLabel);
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }
}
