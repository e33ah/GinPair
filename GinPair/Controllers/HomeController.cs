using System.Diagnostics;
using GinPair.Data;
using GinPair.Migrations;
using GinPair.Models;
using GinPair.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Controllers
{
    public class HomeController : Controller
    {
        protected GinPairDbContext gpdb;
        public HomeController(GinPairDbContext ginPairContext)
        {
            gpdb = ginPairContext;
        }

        public async Task<IActionResult> Index(string searchstring)
        {
            ViewData["CurrentFilter"] = searchstring;
            var gins = from g in gpdb.Gins
                       select g;
            if (String.IsNullOrEmpty(searchstring))
            {
                var vm = new PairingVM();
                return View(vm);
            }
            else
            {
                gins = gins.Where(s => s.GinName.ToUpper().Contains(searchstring.ToUpper()) || s.Distillery.ToUpper().Contains(searchstring.ToUpper()));
                var ginList = gins.ToList();
                if (ginList.Count == 0)
                {
                    var vm = new PairingVM();
                    ViewData["CurrentFilter"] = string.Empty;
                    vm.Message = $"Sorry, a gin matching \"{searchstring}\" was not found! \n Try searching again, or add it to our collection.";
                    return View(vm);
                }
                else
                {
                    Gin firstMatch = ginList.FirstOrDefault();
                    int ginId = firstMatch.GinId; //TODO: this is currently hard coded. needs fixing for multiple returns
                    var result = await (from tonic in gpdb.Tonics
                                        join pairing in gpdb.Pairings on tonic.TonicId equals pairing.TonicId
                                        join gin in gpdb.Gins on pairing.GinId equals gin.GinId
                                        where gin.GinId == ginId
                                        select new
                                        {
                                            tonic.TonicBrand,
                                            tonic.TonicFlavour
                                        }).ToListAsync();
                    if (result.Count == 0) {
                        var vm = new PairingVM();
                        ViewData["CurrentFilter"] = string.Empty;
                        vm.Message = $"Sorry, there is no pairing available for \"{firstMatch.Distillery} {firstMatch.GinName}\". \n Try searching again, or add one to our collection.";
                        return View(vm);
                    }
                    else
                    {
                        var vm = result.Select(p => new PairingVM()
                        {
                            TonicBrand = p.TonicBrand,
                            TonicFlavour = p.TonicFlavour,
                            GinName = firstMatch.GinName,
                            Distillery = firstMatch.Distillery
                        }).ToList();
                    
                        Random random = new Random();
                        int r = random.Next(0, vm.Count);
                        ViewData["CurrentFilter"] = vm[r].Distillery + " " + vm[r].GinName;
                        vm[r].Message = $"Try pairing {vm[r].Distillery} {vm[r].GinName} gin with a {vm[r].TonicBrand} {vm[r].TonicFlavour} tonic!";
                        return View(vm[r]);
                    }
                }
            }
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
            var tonicList = gpdb.Tonics.Select(t => new SelectListItem
            {
                Value = t.TonicId.ToString(),
                Text = t.TonicBrand.ToString() + " " + t.TonicFlavour.ToString()
            }).ToList();
            var ginList = gpdb.Gins.Select(g => new SelectListItem
            {
                Value = g.GinId.ToString(),
                Text = g.Distillery.ToString() + " " + g.GinName.ToString()
            }).ToList();
            var vm = new AddGntVM
            {
                TonicFlavours = tonicList,
                Gins = ginList,
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
                    return RedirectToAction("AddGnt", "Home", new { isGinInvalid = isGinInvalid });

                }
                _ = gpdb.Gins.Add(gn);
                if (!vm.AddPairingLater)
                {
                    Tonic t = gpdb.Tonics.Find(vm.TonicId);
                    Pairing pr = new()
                    {
                        PairedGin = gn,
                        PairedTonic = t
                    };
                    _ = gpdb.Pairings.Add(pr);
                }

                _ = gpdb.SaveChanges();
                return RedirectToAction("NotifyUserGin", "Home", new { id = gn.GinId });
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
                    return RedirectToAction("AddGnt", "Home", new { isTonicInvalid = isTonicInvalid });

                }
                _ = gpdb.Tonics.Add(ton);
                _ = gpdb.SaveChanges();
                return RedirectToAction("NotifyUserTonic", "Home", new { id = ton.TonicId });
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
                return RedirectToAction("NotifyUserPairing", "Home", new { id = p.PairingId });
            }
            else
            {
                return RedirectToAction("AddGnt", "Home");
            }
        }
        public IActionResult NotifyUserGin(int id = 0)
        {
            NotifyUserGinVM vm = new();
            if (id != 0)
            {
                var f = gpdb.Gins.FirstOrDefault(e => e.GinId == id);
                if (f != null)
                {
                    vm.Message = $"\"{f.Distillery} {f.GinName}\" gin was added successfully!";
                }
            }
            return View(vm);
        }
        public IActionResult NotifyUserTonic(int id = 0)
        {
            NotifyUserTonicVM vm = new();
            if (id != 0)
            {
                var f = gpdb.Tonics.FirstOrDefault(e => e.TonicId == id);
                if (f != null)
                {
                    vm.Message = $"\"{f.TonicBrand} {f.TonicFlavour}\" tonic was added successfully!";
                }
            }
            return View(vm);
        }
        public IActionResult NotifyUserPairing(int id = 0)
        {
            NotifyUserPairingVM vm = new();
            if (id != 0)
            {
                var f = gpdb.Pairings.FirstOrDefault(e => e.PairingId == id);
                if (f != null)
                {
                    Tonic pairedTonic = gpdb.Tonics.Find(f.TonicId);
                    Gin pairedGin = gpdb.Gins.Find(f.GinId);
                    vm.Message = $"\"{pairedGin.Distillery} {pairedGin.GinName}\" gin and \"{pairedTonic.TonicBrand} {pairedTonic.TonicFlavour}\" tonic were paired successfully!";
                }
            }
            return View(vm);
        }
        [HttpPost]
        public IActionResult NotifyUserGin()
        {
            return RedirectToAction("AddGnt", "Home");
        }        
        [HttpPost]
        public IActionResult NotifyUserTonic()
        {
            return RedirectToAction("AddGnt", "Home");
        }        
        [HttpPost]
        public IActionResult NotifyUserPairing()
        {
            return RedirectToAction("AddGnt", "Home");
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
}