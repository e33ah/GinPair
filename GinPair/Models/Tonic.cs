using System.ComponentModel.DataAnnotations;

namespace GinPair.Models
{
    public class Tonic
    {
        public int TonicId { get; set; }
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9'& ]*$")]
        public string TonicBrand { get; set; }
        [Required]
        [StringLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9'& ]*$")]
        public string TonicFlavour { get; set; }
        public ICollection<Pairing> Pairings { get; set; }
    }
}