using Realization.Perception;
using System.Collections;

namespace Realization.Perception
{
    /// <summary>
    ///     This rolling buffer can handle storing a fixed number of items and then rolling off the oldest item when a new item is added.
    /// </summary>
    public class RollingBuffer : IEnumerable<TokenizedString>
    {
   
        private int Limit { get; set; }
        private int Current { get; set; }

        
        private List<TokenizedString> Objects { get; } = new();

        public RollingBuffer(int limit)
        {
            Limit = limit;
            Current = 0;
        }

        public TokenizedString this[int i]
        {
            get => Objects[i];
        }

        public IEnumerator<TokenizedString> GetEnumerator() => Objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TokenizedString message)
        {
            var newTotal = Current + message.Tokens;
            while (newTotal >= Limit)
            {
                var remove = Objects[0].Tokens;
                Objects.RemoveAt(0);
                Current -= remove;
                newTotal = Current + message.Tokens;
            }
            Objects.Add(message);
            Current += message.Tokens;
        }
    }
}
