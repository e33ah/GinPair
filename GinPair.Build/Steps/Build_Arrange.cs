using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace GinPair.Build;
public partial class Build : NukeBuild {
    public Target ArrangeStep => _ => _
    .Triggers(Clean, Restore)
    .Executes(() => {
        Log.Information("Arrange Step");
    });

    private Target Clean => _ => _
        .DependsOn(ArrangeStep)
        .Executes(() => {
            Log.Information("Cleaning build artefacts...");
            DotNetTasks.DotNetClean(s => s
                .SetConfiguration(Configuration)
                .SetProject(Solution));
        });

    private Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() => {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution)
            );
        });
}
