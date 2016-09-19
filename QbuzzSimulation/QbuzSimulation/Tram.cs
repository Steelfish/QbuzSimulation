using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QbuzzSimulation
{
    //Tram klasse om trams te simuleren
    public class Tram: AggregateRoot
    {
        public bool AtEndPoint => Destination.IsEndPoint;
        public int Route => Destination.Route;
        public bool Driving { get; private set; }
        public int DeltaT { get; set; }
        public int TimeToSpare { get; set; }

        public TramStop Destination { get; set; }
        private List<Passenger> _passengers = new List<Passenger>();
        private readonly int _turnAroundTime;

        public Tram(TramStop start, int turnAroundTime)
        {
            Destination = start;
            _turnAroundTime = turnAroundTime;
        }

        private void Apply(TramEstimatedStartEvent @event)
        {
            //Pick up any additional passengers that may have arrived
            DeltaT = Destination.Passengers.Sum(p => p.Enter(@event.TimeStamp - TimeToSpare));
            _passengers.AddRange(Destination.Passengers);
            Destination.Passengers.Clear();
            TimeToSpare = 0;
        }

        private void Apply(TramStartEvent @event)
        {
            if (Driving) throw new InvalidOperationException("Can't start Tram that's already driving.");
            if (Destination.IsEndPoint) throw new InvalidOperationException("Can't start Tram on endpoint.");
            DeltaT = Destination.AvgTimeToNextDestination;
            Driving = true;
            Destination = Destination.NextStop;
        }

        private void Apply(TramStopEvent @event)
        {
            if (!Driving) throw new InvalidOperationException("Can't stop Tram that's not driving.");
            Driving = false;
            //Uitstappen passagiers
            DeltaT = _passengers.Where(p => p.Destination == Destination.Name).Sum(p => p.Exit());
            _passengers = _passengers.Where(p => p.Destination != Destination.Name).ToList();
            //Instappen nieuwe passagiers
            DeltaT += Destination.Passengers.Sum(p => p.Enter(@event.TimeStamp + DeltaT));
            _passengers.AddRange(Destination.Passengers);
            Destination.Passengers.Clear();
        }

        private void Apply(TramChangeTrackEvent @event)
        {
            if (!AtEndPoint) throw new InvalidOperationException("Can only change tracks at end points of route.");
            Destination = @event.NewRoute;
            DeltaT += _turnAroundTime;
        }
    }
}
