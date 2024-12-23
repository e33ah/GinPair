namespace GinPair.Models
{
    internal class PairingVM
    {
        public string GinName { get; set; }
        public string Distillery { get; set; }
        public string TonicBrand { get; set; }
        public string TonicFlavour { get; set; }
        public string Message { get; internal set; }

    }
}