using System;
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
        
        [HttpGet]
        public ActionResult Get()
        {
            string message = "Hello World";
            return Ok(message);
        }
        [HttpGet("matchGin")]
        public async Task<IActionResult> MatchGin(string partial)
        {
            var results = await _context.Gins
                .Where(s => s.GinName.ToUpper().Contains(partial.ToUpper()) || s.Distillery.ToUpper().Contains(partial.ToUpper()))
                .Take(10)
                .ToListAsync();
            return Ok(results);
        }
        //TODO: Add get method to get pairing by gin id. Bug: that search will not work with full gin+distillery input
    }
}