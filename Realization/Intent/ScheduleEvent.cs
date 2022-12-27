﻿using ThotLibrary;

namespace Realization.Intent
{
    public class ScheduleEvent : Intention
    {
        public float Threshold { get; set; } = 0.80F;

        public string Name { get; set; } = "ScheduleEvent";
    }
}
