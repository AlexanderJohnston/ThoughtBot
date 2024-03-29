﻿using Memory.Converse;

namespace Realization.Skill
{
    public class VentralStream
    {
        public Dictionary<People, Conversation> Dialogue = new();
        public Dictionary<Person, string> TopicLookup = new();


        public void Listen(AuditorySignal signal)
        {
            var conversation = RecognizeConversation(signal);
            conversation.Update(signal);
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
        public Conversation RecognizeConversation(AuditorySignal signal)
        {
            //if (TopicExists(signal.Source))
            //{
            //    var knownConvo = Dialogue.First(dialogue => Conversation(dialogue).Topic == signal.Topic);
            //    var currentTopic = knownConvo.Value.Topic;
            //    if (currentTopic != signal.Topic)
            //    {
            //        knownConvo.Value.Context.Add(currentTopic);
            //    }
            //}
            if (ConversationExists(signal))
            {

                return Dialogue.First(dialogue => Conversation(dialogue).Id == signal.Channel).Value;
            }
            else
            {
                var person = new Person { Id = signal.Source };
                var people = new People(new() { person });
                var convo = new Conversation(signal);
                Dialogue.Add(people, convo);
                //TopicLookup.Add(person, signal.Topic);
                return convo;
            }
        }
        public Conversation Conversation(KeyValuePair<People, Conversation> selected) => selected.Value;
        private bool ConversationExists(AuditorySignal signal) =>
            Dialogue.Any(dialogue => Conversation(dialogue).Id == signal.Channel);
        //private bool ContextExists(AuditorySignal signal) =>
        //    Dialogue.Any(dialogue => dialogue.)

        public Conversation GetConversation(string topic) => Dialogue.First(dialogue => Conversation(dialogue).Topic == topic).Value;
        public Conversation GetConversation(ulong channelId) => Dialogue.First(dialogue => Conversation(dialogue).Id == channelId).Value;

        public string GetTopic(ulong person) => TopicLookup.First(topic => topic.Key.Id == person).Value;
        public bool TopicExists(ulong person) => TopicLookup.Any(topic => topic.Key.Id == person);

        public VentralStream() { }
        public VentralStream(Dictionary<People, Conversation> dialogue)
        {
            Dialogue = dialogue;
        }
    }
}
