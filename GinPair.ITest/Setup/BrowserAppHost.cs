namespace GinPair.ITest.Setup;

public sealed class BrowserAppHost(int? databaseInitialisationDelayMs = null) : IAsyncDisposable {
    private const string LOCALDBDATASOURCE = "Data Source=(LocalDb)\\MSSQLLocalDB";
    private const string DATABASESTATUSPATH = "/api/ginapi/databaseStatus";
    private static readonly TimeSpan applicationReadinessPollingInterval = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan startupTimeout = TimeSpan.FromSeconds(90);
    private readonly List<string> appLogLines = [];
    private readonly Lock logLock = new();
    private Process? appProcess;

    public string ConnectionString { get; private set; } = string.Empty;
    public string DatabaseName { get; private set; } = string.Empty;
    public Uri BaseUri { get; private set; } = null!;

    public async Task StartAsync(bool waitForReady = true) {
        int port = GetAvailablePort();
        BaseUri = new Uri($"http://127.0.0.1:{port}");
        DatabaseName = $"GinPairITest_{Guid.NewGuid():N}";
        ConnectionString = BuildConnectionString(DatabaseName);

        StartAppProcess(databaseInitialisationDelayMs);
        if (waitForReady) {
            await WaitForDatabaseReadyStateAsync(expectedIsReady: true, startupTimeout);
        }
    }

    public HttpClient CreateApiClient() {
        var result = new HttpClient {
            BaseAddress = BaseUri,
            Timeout = Timeout.InfiniteTimeSpan
        };

        return result;
    }

    public async Task WaitForDatabaseReadyStateAsync(bool expectedIsReady, TimeSpan timeout) {
        using var client = CreateApiClient();
        using var startupCancellationTokenSource = new CancellationTokenSource(timeout);
        var cancellationToken = startupCancellationTokenSource.Token;

        while (!cancellationToken.IsCancellationRequested) {
            ThrowIfApplicationExited();

            try {
                using var response = await client.GetAsync(DATABASESTATUSPATH, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (response.IsSuccessStatusCode) {
                    await using var result = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var payload = await JsonDocument.ParseAsync(result, cancellationToken: cancellationToken);

                    if (payload.RootElement.TryGetProperty("isReady", out var isReady) && isReady.GetBoolean() == expectedIsReady) {
                        return;
                    }
                }
            } catch (HttpRequestException) {
            }

            try {
                await Task.Delay(applicationReadinessPollingInterval, cancellationToken);
            } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                break;
            }
        }

        ThrowIfApplicationExited();
        throw new TimeoutException($"Timed out waiting for GinPair to report database ready state '{expectedIsReady}'.{Environment.NewLine}{GetCapturedLogs()}");
    }

    public async ValueTask DisposeAsync() {
        Exception? result = null;

        try {
            await StopApplicationAsync();
        } catch (Exception ex) {
            result ??= ex;
        }

        try {
            await DropDatabaseAsync();
        } catch (Exception ex) {
            result ??= ex;
        }

        if (result != null) {
            throw result;
        }
    }

    private void StartAppProcess(int? startupDelayMs) {
        string projectPath = GetApplicationProjectPath();
        string workingDirectory = Path.GetDirectoryName(projectPath) ?? throw new InvalidOperationException("Application project directory could not be resolved.");
        string applicationArguments = $"--urls \"{BaseUri}\"";
        if (startupDelayMs.HasValue) {
            applicationArguments += $" --DatabaseInitialisationDelayMs {startupDelayMs.Value}";
        }

        var startInfo = new ProcessStartInfo("dotnet", $"run --project \"{projectPath}\" --no-build -- {applicationArguments}") {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
        startInfo.Environment["DatabaseProvider"] = "SqlServer";
        startInfo.Environment["ConnectionStrings__SqlServerConnection"] = ConnectionString;
        startInfo.Environment["ConnectionStrings__ApplicationInsights"] = string.Empty;

        appProcess = new Process {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        appProcess.OutputDataReceived += (_, args) => AppendLog("stdout", args.Data);
        appProcess.ErrorDataReceived += (_, args) => AppendLog("stderr", args.Data);

        bool result = appProcess.Start();
        if (!result) {
            throw new InvalidOperationException("The GinPair application process could not be started.");
        }

        appProcess.BeginOutputReadLine();
        appProcess.BeginErrorReadLine();
    }

    private void ThrowIfApplicationExited() {
        if (appProcess?.HasExited == true) {
            throw new InvalidOperationException($"The GinPair application exited before becoming ready. Exit code: {appProcess.ExitCode}.{Environment.NewLine}{GetCapturedLogs()}");
        }
    }

    private async Task StopApplicationAsync() {
        if (appProcess == null) {
            return;
        }

        if (!appProcess.HasExited) {
            appProcess.Kill(entireProcessTree: true);
            await appProcess.WaitForExitAsync();
        }

        appProcess.Dispose();
        appProcess = null;
    }

    private async Task DropDatabaseAsync() {
        if (string.IsNullOrWhiteSpace(DatabaseName)) {
            return;
        }

        string escapedDatabaseName = DatabaseName.Replace("]", "]]", StringComparison.Ordinal);
        string masterConnectionString = $"{LOCALDBDATASOURCE};Initial Catalog=master;Integrated Security=SSPI;TrustServerCertificate=True;";

        try {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            string commandText =
                $"""
                IF DB_ID(N'{DatabaseName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{escapedDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{escapedDatabaseName}];
                END
                """;

            await using var command = new SqlCommand(commandText, connection);
            _ = await command.ExecuteNonQueryAsync();
        } catch (SqlException ex) when (IsExpectedTransportDisconnect(ex)) {
            return;
        }
    }

    private static bool IsExpectedTransportDisconnect(SqlException ex) {
        string message = ex.Message;
        bool result =
            message.Contains("A transport-level error has occurred when receiving results from the server.", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("Named Pipes Provider", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("The pipe has been ended.", StringComparison.OrdinalIgnoreCase);
        return result;
    }

    private void AppendLog(string source, string? message) {
        if (string.IsNullOrWhiteSpace(message)) {
            return;
        }

        lock (logLock) {
            appLogLines.Add($"[{source}] {message}");
            if (appLogLines.Count > 200) {
                appLogLines.RemoveAt(0);
            }
        }
    }

    private string GetCapturedLogs() {
        lock (logLock) {
            return string.Join(Environment.NewLine, appLogLines);
        }
    }

    private static string BuildConnectionString(string databaseName) {
        string result = $"{LOCALDBDATASOURCE};Initial Catalog={databaseName};Integrated Security=SSPI;TrustServerCertificate=True;";
        return result;
    }

    private static int GetAvailablePort() {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int result = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return result;
    }

    private static string GetApplicationProjectPath() {
        string result = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "GinPair", "GinPair.csproj"));
        return result;
    }
}
