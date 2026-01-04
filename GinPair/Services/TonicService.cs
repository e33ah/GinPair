#pragma warning disable CA1873 
namespace GinPair.Services;

public class TonicService(GinPairDbContext context, ILogger<TonicService> logger) : ITonicService {
    private readonly GinPairDbContext gpdb = context;
    private readonly ILogger<TonicService> logger = logger;

    public async Task<ServiceResult> AddTonicAsync(string tonicBrand, string tonicFlavour) {
        if (await TonicExistsAsync(tonicBrand, tonicFlavour)) {
            logger.LogWarning("AddTonic validation failed: duplicate tonic. {TonicBrand} {TonicFlavour} already exists", tonicBrand, tonicFlavour);
            return ServiceResult.Failure("Sorry this tonic cannot be added as it is already part of our collection!");
        }

        try {
            var tonic = new Tonic {
                TonicBrand = tonicBrand,
                TonicFlavour = tonicFlavour,
            };

            _ = gpdb.Tonics.Add(tonic);
            _ = await gpdb.SaveChangesAsync();

            logger.LogInformation("Tonic added successfully: {TonicBrand} {TonicFlavour}", tonicBrand, tonicFlavour);
            return ServiceResult.Success($"✅ Success! \"{tonicBrand} {tonicFlavour}\" tonic was added!");
        } catch (DbUpdateException ex) {
            logger.LogError(ex, "Failed to add tonic: {TonicBrand} {TonicFlavour}", tonicBrand, tonicFlavour);
            throw;
        }
    }

    public async Task<ServiceResult> DeleteTonicAsync(int tonicId) {
        var tonic = await gpdb.Tonics.FindAsync(tonicId);

        if (tonic == null) {
            logger.LogWarning("DeleteTonic validation failed: tonic not found. TonicId: {TonicId}", tonicId);
            return ServiceResult.Warning("Tonic not found");
        }

        try {
            string tonicToBeDeleted = $"{tonic.TonicBrand} {tonic.TonicFlavour}";
            _ = gpdb.Tonics.Remove(tonic);
            _ = await gpdb.SaveChangesAsync();

            logger.LogInformation("Tonic deleted successfully: {TonicBrand} {TonicFlavour}", tonic.TonicBrand, tonic.TonicFlavour);
            return ServiceResult.Success($"✅ Success! \"{tonicToBeDeleted}\" tonic was removed!");
        } catch (DbUpdateException ex) {
            logger.LogError(ex, "Failed to delete tonic: TonicId={TonicId}", tonicId);
            throw;
        }
    }

    public async Task<Tonic?> GetTonicByIdAsync(int tonicId) {
        return await gpdb.Tonics.FindAsync(tonicId);
    }

    public async Task<bool> TonicExistsAsync(string tonicBrand, string tonicFlavour) {
        return await gpdb.Tonics.AnyAsync(
            m =>
            m.TonicBrand != null &&
            m.TonicBrand.ToLower() == tonicBrand.ToLower() &&
            m.TonicFlavour != null &&
            m.TonicFlavour.ToLower() == tonicFlavour.ToLower());
    }

    public async Task<IEnumerable<SelectListItem>> GetTonicListAsync() {
        var result = await gpdb.Tonics
            .Select(t => new SelectListItem {
                Value = t.TonicId.ToString(),
                Text = $"{t.TonicBrand} {t.TonicFlavour}"
            })
            .ToListAsync();

        return result.OrderBy(o => o.Text);
    }
}
