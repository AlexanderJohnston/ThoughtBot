﻿using Memory;

namespace Memory.Intent
{
    public class Praise : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "Praise";
    }
}
