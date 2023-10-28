namespace REBL.Commands
{
    public interface SetupCommand<T>
    {
        public T Create(string input, REBLConsole console);
    }
}