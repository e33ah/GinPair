using GinPair.ITest.Setup;

namespace GinPair.ITest.BrowserTests;

[Collection(BrowserAppCollection.COLLECTION_NAME)]
public sealed class EndToEndBrowserTests(BrowserAppFixture fixture) : BrowserTestBase(fixture) {
    [Fact]
    public async Task AddDeleteJourney_UpdatesCollection_WhenManagingRecordsThroughUi() {
        var scenario = CreateScenarioData();
        var sut = await Fixture.CreatePageAsync();
        JourneyTestHelper helper = new(sut, scenario);

        try {
            await sut.GotoAsync("/Home/AddGnt");
            await helper.AddGinAsync();
            await helper.AddTonicAsync();
            await helper.AddPairingAsync();

            await sut.GotoAsync("/Home/DeleteGnt");
            await helper.DeletePairingAsync();
            await helper.DeleteGinAsync();
            await helper.DeleteTonicAsync();
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }

    private sealed class JourneyTestHelper(IPage sut, TestScenarioData scenario) {
        public async Task AddGinAsync() {
            await OpenSectionAsync(sut, AddGinSection);
            await WaitForDropdownOptionsAsync(sut, BrowserUi.Selectors.GINDROPDOWN);

            await sut.GetByLabel(BrowserUi.Labels.DISTILLERY).FillAsync(scenario.Distillery);
            await sut.GetByLabel(BrowserUi.Labels.GINNAME).FillAsync(scenario.GinName);
            await sut.GetByLabel(BrowserUi.Labels.DESCRIPTION).FillAsync(scenario.Description);
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.ADDGIN }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, BrowserUi.Selectors.ADDGINRESULT);
            result.ShouldContain($"\"{scenario.GinLabel}\" gin was added!");
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.GINDROPDOWN, scenario.GinLabel);
        }

        public async Task AddTonicAsync() {
            await OpenSectionAsync(sut, AddTonicSection);
            await WaitForDropdownOptionsAsync(sut, BrowserUi.Selectors.TONICDROPDOWN);

            await sut.GetByLabel(BrowserUi.Labels.TONICBRAND).FillAsync(scenario.TonicBrand);
            await sut.GetByLabel(BrowserUi.Labels.TONICFLAVOUR).FillAsync(scenario.TonicFlavour);
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.ADDTONIC }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, BrowserUi.Selectors.ADDTONICRESULT);
            result.ShouldContain($"\"{scenario.TonicLabel}\" tonic was added!");
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.TONICDROPDOWN, scenario.TonicLabel);
        }

        public async Task AddPairingAsync() {
            await OpenSectionAsync(sut, AddPairingSection);
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.GINDROPDOWN, scenario.GinLabel);
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.TONICDROPDOWN, scenario.TonicLabel);
            await sut.GetByLabel(BrowserUi.Labels.GINTOPAIR).SelectOptionAsync(new SelectOptionValue { Label = scenario.GinLabel });
            await sut.GetByLabel(BrowserUi.Labels.TONICTOPAIR).SelectOptionAsync(new SelectOptionValue { Label = scenario.TonicLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.SUBMITPAIRING }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, BrowserUi.Selectors.ADDPAIRINGRESULT);
            result.ShouldContain($"\"{scenario.GinLabel}\" gin and \"{scenario.TonicLabel}\" tonic were paired!");
        }

        public async Task DeletePairingAsync() {
            await OpenSectionAsync(sut, DeletePairingSection);
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.PAIRINGDROPDOWN, scenario.PairingLabel);
            await sut.GetByLabel(BrowserUi.Labels.PAIRINGTOREMOVE).SelectOptionAsync(new SelectOptionValue { Label = scenario.PairingLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.REMOVESELECTEDPAIRING }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.CONFIRMDELETEPAIRING }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, BrowserUi.Selectors.DELETEPAIRINGRESULT);
            result.ShouldContain($"\"{scenario.PairingLabel}\" pairing was removed!");
            await WaitForDropdownOptionToBeAbsentAsync(sut, BrowserUi.Selectors.PAIRINGDROPDOWN, scenario.PairingLabel);
        }

        public async Task DeleteGinAsync() {
            await OpenSectionAsync(sut, DeleteGinSection);
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.GINDROPDOWN, scenario.GinLabel);
            await sut.GetByLabel(BrowserUi.Labels.GINTOREMOVE).SelectOptionAsync(new SelectOptionValue { Label = scenario.GinLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.REMOVESELECTEDGIN }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.CONFIRMDELETEGIN }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, BrowserUi.Selectors.DELETEGINRESULT);
            result.ShouldContain($"\"{scenario.GinLabel}\" gin was removed!");
            await WaitForDropdownOptionToBeAbsentAsync(sut, BrowserUi.Selectors.GINDROPDOWN, scenario.GinLabel);
        }

        public async Task DeleteTonicAsync() {
            await OpenSectionAsync(sut, DeleteTonicSection);
            await WaitForDropdownOptionAsync(sut, BrowserUi.Selectors.TONICDROPDOWN, scenario.TonicLabel);
            await sut.GetByLabel(BrowserUi.Labels.TONICTOREMOVE).SelectOptionAsync(new SelectOptionValue { Label = scenario.TonicLabel });
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.REMOVESELECTEDTONIC }).ClickAsync();
            await sut.GetByRole(AriaRole.Button, new() { Name = BrowserUi.Buttons.CONFIRMDELETETONIC }).ClickAsync();

            string result = await WaitForResultTextAsync(sut, BrowserUi.Selectors.DELETETONICRESULT);
            result.ShouldContain($"\"{scenario.TonicLabel}\" tonic was removed!");
            await WaitForDropdownOptionToBeAbsentAsync(sut, BrowserUi.Selectors.TONICDROPDOWN, scenario.TonicLabel);
        }
    }
}
