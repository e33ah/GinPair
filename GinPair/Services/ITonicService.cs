namespace GinPair.Services;

public interface ITonicService {
    Task<ServiceResult> AddTonicAsync(string tonicBrand, string tonicFlavour);
    Task<ServiceResult> DeleteTonicAsync(int tonicId);
    Task<Tonic?> GetTonicByIdAsync(int tonicId);
    Task<bool> TonicExistsAsync(string tonicBrand, string tonicFlavour);
    Task<IEnumerable<SelectListItem>> GetTonicListAsync();
}
