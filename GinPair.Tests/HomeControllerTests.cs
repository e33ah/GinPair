namespace GinPair.Tests;
public class HomeControllerTests {

    [Fact]
    public void Index_Returns_ViewResult_With_ViewModel() {
        var controller = new HomeController();

        var result = controller.Index();

        var viewResult = result.ShouldBeOfType<ViewResult>();
        viewResult.Model.ShouldBeOfType<PairingVM>();
        viewResult.ViewName.ShouldBeNull();
    }

    [Fact]
    public void Privacy_Returns_ViewResult() {
        var controller = new HomeController();

        var result = controller.Privacy();

        var viewResult = result.ShouldBeOfType<ViewResult>();
    }

    [Fact]
    public void Error_Returns_ViewResult_With_ViewModel() {
        var controller = new HomeController();

        var result = controller.Error();

        var viewResult = result.ShouldBeOfType<ViewResult>();
        viewResult.Model.ShouldBeOfType<ErrorViewModel>();
        viewResult.ViewName.ShouldBeNull();
        var model = (ErrorViewModel)viewResult.Model;
        model.RequestId.ShouldBeEmpty();
    }

    [Fact]
    public void Error_Returns_ViewWithActivityRequestId() {
        var controller = new HomeController();
        Activity.Current = new Activity("TestActivity").SetIdFormat(ActivityIdFormat.W3C).Start();

        var result = controller.Error() as ViewResult;

        result.ShouldNotBeNull();
        var viewResult = result.ShouldBeOfType<ViewResult>();
        viewResult.Model.ShouldBeOfType<ErrorViewModel>();
        var model = (ErrorViewModel)viewResult.Model;
        model.RequestId.ShouldNotBeEmpty();
        model.RequestId.ShouldBe(Activity.Current.Id);

        Activity.Current.Stop();
    }

    [Fact]
    public void Error_ReturnsViewWithRequestId_FromHttpContext() {
        var controller = new HomeController();
        var httpContext = new DefaultHttpContext {
            TraceIdentifier = "HttpTrace123"
        };
        controller.ControllerContext = new ControllerContext {
            HttpContext = httpContext
        };

        Activity.Current = null;

        var result = controller.Error() as ViewResult;
        var model = result?.Model as ErrorViewModel;

        result.ShouldNotBeNull();
        model.ShouldNotBeNull();
        model.RequestId.ShouldBe("HttpTrace123");
    }


    [Fact]
    public void AddGnT_Returns_ViewResult_With_ViewModel() {
        var controller = new HomeController();

        var result = controller.AddGnt();

        var viewResult = result.ShouldBeOfType<ViewResult>();
        viewResult.Model.ShouldBeOfType<AddGntVM>();
    }

    [Fact]
    public void DeleteGnT_Returns_ViewResult_With_ViewModel() {
        var controller = new HomeController();

        var result = controller.DeleteGnt();

        var viewResult = result.ShouldBeOfType<ViewResult>();
        viewResult.Model.ShouldBeOfType<DeleteGntVM>();
    }
}