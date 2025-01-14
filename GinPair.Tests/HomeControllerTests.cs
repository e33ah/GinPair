using GinPair.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GinPair.Models;
using GinPair.Data;

namespace GinPair.Tests;

public class HomeControllerTests
{
    private static DbContextOptions<GinPairDbContext> GetDbContextOptions()
    {
        return new DbContextOptionsBuilder<GinPairDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void Index_Returns_ViewResult_With_ViewModel()
    {
        // Arrange - configure mock in-memory db
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        var controller = new HomeController(mockContext);

        // Act - call Index action of HomeController
        var result =  controller.Index(null);

        // Assert result of the Index action is of type ViewResult (subclass of ActionResult class) and model is a Pairing VM
        var viewResult = Assert.IsType<ViewResult>(result); 
        var modelResult = Assert.IsType<PairingVM>(viewResult.Model); 
        Assert.Null(viewResult.ViewName); // ViewName will be null if default View is being returned (Index)
    }
    [Fact]
    public void Index_With_EmptySearch_Returns_ViewResult()
    {
        var options = GetDbContextOptions();
        using var mockContext = new GinPairDbContext(options);
        var controller = new HomeController(mockContext);

        var result = controller.Index("");

        // Assert 
        var viewResult = Assert.IsType<ViewResult>(result); 
        Assert.Null(viewResult.ViewName); 
    }
}


