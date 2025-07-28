using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace GinPair.Build;
public partial class Build : NukeBuild {
    public Target ConstructStep => _ => _
        .Before(ExamineStep)
        .DependsOn(ArrangeStep)
        .Triggers(Compile)
        .Executes(() => {
            Log.Information("Construct Step");
        });

    private Target Compile => _ => _
        .Before(ExamineStep)
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
