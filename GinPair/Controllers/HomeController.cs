using System.Diagnostics;
using GinPair.Data;
using GinPair.Migrations;
using GinPair.Models;
using Microsoft.AspNetCore.Mvc;
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

        //public async Task<IActionResult> Index(string searchstring)
        //{
        //    ViewData["CurrentFilter"] = searchstring;
        //    var gins = from g in gpdb.Gins
        //               select g;
        //    if (!String.IsNullOrEmpty(searchstring))
        //    {
        //        gins = gins.Where(s => s.GinName.ToUpper().Contains(searchstring.ToUpper()));
        //    }
        //    return View(await gins.ToListAsync());
        //}
        public async Task<IActionResult> Index(string searchstring)
        {
            ViewData["CurrentFilter"] = searchstring;
            var gins = from g in gpdb.Gins
                       select g;
            if (!String.IsNullOrEmpty(searchstring))
            {
                gins = gins.Where(s => s.GinName.ToUpper().Contains(searchstring.ToUpper()));
                var ginList = gins.ToList();
                int ginId1 = ginList[0].GinId; //TODO: this is currently hard coded. needs fixing for multiple returns

                var result = await (from tonic in gpdb.Tonics
                                    join pairing in gpdb.Pairings on tonic.TonicId equals pairing.TonicId
                                    join gin in gpdb.Gins on pairing.GinId equals gin.GinId
                                    where gin.GinId == ginId1
                                    select new
                                    {
                                        tonic.TonicBrand,
                                        tonic.TonicFlavour
                                    }).ToListAsync();

                var vm = result.Select(r => new PairingVM()
                {
                    TonicBrand = r.TonicBrand,
                    TonicFlavour = r.TonicFlavour,
                    GinName = ginList[0].GinName,
                    Distillery = ginList[0].Distillery
                }).ToList();

                //TODO: fix this result return to not be a list
                return View(vm[0]);
            }
            else { 
                var vm = new PairingVM();
                return View(vm);
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult UpdatePage()
        {
            var vm = new UpdatePageVM();

            var res = gpdb.Metas.FirstOrDefault(x => x.Name == "releasename");
            if (res == null)
            {
                vm.ReleaseName = "There is no release found in the db";
            }
            else
            {
                vm.ReleaseName = res.Name;
            }

            vm.SomeData = "some data data";
            vm.LifeEvents = gpdb.Events.Where(x => x.When > DateTime.Now.AddDays(-3).ToUniversalTime()).ToList();
            vm.Gins = gpdb.Gins.ToList();
            return View(vm);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult AddEvent() {
            var vm = new AddEventVM();
            return View(vm); }
        [HttpPost]
        public IActionResult AddEvent(AddEventVM vm)
        {
            if (vm.isValid() == false)
            {
                TempData["ErrorMessage"] = "Event must have a value";
                return RedirectToAction("AddEvent", "Home");

            }
            vm.Type = EventType.Unknown;
            vm.When = DateTime.Now;

            LifeEvent le = new()
            {
                EventName = vm.EventName,
                Type = vm.Type,
                When = vm.When.ToUniversalTime()
            };
            _ = gpdb.Events.Add(le);
            _ = gpdb.SaveChanges();
            return RedirectToAction("NotifyUserEvent", "Home", new { id = le.Id });
        }
        public IActionResult NotifyUserEvent(int id=0)
        {
            NotifyUserEventVM vm = new();
            if (id != 0)
            {
                var f = gpdb.Events.FirstOrDefault(e => e.Id == id);
                if (f != null)
                {
                    vm.Message = $"Event \"{f.EventName}\" of type {f.Type} was added successfully!";
                }
            }
            return View(vm);
        }
        [HttpPost]
        public IActionResult NotifyUserEvent()
        {
            return RedirectToAction("AddEvent", "Home");
        }
        public IActionResult AddGnt()
        {
            var vm = new AddGntVM();
            return View(vm);
        }
        [HttpPost]
        public IActionResult AddGnt(AddGntVM vm)
        {
            if (vm.GinName != null)
            {
                Gin gn = new()
                {
                    GinName = vm.GinName,
                    Distillery = vm.Distillery,
                    GinDescription = vm.GinDescription
                };
                _ = gpdb.Gins.Add(gn);
                _ = gpdb.SaveChanges();
                return RedirectToAction("NotifyUserGin", "Home", new { id = gn.GinId });
            }
            else if (vm.TonicFlavour != null)
            {
                Tonic ton = new()
                {
                    TonicBrand = vm.TonicBrand,
                    TonicFlavour = vm.TonicFlavour,
                };
                _ = gpdb.Tonics.Add(ton);
                _ = gpdb.SaveChanges();
                return RedirectToAction("NotifyUserTonic", "Home", new { id = ton.TonicId });
            }
            else
            {
                return View(vm);
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
                    vm.Message = $"Gin \"{f.GinName}\" was added successfully!";
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
        [HttpPost]
        public IActionResult NotifyUserGin()
        {
            return RedirectToAction("AddGnt", "Home");
        }
    }
}