namespace GinPair.Tests;
public class HomeControllerTests {
    private static DbContextOptions<GinPairDbContext> GetDbContextOptions() {
        return new DbContextOptionsBuilder<GinPairDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void Index_Returns_ViewResult_With_ViewModel() {
        var controller = new HomeController();

        // Act - call Index action of HomeController
        var result = controller.Index();

        // Assert result of the Index action is of type ViewResult (subclass of ActionResult class) and model is a Pairing VM
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<PairingVM>(viewResult.Model);
        Assert.Null(viewResult.ViewName); // ViewName will be null if default View is being returned (Index)
    }
}