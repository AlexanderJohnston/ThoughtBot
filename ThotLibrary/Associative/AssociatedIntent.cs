using System;
using System.Collections.Generic;
using System.Text;
using ThotLibrary.Events;
using Totem.Timeline;

namespace ThotLibrary.Associative
{
    public class AssociatedIntent : Topic
    {
        public Dictionary<string, HashSet<Intention>> ResourceAssociations = new Dictionary<string, HashSet<Intention>>();

        void When(Association e)
        {
            if (ResourceAssociations.ContainsKey(e.Resource))
            {
                if (ResourceAssociations[e.Resource].Contains(e.Intent))
                {
                    return;
                }
                else
                {
                    ResourceAssociations[e.Resource].Add(e.Intent);
                }
            }
        }
    }
}
