namespace REBL.Commands
{
    public class Buffer : Command, SetupCommand<Buffer>
    {
        public Buffer(REBLConsole console) : base(console)
        {
            Name = "buffer";
            Act = () => GetBuffer();
        }
        public Buffer(string key, REBLConsole console) : base(console)
        {
            Name = "buffer";
            Key = key;
            Act = () => GetBuffer();
        }

        public string Key { get; set; }

        public Buffer Create(string input, REBLConsole console) => new Buffer(input, console);

        public void GetBuffer()
        {
            Rebel.Dynamic(Key, new List<object>());
        }
    }
}