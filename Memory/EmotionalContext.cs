namespace Memory
{
    // Represents an emotion and the context of memories where it was felt.
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
}