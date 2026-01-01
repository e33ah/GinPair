using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace GinPair.Build;

public partial class Build : NukeBuild {

    [NuGetPackage(packageId: "dotnet-stryker", packageExecutable: "Stryker.CLI.dll", Framework = "net8.0")]
    protected Tool StrykerNet { get; set; }

    public Target ExamineStep => _ => _
        .DependsOn(ConstructStep)
        .Triggers(UnitTest, MutationAnalysis)
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
    private Target MutationAnalysis => _ => _
    .After(UnitTest)
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
}
