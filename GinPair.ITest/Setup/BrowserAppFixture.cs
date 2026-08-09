namespace GinPair.ITest.Setup;

public sealed class BrowserAppFixture : IAsyncLifetime {
    private BrowserAppHost host = null!;
    private IPlaywright? playwright;

    public IBrowser Browser { get; private set; } = null!;
    public string ConnectionString => host.ConnectionString;
    public string DatabaseName => host.DatabaseName;
    public Uri BaseUri => host.BaseUri;

    public async Task InitializeAsync() {
        host = new BrowserAppHost();
        await host.StartAsync();

        playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {
            Headless = true
        });
    }

    public async Task DisposeAsync() {
        Exception? result = null;

        try {
            if (Browser != null) {
                await Browser.CloseAsync();
            }
        } catch (Exception ex) {
            result ??= ex;
        }

        try {
            playwright?.Dispose();
        } catch (Exception ex) {
            result ??= ex;
        }

        try {
            if (host != null) {
                await host.DisposeAsync();
            }
        } catch (Exception ex) {
            result ??= ex;
        }

        if (result != null) {
            throw result;
        }
    }

    public HttpClient CreateApiClient() {
        return host.CreateApiClient();
    }

    public async Task<IPage> CreatePageAsync() {
        return await CreatePageAsync(BaseUri);
    }

    public async Task<IPage> CreatePageAsync(Uri baseUri) {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions {
            BaseURL = baseUri.ToString()
        });

        var result = await context.NewPageAsync();
        return result;
    }

    public static async Task ClosePageAsync(IPage page) {
        await page.Context.CloseAsync();
    }
}
