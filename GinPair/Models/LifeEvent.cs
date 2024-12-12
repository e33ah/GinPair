namespace GinPair.Models
{
    public class LifeEvent
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public DateTime When { get; set; } = DateTime.UtcNow;
        public EventType Type { get; set; }
    }
}