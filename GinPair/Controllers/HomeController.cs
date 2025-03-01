namespace GinPair.Controllers;
public class HomeController(GinPairDbContext ginPairContext) : Controller {
    protected GinPairDbContext gpdb = ginPairContext;

    public IActionResult Index() {
        return View(new PairingVM());
    }

    public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult AddGnt() {
        return View(new AddGntVM());
    }

    public IActionResult DeleteGnt() {
        return View(new DeleteGntVM());
    }
}