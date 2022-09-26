using Realization.Converse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Skill
{
    public class VentralStream
    {
        public Dictionary<People, Conversation> Dialogue = new();
        public Dictionary<Person, string> TopicLookup = new();


        public void Listen(AuditorySignal signal)
        {
            var conversation = RecognizeConversation(signal);
            conversation.Memories.Add(new Message() { Id = signal.MemoryId, Author = signal.Source });
            UpdateTopic(signal.Source, signal.Topic);
        }
        private void UpdateTopic(ulong personId, string topic)
        {
            if (TopicExists(personId))
            {
                var person = TopicLookup.First(person => person.Key.Id == personId);
                TopicLookup.Remove(person.Key);
                TopicLookup.Add(new() { Id = personId }, topic);
            }
            else
            {
                TopicLookup.Add(new() { Id = personId }, topic);
            }
        }
        private Conversation RecognizeConversation(AuditorySignal signal)
        {
            if (ConversationExists(signal))
                return Dialogue.First(dialogue => Conversation(dialogue).Topic == signal.Topic).Value;
            else
            {
                var person = new Person { Id = signal.Source };
                var people = new People(new() { person });
                var convo = new Conversation()
                {
                    Topic = signal.Topic,
                    Author = signal.Source,
                    Context = new List<string>() { signal.Context }
                };
                Dialogue.Add(people, convo);
                TopicLookup.Add(person, signal.Topic);
                return convo;
            }
        }
        private Conversation Conversation(KeyValuePair<People, Conversation> selected) => selected.Value;
        private bool ConversationExists(AuditorySignal signal) =>
            Dialogue.Any(dialogue => Conversation(dialogue).Topic == signal.Topic);

        public string GetTopic(ulong person) => TopicLookup.First(topic => topic.Key.Id == person).Value;
        public bool TopicExists(ulong person) => TopicLookup.Any(topic => topic.Key.Id == person);

        public VentralStream() { }
        public VentralStream(Dictionary<People, Conversation> dialogue)
        {
            Dialogue = dialogue;
        }
    }

    public class People
    {
        public List<Person> Group = new();
        public People(List<Person> group)
        {
            Group = group;
        }
        public void Add(Person person)
        {
            Group.Add(person);
        }
        public void Remove(Person person)
        {
            Group.Remove(person);
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public ulong Id { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Person person &&
                   Id == person.Id;
        }
    }

    public class AuditorySignal
    {
        public ulong Source { get; set; }
        public Guid MemoryId { get; set; }
        public string Context { get; set; }
        public string Topic { get; set; }
    }
}
