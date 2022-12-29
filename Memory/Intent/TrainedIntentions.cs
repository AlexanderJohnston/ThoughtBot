using Memory.Learning;
using ThotLibrary.Cognitive;

namespace Memory.Intent
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
