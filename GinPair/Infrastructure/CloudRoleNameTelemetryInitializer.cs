using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace GinPair.Infrastructure;

public class CloudRoleNameTelemetryInitializer(string roleName) : ITelemetryInitializer {
    public void Initialize(ITelemetry telemetry) {
        telemetry.Context.Cloud.RoleName = roleName;
    }
}
