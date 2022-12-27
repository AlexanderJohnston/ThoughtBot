namespace Realization.Services
{
    public class SchedulerService
    {
        private ILogger _log = Log.ForContext<SchedulerService>();
        private Dictionary<string, List<ScheduledEvent>> Events;

        public SchedulerService()
        {
            Events = new Dictionary<string, List<ScheduledEvent>>();
        }

        public Task Schedule(string name, DateTime date, DateTime time, string userName)
        {
            var schedule = new ScheduledEvent(name, date, time, userName);
            Track(schedule, userName);
            _log.Information("Scheduled a task.");
            return Task.CompletedTask;
        }

        private bool Exists(string userName) => Events.Any(scheduled => scheduled.Key == userName);
        private List<ScheduledEvent> OwnedBy(string userName) => Events.First(scheduled => scheduled.Key == userName).Value;
        private List<ScheduledEvent> Lookup(string userName)
        {
            List<ScheduledEvent> events;
            if (Exists(userName))
                events = OwnedBy(userName);
            else
            {
                Events.Add(userName, new List<ScheduledEvent>());
                events = OwnedBy(userName);
            }
            return events;
        }
        private void Track(ScheduledEvent schedule, string userName)
        {
            var events = Lookup(userName);
            events.Add(schedule);
        }

        public string SetTitle(string eventName, string userName)
        {
            var lastEvent = Lookup(userName).LastOrDefault();
            lastEvent.Name = eventName;
            return lastEvent.Id;
        }
    }
}
