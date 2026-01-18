namespace GinPair.Build;

public partial class Build : NukeBuild {
    public Target ReleaseStep => _ => _
        .DependsOn(PublishStep)
        .Triggers(Release)
        .Executes(() => {
            Log.Information("Release Step");
        });

    private Target Release => _ => _
        .DependsOn(Publish)
        .Requires(() => PublishDirectory)
        .Requires(() => AzureResourceGroup)
        .Requires(() => AzureWebAppName)
        .Executes(() => {
            var zipFile = TemporaryDirectory / "deploy.zip";

            zipFile.DeleteFile();
            CompressionExtensions.ZipTo(PublishDirectory, zipFile);

            string azureWebAppDeployArguments =
                $"webapp deploy" +
                $" --resource-group {AzureResourceGroup}" +
                $" --name {AzureWebAppName}" +
                $" --src-path \"{zipFile}\"" +
                $" --type zip" +
                $" --restart true" +
                $" --timeout 1800";

            ProcessTasks.StartProcess(
                "az",
                azureWebAppDeployArguments
                ).AssertZeroExitCode();

            Log.Information("Release completed successfully.");
        });
}
