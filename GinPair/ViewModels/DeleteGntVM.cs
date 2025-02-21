namespace GinPair.Models;

public class DeleteGntVM {
    public int GinId { get; set; }
    public string? GinToBeDeleted { get; set; }
    public IEnumerable<SelectListItem>? Gins { get; set; }
    public IEnumerable<SelectListItem>? Tonics { get; set; }
    public int TonicId { get; set; }
    public string? TonicToBeDeleted { get; set; }
    public string? Message { get; internal set; }
}
