using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace ThotBot
{
    public static class LogExtensions
    {
        public const string Template =
            "{Timestamp:yyyy-MM-dd HH:mm:ss} |{Level:u3}: [Thread:{ThreadId}|{SourceContext}]{Indent:l} {Message:lj}{NewLine}{Exception}";
        
        public static ILogger VerboseLogger()
        {
            var config = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .Enrich.WithThreadId()
                        .WriteTo.Console(outputTemplate: Template, theme: ConsoleExtensions.BlueConsole);
            return config.CreateLogger();
        }
    }
}
