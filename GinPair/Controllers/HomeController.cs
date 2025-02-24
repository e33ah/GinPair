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
        var ginList = gpdb.Gins.Select(g => new SelectListItem {
            Value = g.GinId.ToString(),
            Text = g.Distillery.ToString() + " " + g.GinName.ToString()
        }).ToList().OrderBy(o => o.Text);
        var tonicList = gpdb.Tonics.Select(t => new SelectListItem {
            Value = t.TonicId.ToString(),
            Text = t.TonicBrand.ToString() + " " + t.TonicFlavour.ToString()
        }).ToList().OrderBy(o => o.Text);
        var vm = new DeleteGntVM {
            Gins = ginList,
            Tonics = tonicList
        };
        return View(vm);
    }
    [HttpPost]
    public IActionResult DeleteGnt(DeleteGntVM vm) {
        // Delete Gin
        if (vm.GinId != 0) {
            var gn = gpdb.Gins.Find(vm.GinId);
            string ginToBeDeleted = $"{gn.Distillery} {gn.GinName}";
            _ = gpdb.Gins.Remove(gn);
            _ = gpdb.SaveChanges();
            return RedirectToAction("NotifyUserDeleteGnt", "Home", new { gntDeleted = ginToBeDeleted });
        }
        // Delete Tonic
        else if (vm.TonicId != 0) {
            var ton = gpdb.Tonics.Find(vm.TonicId);
            string tonicToBeDeleted = $"{ton.TonicBrand} {ton.TonicFlavour}";
            _ = gpdb.Tonics.Remove(ton);
            _ = gpdb.SaveChanges();
            return RedirectToAction("NotifyUserDeleteGnt", "Home", new { gntDeleted = tonicToBeDeleted });
        } else {
            return RedirectToAction("DeleteGnt", "Home");
        }
    }

    public IActionResult NotifyUserDeleteGnt(string gntDeleted) {
        NotifyUserGinVM vm = new();
        if (!string.IsNullOrEmpty(gntDeleted)) {
            vm.Message = $"\"{gntDeleted}\" was removed!";
        }
        return View(vm);
    }
    [HttpPost]
    public IActionResult NotifyUserAddButton() {
        return RedirectToAction("AddGnt", "Home");
    }
    [HttpPost]
    public IActionResult NotifyUserDeleteButton() {
        return RedirectToAction("DeleteGnt", "Home");
    }
}