using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace QbuzzSimulation
{
    //System klasse die verantwoordelijk is voor het draaien van de simulatie
    public class System
    {
        private readonly List<Tram> _trams = new List<Tram>();
        private readonly List<Passenger> _passengers = new List<Passenger>();
        private List<ScheduledEvent> _eventList = new List<ScheduledEvent>();

        //Run input
        private readonly int _maxTime;
        private readonly int _f;
        private readonly int _q;
        private readonly int _t;

        private int _time;
        private int _route1NextStart;
        private int _route2NextStart;
        private int _ridesRoute1;
        private int _ridesRoute2;
        //P+R -> CS
        private TramStop _route1;
        //CS -> P+R
        private TramStop _route2;

        //Meetpunten
        private readonly List<int> _delaysRoute1 = new List<int>();
        private readonly List<int> _delaysRoute2 = new List<int>(); 


        public System(int maxTime, int f, int q, int t)
        {
            _maxTime = maxTime;
            _f = f;
            _q = q;
            _t = t;
            InitializeRoute();
        }

        public void Run()
        {
            for (var i = 0; i < _t; i++)
            {
                var tram1 = new Tram(_route1, _q) { TimeToSpare = _route1NextStart };
                var tram2 = new Tram(_route2, _q) { TimeToSpare = _route2NextStart };
                _trams.Add(tram1);
                _trams.Add(tram2);
                ScheduleEvent(new TramEstimatedStartEvent(_route1NextStart), tram1);
                ScheduleEvent(new TramEstimatedStartEvent(_route2NextStart), tram2);
                _route1NextStart += 60 / _f * 60;
                _route2NextStart += 60 / _f * 60;
            }
            _ridesRoute1 = 0;
            _ridesRoute2 = 0;
            var nextEvent = GetNextEvent();
            while (nextEvent.Event.TimeStamp < _maxTime)
            {
                _time = nextEvent.Event.TimeStamp;
                var tram = nextEvent.Target as Tram;
                if (tram != null)
                {
                    Operate(tram, nextEvent.Event);
                }
                var tramstop = nextEvent.Target as TramStop;
                if (tramstop != null)
                {
                    tramstop.ApplyChange(nextEvent.Event);
                    _passengers.Add(tramstop.Passengers.Last());
                    ScheduleEvent(new PassengerArrivalEvent(_time + tramstop.InterArrivalTime), tramstop);
                }
                nextEvent = GetNextEvent();
            }

            Console.WriteLine();
            Console.WriteLine("Total Delay: {0}", _delaysRoute1.Sum() + _delaysRoute2.Sum());
            Console.WriteLine("Total passenger wait time: {0}", _passengers.Sum(p => p.WaitTime));
            Console.WriteLine("Total passengers: {0}", _passengers.Count(p => p.Participated));
            Console.WriteLine("Avg. waittime / passenger: {0}", _passengers.Average(p => p.WaitTime));
        }

        private void Operate(Tram tram, Event @event)
        {
            tram.ApplyChange(@event);
            switch (@event.Name)
            {
                case TramEstimatedStartEvent.Name:
                    if (tram.DeltaT > 0 && tram.Destination.WaitList.Count == 0)
                    {
                        ScheduleEvent(new TramEstimatedStartEvent(_time + tram.DeltaT), tram);
                    }
                    else
                    {
                        if (tram.Destination == _route1)
                        {
                            _delaysRoute1.Add(_time - _ridesRoute1++ * 60/_f*60);
                        }
                        else if(tram.Destination == _route2)
                        {
                            _delaysRoute2.Add(_time - _ridesRoute2++ * 60/_f*60);
                        }
                        //Should probably schedule TramStop.
                        ScheduleEvent(new TramStartEvent(_time), tram);
                    }
                    break;
                    //Might be redundant.
                case TramStartEvent.Name:
                    ScheduleEvent(new TramEstimatedStopEvent(_time + tram.DeltaT), tram);
                    var waitingTram = tram.Previous.WaitList.FirstOrDefault();
                    if (waitingTram != null)
                    {
                        //TODO 40 seconden wordt nu hier verwerkt. Moet eigenlijk in starten en stoppen van trams?
                        ScheduleEvent(new TramStopEvent(_time + 40), tram);
                        tram.Previous.WaitList.Remove(tram);
                    }
                    break;
                case TramEstimatedStopEvent.Name:
                    if (tram.ShouldStop)
                    {
                        //TODO omgaan met eindpunten
                       if (tram.Destination.Occupied)
                            tram.Destination.WaitList.Add(tram); 
                    }
                    else
                        ScheduleEvent(new TramStartEvent(_time + tram.DeltaT), tram);
                    break;
                case TramStopEvent.Name:
                    if (tram.AtEndPoint)
                        ScheduleEvent(new TramChangeTrackEvent(_time + tram.DeltaT, tram.Route == 1 ? _route2 : _route1), tram);
                    else
                        ScheduleEvent(new TramEstimatedStartEvent(_time + tram.DeltaT), tram);
                    break;
                //Note: tram.DeltaT = _q -> Redundant attribute turnaround time?
                case TramChangeTrackEvent.Name:
                    if (tram.Route == 1 && _route1NextStart < _time + tram.DeltaT
                     || tram.Route == 2 && _route2NextStart < _time + tram.DeltaT)
                    {
                        ScheduleEvent(new TramEstimatedStartEvent(_time + tram.DeltaT), tram);
                    }
                    else
                    {
                        var extraTime = (tram.Route == 1 ? _route1NextStart : _route2NextStart) - _time - tram.DeltaT;
                        tram.TimeToSpare = extraTime;
                        ScheduleEvent(new TramEstimatedStartEvent(tram.Route == 1 ? _route1NextStart : _route2NextStart), tram);
                        if (tram.Route == 1)
                            _route1NextStart += 60 / _f * 60;
                        else
                            _route2NextStart += 60 / _f * 60;
                    }
                    if (tram.Route == 1)
                        _route1NextStart += 60/_f*60;
                    else
                        _route2NextStart += 60/_f*60;
                    break;
            }

        }

        //Maakt route aan
        private void InitializeRoute()
        {
            //TODO bereken exit probablility
            var PRDeUithof = new TramStop { Name = "P+R De Uithof", AvgTimeToNextDestination = 110, Route = 1, ExitProbability = 0};
            ScheduleEvent(new PassengerArrivalEvent(_time + PRDeUithof.InterArrivalTime), PRDeUithof);
            var WKZ = new TramStop { Name = "WKZ", AvgTimeToNextDestination = 78, Route = 1, ExitProbability = 0.05};
            ScheduleEvent(new PassengerArrivalEvent(_time + WKZ.InterArrivalTime), WKZ);
            var UMC = new TramStop { Name = "UMC", AvgTimeToNextDestination = 82, Route = 1, ExitProbability = 0.05};
            ScheduleEvent(new PassengerArrivalEvent(_time + UMC.InterArrivalTime), UMC);
            var Heidelberglaan = new TramStop { Name = "Heidelberglaan", AvgTimeToNextDestination = 60, Route = 1, ExitProbability = 0.05};
            ScheduleEvent(new PassengerArrivalEvent(_time + Heidelberglaan.InterArrivalTime), Heidelberglaan);
            var Padualaan = new TramStop { Name = "Padualaan", AvgTimeToNextDestination = 100, Route = 1, ExitProbability = 0.05};
            ScheduleEvent(new PassengerArrivalEvent(_time + Padualaan.InterArrivalTime), Padualaan);
            var KrommeRijn = new TramStop { Name = "Kromme Rijn", AvgTimeToNextDestination = 59, Route = 1, ExitProbability = 0.1};
            ScheduleEvent(new PassengerArrivalEvent(_time + KrommeRijn.InterArrivalTime), KrommeRijn);
            var GalgenWaard = new TramStop { Name = "Galgenwaard", AvgTimeToNextDestination = 243, Route = 1, ExitProbability = 0.1};
            ScheduleEvent(new PassengerArrivalEvent(_time + GalgenWaard.InterArrivalTime), GalgenWaard);
            var VaartscheRijn = new TramStop { Name = "Vaartsche Rijn", AvgTimeToNextDestination = 135, Route = 1, ExitProbability = 0.1};
            ScheduleEvent(new PassengerArrivalEvent(_time + VaartscheRijn.InterArrivalTime), VaartscheRijn);
            var CentraalStation = new TramStop { Name = "Centraal Station", IsEndPoint = true, Route = 1, ExitProbability = 0.5};

            var CentraalStation2 = new TramStop { Name = "Centraal Station", AvgTimeToNextDestination = 134, Route = 2, ExitProbability = 0};
            ScheduleEvent(new PassengerArrivalEvent(_time + CentraalStation2.InterArrivalTime), CentraalStation2);
            var VaartscheRijn2 = new TramStop { Name = "Vaartsche Rijn", AvgTimeToNextDestination = 243, Route = 2, ExitProbability = 0.05};
            ScheduleEvent(new PassengerArrivalEvent(_time + VaartscheRijn2.InterArrivalTime), VaartscheRijn2);
            var GalgenWaard2 = new TramStop { Name = "Galgenwaard", AvgTimeToNextDestination = 59, Route = 2, ExitProbability = 0.1};
            ScheduleEvent(new PassengerArrivalEvent(_time + GalgenWaard2.InterArrivalTime), GalgenWaard2);
            var KrommeRijn2 = new TramStop { Name = "Kromme Rijn", AvgTimeToNextDestination = 59, Route = 2, ExitProbability = 0.05};
            ScheduleEvent(new PassengerArrivalEvent(_time + KrommeRijn2.InterArrivalTime), KrommeRijn2);
            var Padualaan2 = new TramStop { Name = "Padualaan", AvgTimeToNextDestination = 100, Route = 2, ExitProbability = 0.3};
            ScheduleEvent(new PassengerArrivalEvent(_time + Padualaan2.InterArrivalTime), Padualaan2);
            var Heidelberglaan2 = new TramStop { Name = "Heidelberglaan", AvgTimeToNextDestination = 60, Route = 2, ExitProbability = 0.2};
            ScheduleEvent(new PassengerArrivalEvent(_time + Heidelberglaan2.InterArrivalTime), Heidelberglaan2);
            var UMC2 = new TramStop { Name = "UMC", AvgTimeToNextDestination = 82, Route = 2, ExitProbability = 0.1};
            ScheduleEvent(new PassengerArrivalEvent(_time + UMC2.InterArrivalTime), UMC2);
            var WKZ2 = new TramStop { Name = "WKZ", AvgTimeToNextDestination = 78, Route = 2, ExitProbability = 0.1};
            var PRDeUithof2 = new TramStop { Name = "P+R De Uithof", IsEndPoint = true, Route = 2, ExitProbability = 0.1};

            PRDeUithof.NextStop = WKZ;
            WKZ.NextStop = UMC;
            UMC.NextStop = Heidelberglaan;
            Heidelberglaan.NextStop = Padualaan;
            Padualaan.NextStop = KrommeRijn;
            KrommeRijn.NextStop = GalgenWaard;
            GalgenWaard.NextStop = VaartscheRijn;
            VaartscheRijn.NextStop = CentraalStation;

            CentraalStation2.NextStop = VaartscheRijn2;
            VaartscheRijn2.NextStop = GalgenWaard2;
            GalgenWaard2.NextStop = KrommeRijn2;
            KrommeRijn2.NextStop = Padualaan2;
            Padualaan2.NextStop = Heidelberglaan2;
            Heidelberglaan2.NextStop = UMC2;
            UMC2.NextStop = WKZ2;
            WKZ2.NextStop = PRDeUithof2;

            _route1 = PRDeUithof;
            _route2 = CentraalStation2;
        }

        private void ScheduleEvent(Event @event, AggregateRoot target)
        {
            var toSchedule = new ScheduledEvent { Event = @event, Target = target };
            _eventList.Add(toSchedule);
            // Sort the eventlist by ascending timestamp, so that the next event is always first in the list.
            _eventList = _eventList.OrderBy(e => e.Event.TimeStamp).ToList();
        }

        private ScheduledEvent GetNextEvent()
        {
            if (!_eventList.Any()) throw new InvalidOperationException("The eventlist is empty.");
            var e = _eventList[0];
            _eventList.RemoveAt(0);
            return e;
        }
    }

}
