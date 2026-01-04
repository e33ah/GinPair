#pragma warning disable CA1873

namespace GinPair.Services;

public class GinService(GinPairDbContext context, ILogger<GinService> logger) : IGinService {
    private readonly GinPairDbContext gpdb = context;
    private readonly ILogger<GinService> logger = logger;

    public async Task<bool> HasAnyGinsAsync() {
        return await gpdb.Gins.AnyAsync();
    }

    public async Task<IEnumerable<Gin>> SearchGinsAsync(string searchTerm) {
        logger.LogInformation("Searching for gin with partial match: {SearchTerm}", searchTerm);

        var results = await gpdb.Gins
            .Where(s => s.GinName!.ToUpper().Contains(searchTerm.ToUpper()) ||
                       s.Distillery!.ToUpper().Contains(searchTerm.ToUpper()))
            .OrderBy(s => s.Distillery)
            .ThenBy(s => s.GinName)
            .Take(10)
            .ToListAsync();

        if (results.Count == 0) {
            logger.LogInformation("No results found for search term: {SearchTerm}", searchTerm);
        } else {
            logger.LogInformation("Found {ResultCount} gin(s) matching search term: {SearchTerm}", results.Count, searchTerm);
        }

        return results;
    }

    public async Task<ServiceResult> AddGinAsync(string ginName, string distillery, string? description) {
        if (await GinExistsAsync(ginName, distillery)) {
            logger.LogWarning("AddGin validation failed: duplicate gin. {Distillery} {GinName} already exists", distillery, ginName);
            return ServiceResult.Failure("Sorry this gin cannot be added as it is already part of our collection!");
        }

        try {
            var gin = new Gin {
                GinName = ginName,
                Distillery = distillery,
                GinDescription = description
            };

            _ = gpdb.Gins.Add(gin);
            _ = await gpdb.SaveChangesAsync();

            logger.LogInformation("Gin added successfully: {Distillery} {GinName}", distillery, ginName);
            return ServiceResult.Success($"✅ Success! \"{distillery} {ginName}\" gin was added!");
        } catch (DbUpdateException ex) {
            logger.LogError(ex, "Failed to add gin: {Distillery} {GinName}", distillery, ginName);
            throw;
        }
    }

    public async Task<ServiceResult> DeleteGinAsync(int ginId) {
        var gin = await gpdb.Gins.FindAsync(ginId);

        if (gin == null) {
            logger.LogWarning("DeleteGin validation failed: gin not found. GinId: {GinId}", ginId);
            return ServiceResult.Warning("Gin not found");
        }

        try {
            string ginToBeDeleted = $"{gin.Distillery} {gin.GinName}";
            _ = gpdb.Gins.Remove(gin);
            _ = await gpdb.SaveChangesAsync();

            logger.LogInformation("Gin deleted successfully: {Distillery} {GinName}", gin.Distillery, gin.GinName);
            return ServiceResult.Success($"✅ Success! \"{ginToBeDeleted}\" gin was removed!");
        } catch (DbUpdateException ex) {
            logger.LogError(ex, "Failed to delete gin: GinId={GinId}", ginId);
            throw;
        }
    }

    public async Task<Gin?> GetGinByIdAsync(int ginId) {
        return await gpdb.Gins.FindAsync(ginId);
    }

    public async Task<bool> GinExistsAsync(string ginName, string distillery) {
        return await gpdb.Gins.AnyAsync(
            m =>
            m.GinName != null &&
            m.GinName.ToLower() == ginName.ToLower() &&
            m.Distillery != null &&
            m.Distillery.ToLower() == distillery.ToLower());
    }

    public async Task<IEnumerable<SelectListItem>> GetGinListAsync() {
        var result = await gpdb.Gins
            .Select(g => new SelectListItem {
                Value = g.GinId.ToString(),
                Text = $"{g.Distillery} {g.GinName}"
            })
            .ToListAsync();

        return result.OrderBy(o => o.Text);
    }
}
