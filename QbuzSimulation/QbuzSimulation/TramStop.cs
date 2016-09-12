using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QbuzSimulation
{
    public class TramStop
    {
        public string Name { get; set; }
        public int AvgTimeToNextDestination { get; set; }
        public TramStop NextStop { get; set; }
        public bool IsEndPoint { get; set; }
        public List<Passenger> Passengers = new List<Passenger>(); 
        public int Route { get; set; }
    }
}
