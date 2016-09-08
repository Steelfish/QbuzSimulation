using System;

namespace QbuzSimulation
{
    //Tram klasse om trams te simuleren
    public class Tram: AggregateRoot
    {
        public bool Driving { get; set; }
        public TramStop Destination { get; set; }
        public int TimeToNextDestination { get; set; }

        //Apply methoden om verschillende events af te handelen
        private void Apply(TramStopEvent @event)
        {
            Driving = false;
            //TODO meer logica zoals uitstappen / instappen van passagiers
        }

        private void Apply(TramStartEvent @event)
        {
            Driving = true;
            Destination = Destination.NextStop;
            TimeToNextDestination = Destination.AvgTimeToNextDestination;
        }

        //Domain logica 
        public void Drive(int time)
        {
            if (!Driving)
                Start(time);
            TimeToNextDestination -= 1;
            if (TimeToNextDestination <= 0)
                Stop(time);
        }

        public void Start(int time)
        {
            if(Driving) throw new InvalidOperationException("Can't start Tram that's already driving.");
            ApplyChange(new TramStartEvent(time));
        }

        public void Stop(int time)
        {
            if (!Driving) throw new InvalidOperationException("Can't stop Tram that's not driving.");
            ApplyChange(new TramStopEvent(time));
        }
    }
}
