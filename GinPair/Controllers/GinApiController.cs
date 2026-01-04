#pragma warning disable CA1873 // Suppress warning regarding logging performance
namespace GinPair.Controllers;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(DatabaseReadyFilter))]
public class GinApiController(
    IGinService ginService,
    ITonicService tonicService,
    IPairingService pairingService,
    ILogger<GinApiController> logger) : ControllerBase {

    private readonly IGinService ginService = ginService;
    private readonly ITonicService tonicService = tonicService;
    private readonly IPairingService pairingService = pairingService;
    private readonly ILogger<GinApiController> logger = logger;

    [HttpGet("matchGin")]
    public async Task<IActionResult> MatchGin(string partial) {
        if (!await ginService.HasAnyGinsAsync()) {
            logger.LogWarning("MatchGin called but database contains no gins");
            return BadRequest();
        }

        var results = await ginService.SearchGinsAsync(partial);
        return Ok(results);
    }

    [HttpGet("getPairing/{ginId}")]
    public async Task<IActionResult> GetPairingByGinId(int ginId) {
        var result = await pairingService.GetRandomPairingForGinAsync(ginId);

        if (!result.IsSuccess) {
            return BadRequest();
        }

        return Ok(result.Data);
    }

    [HttpGet("getGinList")]
    public async Task<IActionResult> GetGinList() {
        var ginList = await ginService.GetGinListAsync();
        return Ok(ginList);
    }

    [HttpGet("getTonicList")]
    public async Task<IActionResult> GetTonicList() {
        var tonicList = await tonicService.GetTonicListAsync();
        return Ok(tonicList);
    }

    [HttpGet("getPairingList")]
    public async Task<IActionResult> GetPairingList() {
        var pairingList = await pairingService.GetPairingListAsync();
        return Ok(pairingList);
    }

    [HttpPost("addGin")]
    public async Task<IActionResult> AddGin([FromBody] JsonElement data) {
        string? ginName = data.GetProperty("ginName").GetString();
        string? distillery = data.GetProperty("distillery").GetString();
        string? description = data.GetProperty("description").GetString();

        if (string.IsNullOrEmpty(ginName) || string.IsNullOrEmpty(distillery)) {
            logger.LogWarning("AddGin validation failed: missing required fields. GinName: {GinName}, Distillery: {Distillery}",
                ginName ?? "(null)", distillery ?? "(null)");
            return Ok(new ApiResponse {
                StatusMessage = "Please provide the name of the Distillery and Gin.",
                BsColor = BsColor.Warning
            });
        }

        try {
            var result = await ginService.AddGinAsync(ginName, distillery, description);
            return Ok(new ApiResponse {
                StatusMessage = result.Message,
                BsColor = result.BsColor
            });
        } catch (DbUpdateException) {
            return BadRequest();
        }
    }

    [HttpPost("addTonic")]
    public async Task<IActionResult> AddTonic([FromBody] JsonElement data) {
        string? tonicBrand = data.GetProperty("tonicBrand").GetString();
        string? tonicFlavour = data.GetProperty("tonicFlavour").GetString();

        if (string.IsNullOrEmpty(tonicBrand) || string.IsNullOrEmpty(tonicFlavour)) {
            logger.LogWarning("AddTonic validation failed: missing required fields. TonicBrand: {TonicBrand}, TonicFlavour: {TonicFlavour}",
                tonicBrand ?? "(null)", tonicFlavour ?? "(null)");
            return Ok(new ApiResponse {
                StatusMessage = "Please provide the tonic brand and flavour/name.",
                BsColor = BsColor.Warning
            });
        }

        try {
            var result = await tonicService.AddTonicAsync(tonicBrand, tonicFlavour);
            return Ok(new ApiResponse {
                StatusMessage = result.Message,
                BsColor = result.BsColor
            });
        } catch (DbUpdateException) {
            return BadRequest();
        }
    }

    [HttpPost("addPairing")]
    public async Task<IActionResult> AddPairing([FromBody] JsonElement data) {
        string? ginId = data.GetProperty("ginId").GetString();
        string? tonicId = data.GetProperty("tonicId").GetString();

        if (string.IsNullOrEmpty(ginId) || string.IsNullOrEmpty(tonicId)) {
            logger.LogWarning("AddPairing validation failed: missing required fields. GinId: {GinId}, TonicId: {TonicId}",
                ginId ?? "(null)", tonicId ?? "(null)");
            return Ok(new ApiResponse {
                StatusMessage = "Please select the gin and tonic to pair",
                BsColor = BsColor.Warning
            });
        }

        try {
            var result = await pairingService.AddPairingAsync(int.Parse(ginId), int.Parse(tonicId));
            return Ok(new ApiResponse {
                StatusMessage = result.Message,
                BsColor = result.BsColor
            });
        } catch (DbUpdateException) {
            return BadRequest();
        }
    }

    [HttpPost("deleteGin")]
    public async Task<IActionResult> DeleteGin([FromBody] JsonElement data) {
        string? ginId = data.GetProperty("ginId").GetString();

        if (string.IsNullOrEmpty(ginId) || ginId == "0") {
            logger.LogWarning("DeleteGin validation failed: invalid GinId: {GinId}", ginId ?? "(null)");
            return Ok(new ApiResponse {
                StatusMessage = "Please select a gin to delete",
                BsColor = BsColor.Warning
            });
        }

        try {
            var result = await ginService.DeleteGinAsync(int.Parse(ginId));
            return Ok(new ApiResponse {
                StatusMessage = result.Message,
                BsColor = result.BsColor
            });
        } catch (DbUpdateException) {
            return BadRequest();
        }
    }

    [HttpPost("deleteTonic")]
    public async Task<IActionResult> DeleteTonic([FromBody] JsonElement data) {
        string? tonicId = data.GetProperty("tonicId").GetString();

        if (string.IsNullOrEmpty(tonicId) || tonicId == "0") {
            logger.LogWarning("DeleteTonic validation failed: invalid TonicId: {TonicId}", tonicId ?? "(null)");
            return Ok(new ApiResponse {
                StatusMessage = "Please select a tonic to delete",
                BsColor = BsColor.Warning
            });
        }

        try {
            var result = await tonicService.DeleteTonicAsync(int.Parse(tonicId));
            return Ok(new ApiResponse {
                StatusMessage = result.Message,
                BsColor = result.BsColor
            });
        } catch (DbUpdateException) {
            return BadRequest();
        }
    }

    [HttpPost("deletePairing")]
    public async Task<IActionResult> DeletePairing([FromBody] JsonElement data) {
        string? pairingId = data.GetProperty("pairingId").GetString();

        if (string.IsNullOrEmpty(pairingId) || pairingId == "0") {
            logger.LogWarning("DeletePairing validation failed: invalid PairingId: {PairingId}", pairingId ?? "(null)");
            return Ok(new ApiResponse {
                StatusMessage = "Please select a pairing to delete",
                BsColor = BsColor.Warning
            });
        }

        try {
            var result = await pairingService.DeletePairingAsync(int.Parse(pairingId));
            return Ok(new ApiResponse {
                StatusMessage = result.Message,
                BsColor = result.BsColor
            });
        } catch (DbUpdateException) {
            return BadRequest();
        }
    }
}