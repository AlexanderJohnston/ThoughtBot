namespace Realization.Converse
{

    internal static class Personalities
    {
        public static Personality Default() => new()
        {
            Name = "Default",
            Description = "Reliable configuration the bot is designed to work best in.",
            Traits = new() {
                new() { Name = "Sleeping", Resistance = 1.0f },
                new() { Name = "Awake", Resistance = 1.0f },
                new() { Name = "Listening", Resistance = 1.0f },
                new() { Name = "Learning", Resistance = 1.0f },
                new() { Name = "Responding", Resistance = 1.0f },
                new() { Name = "Reflecting", Resistance = 1.0f }
            }
        };
        public static Personality Eager() => new()
        {
            Name = "Eager",
            Description = "Eager configuration focuses more on conversation and less on learning.",
            Traits = new() {
                new() { Name = "Sleeping", Resistance = 1.0f },
                new() { Name = "Awake", Resistance = 1.0f },
                new() { Name = "Listening", Resistance = 1.0f },
                new() { Name = "Learning", Resistance = 0.1f },
                new() { Name = "Responding", Resistance = 1.0f },
                new() { Name = "Reflecting", Resistance = 0.1f }
            }
        };
        public static Personality Strict() => new()
        {
            Name = "Strict",
            Description = "Strict configuration provides more consistent responses and does not train.",
            Traits = new() {
                new() { Name = "Sleeping", Resistance = 1.0f },
                new() { Name = "Awake", Resistance = 1.0f },
                new() { Name = "Listening", Resistance = 1.0f },
                new() { Name = "Learning", Resistance = 0.0f },
                new() { Name = "Responding", Resistance = 1.0f },
                new() { Name = "Reflecting", Resistance = 0.1f }
            }
        };
        public static Personality Playful() => new()
        {
            Name = "Playful",
            Description = "Playful configuration provides less consistent responses and spends more time reflecting.",
            Traits = new() {
                new() { Name = "Sleeping", Resistance = 1.0f },
                new() { Name = "Awake", Resistance = 1.0f },
                new() { Name = "Listening", Resistance = 1.0f },
                new() { Name = "Learning", Resistance = 0.1f },
                new() { Name = "Responding", Resistance = 1.0f },
                new() { Name = "Reflecting", Resistance = 1.0f }
            }
        };
    }

    internal class Personality
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<PersonalityTrait> Traits { get; set; }
    }

    internal class Mood
    {
        private PersonalityTrait _currentMood;
        public Mood(PersonalityTrait currentMood)
        {
            _currentMood = currentMood;
        }

        /// <summary>
        /// Increase or decrease the resistance left on current mood.
        /// </summary>
        public void Shift(float amount) => _currentMood.Resistance += amount;

        /// <summary>
        /// Decide if there is enough resistance left to maintain current mood.
        /// </summary>
        public bool StableMood() => _currentMood.Resistance > 0f;
    }

    /// <summary>
    /// Represents a personality trait for the bot.
    /// </summary>
    internal class PersonalityTrait
    {
        /// <summary>
        /// The mood representing this trait, e.g. "Happy", "Bored".
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// How strong this trait is expressed. The more resistance, the harder to change moods.
        /// </summary>
        public float Resistance { get; set; }
    }
}