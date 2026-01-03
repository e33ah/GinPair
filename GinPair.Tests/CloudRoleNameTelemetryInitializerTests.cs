using GinPair.Infrastructure;
using Microsoft.ApplicationInsights.DataContracts;

namespace GinPair.Tests;

public class CloudRoleNameTelemetryInitializerTests {
    [Fact]
    public void Initialize_SetsCloudRoleName_WhenTelemetryProvided() {
        string expectedRoleName = "TestRole";
        var sut = new CloudRoleNameTelemetryInitializer(expectedRoleName);
        var telemetry = new RequestTelemetry();

        sut.Initialize(telemetry);

        telemetry.Context.Cloud.RoleName.ShouldBe(expectedRoleName);
    }

    [Theory]
    [InlineData("GinPair-Test")]
    [InlineData("GinPair-Development")]
    public void Initialize_SetsCloudRoleName_WhenDifferentRoleNamesProvided(string roleName) {
        var sut = new CloudRoleNameTelemetryInitializer(roleName);
        var telemetry = new EventTelemetry();

        sut.Initialize(telemetry);

        telemetry.Context.Cloud.RoleName.ShouldBe(roleName);
    }

    [Fact]
    public void Initialize_PreservesOtherTelemetryProperties_WhenCloudRoleNameSet() {
        string roleName = "TestRole";
        var sut = new CloudRoleNameTelemetryInitializer(roleName);
        var telemetry = new RequestTelemetry { Name = "TestRequest" };
        string originalName = telemetry.Name;

        sut.Initialize(telemetry);

        telemetry.Name.ShouldBe(originalName);
        telemetry.Context.Cloud.RoleName.ShouldBe(roleName);
    }

    [Fact]
    public void Initialize_HandlesEmptyString_BySettingToNull() {
        var sut = new CloudRoleNameTelemetryInitializer(string.Empty);
        var telemetry = new EventTelemetry();

        sut.Initialize(telemetry);

        telemetry.Context.Cloud.RoleName.ShouldBeNull();
    }
}