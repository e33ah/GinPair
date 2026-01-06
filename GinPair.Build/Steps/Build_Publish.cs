namespace GinPair.Build;

public partial class Build : NukeBuild {
    [Parameter("Output directory for publish artifacts")]
    readonly AbsolutePath PublishDirectory = RootDirectory / "publish";

    public Target PublishStep => _ => _
        .DependsOn(ExamineStep)
        .Triggers(Publish)
        .Executes(() => {
            Log.Information("Publish Step");
        });

    private Target Publish => _ => _
        .DependsOn(PublishStep)
        .Executes(() => {
            var webProjects = Solution.GetAllProjects("GinPair");
            if (!webProjects.Any()) {
                Log.Error("Could not find GinPair project in solution");
                throw new InvalidOperationException("GinPair project not found");
            }

            var webProject = webProjects.First();
            Log.Information("Publishing {ProjectName} to {PublishDirectory}", webProject.Name, PublishDirectory);

            DotNetTasks.DotNetPublish(s => s
                .SetProject(webProject)
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory)
                .SetRuntime("win-x64")
                .SetSelfContained(false)
                .SetProperty("PublishReadyToRun", "true")
                .SetProperty("PublishTrimmed", "false")
               // .EnableNoBuild()
            );

            Log.Information("Publish completed successfully to {PublishDirectory}", PublishDirectory);
        });
}
