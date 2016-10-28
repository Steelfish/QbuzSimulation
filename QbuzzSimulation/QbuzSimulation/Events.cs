using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QbuzzSimulation
{
    //Events die kunnen gebeuren in het systeem

    public class SimulationStartEvent : Event
    {
        public new const string Name = "SimulationStart";
        public SimulationStartEvent(int time) : base(Name, time) { }
    }

    public class PassengerArrivalEvent : Event
    {
        public new const string Name = "PassengerArrival";
        public string Destination { get; set; }

        public PassengerArrivalEvent(int time, string destination) : base(Name, time)
        {
            Destination = destination;
        }

        public override string Export()
        {
            return string.Join(";", TimeStamp.ToString(), Name, Destination);
        }
    }

    public class TramStopEvent : Event
    {
        public new const string Name = "TramStop";
        public TramStopEvent(int time) : base(Name, time) { }
    }

    public class TramEstimatedStartEvent : Event
    {
        public new const string Name = "TramEstimatedStart";
        public TramEstimatedStartEvent(int time) : base(Name, time) { }
    }

    public class TramStartEvent : Event
    {
        public new const string Name = "TramLeave";
        public TramStartEvent(int time) : base(Name, time) { }
    }

    public class TramPassengersEnterEvent : Event
    {
        public new const string Name = "TramPassengersEnter";
        public TramPassengersEnterEvent(int time) : base(Name, time) { }
    }

    public class TramChangeTrackEvent : Event
    {
        public new const string Name = "TramChangeTrackEvent";
        public TramStop NewRoute { get; }
        public TramChangeTrackEvent(int time, TramStop newRoute)
            : base(Name, time)
        {
            NewRoute = newRoute;
        }
    }

    public class ArrivalRateChangeEvent : Event
    {
        public new const string Name = "ArrivalRateChange";
        public ArrivalRateChangeEvent(int time) : base(Name, time) { }
    }


    public abstract class Event
    {
        public readonly Guid Id = Guid.NewGuid();
        public int TimeStamp { get; }
        public string Name { get; }

        protected Event(string name, int time)
        {
            Name = name;
            TimeStamp = time;
        }

        public virtual string Export()
        {
            return string.Join(";", TimeStamp.ToString(), Name);
        }
    }
}
