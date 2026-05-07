using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GinPair;

public class DatabaseReadyFilter(DatabaseInitializationState dbInitState) : IActionFilter {
    private readonly DatabaseInitializationState dbInitState = dbInitState;

    public void OnActionExecuting(ActionExecutingContext context) {
        if (!dbInitState.IsDatabaseReady) {
            context.Result = new ObjectResult("Hang tight! We're getting things ready. Please try again in a minute.") {
                StatusCode = 503
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
