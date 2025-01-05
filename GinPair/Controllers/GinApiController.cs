using GinPair.Data;
using GinPair.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GinApiController : ControllerBase
    {
        protected GinPairDbContext _context;
        public GinApiController(GinPairDbContext ginPairContext)
        {
            _context = ginPairContext;
        }
        //    [HttpGet("{id}")]
        //    public async Task<ActionResult<Gin>> GetGinById(int id)
        //    {
        //        var gin = await _context.Gins.FindAsync(id);

        //        if (gin == null)
        //        {
        //            return NotFound();
        //        }
        //        return Ok(gin);
        //    }
        [HttpGet]
        public ActionResult Get()
        {
            string message = "Hello World";
            return Ok(message);
        }
        [HttpGet("search")]
        public IActionResult Search(string query)
        {
            var gins = from g in _context.Gins
                select g;
            var results = gins.Where(s => s.GinName.ToUpper().Contains(query.ToUpper())).ToList();
            return Ok(results);

        }
    }
}
