using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization
{

    public class Attention
    {
        public List<string> States;

        public Attention(List<string> states)
        {
            States = states;
        }
        public Attention()
        {
            States = new()
            {
                "Asleep",
                "Awake",
                "Listening",
                "Focused",
                "Learning"
            };
        }

        public string CurrentState = "Asleep";

        public void Change(string state)
        {
            CurrentState = state;
        }
    }
}
