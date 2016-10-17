using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QbuzzSimulation
{
    public class TramStop: AggregateRoot
    {
        public bool GeneratingEvents { get; set; }

        public string Name { get; set; }
        public int AvgTimeToNextDestination { get; set; }
        public double DistanceToNextStop { get; set; }
        public TramStop NextStop { get; set; }
        public bool IsEndPoint { get; set; }
        public List<Passenger> Passengers = new List<Passenger>();
        public int Route { get; set; }
        public List<Tram> Occupied = new List<Tram>();

        public int MaxQueueLength = 0;
        private int QueueLengthOverTime = 0;
        private int _lastEvent = 0;

        private void Apply(PassengerArrivalEvent @event)
        {
            if (IsEndPoint) throw new InvalidOperationException("Passengers can't arrive on an endpoint.");
            QueueLengthOverTime += Passengers.Count*(@event.TimeStamp - _lastEvent);
            Passengers.Add(new Passenger(@event.TimeStamp, Name, @event.Destination));
            if (Passengers.Count > MaxQueueLength)
                MaxQueueLength = Passengers.Count;
            _lastEvent = @event.TimeStamp;
        }

        public int GetQueueLengthOverTime(int finalTime)
        {
            QueueLengthOverTime += Passengers.Count * (finalTime - _lastEvent);
            return QueueLengthOverTime;
        }

        public int GetTimeToNextDestination()
        {
            // Use a Lognormal distribution to simulate driving times as per the fit on the Nieuwegeinlijn.
            // The mean is set to the average driving time and the standard deviation is found to be 3.01% of the mean on average.
            // The mean and standard deviation are logarithmic.
            int time = RandomDistribution.GenerateNextLognormal((float)Math.Log(AvgTimeToNextDestination), (float)Math.Log(AvgTimeToNextDestination)*0.0301f);
            return Math.Abs(time);
        }
    }
}

