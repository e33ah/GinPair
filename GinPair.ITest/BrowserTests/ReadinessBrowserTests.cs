using GinPair.ITest.Setup;

namespace GinPair.ITest.BrowserTests;

[Collection(BrowserAppCollection.COLLECTION_NAME)]
public sealed class ReadinessBrowserTests(BrowserAppFixture fixture) : BrowserTestBase(fixture) {
    [Fact]
    public async Task Layout_ShowsLoadingOverlay_WhenDatabaseIsNotReady() {
        await using BrowserAppHost host = new(DATABASEINITIALISATIONDELAYMS);
        await host.StartAsync(waitForReady: false);
        await host.WaitForDatabaseReadyStateAsync(expectedIsReady: false, TimeSpan.FromSeconds(10));

        var sut = await Fixture.CreatePageAsync(host.BaseUri);

        try {
            await sut.GotoAsync("/Home/AddGnt");
            var result = sut.GetByText("Loading everything, please wait...");
            await result.WaitForAsync();
            (await result.IsVisibleAsync()).ShouldBeTrue();
        } finally {
            await BrowserAppFixture.ClosePageAsync(sut);
        }
    }
}
