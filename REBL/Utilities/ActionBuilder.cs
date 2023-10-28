using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REBL.Utilities
{
    public class ActionBuilder
    {
        public static List<Type> GetInstances<T>()
        {
            var found = new List<Type>();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.IsSubclassOf(typeof(T)));
            foreach (Type foundType in types)
            {
                found.Add(foundType);
            }
            return found;
        }
    }
}
