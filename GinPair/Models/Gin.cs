using System.ComponentModel.DataAnnotations;

namespace GinPair.Models;

public class Gin {
    public int GinId { get; set; }
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9'& ]*$")]
    public string? GinName { get; set; }
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9'& ]*$")]
    public string? Distillery { get; set; }
    [StringLength(500)]
    public string? GinDescription { get; set; }

    public ICollection<Pairing>? Pairings { get; set; }
}