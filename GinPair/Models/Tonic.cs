namespace GinPair.Models
{
    public class Tonic
    {
        public int TonicId { get; set; }
        public string TonicBrand { get; set; }
        public string TonicFlavour { get; set; }
        public ICollection<Pairing> Pairings { get; set; }

    }
}