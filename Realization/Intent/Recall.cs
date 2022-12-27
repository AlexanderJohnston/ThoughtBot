using ThotLibrary;

namespace Realization.Intent
{
    public class Recall : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "Recall";
    }
}
