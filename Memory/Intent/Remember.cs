﻿using Memory;

namespace Memory.Intent
{
    public class Remember : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "Remember";
    }
}
