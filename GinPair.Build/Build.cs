using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;

namespace GinPair.Build;

[GitHubActions("ginpair-ci", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.Push, GitHubActionsTrigger.PullRequest },
    InvokedTargets = new[] { nameof(ArrangeStep), nameof(ConstructStep), nameof(ExamineStep) }, AutoGenerate = false)]
public partial class Build : NukeBuild {
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    readonly Solution Solution;

}