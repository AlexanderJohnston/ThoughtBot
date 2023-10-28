namespace Dialogue
{
    public class Expression
    {
        public Expression(string? name, string? phrase)
        {
            Name = name;
            Phrase = phrase;
        }

        public string? Name { get; set; }
        public string? Phrase { get; set; }

        public string Write()
        {
            return Phrase ?? "";
        }
    }
}