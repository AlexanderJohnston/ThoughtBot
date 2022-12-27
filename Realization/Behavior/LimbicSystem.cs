namespace Realization.Behavior
{
    // Emotion System
    public class Emotion : IEquatable<Emotion>
    {
        public string Name { get; set; }
        public string Cause { get; set; }

        public bool Equals(Emotion? other) => other.Name == Name && other.Cause == Cause;
    }
    public class EmotionalContext
    {
        public Emotion Emotion { get; set; }
        public List<string> Memories { get; set; }
        public EmotionalContext(Emotion emotion, List<string> memories)
        {
            Emotion = emotion;
            Memories = memories;
        }
    }
    public class LimbicSystem
    {
        public Dictionary<Emotion, EmotionalContext> EmotionalStates { get; set; }
        public Emotion CurrentEmotion { get; set; }

        public void UpdateEmotionalState(Emotion emotion, EmotionalContext context)
        {
            if (EmotionalStates.ContainsKey(emotion))
            {
                EmotionalStates[emotion] = context;
            }
            else
            {
                EmotionalStates.Add(emotion, context);
            }
            CurrentEmotion = emotion;
        }
    }
}