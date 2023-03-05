using Prompter;

namespace Realization
{
    internal class UserSettings
    {
        private Dictionary<ulong, float> Temperature { get; set; } = new();
        private Dictionary<ulong, string> Prompt { get; set; } = new();

        // Add or update a user to the temperature dictionary.
        public void AddTemperature(ulong userId, float temp)
        {
            if (!Temperature.ContainsKey(userId))
            {
                Temperature.Add(userId, temp);
            }
            else
            {
                Temperature[userId] = temp;
            }
        }
        
        // Add or update a user to the prompt dictionary.
        public void AddPrompt(ulong userId, string prompt)
        {
            if (!Prompt.ContainsKey(userId))
            {
                Prompt.Add(userId, prompt);
            }
            else
            {
                Prompt[userId] = prompt;
            }
        }

        // Get a temperature or return default 0.7f.
        public double GetTemperature(ulong userId)
        {
            if (Temperature.ContainsKey(userId))
            {
                return Temperature[userId];
            }
            else
            {
                return 0.7f;
            }
        }

        public string GetPrompt(ulong userId)
        {
            if (Prompt.ContainsKey(userId))
            {
                return Prompt[userId];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}