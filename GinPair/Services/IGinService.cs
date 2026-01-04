namespace GinPair.Services;

public interface IGinService {
    Task<IEnumerable<Gin>> SearchGinsAsync(string searchTerm);
    Task<ServiceResult> AddGinAsync(string ginName, string distillery, string? description);
    Task<ServiceResult> DeleteGinAsync(int ginId);
    Task<Gin?> GetGinByIdAsync(int ginId);
    Task<bool> GinExistsAsync(string ginName, string distillery);
    Task<IEnumerable<SelectListItem>> GetGinListAsync();
    Task<bool> HasAnyGinsAsync();
}
