using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace QbuzzSimulation
{
    //Tram klasse om trams te simuleren
    public class Tram: AggregateRoot
    {
        public bool AtEndPoint => Destination.IsEndPoint;

        public bool ShouldStop => AtEndPoint || Destination.Occupied.Count > 0
                               || _passengers.Count(p => p.Destination == Destination.Name) > 0;
        public int Route => Destination.Route;

        public bool Waiting { get; private set; }
        public bool Driving { get; private set; }
        public int DeltaT { get; set; }
        public int StartWaiting { get; set; }

        public TramStop Previous { get; set; }
        public TramStop Destination { get; set; }

        public Tram Ahead { get; set; }
        public Tram Behind { get; set; }

        private List<Passenger> _passengers = new List<Passenger>();

        public Tram(TramStop start)
        {
            Previous = start;
            Destination = start;
        }

        private void Apply(TramEstimatedStartEvent @event)
        {
            //Pick up any additional passengers that may have arrived
            DeltaT = CalculateStopDelay() - (@event.TimeStamp - StartWaiting);
            _passengers.AddRange(Destination.Passengers);
            Destination.Passengers.Clear();
        }

        private void Apply(TramStartEvent @event)
        {
            if (Destination.IsEndPoint) throw new InvalidOperationException("Can't start Tram on endpoint.");
            foreach (var passenger in _passengers.Where(p => p.Stop == Destination.Name))
            {
                passenger.Enter(@event.TimeStamp);
            }
            Driving = true;
            if (Previous != null && Previous.IsEndPoint)
                Previous.Occupied.Remove(this);
            else 
                Destination.Occupied.Remove(this);

            DeltaT = Destination.GetTimeToNextDestination();
            Previous = Destination;
            Destination = Destination.NextStop;
        }

        private void Apply(TramStopEvent @event)
        {
            if (!Driving) throw new InvalidOperationException("Can't stop Tram that's not driving.");
            Driving = false;
            DeltaT = CalculateStopDelay();
            //Uitstappen passagiers
            _passengers = _passengers.Where(p => p.Destination != Destination.Name).ToList();
            //Instappen nieuwe passagiers
            _passengers.AddRange(Destination.Passengers);
            Destination.Passengers.Clear();
            Destination.Occupied.Add(this);
            Waiting = Destination.Occupied.Count > 1;
            if (Waiting)
                StartWaiting = @event.TimeStamp;
        }

        private void Apply(TramChangeTrackEvent @event)
        {
            if (!AtEndPoint) throw new InvalidOperationException("Can only change tracks at end points of route.");
            Previous = Destination;
            Destination = @event.NewRoute;
            DeltaT = 0;
        }

        private int CalculateStopDelay()
        {
            var passengersIn = Destination.Passengers.Count;
            var passengersOut = _passengers.Count(p => p.Destination == Destination.Name);
            var passengersTransfer = _passengers.Count - passengersOut;

            // QBuzz style delay calculation.
            return (int) Math.Round(12.5 + 0.22 * passengersIn + 0.13 * passengersOut);

            // Literature style delay calculation.
//            return (int)(2.3E-5 * passengersTransfer * (passengersIn + passengersOut));
        }

        public List<int> ExportDrivingTimes()
        {
            var result = new List<int>();
            var t = 0;
            foreach (var evt in _events)
            {
                if (evt is TramStartEvent)
                    t = evt.TimeStamp;
                if (evt is TramStopEvent)
                    result.Add(evt.TimeStamp - t);
            }
            return result;
        }
    }
}
