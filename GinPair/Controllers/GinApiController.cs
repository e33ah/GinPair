namespace GinPair.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GinApiController(GinPairDbContext ginPairContext) : ControllerBase {
    protected GinPairDbContext gpdb = ginPairContext;

    [HttpGet("matchGin")]
    public async Task<IActionResult> MatchGin(string partial) {
        if (!gpdb.Gins.Any()) {
            return BadRequest();
        }
        var results = await gpdb.Gins
                .Where(s => s.GinName!.ToUpper().Contains(partial.ToUpper()) || s.Distillery!.ToUpper().Contains(partial.ToUpper()))
            .Take(10)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("getPairing/{ginId}")]
    public async Task<IActionResult> GetPairingByGinId(int ginId) {
        var ginfind = await gpdb.Gins.FindAsync(ginId);
        var response = new ApiResponse();

        if (ginfind == null) {
            return BadRequest();
        }

        var result = await (from tonic in gpdb.Tonics
                            join pairing in gpdb.Pairings on tonic.TonicId equals pairing.TonicId
                            join gin in gpdb.Gins on pairing.GinId equals gin.GinId
                            where gin.GinId == ginfind.GinId
                            select new {
                                tonic.TonicBrand,
                                tonic.TonicFlavour
                            }).ToListAsync();
        if (result.Count == 0) {
            response.StatusMessage = $"<p>Sorry, there is no pairing available for \"{ginfind.Distillery} {ginfind.GinName}\".<br>Try searching again, or <a href='/Home/AddGnt/'>add it</a> to our collection.</p>";
            response.BsColor = BsColor.Warning;
        } else {
            Random random = new();
            int r = random.Next(0, result.Count);

            var pairingResult = new PairingVM {
                GinName = ginfind.GinName,
                Distillery = ginfind.Distillery,
                TonicBrand = result[r].TonicBrand,
                TonicFlavour = result[r].TonicFlavour,
            };
            response.StatusMessage = $"Try pairing {pairingResult.Distillery} {pairingResult.GinName} gin with<br>a {pairingResult.TonicBrand} {pairingResult.TonicFlavour} tonic!";
            response.BsColor = BsColor.Primary;
        }
        return Ok(response);
    }
    [HttpGet("getGinList")]
    public IActionResult GetGinList() {
        var ginList = gpdb.Gins.Select(g => new SelectListItem {
            Value = g.GinId.ToString(),
            Text = $"{g.Distillery} {g.GinName}"
        })
            .ToList().
            OrderBy(o => o.Text);
        return Ok(ginList);
    }

    [HttpGet("getTonicList")]
    public IActionResult GetTonicList() {
        var tonicList = gpdb.Tonics.Select(t => new SelectListItem {
            Value = t.TonicId.ToString(),
            Text = $"{t.TonicBrand} {t.TonicFlavour}"
        })
            .ToList()
            .OrderBy(o => o.Text);

        return Ok(tonicList);
    }

    [HttpGet("getPairingList")]
    public IActionResult GetPairingList() {
        var pairingList = gpdb.Pairings
            .Where(p => p.PairedGin != null && p.PairedTonic != null) // Ensure non-null references
            .Select(p => new SelectListItem {
                Value = p.PairingId.ToString(),
                Text = $"{p.PairedGin!.Distillery} {p.PairedGin.GinName} gin and {p.PairedTonic!.TonicBrand} {p.PairedTonic.TonicFlavour} tonic"
            })
            .ToList()
            .OrderBy(o => o.Text);

        return Ok(pairingList);
    }

    [HttpPost("addGin")]
    public IActionResult AddGin([FromBody] JsonElement data) {
        var response = new ApiResponse();
        string? ginName = data.GetProperty("ginName").GetString();
        string? distillery = data.GetProperty("distillery").GetString();
        string? description = data.GetProperty("description").GetString();

        if (string.IsNullOrEmpty(ginName) || string.IsNullOrEmpty(distillery)) {
            response.StatusMessage = "Please provide the name of the Distillery and Gin.";
            response.BsColor = BsColor.Warning;

            return Ok(response);
        }
        if (IsGinPresent(ginName, distillery)) {
            response.StatusMessage = "Sorry this gin cannot be added as it is already part of our collection!";
            response.BsColor = BsColor.Danger;

            return Ok(response);
        }
        try {
            _ = gpdb.Gins.Add(new Gin {
                GinName = ginName,
                Distillery = distillery,
                GinDescription = description
            });
            _ = gpdb.SaveChanges();

            response.StatusMessage = $"✅ Success! \"{distillery} {ginName}\" gin was added!";
            response.BsColor = BsColor.Success;

            return Ok(response);
        } catch (DbUpdateException ex) {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }

    [HttpPost("addTonic")]
    public IActionResult AddTonic([FromBody] JsonElement data) {
        var response = new ApiResponse();
        string? tonicBrand = data.GetProperty("tonicBrand").GetString();
        string? tonicFlavour = data.GetProperty("tonicFlavour").GetString();

        if (string.IsNullOrEmpty(tonicBrand) || string.IsNullOrEmpty(tonicFlavour)) {
            response.StatusMessage = "Please provide the tonic brand and flavour/name.";
            response.BsColor = BsColor.Warning;

            return Ok(response);
        }
        if (IsTonicPresent(tonicBrand, tonicFlavour)) {
            response.StatusMessage = "Sorry this tonic cannot be added as it is already part of our collection!";
            response.BsColor = BsColor.Danger;

            return Ok(response);
        }
        try {
            _ = gpdb.Tonics.Add(new() {
                TonicBrand = tonicBrand,
                TonicFlavour = tonicFlavour,
            });
            _ = gpdb.SaveChanges();

            response.StatusMessage = $"✅ Success! \"{tonicBrand} {tonicFlavour}\" tonic was added!";
            response.BsColor = BsColor.Success;

            return Ok(response);

        } catch (DbUpdateException ex) {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }

    [HttpPost("addPairing")]
    public IActionResult AddPairing([FromBody] JsonElement data) {
        var response = new ApiResponse();
        string? ginId = data.GetProperty("ginId").GetString();
        string? tonicId = data.GetProperty("tonicId").GetString();

        if (string.IsNullOrEmpty(ginId) || string.IsNullOrEmpty(tonicId)) {
            response.StatusMessage = "Please select the gin and tonic to pair";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }

        var pairedTonic = gpdb.Tonics.Find(int.Parse(tonicId));
        var pairedGin = gpdb.Gins.Find(int.Parse(ginId));

        if (pairedTonic == null || pairedGin == null) {
            response.StatusMessage = "Please select the gin and tonic to pair";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }

        if (IsPairingPresent(pairedGin.GinId, pairedTonic.TonicId)) {
            response.StatusMessage = $"\"{pairedGin.Distillery} {pairedGin.GinName}\" gin and \"{pairedTonic.TonicBrand} {pairedTonic.TonicFlavour}\" tonic are already paired!";
            response.BsColor = BsColor.Danger;
            return Ok(response);
        }

        try {
            _ = gpdb.Pairings.Add(new() {
                GinId = pairedGin.GinId,
                TonicId = pairedTonic.TonicId,
            });
            _ = gpdb.SaveChanges();

            response.StatusMessage = $"✅ Success! \"{pairedGin.Distillery} {pairedGin.GinName}\" gin and \"{pairedTonic.TonicBrand} {pairedTonic.TonicFlavour}\" tonic were paired!";
            response.BsColor = BsColor.Success;

            return Ok(response);

        } catch (DbUpdateException ex) {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }
    [HttpPost("deleteGin")]
    public IActionResult DeleteGin([FromBody] JsonElement data) {
        var response = new ApiResponse();
        string? ginId = data.GetProperty("ginId").GetString();
        if (string.IsNullOrEmpty(ginId) || ginId == "0") {
            response.StatusMessage = "Please select a gin to delete";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }
        try {
            var gn = gpdb.Gins.Find(int.Parse(ginId));
            if (gn == null) {
                response.StatusMessage = "Gin not found";
                response.BsColor = BsColor.Warning;
                return Ok(response);
            }
            string ginToBeDeleted = $"{gn.Distillery} {gn.GinName}";
            _ = gpdb.Gins.Remove(gn);
            _ = gpdb.SaveChanges();

            response.StatusMessage = $"✅ Success! \"{ginToBeDeleted}\" gin was removed!";
            response.BsColor = BsColor.Success;
            return Ok(response);
        } catch (DbUpdateException ex) {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }

    [HttpPost("deleteTonic")]
    public IActionResult DeleteTonic([FromBody] JsonElement data) {
        var response = new ApiResponse();
        string? tonicId = data.GetProperty("tonicId").GetString();
        if (string.IsNullOrEmpty(tonicId) || tonicId == "0") {
            response.StatusMessage = "Please select a tonic to delete";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }
        try {
            var tn = gpdb.Tonics.Find(int.Parse(tonicId));
            if (tn == null) {
                response.StatusMessage = "Tonic not found";
                response.BsColor = BsColor.Warning;
                return Ok(response);
            }
            string tonicToBeDeleted = $"{tn.TonicBrand} {tn.TonicFlavour}";
            _ = gpdb.Tonics.Remove(tn);
            _ = gpdb.SaveChanges();

            response.StatusMessage = $"✅ Success! \"{tonicToBeDeleted}\" tonic was removed!";
            response.BsColor = BsColor.Success;
            return Ok(response);
        } catch (DbUpdateException ex) {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }

    [HttpPost("deletePairing")]
    public IActionResult DeletePairing([FromBody] JsonElement data) {
        var response = new ApiResponse();
        string? pairingId = data.GetProperty("pairingId").GetString();
        if (string.IsNullOrEmpty(pairingId) || pairingId == "0") {
            response.StatusMessage = "Please select a pairing to delete";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }

        var pr = gpdb.Pairings.Find(int.Parse(pairingId));
        if (pr == null) {
            response.StatusMessage = "Pairing not found";
            response.BsColor = BsColor.Warning;
            return Ok(response);
        }

        var pairedGin = gpdb.Gins.Find(pr.GinId);
        var pairedTonic = gpdb.Tonics.Find(pr.TonicId);
        if (pairedGin == null || pairedTonic == null) {
            response.StatusMessage = "An Error occured: Gin or Tonic not found. Not able to delete pairing.";
            response.BsColor = BsColor.Danger;
            return Ok(response);
        }

        try {
            string pairingToBeDeleted = $"{pairedGin.Distillery} {pairedGin.GinName} gin and {pairedTonic.TonicBrand} {pairedTonic.TonicFlavour} tonic";
            _ = gpdb.Pairings.Remove(pr);
            _ = gpdb.SaveChanges();
            response.StatusMessage = $"✅ Success! \"{pairingToBeDeleted}\" pairing was removed!";
            response.BsColor = BsColor.Success;
            return Ok(response);
        } catch (DbUpdateException ex) {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }

    public bool IsGinPresent(string ginName, string distillery) {
        bool ginExists = gpdb.Gins.Any(
            m =>
            m.GinName != null &&
            m.GinName.ToLower() == ginName.ToLower() &&
            m.Distillery != null &&
            m.Distillery.ToLower() == distillery.ToLower());
        return ginExists;
    }
    public bool IsTonicPresent(string tonicBrand, string tonicFlavour) {
        bool tonicExists = gpdb.Tonics.Any(
            m =>
            m.TonicBrand != null &&
            m.TonicBrand.ToLower() == tonicBrand.ToLower() &&
            m.TonicFlavour != null &&
            m.TonicFlavour.ToLower() == tonicFlavour.ToLower());
        return tonicExists;
    }
    public bool IsPairingPresent(int ginId, int tonicId) {
        bool pairingExists = gpdb.Pairings.Any(m => m.GinId == ginId && m.TonicId == tonicId);
        return pairingExists;
    }
}