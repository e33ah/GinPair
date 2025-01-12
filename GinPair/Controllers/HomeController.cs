using System.Diagnostics;
using GinPair.Data;
using GinPair.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Controllers;
public class HomeController(GinPairDbContext ginPairContext) : Controller
{
    protected GinPairDbContext gpdb = ginPairContext;

    public async Task<IActionResult> Index(string searchstring)
    {
        var gins = from g in gpdb.Gins
                   select g;
        //if (String.IsNullOrEmpty(searchstring))
        //{
        var vm = new PairingVM();
        return View(vm);
        // TO BE DELETED?
        //}
        //else
        //{
        //    gins = gins.Where(s => s.GinName.ToUpper().Contains(searchstring.ToUpper()) || s.Distillery.ToUpper().Contains(searchstring.ToUpper()));
        //    var ginList = gins.ToList();
        //    if (ginList.Count == 0)
        //    {
        //        var vm = new PairingVM();
        //        vm.Message = $"Sorry, a gin matching \"{searchstring}\" was not found!<br>Try searching again, or <a href='/Home/AddGnt/'>add it</a> to our collection.";
        //        return View(vm);
        //    }
        //    else
        //    {
        //        Gin firstMatch = ginList.FirstOrDefault();
        //        int ginId = firstMatch.GinId; //TODO: this is currently hard coded. needs fixing for multiple returns
        //        var result = await (from tonic in gpdb.Tonics
        //                            join pairing in gpdb.Pairings on tonic.TonicId equals pairing.TonicId
        //                            join gin in gpdb.Gins on pairing.GinId equals gin.GinId
        //                            where gin.GinId == ginId
        //                            select new
        //                            {
        //                                tonic.TonicBrand,
        //                                tonic.TonicFlavour
        //                            }).ToListAsync();
        //        if (result.Count == 0) {
        //            var vm = new PairingVM();
        //            vm.Message = $"Sorry, there is no pairing available for \"{firstMatch.Distillery} {firstMatch.GinName}\".<br>Try searching again, or <a href='/Home/AddGnt/'>add it</a> to our collection.";
        //            return View(vm);
        //        }
        //        else
        //        {
        //            var vm = result.Select(p => new PairingVM()
        //            {
        //                TonicBrand = p.TonicBrand,
        //                TonicFlavour = p.TonicFlavour,
        //                GinName = firstMatch.GinName,
        //                Distillery = firstMatch.Distillery
        //            }).ToList();
                
        //            Random random = new Random();
        //            int r = random.Next(0, vm.Count);
        //            vm[r].Message = $"Try pairing {vm[r].Distillery} {vm[r].GinName} gin with <br>a {vm[r].TonicBrand} {vm[r].TonicFlavour} tonic!";
        //            return View(vm[r]);
        //        }
        //    }
        //}
    }

    public IActionResult Privacy()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult AddGnt(bool isGinInvalid, bool isTonicInvalid)
    {
        var ginList = gpdb.Gins.Select(g => new SelectListItem
        {
            Value = g.GinId.ToString(),
            Text = g.Distillery.ToString() + " " + g.GinName.ToString()
        }).ToList().OrderBy(o => o.Text);
        var tonicList = gpdb.Tonics.Select(t => new SelectListItem
        {
            Value = t.TonicId.ToString(),
            Text = t.TonicBrand.ToString() + " " + t.TonicFlavour.ToString()
        }).ToList().OrderBy(o => o.Text);
        var vm = new AddGntVM
        {
            Gins = ginList,
            TonicFlavours = tonicList,
            IsGinInvalid = isGinInvalid,
            IsTonicInvalid = isTonicInvalid
        };
        return View(vm);
    }
    [HttpPost]
    public IActionResult AddGnt(AddGntVM vm)
    {
        // Add Gin
        if (vm.GinName != null && vm.Distillery != null)
        {
            Gin gn = new()
            {
                GinName = vm.GinName,
                Distillery = vm.Distillery,
                GinDescription = vm.GinDescription
            };
            if (IsGinPresent(gn.GinName, gn.Distillery))
            {
                bool isGinInvalid = true;
                return RedirectToAction("AddGnt", "Home", new { isGinInvalid });

            }
            try
            {
                _ = gpdb.Gins.Add(gn);
                _ = gpdb.SaveChanges();
                TempData["Message"] = $"\"{gn.Distillery} {gn.GinName}\" gin was added successfully!";
                return RedirectToAction("NotifyUserGin", "Home");
            }
            catch (DbUpdateException ex) {
                Console.WriteLine(ex.Message);
                return RedirectToAction("AddGnt", "Home");
            }
        }
        // Add Tonic
        else if (vm.TonicFlavour != null)
        {
            Tonic ton = new()
            {
                TonicBrand = vm.TonicBrand,
                TonicFlavour = vm.TonicFlavour,
            };
            if (IsTonicPresent(ton.TonicBrand, ton.TonicFlavour))
            {
                bool isTonicInvalid = true;
                return RedirectToAction("AddGnt", "Home", new { isTonicInvalid });

            }
            _ = gpdb.Tonics.Add(ton);
            _ = gpdb.SaveChanges();
            TempData["Message"] = $"\"{ton.TonicBrand} {ton.TonicFlavour}\" tonic was added successfully!";
            return RedirectToAction("NotifyUserGin", "Home");
        }
        // Add Pairing
        else if (vm.GinId != 0)
        {
            Pairing p = new()
            {
                GinId = vm.GinId,
                TonicId = vm.TonicId
            };
            _ = gpdb.Pairings.Add(p);
            _ = gpdb.SaveChanges();
            var pairedTonic = gpdb.Tonics.Find(p.TonicId);
            var pairedGin = gpdb.Gins.Find(p.GinId);
            if (pairedTonic != null && pairedGin != null)
            {
                TempData["Message"] = $"\"{pairedGin.Distillery} {pairedGin.GinName}\" gin and \"{pairedTonic.TonicBrand} {pairedTonic.TonicFlavour}\" tonic were paired successfully!";
            }
            return RedirectToAction("NotifyUserGin", "Home");
        }
        else
        {
            return RedirectToAction("AddGnt", "Home");
        }
    }
    public IActionResult DeleteGnt()
    {
        var ginList = gpdb.Gins.Select(g => new SelectListItem
        {
            Value = g.GinId.ToString(),
            Text = g.Distillery.ToString() + " " + g.GinName.ToString()
        }).ToList().OrderBy(o => o.Text);
        var tonicList = gpdb.Tonics.Select(t => new SelectListItem
        {
            Value = t.TonicId.ToString(),
            Text = t.TonicBrand.ToString() + " " + t.TonicFlavour.ToString()
        }).ToList().OrderBy(o => o.Text);
        var vm = new DeleteGntVM
        {
            Gins = ginList,
            Tonics = tonicList
        };
        return View(vm);
    }
    [HttpPost]
    public IActionResult DeleteGnt(DeleteGntVM vm)
    {
        // Delete Gin
        if (vm.GinId != 0)
        {
            var gn = gpdb.Gins.Find(vm.GinId);
            string ginToBeDeleted = $"{gn.Distillery} {gn.GinName}";
            _ = gpdb.Gins.Remove(gn);
            _ = gpdb.SaveChanges();
            return RedirectToAction("NotifyUserDeleteGnt", "Home", new { gntDeleted = ginToBeDeleted });
        }
        // Delete Tonic
        else if (vm.TonicId != 0)
        {
            var ton = gpdb.Tonics.Find(vm.TonicId);
            string tonicToBeDeleted = $"{ton.TonicBrand} {ton.TonicFlavour}";
            _ = gpdb.Tonics.Remove(ton);
            _ = gpdb.SaveChanges();
            return RedirectToAction("NotifyUserDeleteGnt", "Home", new { gntDeleted = tonicToBeDeleted });
        }
        else
        {
            return RedirectToAction("DeleteGnt", "Home");
        }
    }
    public IActionResult NotifyUserGin()
    {
        NotifyUserGinVM vm = new();
        if (TempData["Message"] != null && !string.IsNullOrEmpty(TempData["Message"].ToString()))
        {
            vm.Message = TempData["Message"].ToString();
        }
        return View(vm);
    }
    public IActionResult NotifyUserDeleteGnt(string gntDeleted)
    {
        NotifyUserGinVM vm = new();
        if (!String.IsNullOrEmpty(gntDeleted))
        {
            vm.Message = $"\"{gntDeleted}\" was removed!";
        }
        return View(vm);
    }
    [HttpPost]
    public IActionResult NotifyUserAddButton()
    {
        return RedirectToAction("AddGnt", "Home");
    }
    [HttpPost]
    public IActionResult NotifyUserDeleteButton()
    {
        return RedirectToAction("DeleteGnt", "Home");
    }

    public bool IsGinPresent(string ginName, string distillery)
    {
        bool ginExists = gpdb.Gins.Any(m => m.GinName == ginName && m.Distillery == distillery);
        return ginExists;
    }
    public bool IsTonicPresent(string tonicBrand, string tonicFlavour)
    {
        bool tonicExists = gpdb.Tonics.Any(m => m.TonicBrand == tonicBrand && m.TonicFlavour == tonicFlavour);
        return tonicExists;
    }        
    public bool IsPairingPresent(int ginId, int tonicId)
    {
        bool pairingExists = gpdb.Pairings.Any(m => m.GinId == ginId && m.TonicId == tonicId);
        return pairingExists;
    }
}