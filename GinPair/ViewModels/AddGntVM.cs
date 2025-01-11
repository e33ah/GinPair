using System;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GinPair.Models;

public class AddGntVM
{
    public AddGntVM() { }
    public int GinId { get; set; }
    public string GinName {  get; set; }
    public string Distillery { get; set; }
    public string GinDescription { get; set; } = string.Empty;
    public IEnumerable<SelectListItem> Gins { get; set;}
    public Tonic Tonic { get; set; }
    public int TonicId { get; set; }
    public string TonicBrand { get; set; }
    public string TonicFlavour { get; set; }
    public IEnumerable<SelectListItem> TonicFlavours { get; set;}
    public bool AddPairingLater { get; set; }
    public bool IsGinInvalid { get; set; } 
    public bool IsTonicInvalid { get; set; } 

}
