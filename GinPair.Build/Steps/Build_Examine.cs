using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace GinPair.Build;
public partial class Build : NukeBuild {
    public Target ExamineStep => _ => _
        .DependsOn(ConstructStep)
        .Triggers(UnitTest)
        .Executes(() => {
            Log.Information("Examine Step");
        });

    private Target UnitTest => _ => _
        .DependsOn(ExamineStep)
        .Executes(() => {
            Log.Information("Running unit tests...");

            var testProjects = Solution.GetAllProjects("*.Tests");
            if (testProjects.Any()) {
                DotNetTasks.DotNetTest(s => s
                    .SetProjectFile(testProjects.First().Directory)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .EnableNoRestore()
                );
            } else {
                Log.Warning("No test projects found in the solution.");
            }
        });

}
