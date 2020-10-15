using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ThotBot.Services;

namespace ThotBot.Modules
{
    [Group("schedule")]
    public class ScheduleModule : ModuleBase<SocketCommandContext>
    {
        private SchedulerService _scheduler;
        private CultureInfo _ci = new CultureInfo("en-US");
        private string[] _dateFormats;
        private string[] _timeFormats;

        public ScheduleModule(SchedulerService scheduler)
        {
            _scheduler = scheduler;
            _dateFormats = new[] { "M-d-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "M.d.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "MMDDYYYY" }
                    .Union(_ci.DateTimeFormat.GetAllDateTimePatterns()).ToArray();
            _timeFormats = new[] { "h:mmtt", "hh:mmtt", "h:mm tt", "h:mm:ss tt", "hh:mm tt", "hh:mm:ss tt" };
        }

        [Command("event")]
        [Summary("Schedule an event.")]
        public async Task ScheduleEventAsync(string arg)
        {
            var args = arg.Split(' ');
            if (!await ValidateArgsLength(args, 3, "!schedule event \"map-night 12/30/2019 7:30PM\""))
                return;

            var name = args[0];
            var startDate = args[1];
            var startTime = args[2];
            var date = DateTime.ParseExact(startDate, _dateFormats, _ci, DateTimeStyles.AssumeLocal);
            var time = DateTime.ParseExact(startTime, _timeFormats, CultureInfo.InvariantCulture);

            await _scheduler.Schedule(name, date, time, Context.User.Username);
            await ReplyAsync(string.Format("Scheduled {0} for {1} at {2}.", name, date, time));
        }

        [Command("title")]
        [Summary("Set the event name.")]
        public async Task SetTitleAsync(string name)
        {
            var eventId = _scheduler.SetTitle(name, Context.User.Username);
            await ReplyAsync(string.Format("Set {0} as the title of the event called {1}.", name, eventId));
        }

        private async Task<bool> ValidateArgsLength(string[] args, int expected, string expectedFormat)
        {
             if (args.Length != expected)
            {
                await ReplyAsync(string.Format(
                    "The wrong number of arguments were given. Expected {0}. Expected format:{1}{2}",
                    expected,
                    Environment.NewLine,
                    expectedFormat));
                return false;
            }
            return true;
        }
    }
}
