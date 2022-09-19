using System;
using System.Collections.Generic;
using System.Text;
using Realization.Intent;
using ThotLibrary.Cognitive;
using ThotLibrary;

namespace Realization.Learning
{
    public class Utterance
    {
        public string Example;
        public Intention Intent;
        public LearnedEntity[] Entities;
    }
}
