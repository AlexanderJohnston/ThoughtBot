namespace Memory
{
    /// <summary>
    /// The Limbic System tracks the current emotional state as well as the
    /// <see cref="EmotionalContext"/> for any known <see cref="Emotion"/>.
    /// </summary>
    public class LimbicSystem
    {
        public Dictionary<Emotion, EmotionalContext> EmotionalStates { get; set; } = new();
        public Emotion? CurrentEmotion { get; set; }

        /// <summary>
        /// Looks up an <see cref="EmotionalContext"/> by a given <see cref="Emotion"/>.
        /// If the emotion is already known, new memories are adding to the existing context
        /// and then it is updated in the EmotionalStates dictionary. Otherwise a new emotion is created.
        /// Current emotion is always tracked.
        /// </summary>
        /// <param name="emotion">The <see cref="Emotion"/> being known.</param>
        /// <param name="context" type="EmotionalContext">The context the <see cref="Emotion"/> is known in.</param>
        public void KnowEmotion(Emotion emotion, EmotionalContext context)
        {
            if (EmotionalStates.ContainsKey(emotion))
            {
                EmotionalStates[emotion].Memories.AddRange(context.Memories);
            }
            else
            {
                EmotionalStates.Add(emotion, context);
            }
            CurrentEmotion = emotion;
        }

        /// <summary>
        ///  Looks up an <see cref="EmotionalContext"/> by a given <see cref="Emotion"/>.
        /// </summary>
        /// <param name="emotion" type="Emotion">A given emotion to find associated memories.</param>
        /// <returns type="EmotionalContext">The context the <see cref="Emotion"/> is known in.</returns>
        public EmotionalContext? FindEmotion(Emotion emotion)
        {
            if (EmotionalStates.ContainsKey(emotion))
            {
                return EmotionalStates[emotion];
            }
            return null;
        }

        /// <summary>
        /// Gets the <see cref="EmotionalContext"/> for the current <see cref="Emotion"/>.
        /// </summary>
        /// <returns type="EmotionalContext">The context the <see cref="Emotion"/> is known in.</returns>
        public EmotionalContext? GetCurrentEmotionalContext() => FindEmotion(CurrentEmotion);
    }
}