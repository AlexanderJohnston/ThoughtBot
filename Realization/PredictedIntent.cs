using ThotLibrary;

namespace Realization
{
    public class PredictedIntent
    {
        public PredictedIntent(Intention intent, string data)
        {
            Intent = intent;
            Data = data;
        }

        public Intention Intent { get; set; }
        public string Data { get; set; }
    }
}
