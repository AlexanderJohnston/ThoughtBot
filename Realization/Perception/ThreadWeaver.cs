using Memory;
using Memory.Chat;
using Prompter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Perception
{
    public class ThreadWeaver
    {
        private Dictionary<ulong, string> Characters { get; set; } = new();
        private Dictionary<ulong, string> UserPrompts { get; set; } = new();
        private Dictionary<ulong, Prompt> DefaultPrompts { get; set; } = new();
        private Dictionary<ulong, Loom> Threads { get; set; } = new();
        public void ThreadInstructions(string instructions, ulong threadId) => For(threadId).AddInstruction(instructions);
        public void ThreadConversation(string message, ulong threadId, string role) => For(threadId).AddConversation(message, role);
        public void ThreadMemories(List<EmbeddedMemory> embedding, ulong threadId)
        {
            var loom = For(threadId);
            loom.ResetMemory();
            foreach (var embed in embedding)
            {
                loom.AddMemory(embed.Memory.ToString());
            }
        }
        public void NewThread(ulong threadId)
        {
            Prompt prompt;
            if (DefaultPrompts.ContainsKey(threadId))
            {
                prompt = GetDefaultPrompt(threadId);
            }
            else
            {
                prompt = new Prompt(Prompter.Templates.Respond);
            }
            Threads.Add(threadId, new Loom(prompt, new Tokenizer()));
        }
        public void InitializeThread(ulong threadId)
        {
            // Check for an existing user prompt
            if (UserPrompts.ContainsKey(threadId))
            {
                var prompt = new Prompt(UserPrompts[threadId]);
                AddDefaultPrompt(threadId, prompt);
            }
            else
            {
                AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Respond));
            }
        }

        public void InitializeThread(ulong threadId, string option, ulong userId)
        {
            // Check for an existing user prompt
            if (UserPrompts.ContainsKey(userId))
            {
                var prompt = new Prompt(UserPrompts[userId]);
                AddDefaultPrompt(threadId, prompt);
            }
            else
            {
                switch (option)
                {
                    case "opt-teach":
                        AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Teach));
                        break;
                    case "opt-chat":
                        AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Chat));
                        break;
                    case "opt-davinci":
                        AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Chat));
                        break;
                    case "opt-character":
                        AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Characterize));
                        break;
                    case "opt-memory":
                        AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Recall));
                        break;
                    default:
                        AddDefaultPrompt(threadId, new Prompt(Prompter.Templates.Respond));
                        break;
                }
            }
        }

        // Add or update DefaultPrompts.
        public void AddDefaultPrompt(ulong userId, Prompt prompt)
        {
            if (!DefaultPrompts.ContainsKey(userId))
            {
                DefaultPrompts.Add(userId, prompt);
            }
            else
            {
                DefaultPrompts[userId] = prompt;
            }
        }

        public void AddCharacter(ulong userId, string character)
        {
            if (!Characters.ContainsKey(userId))
            {
                Characters.Add(userId, character);
            }
            else
            {
                Characters[userId] = character;
            }
        }

        public void ResetCharacter(ulong userId)
        {
            if (Characters.ContainsKey(userId))
            {
                Characters[userId] = string.Empty;
            }
        }

        public void UpdateUserSetting(ulong userId, string defaultPrompt)
        {
            // Add or update UserPrompts
            if (!UserPrompts.ContainsKey(userId))
            {
                UserPrompts.Add(userId, defaultPrompt);
            }
            else
            {
                UserPrompts[userId] = defaultPrompt;
            }
        }

        public void DropUserSettings(ulong userId)
        {
            if (UserPrompts.ContainsKey(userId))
            {
                UserPrompts.Remove(userId);
            }
        }

        public bool Exists(ulong threadId) => Threads.Any(x => x.Key == threadId);

        private Loom For(ulong threadId) => Threads.First(loom => loom.Key == threadId).Value;
        public string GeneratePromptFor(ulong threadId) => Threads.First(loom => loom.Key == threadId).Value.GeneratePrompt();
        public List<GptChatMessage> GenerateChatFor(ulong threadId) => Threads.First(loom => loom.Key == threadId).Value.GenerateChat();

        public Prompt GetDefaultPrompt(ulong threadId) => DefaultPrompts.First(prompt => prompt.Key == threadId).Value;

        public string GetCharacter(ulong userId) => Characters.First(character => character.Key == userId).Value;
    }
}
