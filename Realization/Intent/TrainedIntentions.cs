using Realization.Learning;
using ThotLibrary.Cognitive;

namespace Realization.Intent
{
    public class TrainedIntentions
    {
        public long Id;
        public string Text;
        public string[] TokenizedText;
        public string IntentLabel;
        public LearnedEntity[] EntityLabels;
        public IntentPrediction[] IntentPredictions;
        public EntityPrediction[] EntityPredictions;
    }
}
