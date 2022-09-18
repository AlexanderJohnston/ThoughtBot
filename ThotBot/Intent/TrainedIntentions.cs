using System;
using System.Collections.Generic;
using System.Text;
using ThotBot.Learning;
using ThotBot.Libray.Cognitive;
using ThotLibrary;

namespace ThotBot.Intent
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
