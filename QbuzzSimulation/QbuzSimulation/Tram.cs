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
        public int TimeToSpare { get; set; }

        public TramStop Previous { get; set; }
        public TramStop Destination { get; set; }

        public Tram Ahead { get; set; }
        public Tram Behind { get; set; }

        private List<Passenger> _passengers = new List<Passenger>();

        private const double AccelerationSpeed = 1.2;
        private const double DecelerationSpeed = 1.2;
        private const double MaxSpeed = 70000.0/3600.0;
        private double _velocity;
        private double _distanceRemaining;

        public Tram(TramStop start)
        {
            Previous = start;
            Destination = start;
        }

        private void Apply(TramEstimatedStartEvent @event)
        {
            //Pick up any additional passengers that may have arrived
            DeltaT = CalculateStopDelay() - TimeToSpare;
            _passengers.AddRange(Destination.Passengers);
            Destination.Passengers.Clear();
            TimeToSpare = 0;
        }

        private void Apply(TramStartEvent @event)
        {
            if (Destination.IsEndPoint) throw new InvalidOperationException("Can't start Tram on endpoint.");
            foreach (var passenger in _passengers.Where(p => p.Stop == Destination.Name))
            {
                passenger.Enter(@event.TimeStamp);
            }
            CalculateTimeToNextDestination();
            Driving = true;
            if (Previous != null && Previous.IsEndPoint)
                Previous.Occupied.Remove(this);
            else 
                Destination.Occupied.Remove(this);
            Previous = Destination;
            Destination = Destination.NextStop;
        }

        private void Apply(TramEstimatedStopEvent @event)
        {
            if (ShouldStop)
            {
                DeltaT = (int)Math.Ceiling(CalculateTime(0, _velocity, DecelerationSpeed));
            }
            else
            {
                var distance = CalculateDistance(0, _velocity, DecelerationSpeed) + _distanceRemaining;
                if (Math.Abs(_velocity - MaxSpeed) < 0.01)
                    DeltaT = (int) Math.Ceiling(distance/_velocity);
                else
                {
                    var accelerationTime = CalculateTime(_velocity, MaxSpeed, AccelerationSpeed);
                    var accelerationDistance = CalculateDistance(_velocity, MaxSpeed, AccelerationSpeed);
                    if (accelerationDistance < distance)
                    {
                        var maxSpeedTime = Math.Max(0, (distance - accelerationDistance)/MaxSpeed);
                        DeltaT = (int) Math.Ceiling(accelerationTime + maxSpeedTime);
                        _velocity = MaxSpeed;
                    }
                    else
                    {
                        DeltaT = (int)Math.Round((Math.Sqrt(Math.Pow(_velocity, 2) - 2 * AccelerationSpeed * distance) - _velocity) / AccelerationSpeed);
                        _velocity = _velocity + AccelerationSpeed * DeltaT;
                    }
                }
            }
                
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
            _velocity = 0;
            Waiting = false;
        }

        private void Apply(TramStartWaitingEvent @event)
        {
            Waiting = true;
            Driving = false;
            _velocity = 0;
            DeltaT = 0;
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
            //return 12.5 + 0.22 * passengersIn + 0.13 * passengersOut;

            // Literature style delay calculation.
            return (int)(2.3E-5 * passengersTransfer * (passengersIn + passengersOut));
        }

        private void CalculateTimeToNextDestination()
        {
            //Estimate time before we should stop
            var t = this.Ahead;
            var distance = Destination.DistanceToNextStop + _distanceRemaining;
            //Todo check for distance between trams?
            while (t.Destination.Name == Destination.Name && t.Driving && t.Id != Id)
            {
                distance -= 40;
                _distanceRemaining += 40;
                t = t.Ahead;
            }
            var maxVelocity = Math.Min(MaxSpeed, CalculateMaxSpeed(distance));
            var accelerationTime = CalculateTime(_velocity, maxVelocity, AccelerationSpeed);
            var accelerationDistance = CalculateDistance(_velocity, maxVelocity, AccelerationSpeed);
            var decelerationDistance = CalculateDistance(0, maxVelocity, DecelerationSpeed);

            var maxSpeedTime = Math.Max(0, (Destination.DistanceToNextStop - accelerationDistance - decelerationDistance) /
                               maxVelocity);
            DeltaT = (int)Math.Floor(accelerationTime + maxSpeedTime);
            _velocity = maxVelocity;
        }

        private double CalculateMaxSpeed(double distance)
        {
            return Math.Sqrt(
                (DecelerationSpeed*_velocity*_velocity + 2*AccelerationSpeed*distance*DecelerationSpeed)
              / (AccelerationSpeed + DecelerationSpeed)
            );
        }

        private double CalculateTime(double v0, double v1, double a)
        {
            return (v1 - v0)/a;
        }

        private double CalculateDistance(double v0, double v1, double a)
        {
            var t = CalculateTime(v0, v1, a);
            return v0*t + 0.5*a*t*t;
        }
    }
}
