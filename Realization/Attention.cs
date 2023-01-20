using Memory.Converse;
using Realization.Skill;

namespace Realization
{
    /*Attention span system to track conversational contexts (threads) between people, sometimes multiple people at once in a single thread.
      This is a very important system for the AI to have, as it is the basis for the AI to be able to understand and respond to the player.
      The AI will be able to track the player's attention span, and will be able to respond to the player's attention span.*/
    public class AttentionSpan
    {
        //This is the list of people that the AI is currently tracking.
        List<Person> people = new();
        //This is the list of conversations that the AI is currently tracking.
        List<Conversation> conversations = new List<Conversation>();
        //This is the list of topics that the AI is currently tracking.
        List<Topic> topics = new List<Topic>();
        //This is the list of topics that the AI is currently tracking.
        List<Subject> subjects = new List<Subject>();

        //This is the list of people that the AI is currently tracking.
        public List<Person> People
        {
            get { return people; }
            set { people = value; }
        }
        //This is the list of conversations that the AI is currently tracking.
        public List<Conversation> Conversations
        {
            get { return conversations; }
            set { conversations = value; }
        }
        //This is the list of topics that the AI is currently tracking.
        public List<Topic> Topics
        {
            get { return topics; }
            set { topics = value; }
        }
        //This is the list of topics that the AI is currently tracking.
        public List<Subject> Subjects
        {
            get { return subjects; }
            set { subjects = value; }
        }

        //This is the constructor for the AttentionSpan class.
        public AttentionSpan()
        {
            //This is the list of people that the AI is currently tracking.
            people = new List<Person>();
            //This is the list of conversations that the AI is currently tracking.
            conversations = new List<Conversation>();
            //This is the list of topics that the AI is currently tracking.
            topics = new List<Topic>();
            //This is the list of topics that the AI is currently tracking.
            subjects = new List<Subject>();
        }

        //This is the method that is used to add a person to the list of people that the AI is currently tracking.
        public void AddPerson(Person person)
        {
            //This is the list of people that the AI is currently tracking.
            people.Add(person);
        }
        //This is the method that is used to remove a person from the list of people that the AI is currently tracking.
        public void RemovePerson(Person person)
        {
            //This is the list of people that the AI is currently tracking.
            people.Remove(person);

        }
    }
    // Topic for the AI to track.
    public class Topic
    {
        //This is the name of the topic.
        string name;
        //This is the list of subjects that the AI is currently tracking.
        List<Subject> subjects = new List<Subject>();

        //This is the name of the topic.
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        //This is the list of subjects that the AI is currently tracking.
        public List<Subject> Subjects
        {
            get { return subjects; }
            set { subjects = value; }
        }

        //This is the constructor for the Topic class.
        public Topic(string name)
        {
            //This is the name of the topic.
            this.name = name;
            //This is the list of subjects that the AI is currently tracking.
            subjects = new List<Subject>();
        }

        //This is the method that is used to add a subject to the list of subjects that the AI is currently tracking.
        public void AddSubject(Subject subject)
        {
            //This is the list of subjects that the AI is currently tracking.
            subjects.Add(subject);
        }
        //This is the method that is used to remove a subject from the list of subjects that the AI is currently tracking.
        public void RemoveSubject(Subject subject)
        {
            //This is the list of subjects that the AI is currently tracking.
            subjects.Remove(subject);
        }
    }

    //Subject tracks the name of the subject and the context.
    public class Subject
    {
        //This is the name of the subject.
        string name;
        //This is the context of the subject.
        string context;

        //This is the name of the subject.
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        //This is the context of the subject.
        public string Context
        {
            get { return context; }
            set { context = value; }
        }

        //This is the constructor for the Subject class.
        public Subject(string name, string context)
        {
            //This is the name of the subject.
            this.name = name;
            //This is the context of the subject.
            this.context = context;
        }
    }



}
