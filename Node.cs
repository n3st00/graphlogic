using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace graphlogic
{
    internal class Node
    {
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public List<string> connections { get; set; }
    }
}
