namespace Dialogue
{
    public class Template
    {
        public Template(string name, string format)
        {
            Name = name;
            Format = format;
        }
        public string? Name { get; set; }
        public string? Format { get; set; }

        public string Write()
        {
            return Format ?? "";
        }
    }
}