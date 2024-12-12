using System;
namespace GinPair.Models
{
    public class AddGntVM
    {
        public AddGntVM() { }
        public string GinName {  get; set; }
        public string Distillery { get; set; } = string.Empty;
        public string GinDescription { get; set; } = string.Empty;
        public int TonicId { get; set; }
        public string? TonicBrand { get; set; }
        public string TonicFlavour { get; set; }
    }
}
