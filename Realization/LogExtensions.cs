namespace Realization
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
        public static IEnumerable<T> Interleave<T>(
    this IEnumerable<T> first, IEnumerable<T> second)
        {
            using (var enumerator1 = first.GetEnumerator())
            using (var enumerator2 = second.GetEnumerator())
            {
                bool firstHasMore;
                bool secondHasMore;

                while ((firstHasMore = enumerator1.MoveNext())
                     | (secondHasMore = enumerator2.MoveNext()))
                {
                    if (firstHasMore)
                        yield return enumerator1.Current;

                    if (secondHasMore)
                        yield return enumerator2.Current;
                }
            }
        }
    }
}
