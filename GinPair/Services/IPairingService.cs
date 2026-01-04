namespace GinPair.Services;

public interface IPairingService {
    Task<ServiceResult<ApiResponse>> GetRandomPairingForGinAsync(int ginId);
    Task<ServiceResult> AddPairingAsync(int ginId, int tonicId);
    Task<ServiceResult> DeletePairingAsync(int pairingId);
    Task<bool> PairingExistsAsync(int ginId, int tonicId);
    Task<IEnumerable<SelectListItem>> GetPairingListAsync();
}
