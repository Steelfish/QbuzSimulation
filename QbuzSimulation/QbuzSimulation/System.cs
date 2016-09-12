using System;
using System.Collections.Generic;
using System.Linq;


namespace QbuzSimulation
{
    //System klasse die verantwoordelijk is voor het draaien van de simulatie
    public class System
    {

        private readonly Random _random = new Random();
        private readonly List<Tram> _trams = new List<Tram>();
        private readonly List<Passenger> _passengers = new List<Passenger>(); 

        //Run input
        private int _maxTime;
        private int _f;
        private int _q;

        private int _time;
        private int _route1TimeTable;
        private int _route2TimeTable;
        //P+R -> CS
        private TramStop _route1;
        //CS -> P+R
        private TramStop _route2;

        //Meetpuntent
        private int _delay;

        //Maakt route aan
        private void InitializeRoute()
        {
            var PRDeUithof = new TramStop { Name = "P+R De Uithof", AvgTimeToNextDestination = 110, Route = 1 };
            var WKZ = new TramStop { Name = "WKZ", AvgTimeToNextDestination = 78, Route = 1 };
            var UMC = new TramStop { Name = "UMC", AvgTimeToNextDestination = 82, Route = 1 };
            var Heidelberglaan = new TramStop { Name = "Heidelberglaan", AvgTimeToNextDestination = 60, Route = 1 };
            var Padualaan = new TramStop { Name = "Padualaan", AvgTimeToNextDestination = 100, Route = 1 };
            var KrommeRijn = new TramStop { Name = "Kromme Rijn", AvgTimeToNextDestination = 59, Route = 1 };
            var GalgenWaard = new TramStop { Name = "Galgenwaard", AvgTimeToNextDestination = 243, Route = 1 };
            var VaartscheRijn = new TramStop { Name = "Vaartsche Rijn", AvgTimeToNextDestination = 135, Route = 1 };
            var CentraalStation = new TramStop { Name = "Centraal Station", IsEndPoint = true, Route = 1 };

            var CentraalStation2 = new TramStop { Name = "Centraal Station", AvgTimeToNextDestination = 134, Route = 2 };
            var VaartscheRijn2 = new TramStop { Name = "Vaartsche Rijn", AvgTimeToNextDestination = 243, Route = 2 };
            var GalgenWaard2 = new TramStop { Name = "Galgenwaard", AvgTimeToNextDestination = 59, Route = 2 };
            var KrommeRijn2 = new TramStop { Name = "Kromme Rijn", AvgTimeToNextDestination = 59, Route = 2 };
            var Padualaan2 = new TramStop { Name = "Padualaan", AvgTimeToNextDestination = 100, Route = 2 };
            var Heidelberglaan2 = new TramStop { Name = "Heidelberglaan", AvgTimeToNextDestination = 60, Route = 2 };
            var UMC2 = new TramStop { Name = "UMC", AvgTimeToNextDestination = 82, Route = 2 };
            var WKZ2 = new TramStop { Name = "WKZ", AvgTimeToNextDestination = 78, Route = 2 };
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
            _route2 = CentraalStation2;
        }

        public System(int maxTime, int f, int q)
        {
            _maxTime = maxTime;
            _f = f;
            _q = q;
            InitializeRoute();
        }

        public void Run()
        {
            var trams = (34 + _q) / (60 / _f);
            _trams.Add(new Tram(_route1, _q));
            _trams.Add(new Tram(_route2, _q));
            while (_time < _maxTime)
            {
                //Handel trams af
                foreach (var tram in _trams)
                {
                    Operate(tram);
                }

                AddPassengersToRoute(_route1);
                AddPassengersToRoute(_route2);
                _time += 1;
            }

            Console.WriteLine();
            Console.WriteLine("Total Delay: {0}", _delay);
            Console.WriteLine("Total passenger wait time: {0}", _passengers.Where(p => p.WaitTime >= 0).Sum(p => p.WaitTime));
            Console.WriteLine("Total passengers: {0}", _passengers.Count);
            Console.WriteLine("Avg. waittime / passenger: {0}", _passengers.Where(p => p.WaitTime >= 0).Average(p => p.WaitTime));
        }

        private void Operate(Tram tram)
        {
            //TODO events zoals breken van trams toevoegen
            if (tram.DeltaT > 0)
                tram.DeltaT--;
            else if (tram.Driving)
            {
                tram.Stop(_time);
            }
            else if (tram.AtEndPoint)
            {
                tram.ChangeTrack(_time, tram.Route == 1 ? _route2 : _route1);
            }
            else if (tram.EnRoute)
            {
                tram.Start(_time);
            }
            //Start route 1 en tijd om te vertrekken
            else if (tram.Route == 1 && _route1TimeTable <= _time)
            {
                tram.Start(_time);
                //Update punctualiteit en volgende vertrektijd
                _delay += _time - _route1TimeTable;
                _route1TimeTable += 60 / _f * 60;
            }
            //Start route 2 en tijd om te vertrekken
            else if (tram.Route == 2 && _route2TimeTable <= _time)
            {
                tram.Start(_time);
                //Update punctualiteit en volgende vertrektijd
                _delay += _time - _route2TimeTable;
                _route2TimeTable += 60 / _f * 60;
            }
            //Waiting at station
            else
            {
                tram.PassengersEnter(_time);
            }
        }

        //Todo verander randomness naar poisson functie afhankelijk van stop
        private void AddPassengersToRoute(TramStop route)
        {
            while (!route.IsEndPoint)
            {
                if (_random.Next(0, 100) >= 99)
                {
                    var passenger = new Passenger(_time, GetRandomDestination(route.Route == 1 ? _route1 : _route2));
                    _passengers.Add(passenger);
                    route.Passengers.Add(passenger);
                }
                route = route.NextStop;
            }
        }
        //Todo verander randomness m.b.v. cumulatieve distributie op tramstops.
        private string GetRandomDestination(TramStop route)
        {
            var r = _random.Next(0, 9);
            var destination = _route1;
            while (r > 0)
            {
                destination = _route1.NextStop;
                r--;
            }
            return destination.Name;
        }
    }

}
