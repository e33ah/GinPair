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

        var result = controller.Index();

        var viewResult = result.ShouldBeOfType<ViewResult>();
        viewResult.Model.ShouldBeOfType<PairingVM>();
        viewResult.ViewName.ShouldBeNull();
    }
}