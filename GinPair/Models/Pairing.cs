namespace GinPair.Models
{
    public class Pairing
    {
        public int PairingId { get; set; }
        public Gin PairedGin { get; set; }
        public int GinId { get; internal set; }
        public Tonic PairedTonic { get; set; }
        public int TonicId { get; internal set; }
    }
}