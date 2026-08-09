namespace GinPair.Build;

[GitHubActions("ginpair-ci", GitHubActionsImage.WindowsLatest, On = new[] { GitHubActionsTrigger.Push, GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(ArrangeStep), nameof(ConstructStep), nameof(ExamineStep) }, AutoGenerate = false)]
public partial class Build : FalloutBuild {
    public static int Main() {
        if (IsLocalBuild) {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Build>(optional: true)
                .Build();
            SetEnvironmentVariableIfMissing(nameof(AzureResourceGroup), config[nameof(AzureResourceGroup)]);
            SetEnvironmentVariableIfMissing(nameof(AzureWebAppName), config[nameof(AzureWebAppName)]);
        }
        return Execute<Build>(x => x.Compile);
    }

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution Solution;

    [Secret]
    [Parameter("Azure Resource Group for deployment")]
    readonly string AzureResourceGroup;

    [Secret]
    [Parameter("Azure Web App Name for deployment")]
    readonly string AzureWebAppName;

    [Parameter("Enable Mutation Analysis using Stryker.NET")]
    readonly bool EnableMutationAnalysis = false;

    static void SetEnvironmentVariableIfMissing(string name, string value) {
        if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))) {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}