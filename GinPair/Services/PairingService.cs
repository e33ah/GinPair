#pragma warning disable CA1873 
namespace GinPair.Services;

public class PairingService(GinPairDbContext context, IGinService ginService, ITonicService tonicService, ILogger<PairingService> logger) : IPairingService {
    private readonly GinPairDbContext gpdb = context;
    private readonly IGinService ginService = ginService;
    private readonly ITonicService tonicService = tonicService;
    private readonly ILogger<PairingService> logger = logger;
    private static readonly Random random = Random.Shared;

    public async Task<ServiceResult<ApiResponse>> GetRandomPairingForGinAsync(int ginId) {
        var gin = await ginService.GetGinByIdAsync(ginId);

        if (gin == null) {
            logger.LogWarning("GetPairingByGinId called with invalid GinId: {GinId}", ginId);
            return ServiceResult<ApiResponse>.Failure("Gin not found");
        }

        var pairings = await (from tonic in gpdb.Tonics
                              join pairing in gpdb.Pairings on tonic.TonicId equals pairing.TonicId
                              join g in gpdb.Gins on pairing.GinId equals g.GinId
                              where g.GinId == ginId
                              select new {
                                  tonic.TonicBrand,
                                  tonic.TonicFlavour
                              }).ToListAsync();

        var response = new ApiResponse();

        if (pairings.Count == 0) {
            logger.LogInformation("No pairings found for gin: {Distillery} {GinName} (GinId: {GinId})", gin.Distillery, gin.GinName, ginId);
            response.StatusMessage = $"<p>Sorry, there is no pairing available for \"{gin.Distillery} {gin.GinName}\".<br>Try searching again, or <a href='/Home/AddGnt/'>add it</a> to our collection.</p>";
            response.BsColor = BsColor.Warning;
        } else {
            int randomIndex = random.Next(0, pairings.Count);
            var selectedPairing = pairings[randomIndex];

            logger.LogInformation("Pairing suggested for {Distillery} {GinName}: {TonicBrand} {TonicFlavour} (selected from {PairingCount} options)",
                gin.Distillery, gin.GinName, selectedPairing.TonicBrand, selectedPairing.TonicFlavour, pairings.Count);

            response.StatusMessage = $"Try pairing <b>{gin.Distillery} {gin.GinName}</b> gin with<br>a <b>{selectedPairing.TonicBrand} {selectedPairing.TonicFlavour}</b> tonic! 🍋🍋‍🟩";
            response.BsColor = BsColor.Primary;
        }

        return ServiceResult<ApiResponse>.Success("Pairing found", response);
    }

    public async Task<ServiceResult> AddPairingAsync(int ginId, int tonicId) {
        var gin = await ginService.GetGinByIdAsync(ginId);
        var tonic = await tonicService.GetTonicByIdAsync(tonicId);

        if (gin == null || tonic == null) {
            logger.LogWarning("AddPairing validation failed: gin or tonic not found. GinId: {GinId}, TonicId: {TonicId}", ginId, tonicId);
            return ServiceResult.Warning("Please select the gin and tonic to pair");
        }

        if (await PairingExistsAsync(ginId, tonicId)) {
            logger.LogWarning("AddPairing validation failed: duplicate pairing. {Distillery} {GinName} with {TonicBrand} {TonicFlavour} already exists",
                gin.Distillery, gin.GinName, tonic.TonicBrand, tonic.TonicFlavour);
            return ServiceResult.Failure($"\"{gin.Distillery} {gin.GinName}\" gin and \"{tonic.TonicBrand} {tonic.TonicFlavour}\" tonic are already paired!");
        }

        try {
            var pairing = new Pairing {
                GinId = ginId,
                TonicId = tonicId,
            };

            _ = gpdb.Pairings.Add(pairing);
            _ = await gpdb.SaveChangesAsync();

            logger.LogInformation("Pairing added successfully: {Distillery} {GinName} with {TonicBrand} {TonicFlavour}",
                gin.Distillery, gin.GinName, tonic.TonicBrand, tonic.TonicFlavour);
            return ServiceResult.Success($"✅ Success! \"{gin.Distillery} {gin.GinName}\" gin and \"{tonic.TonicBrand} {tonic.TonicFlavour}\" tonic were paired!");
        } catch (DbUpdateException ex) {
            logger.LogError(ex, "Failed to add pairing: GinId={GinId}, TonicId={TonicId}", ginId, tonicId);
            throw;
        }
    }

    public async Task<ServiceResult> DeletePairingAsync(int pairingId) {
        var pairing = await gpdb.Pairings.FindAsync(pairingId);

        if (pairing == null) {
            logger.LogWarning("DeletePairing validation failed: pairing not found. PairingId: {PairingId}", pairingId);
            return ServiceResult.Warning("Pairing not found");
        }

        var gin = await ginService.GetGinByIdAsync(pairing.GinId);
        var tonic = await tonicService.GetTonicByIdAsync(pairing.TonicId);

        if (gin == null || tonic == null) {
            logger.LogWarning("DeletePairing validation failed: gin or tonic not found for pairing. PairingId: {PairingId}, GinId: {GinId}, TonicId: {TonicId}",
                pairingId, pairing.GinId, pairing.TonicId);
            return ServiceResult.Failure("An Error occurred: Gin or Tonic not found. Not able to delete pairing.");
        }

        try {
            string pairingToBeDeleted = $"{gin.Distillery} {gin.GinName} gin and {tonic.TonicBrand} {tonic.TonicFlavour} tonic";
            _ = gpdb.Pairings.Remove(pairing);
            _ = await gpdb.SaveChangesAsync();

            logger.LogInformation("Pairing deleted successfully: {Distillery} {GinName} with {TonicBrand} {TonicFlavour}",
                gin.Distillery, gin.GinName, tonic.TonicBrand, tonic.TonicFlavour);
            return ServiceResult.Success($"✅ Success! \"{pairingToBeDeleted}\" pairing was removed!");
        } catch (DbUpdateException ex) {
            logger.LogError(ex, "Failed to delete pairing: PairingId={PairingId}", pairingId);
            throw;
        }
    }

    public async Task<bool> PairingExistsAsync(int ginId, int tonicId) {
        return await gpdb.Pairings.AnyAsync(m => m.GinId == ginId && m.TonicId == tonicId);
    }

    public async Task<IEnumerable<SelectListItem>> GetPairingListAsync() {
        var result = await gpdb.Pairings
            .Where(p => p.PairedGin != null && p.PairedTonic != null)
            .Select(p => new SelectListItem {
                Value = p.PairingId.ToString(),
                Text = $"{p.PairedGin!.Distillery} {p.PairedGin.GinName} gin and {p.PairedTonic!.TonicBrand} {p.PairedTonic.TonicFlavour} tonic"
            })
            .ToListAsync();

        return result.OrderBy(o => o.Text);
    }
}
