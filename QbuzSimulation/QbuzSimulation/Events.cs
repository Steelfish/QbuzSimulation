using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QbuzSimulation
{
    //Events die kunnen gebeuren in het systeem

    public class TramStopEvent : Event
    {
       public TramStopEvent(int time) : base("TramStop", time) { }
    }

    public class TramStartEvent : Event
    {
        public TramStartEvent(int time) : base("TramLeave", time) { }
    }

    public class TramPassengersExitEvent : Event
    {
        public TramPassengersExitEvent(int time) : base("TramPassengersExit", time) {}
    }

    public class TramPassengersEnterEvent : Event
    {
        public TramPassengersEnterEvent(int time) : base("TramPassengersEnter", time) { }
    }

    public class TramChangeTrackEvent : Event
    {
        public TramStop NewRoute { get; }
        public TramChangeTrackEvent(int time, TramStop newRoute) 
            : base("TramChangeTrackEvent", time)
        {
            NewRoute = newRoute;
        }
    }

    //TODO Meer events toevoegen in het systeem

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
    }
}
