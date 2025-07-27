using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GinPair.Tests;

public class DatabaseReadyFilterTests {
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
