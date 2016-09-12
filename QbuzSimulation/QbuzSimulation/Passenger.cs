using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QbuzSimulation
{
    public class Passenger
    {
        private readonly int _arrivalTime;
        private int _boardTime;
        public int WaitTime => _boardTime - _arrivalTime;
        public string Destination { get; private set; }

        public Passenger(int arrivalTime, string destination)
        {
            _arrivalTime = arrivalTime;
            Destination = destination;
        }

        public int Enter(int time)
        {
            _boardTime = time;
            return 1;
        }

        public int Exit()
        {
            return 1;
        }
    }
}
