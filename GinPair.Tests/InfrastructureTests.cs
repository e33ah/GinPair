using Microsoft.AspNetCore.Http;
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
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            },
            new List<IFilterMetadata>(),
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
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            },
            new List<IFilterMetadata>(),
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
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());

        sut.OnActionExecuting(context);

        context.Result.ShouldBeNull();
    }
}
