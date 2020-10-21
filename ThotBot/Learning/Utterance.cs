using System;
using System.Collections.Generic;
using System.Text;
using ThotBot.Intent;

namespace ThotBot.Learning
{
    public class Utterance
    {
        public string Example;
        public Intention Intent;
        public LearnedEntity[] Entities;
    }
}
