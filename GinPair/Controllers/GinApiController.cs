using GinPair.Data;
using GinPair.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;

namespace GinPair.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GinApiController(GinPairDbContext ginPairContext) : ControllerBase
{
    protected GinPairDbContext _context = ginPairContext;

    [HttpGet("matchGin")]
    public async Task<IActionResult> MatchGin(string partial)
    {
        var results = await _context.Gins
                .Where(s => s.GinName.ToUpper().Contains(partial.ToUpper()) || s.Distillery.ToUpper().Contains(partial.ToUpper()))
            .Take(10)
            .ToListAsync();
        return Ok(results);
    }
    [HttpGet("getPairing/{ginId}")]
    public async Task<IActionResult> GetPairingByGinId(int ginId)
    {
        var ginfind = await _context.Gins.FindAsync(ginId);
        var response = new ApiResponse();

        if (ginfind == null)
        {
            return BadRequest();
        }

        var result = await (from tonic in _context.Tonics
                            join pairing in _context.Pairings on tonic.TonicId equals pairing.TonicId
                            join gin in _context.Gins on pairing.GinId equals gin.GinId
                            where gin.GinId == ginfind.GinId
                            select new
                            {
                                tonic.TonicId,
                                tonic.TonicBrand,
                                tonic.TonicFlavour
                            }).ToListAsync();
        if (result.Count == 0)
        {
            response.StatusMessage = $"<p>Sorry, there is no pairing available for \"{ginfind.Distillery} {ginfind.GinName}\".<br>Try searching again, or <a href='/Home/AddGnt/'>add it</a> to our collection.</p>";
        }
        else
        {
            Random random = new();
            int r = random.Next(0, result.Count);

            var pairingResult = new PairingVM
            {
                GinName = ginfind.GinName,
                Distillery = ginfind.Distillery,
                TonicBrand = result[r].TonicBrand,
                TonicFlavour = result[r].TonicFlavour,
            };
            response.StatusMessage = $"Try pairing {pairingResult.Distillery} {pairingResult.GinName} gin with<br>a {pairingResult.TonicBrand} {pairingResult.TonicFlavour} tonic!";
        }
        return Ok(response);
    }
}