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
    [HttpGet("getGinList")]
    public IActionResult GetGinList()
    {
        var ginList = _context.Gins.Select(g => new SelectListItem
        {
            Value = g.GinId.ToString(),
            Text = g.Distillery.ToString() + " " + g.GinName.ToString()
        })
            .ToList().
            OrderBy(o => o.Text);
        return Ok(ginList);
    }

    [HttpGet("getTonicList")]
    public IActionResult GetTonicList()
    {
        var tonicList = _context.Tonics.Select(t => new SelectListItem
        {
            Value = t.TonicId.ToString(),
            Text = t.TonicBrand.ToString() + " " + t.TonicFlavour.ToString()
        })
            .ToList()
            .OrderBy(o => o.Text);

        return Ok(tonicList);
    }

    [HttpPost("addGin")]
    public IActionResult AddGin([FromBody] JsonElement data)
    {
        var response = new ApiResponse();
        string? ginName = data.GetProperty("ginName").GetString();
        string? distillery = data.GetProperty("distillery").GetString();
        string? description = data.GetProperty("description").GetString();

        if (string.IsNullOrEmpty(ginName) || string.IsNullOrEmpty(distillery))
        {
            response.StatusMessage = "Please provide the name of the Distillery and Gin.";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }
        if (IsGinPresent(ginName, distillery))
        {
            response.StatusMessage = "Sorry this gin cannot be added as it is already part of our collection!";
            response.BsColor = BsColor.Danger;
            return Ok(response);
        }
        try
        {
            _ = _context.Gins.Add(new Gin
            {
                GinName = ginName,
                Distillery = distillery,
                GinDescription = description
            });
            _ = _context.SaveChanges();
            response.StatusMessage = $"✅ Success! \"{distillery} {ginName}\" gin was added!";
            response.BsColor = BsColor.Success;
            return Ok(response);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }
    public bool IsGinPresent(string ginName, string distillery)
    {
        bool ginExists = _context.Gins.Any(m => m.GinName.ToLower() == ginName.ToLower() && m.Distillery.ToLower() == distillery.ToLower());
        return ginExists;
    }
    [HttpPost("addTonic")]
    public IActionResult AddTonic([FromBody] JsonElement data)
    {
        var response = new ApiResponse();
        string? tonicBrand = data.GetProperty("tonicBrand").GetString();
        string? tonicFlavour = data.GetProperty("tonicFlavour").GetString();
        if (string.IsNullOrEmpty(tonicBrand) || string.IsNullOrEmpty(tonicFlavour))
        {
            response.StatusMessage = "Please provide the tonic brand and flavour/name.";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }
        if (IsTonicPresent(tonicBrand, tonicFlavour))
        {
            response.StatusMessage = "Sorry this tonic cannot be added as it is already part of our collection!";
            response.BsColor = BsColor.Danger;
            return Ok(response);
        }
        try
        {
            _ = _context.Tonics.Add(new()
            {
                TonicBrand = tonicBrand,
                TonicFlavour = tonicFlavour,
            });
            _ = _context.SaveChanges();
            response.StatusMessage = $"\"{tonicBrand} {tonicFlavour}\" tonic was added successfully!";
            response.BsColor = BsColor.Success;
            return Ok(response);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }
    public bool IsTonicPresent(string tonicBrand, string tonicFlavour)
    {
        bool tonicExists = _context.Tonics.Any(m => m.TonicBrand.ToLower() == tonicBrand.ToLower() && m.TonicFlavour.ToLower() == tonicFlavour.ToLower());
        return tonicExists;
    }
    [HttpPost("addPairing")]
    public IActionResult AddPairing([FromBody] JsonElement data)
    {
        var response = new ApiResponse();
        string? ginId = data.GetProperty("ginId").GetString();
        string? tonicId = data.GetProperty("tonicId").GetString();

        if (string.IsNullOrEmpty(ginId) || string.IsNullOrEmpty(tonicId))
        {
            response.StatusMessage = "Please select the gin and tonic to pair";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }

        var pairedTonic = _context.Tonics.Find(Int32.Parse(tonicId));
        var pairedGin = _context.Gins.Find(Int32.Parse(ginId));

        if (pairedTonic == null || pairedGin == null)
        {
            response.StatusMessage = "Please select the gin and tonic to pair";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }

        try
        {
            _ = _context.Pairings.Add(new()
            {
                GinId = pairedGin.GinId,
                TonicId = pairedTonic.TonicId,

            });
            _ = _context.SaveChanges();
            response.StatusMessage = $"\"{pairedGin.Distillery} {pairedGin.GinName}\" gin and \"{pairedTonic.TonicBrand} {pairedTonic.TonicFlavour}\" tonic were paired successfully!";
            response.BsColor = BsColor.Success;
            return Ok(response);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }
}
