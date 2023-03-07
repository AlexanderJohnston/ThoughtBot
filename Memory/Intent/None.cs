using Memory;

namespace Memory.Intent
{
    public class None : Intention
    {
        public float Threshold { get; set; } = 0.50F;

        public string Name { get; set; } = "None";
        public None() { }
        public None(string name)
        {
            Name = name;
        }
    }
}
