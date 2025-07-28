using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace GinPair.Build;
public partial class Build : NukeBuild {
    public Target ConstructStep => _ => _
        .DependsOn(Restore)
        .Triggers(Compile)
        .Executes(() => {
            Log.Information("Construct Step");
        });

    private Target Compile => _ => _
        .DependsOn(ConstructStep)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });

}
