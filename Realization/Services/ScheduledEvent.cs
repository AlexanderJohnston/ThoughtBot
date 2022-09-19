using System;

namespace Realization.Services
{
    public class ScheduledEvent
    {
        public string Id;
        public string Name;
        public DateTime Date;
        public DateTime Time;
        public string Creator;


        public ScheduledEvent(string id, DateTime date, DateTime time, string creator)
        {
            this.Id = id;
            this.Date = date;
            this.Time = time;
            this.Creator = creator;
        }
    }
}
