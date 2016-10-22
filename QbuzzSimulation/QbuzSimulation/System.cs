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
        private readonly List<TramStop> _stops = new List<TramStop>();
        private List<ScheduledEvent> _eventList = new List<ScheduledEvent>();

        //Run input
        private readonly int _maxTime;
        private readonly int _f;
        private readonly int _q;
        private readonly int _t;
        private readonly List<TramStopRate> _passengerRates;

        private int _time;
        private int _route1NextStart;
        private int _route2NextStart;
        private int _ridesRoute1;
        private int _ridesRoute2;
        //P+R -> CS
        private TramStop _route1;
        private TramStop _route1End;
        //CS -> P+R
        private TramStop _route2;
        private TramStop _route2End;

        //Meetpunten
        private readonly List<int> _delaysRoute1 = new List<int>();
        private readonly List<int> _delaysRoute2 = new List<int>();


        public System(int maxTime, int f, int q, int t, List<TramStopRate> passengerRates)
        {
            _maxTime = maxTime;
            _f = f;
            _q = q;
            _t = t;
            _passengerRates = passengerRates;
            InitializeRoute();

            foreach (var i in Enumerable.Range(0, 61).Select(x => (x + 1) * 900))
            {
                var stop = _route1;
                while (!stop.IsEndPoint)
                {
                    ScheduleEvent(new ArrivalRateChangeEvent(i), stop);
                    stop = stop.NextStop;
                }
                stop = _route2;
                while (!stop.IsEndPoint)
                {
                    ScheduleEvent(new ArrivalRateChangeEvent(i), stop);
                    stop = stop.NextStop;
                }
            }
        }

        public void Run()
        {
            for (var i = 0; i < _t; i++)
            {
                var tram1 = new Tram(_route1) { StartWaiting = _time };
                var tram2 = new Tram(_route2) { StartWaiting = _time };
                _trams.Add(tram1);
                _trams.Add(tram2);
                if (i == 0)
                {
                    ScheduleEvent(new TramStartEvent(_route1NextStart), tram1);
                    ScheduleEvent(new TramStartEvent(_route2NextStart), tram2);
                }
                else
                {
                    ScheduleEvent(new TramEstimatedStartEvent(_route1NextStart), tram1);
                    ScheduleEvent(new TramEstimatedStartEvent(_route2NextStart), tram2);
                }
                _route1NextStart += 60 / _f * 60;
                _route2NextStart += 60 / _f * 60;
            }
            for (var i = 0; i < _trams.Count; i++)
            {
                _trams[i].Index = i;
                _trams[i].Ahead = _trams[(_trams.Count + i - 2) % _trams.Count];
                _trams[i].Behind = _trams[(i + 2) % _trams.Count];
            }
            _ridesRoute1 = 1;
            _ridesRoute2 = 1;
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
                    switch (nextEvent.Event.Name)
                    {
                        case PassengerArrivalEvent.Name:
                            _passengers.Add(tramstop.Passengers.Last());
                            ScheduleNewArrivalEvent(tramstop);
                            break;
                        case ArrivalRateChangeEvent.Name:
                            if (!tramstop.GeneratingEvents)
                                ScheduleNewArrivalEvent(tramstop);
                            break;
                    }
                }

                nextEvent = GetNextEvent();
            }
        }

        private void Operate(Tram tram, Event @event)
        {
            tram.ApplyChange(@event);
            switch (@event.Name)
            {
                case TramEstimatedStartEvent.Name:
                    if (tram.DeltaT > 0 && !tram.Behind.Waiting)
                    {
                        ScheduleEvent(new TramEstimatedStartEvent(_time + tram.DeltaT), tram);
                    }
                    else
                    {
                        if (tram.Destination == _route1)
                        {
                            _delaysRoute1.Add(_time - (_ridesRoute1++ * (60 / _f * 60)));
                            _route2End.Occupied.Remove(tram);
                            var toStart = _route2End.Occupied.Where(t => t.Waiting).FirstOrDefault();
                            if (toStart != null)
                                ScheduleEvent(new TramChangeTrackEvent(_time + 40, _route1), toStart);
                        }
                        else if (tram.Destination == _route2)
                        {
                            _delaysRoute2.Add(_time - (_ridesRoute2++ * (60 / _f * 60)));
                            var removed = _route1End.Occupied.Remove(tram);
                            var toStart = _route1End.Occupied.Where(t => t.Waiting).FirstOrDefault();
                            if (toStart != null && removed)
                                ScheduleEvent(new TramChangeTrackEvent(_time + 40, _route2), toStart);
                        }
                        //Should probably schedule TramEstimatedStop.
                        ScheduleEvent(new TramStartEvent(_time), tram);
                    }
                    break;
                //Might be redundant.
                case TramStartEvent.Name:
                    ScheduleEvent(new TramStopEvent(_time + tram.DeltaT), tram);
                    if (tram.Behind.Waiting && !tram.Behind.AtEndPoint)
                    {
                        ScheduleEvent(new TramEstimatedStartEvent(_time + 40), tram.Behind);
                    }
                    break;
                case TramStopEvent.Name:
                    //Todo tram waiting on end points
                    if (!tram.Waiting)
                    {
                        if (tram.AtEndPoint)
                            ScheduleEvent(new TramChangeTrackEvent(_time + tram.DeltaT, tram.Route == 1 ? _route2 : _route1), tram);
                        else
                            ScheduleEvent(new TramEstimatedStartEvent(_time + tram.DeltaT), tram);
                    }
                    break;
                case TramChangeTrackEvent.Name:
                    if (tram.Route == 1 && _route1NextStart < _time + _q
                     || tram.Route == 2 && _route2NextStart < _time + _q)
                    {
                        ScheduleEvent(new TramEstimatedStartEvent(_time + _q), tram);
                    }
                    else
                    {
                        tram.StartWaiting = _time;
                        ScheduleEvent(new TramEstimatedStartEvent(tram.Route == 1 ? _route1NextStart : _route2NextStart), tram);
                    }
                    if (tram.Route == 1)
                        _route1NextStart += 60 / _f * 60;
                    else
                        _route2NextStart += 60 / _f * 60;
                    break;
            }

        }

        //Maakt route aan
        private void InitializeRoute()
        {
            var PRDeUithof = new TramStop { Name = "P+R De Uithof", DistanceToNextStop = 600, AvgTimeToNextDestination = 110, Route = 1 };
            var WKZ = new TramStop { Name = "WKZ", DistanceToNextStop = 600, AvgTimeToNextDestination = 78, Route = 1 };
            var UMC = new TramStop { Name = "UMC", DistanceToNextStop = 400, AvgTimeToNextDestination = 82, Route = 1 };
            var Heidelberglaan = new TramStop { Name = "Heidelberglaan", DistanceToNextStop = 400, AvgTimeToNextDestination = 60, Route = 1 };
            var Padualaan = new TramStop { Name = "Padualaan", DistanceToNextStop = 800, AvgTimeToNextDestination = 100, Route = 1 };
            var KrommeRijn = new TramStop { Name = "Kromme Rijn", DistanceToNextStop = 600, AvgTimeToNextDestination = 59, Route = 1 };
            var GalgenWaard = new TramStop { Name = "Galgenwaard", DistanceToNextStop = 3100, AvgTimeToNextDestination = 243, Route = 1 };
            var VaartscheRijn = new TramStop { Name = "Vaartsche Rijn", DistanceToNextStop = 1400, AvgTimeToNextDestination = 135, Route = 1 };
            var CentraalStation = new TramStop { Name = "Centraal Station", IsEndPoint = true, Route = 1 };
            var CentraalStation2 = new TramStop { Name = "Centraal Station", DistanceToNextStop = 1400, AvgTimeToNextDestination = 134, Route = 2 };
            var VaartscheRijn2 = new TramStop { Name = "Vaartsche Rijn", DistanceToNextStop = 3100, AvgTimeToNextDestination = 243, Route = 2 };
            var GalgenWaard2 = new TramStop { Name = "Galgenwaard", DistanceToNextStop = 600, AvgTimeToNextDestination = 59, Route = 2 };
            var KrommeRijn2 = new TramStop { Name = "Kromme Rijn", DistanceToNextStop = 800, AvgTimeToNextDestination = 101, Route = 2 };
            var Padualaan2 = new TramStop { Name = "Padualaan", DistanceToNextStop = 400, AvgTimeToNextDestination = 60, Route = 2 };
            var Heidelberglaan2 = new TramStop { Name = "Heidelberglaan", DistanceToNextStop = 400, AvgTimeToNextDestination = 86, Route = 2 };
            var UMC2 = new TramStop { Name = "UMC", DistanceToNextStop = 600, AvgTimeToNextDestination = 78, Route = 2 };
            var WKZ2 = new TramStop { Name = "WKZ", DistanceToNextStop = 600, AvgTimeToNextDestination = 113, Route = 2 };
            var PRDeUithof2 = new TramStop { Name = "P+R De Uithof", IsEndPoint = true, Route = 2 };

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
            _route1End = CentraalStation;
            _route2 = CentraalStation2;
            _route2End = PRDeUithof2;

            var stop = PRDeUithof;
            while (!stop.IsEndPoint)
            {
                _stops.Add(stop);
                ScheduleNewArrivalEvent(stop);
                stop = stop.NextStop;
            }
            stop = CentraalStation2;
            while (!stop.IsEndPoint)
            {
                _stops.Add(stop);
                ScheduleNewArrivalEvent(stop);
                stop = stop.NextStop;
            }
        }

        public void Export(string outputPath)
        {
            var measurements = Path.Combine(outputPath, "measurements.txt");
            var drivingTimes = Path.Combine(outputPath, "drivingtimes.csv");
            var tramEventPath = Path.Combine(outputPath, "Debug/Trams");
            var tramstopEventPath = Path.Combine(outputPath, "Debug/Tramstops");
            if (!Directory.Exists(tramEventPath))
                Directory.CreateDirectory(tramEventPath);
            if (!Directory.Exists(tramstopEventPath))
                Directory.CreateDirectory(tramstopEventPath);

            //Tram stats
            var delayPercentage1 = (double)_delaysRoute1.Where(x => x > 60).Count() / (_delaysRoute1.Count + _delaysRoute2.Count);
            var delayPercentage2 = (double)_delaysRoute2.Where(x => x > 60).Count() / (_delaysRoute1.Count + _delaysRoute2.Count);
            foreach (int i in _delaysRoute1)
            {
                Console.WriteLine(i);
            }
            var delayPercentage = (delayPercentage1 + delayPercentage2) * 100;
            var avgDelay = (double)(_delaysRoute1.Sum() + _delaysRoute2.Sum()) / (_delaysRoute1.Count + _delaysRoute2.Count);
            var maxDelay = _delaysRoute1.Max() > _delaysRoute2.Max() ? _delaysRoute1.Max() : _delaysRoute2.Max();
            //Passengers stats
            var passengers = _passengers.Where(p => p.Participated).ToList();
            var pDelayPercentage = (double)passengers.Count(p => p.WaitTime > 300) / passengers.Count() * 100;
            //Tramstop stats
            var maxQueueLength = _stops.Max(x => x.MaxQueueLength);
            var avgQueueLength = _stops.Sum(x => x.GetQueueLengthOverTime(_time)) / _time / _stops.Count;
            var stats = new[]
            {
                "TRAMS",
                "",
                $"% with more than 1 minute delay: {delayPercentage}",
                $"Average delay: {avgDelay}",
                $"Max delay: {maxDelay}",
                "",
                "PASSENGERS",
                "",
                $"% with more than 5 minute delay: {pDelayPercentage}",
                $"Average delay: {passengers.Average(p => p.WaitTime)}",
                $"Max delay: {passengers.Max(p => p.WaitTime)}",
                $"Passengers transported: {passengers.Count(p => p.Participated)}",
                "",
                "STOPS",
                "",
                $"Max queue length: {maxQueueLength}",
                $"Average queue length: {avgQueueLength}"
            };

            File.WriteAllLines(measurements, stats);
            foreach (var stat in stats) Console.WriteLine(stat);

            for (var i = 0; i < _trams.Count; i++)
            {
                var path = Path.Combine(tramEventPath, $"Tram{i}.csv");
                using (var writer = new StreamWriter(path))
                {
                    writer.WriteLine("Timestap;Event name");
                    foreach (var line in _trams[i].ExportEvents())
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            foreach (var t in _stops)
            {
                var path = Path.Combine(tramstopEventPath, $"{t.Name}_{t.Route}.csv");
                using (var writer = new StreamWriter(path))
                {
                    writer.WriteLine("Timestap;Event name");
                    foreach (var line in t.ExportEvents())
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            var driveTimes = new List<List<int>>();
            for (var i = 0; i < 16; i++)
            {
                driveTimes.Add(new List<int>());
            }
            foreach (var tram in _trams)
            {
                var index = tram.Route == 1 ? 0 : 8;
                foreach (var times in tram.ExportDrivingTimes().Split(16))
                {
                    driveTimes[index].AddRange(times);
                    index = (index + 1) % 16;
                }
            }
            using (var writer = new StreamWriter(drivingTimes))
            {
                writer.WriteLine("Rijtijden vanaf halte:" + Environment.NewLine);
                writer.WriteLine("P+R;WKZ;UMC;Heidelberglaan;Padualaan;Kromme Rijn;Galgenwaard;Vaartsche Rijn;Centraal Station;Vaartsche Rijn;Galgenwaard;Kromme Rijn;Padualaan;Heidelberglaan;UMC;WKZ");
                for (var i = 0; i < driveTimes[1].Count; i++)
                {
                    var str = "";
                    for (var j = 0; j < 16; j++)
                    {
                        str += (driveTimes[j].Count > i) ? driveTimes[j][i] + ";" : ";";
                    }
                    writer.WriteLine(str);
                }
            }
        }

        private void ScheduleNewArrivalEvent(TramStop stop)
        {
            var rates = _passengerRates.ToArray();
            var arrivalRate = rates.Where(r => r.Route == stop.Route && r.Name == stop.Name && r.TimeEnd == _time + 900 - _time % 900).First();
            if (Math.Abs(arrivalRate.RateIn) < 0.0001)
            {
                stop.GeneratingEvents = false;
                return;
            }
            else
                stop.GeneratingEvents = true;
            var time = _time + RandomDistribution.GenerateNextPoisson(arrivalRate.RateIn);

            if (stop.Route == 2)
                rates = rates.Reverse().ToArray();
            var routes = rates.Where(r => r.Route == stop.Route && r.TimeEnd == _time + 900 - _time % 900)
                    .SkipWhile(r => r.Name != stop.Name).Skip(1).ToArray();
            var destination = routes[RandomDistribution.GenerateNextEmpirical(routes.Select(r => r.RateOut).ToArray())].Name;
            ScheduleEvent(new PassengerArrivalEvent(time, destination), stop);
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
