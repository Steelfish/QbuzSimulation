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
        private readonly Random _r = new Random();

        public string Name { get; set; }
        public int AvgTimeToNextDestination { get; set; }
        public double DistanceToNextStop { get; set; }
        public TramStop NextStop { get; set; }
        public bool IsEndPoint { get; set; }
        public List<Passenger> Passengers = new List<Passenger>();
        public int Route { get; set; }
        public double ExitProbability { get; set; }
        public List<Tram> Occupied = new List<Tram>();

        //Todo change to distribution that changes over time
        public int InterArrivalTime => _r.Next(0, 20);

        public int MaxQueueLength = 0;
        private int QueueLengthOverTime = 0;
        private int _lastEvent = 0;


        private void Apply(PassengerArrivalEvent @event)
        {
            if (IsEndPoint) throw new InvalidOperationException("Passengers can't arrive on an endpoint.");

            var scaleFactor = 1/ProbabilityRemaining;
            var num = _r.NextDouble();
            var aggr = 0.0;
            var stop = NextStop;
            while (stop.NextStop != null)
            {
                aggr += stop.ExitProbability*scaleFactor;
                if (num < aggr)
                    break;
            }

            QueueLengthOverTime += Passengers.Count*(@event.TimeStamp - _lastEvent);
            Passengers.Add(new Passenger(@event.TimeStamp, Name, stop.Name));
            if (Passengers.Count > MaxQueueLength)
                MaxQueueLength = Passengers.Count;
            _lastEvent = @event.TimeStamp;
        }

        public int GetQueueLengthOverTime(int finalTime)
        {
            QueueLengthOverTime += Passengers.Count * (finalTime - _lastEvent);
            return QueueLengthOverTime;
        }

        private double ProbabilityRemaining => NextStop?.ProbabilityRemaining + NextStop?.ExitProbability ?? 0;
        private int StopsRemaining => NextStop?.StopsRemaining + 1 ?? 0;
    }
}

