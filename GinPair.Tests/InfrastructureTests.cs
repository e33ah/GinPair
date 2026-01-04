using Microsoft.AspNetCore.Mvc.Filters;

namespace GinPair.Tests;

public class InfrastructureTests {
    [Fact]
    public void DatabaseInitializationState_Defaults_IsDatabaseReady_False() {
        var dbInitState = new DatabaseInitializationState();
        dbInitState.IsDatabaseReady.ShouldBeFalse();
    }

    [Fact]
    public void OnActionExecuting_Sets503Result_WhenDatabaseNotReady() {
        var dbInitState = new DatabaseInitializationState { IsDatabaseReady = false };
        var sut = new DatabaseReadyFilter(dbInitState);

        var context = new ActionExecutingContext(
            new ActionContext {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            },
            [],
            new Dictionary<string, object?>(),
            new object());

        sut.OnActionExecuting(context);

        var result = context.Result;
        result.ShouldBeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.ShouldBe(503);
    }

    [Fact]
    public void DatabaseReadyFilter_ShowsMessage_WhenDatabaseNotReady() {
        var dbInitState = new DatabaseInitializationState { IsDatabaseReady = false };
        var sut = new DatabaseReadyFilter(dbInitState);
        var context = new ActionExecutingContext(
            new ActionContext {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            },
            [],
            new Dictionary<string, object?>(),
            new object());
        sut.OnActionExecuting(context);
        var result = context.Result;
        result.ShouldBeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.Value.ShouldBe("Hang tight! We're getting things ready. Please try again in a minute.");
    }

    [Fact]
    public void OnActionExecuting_DoesNotSetResult_WhenDatabaseReadyWorks() {
        var dbInitState = new DatabaseInitializationState { IsDatabaseReady = true };
        var sut = new DatabaseReadyFilter(dbInitState);

        var context = new ActionExecutingContext(
            new ActionContext {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ControllerActionDescriptor()
            },
            [],
            new Dictionary<string, object?>(),
            new object());

        sut.OnActionExecuting(context);

        context.Result.ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetDatabaseStatus_ReturnsOk_WithCorrectStatus(bool isReady) {
        var context = CreateInMemoryGinPairDbContext();
        var dbInitState = new DatabaseInitializationState { IsDatabaseReady = isReady };
        var ginServiceLogger = new NullLogger<GinService>();
        var tonicServiceLogger = new NullLogger<TonicService>();
        var pairingServiceLogger = new NullLogger<PairingService>();
        var controllerLogger = new NullLogger<GinApiController>();

        var ginService = new GinService(context, ginServiceLogger);
        var tonicService = new TonicService(context, tonicServiceLogger);
        var pairingService = new PairingService(context, ginService, tonicService, pairingServiceLogger);
        var sut = new GinApiController(ginService, tonicService, pairingService, dbInitState, controllerLogger);

        var result = sut.GetDatabaseStatus();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        object? response = okResult.Value;
        response.ShouldNotBeNull();
        var isReadyProperty = response.GetType().GetProperty("isReady");
        isReadyProperty.ShouldNotBeNull();
        bool actualIsReady = (bool)isReadyProperty!.GetValue(response)!;
        actualIsReady.ShouldBe(isReady);
    }
}
