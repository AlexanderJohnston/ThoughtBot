using Memory;

namespace Memory.Intent
{
    public class Wake : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "Wake";
    }
}
