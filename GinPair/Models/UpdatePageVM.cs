namespace GinPair.Models
{
    public class UpdatePageVM
    {
        public string SomeData { get; set; }
        public string ReleaseName { get; set; }
        public List<LifeEvent> LifeEvents { get; set; } = new List<LifeEvent>();
        public List<Gin> Gins { get; set; } = new List<Gin>();
    }
}
