using Memory;
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
        private Dictionary<ulong, Loom> Threads { get; set; } = new();
        public void ThreadInstructions(string instructions, ulong threadId) => For(threadId).AddInstruction(instructions);
        public void ThreadConversation(string message, ulong threadId) => For(threadId).AddConversation(message);
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
            var prompt = new Prompt(Prompter.Templates.Respond);
            Threads.Add(threadId, new Loom(prompt, new Tokenizer()));
        }
        public bool Exists(ulong threadId) => Threads.Any(x => x.Key == threadId);

        private Loom For(ulong threadId) => Threads.First(loom => loom.Key == threadId).Value;
        public string GeneratePromptFor(ulong threadId) => Threads.First(loom => loom.Key == threadId).Value.GeneratePrompt();


    }
}
