using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace graphlogic
{
    internal class Edge
    {
        public string name { get; set; }
        public List<string> connections { get; set; }
    }
}
