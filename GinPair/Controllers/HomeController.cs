namespace GinPair.Controllers;
public class HomeController() : Controller {

    public IActionResult Index() {
        return View(new PairingVM());
    }

    public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        string requestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier ?? string.Empty;
        return View(new ErrorViewModel { RequestId = requestId });
    }

    public IActionResult AddGnt() {
        return View(new AddGntVM());
    }

    public IActionResult DeleteGnt() {
        return View(new DeleteGntVM());
    }
}