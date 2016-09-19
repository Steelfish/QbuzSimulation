using System;

namespace QbuzzSimulation
{
    public class Passenger
    {
        private readonly int _arrivalTime;
        private int _boardTime;
        public bool Participated => _boardTime != 0;
        public int WaitTime => Math.Max(0, _boardTime - _arrivalTime);
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
