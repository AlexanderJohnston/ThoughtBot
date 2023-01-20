namespace Memory.Converse
{
    public class Message
    {
        public Message(ulong author, Guid id, string text)
        {
            Id = id;
            Author = author;
            Text = text;
        }

        public Guid Id { get; set; }
        public ulong Author { get; set; }
        public string Text { get; set; }
    }
}