namespace Memory
{
    // Represents an emotion by name and a way to remember the cause of the emotion.
    public class Emotion : IEquatable<Emotion>
    {
        public string Name { get; set; }
        public string Cause { get; set; }

        public bool Equals(Emotion? other) => other.Name == Name && other.Cause == Cause;
    }
}