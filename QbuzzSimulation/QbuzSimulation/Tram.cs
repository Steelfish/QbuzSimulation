using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QbuzzSimulation
{
    //Tram klasse om trams te simuleren
    public class Tram: AggregateRoot
    {
        public bool AtEndPoint => _destination.IsEndPoint && DeltaT == 0;
        public int Route => _destination.Route;
        public bool Driving { get; private set; }
        public bool EnRoute { get; private set; }
        public int DeltaT { get; set; }

        private TramStop _destination;
        private List<Passenger> _passengers = new List<Passenger>();
        private readonly int _turnAroundTime;

        public Tram(TramStop start, int turnAroundTime)
        {
            _destination = start;
            _turnAroundTime = turnAroundTime;
        }

        //Apply methoden om verschillende events af te handelen
        private void Apply(TramPassengersExitEvent @event)
        {
            DeltaT += _passengers.Where(p => p.Destination == _destination.Name).Sum(p => p.Exit());
            _passengers = _passengers.Where(p => p.Destination != _destination.Name).ToList();
        }

        private void Apply(TramPassengersEnterEvent @event)
        {
            DeltaT += _destination.Passengers.Sum(p => p.Enter(@event.TimeStamp));
            _passengers.AddRange(_destination.Passengers);
            _destination.Passengers.Clear();
        }

        private void Apply(TramStartEvent @event)
        {
            PassengersEnter(@event.TimeStamp);
            Driving = true;
            EnRoute = true;
            DeltaT += _destination.AvgTimeToNextDestination;
            _destination = _destination.NextStop;
        }

        private void Apply(TramStopEvent @event)
        {
            Driving = false;
            EnRoute = !AtEndPoint;
            PassengersExit(@event.TimeStamp);
        }

        private void Apply(TramChangeTrackEvent @event)
        {
            _destination = @event.NewRoute;
            DeltaT += _turnAroundTime;
        }

        public void Start(int time)
        {
            if(Driving) throw new InvalidOperationException("Can't start Tram that's already driving.");
            ApplyChange(new TramStartEvent(time));
        }

        public void PassengersEnter(int time)
        {
            if (Driving) throw new InvalidOperationException("Can't accept passengers on a Tram that's driving.");
            ApplyChange(new TramPassengersEnterEvent(time));
        }

        public void PassengersExit(int time)
        {
            if (Driving) throw new InvalidOperationException("Can't let out passengers on a Tram that's driving.");
            ApplyChange(new TramPassengersExitEvent(time));
        }

        public void Stop(int time)
        {
            if (!Driving) throw new InvalidOperationException("Can't stop Tram that's not driving.");
            ApplyChange(new TramStopEvent(time));
        }

        public void ChangeTrack(int time, TramStop newRoute)
        {
            if (!AtEndPoint) throw new InvalidOperationException("Can only change tracks at end points of route.");
            ApplyChange(new TramChangeTrackEvent(time, newRoute));
        }

    }
}
