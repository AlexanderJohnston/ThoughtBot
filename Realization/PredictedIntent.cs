using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThotLibrary;

namespace Realization
{
    public class PredictedIntent
    {
        public PredictedIntent(Intention intent, string data)
        {
            Intent = intent;
            Data = data;
        }

        public Intention Intent { get; set; }
        public string Data { get; set; }
    }
}
