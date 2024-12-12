
namespace GinPair.Models
{
    public class AddEventVM
    {
        public AddEventVM()
        { }
        public string EventName {
            get; set;
        }
        public EventType Type { get; internal set; }
        public DateTime When { get; internal set; }

        public bool isValid() {
            if (EventName == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}