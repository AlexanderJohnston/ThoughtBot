namespace REBL.Commands
{
    public abstract class Command
    {
        public Command(REBLConsole console) => Rebel = console;

        public REBLConsole Rebel { get; set; }
        public virtual Action Act { get; set; }
        public virtual string Name { get; set; }
    }
}