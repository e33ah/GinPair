namespace GinPair.Build;

public partial class Build : FalloutBuild {
    [Parameter("Output directory for publish artifacts")]
    readonly AbsolutePath PublishDirectory = RootDirectory / "publish";

    public Target PublishStep => _ => _
        .Before(ReleaseStep)
        .DependsOn(ExamineStep)
        .Triggers(Publish)
        .Executes(() => {
            Log.Information("Publish Step");
        });

    private Target Publish => _ => _
        .Before(ReleaseStep)
        .DependsOn(PublishStep)
        .Executes(() => {
            var project = Solution.Projects.FirstOrDefault(x => x.Name == "GinPair");
            if (project == null) {
                Log.Error("Could not find GinPair project in solution");
                throw new InvalidOperationException("GinPair project not found");
            }
            Log.Information("Publishing {ProjectName} to {PublishDirectory}", project.Name, PublishDirectory);

            DotNetTasks.DotNetPublish(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .SetOutput(PublishDirectory)
                .EnableNoRestore()
            );

            Log.Information("Publish completed successfully to {PublishDirectory}", PublishDirectory);
        });
}
