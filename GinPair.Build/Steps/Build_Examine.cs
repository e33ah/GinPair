namespace GinPair.Build;

public partial class Build : NukeBuild {

    [NuGetPackage(packageId: "dotnet-stryker", packageExecutable: "Stryker.CLI.dll", Framework = "net8.0")]
    protected Tool StrykerNet { get; set; }

    public Target ExamineStep => _ => _
        .Before(PublishStep)
        .DependsOn(ConstructStep)
        .Triggers(UnitTest, IntegrationTest, MutationAnalysis)
        .Executes(() => {
            Log.Information("Examine Step");
        });

    private Target UnitTest => _ => _
        .Before(PublishStep)
        .DependsOn(ExamineStep)
        .Executes(() => {
            Log.Information("Running unit tests...");

            var testProjects = Solution.GetAllProjects("*.Tests");
            if (testProjects.Any()) {
                foreach (var testProject in testProjects) {
                    DotNetTasks.DotNetTest(s => s
                        .SetProjectFile(testProject.Path)
                        .SetConfiguration(Configuration)
                        .EnableNoBuild()
                        .EnableNoRestore()
                    );
                }
            } else {
                Log.Warning("No test projects found in the solution.");
            }
        });

    private Target IntegrationTest => _ => _
        .Before(PublishStep)
        .After(UnitTest)
        .Executes(() => {
            var integrationTestProjects = Solution.GetAllProjects("*.ITest");
            if (!integrationTestProjects.Any()) {
                Log.Warning("No integration test projects found in the solution.");
                return;
            }

            if (!OperatingSystem.IsWindows()) {
                Log.Warning("Skipping integration tests because the current Playwright setup depends on SQL Server LocalDB and only runs on Windows.");
                return;
            }

            Log.Information("Running integration tests...");

            foreach (var integrationTestProject in integrationTestProjects) {
                InstallPlaywrightBrowsers(integrationTestProject);

                DotNetTasks.DotNetTest(s => s
                    .SetProjectFile(integrationTestProject.Path)
                    .SetConfiguration(Configuration)
                    .SetProcessEnvironmentVariable("BUILD_CONFIGURATION", Configuration.ToString())
                    .EnableNoBuild()
                    .EnableNoRestore()
                );
            }
        });

    private Target MutationAnalysis => _ => _
        .OnlyWhenDynamic(() => EnableMutationAnalysis)
        .After(UnitTest)
        .Before(PublishStep)
        .Executes(() => {
            try {
                Log.Information("Running mutation analysis...");
                var testProjects = Solution.GetAllProjects("*.Tests");
                if (testProjects.Any()) {
                    string strykerArgs = "";
                    string strykerConfigPath = Path.Combine(testProjects.First().Directory, "stryker-config.json");
                    if (File.Exists(strykerConfigPath)) {
                        strykerArgs = $"--config-file {strykerConfigPath}";
                    }

                    StrykerNet(arguments: strykerArgs, workingDirectory: testProjects.First().Directory);
                } else {
                    Log.Warning("No test projects found for mutation analysis. Skipping Stryker execution.");
                }
            } catch (Exception ex) {
                Log.Error(ex, "Mutation analysis failed while running Stryker.");
                throw;
            }
        });

    private void InstallPlaywrightBrowsers(Project integrationTestProject) {
        string projectBinDirectory = Path.Combine(integrationTestProject.Directory, "bin", Configuration.ToString());
        string integrationTestFramework = GetIntegrationTestFrameworkFolder(projectBinDirectory);
        string playwrightScriptPath = Path.Combine(projectBinDirectory, integrationTestFramework, "playwright.ps1");
        if (!File.Exists(playwrightScriptPath)) {
            throw new FileNotFoundException($"Playwright install script was not found for {integrationTestProject.Name}. Expected path: {playwrightScriptPath}");
        }

        Log.Information("Installing Playwright Chromium for {ProjectName}", integrationTestProject.Name);

        ProcessTasks.StartProcess(
                toolPath: "powershell",
                arguments: $"-ExecutionPolicy Bypass -File \"{playwrightScriptPath}\" install chromium",
                workingDirectory: integrationTestProject.Directory)
            .AssertZeroExitCode();
    }
    private static string GetIntegrationTestFrameworkFolder(string projectBinDirectory) {
        if (!Directory.Exists(projectBinDirectory)) {
            throw new DirectoryNotFoundException($"Could not find bin directory: {projectBinDirectory}");
        }

        string[] frameworkDirectories = Directory.GetDirectories(projectBinDirectory);
        if (frameworkDirectories.Length == 0) {
            throw new InvalidOperationException($"No framework directories found in: {projectBinDirectory}");
        }

            string result = Path.GetFileName(frameworkDirectories[^1]);
        return result;
    }
}
