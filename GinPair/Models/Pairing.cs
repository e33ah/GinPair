namespace GinPair.Models
{
    public class Pairing
    {
        public int PairingId { get; set; }
        public required Gin PairedGin { get; set; }
        public required Tonic PairedTonic { get; set; }
    }
}