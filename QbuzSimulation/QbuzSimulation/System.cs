using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace QbuzSimulation
{

    //System klasse die verantwoordelijk is voor het draaien van de simulatie
    public static class System
    {
        //Tijd in seconden
        private static int _time;
        private static TramStop _start;
        private static readonly List<Tram> Trams = new List<Tram>();

        //Maakt route aan
        private static void InitializeRoute(int q)
        {
            var PRDeUithof = new TramStop { Name = "P+R De Uithof", AvgTimeToNextDestination = 110 };
            var WKZ = new TramStop { Name = "WKZ", AvgTimeToNextDestination = 78 };
            var UMC = new TramStop { Name = "UMC", AvgTimeToNextDestination = 82 };
            var Heidelberglaan = new TramStop { Name = "Heidelberglaan", AvgTimeToNextDestination = 60 };
            var Padualaan = new TramStop { Name = "Padualaan", AvgTimeToNextDestination = 100 };
            var KrommeRijn = new TramStop { Name = "Kromme Rijn", AvgTimeToNextDestination = 59 };
            var GalgenWaard = new TramStop { Name = "Galgenwaard", AvgTimeToNextDestination = 243 };
            var VaartscheRijn = new TramStop { Name = "Vaartsche Rijn", AvgTimeToNextDestination = 135 };
            var CentraalStation = new TramStop { Name = "Centraal Station", AvgTimeToNextDestination = q };
            var CentraalStation2 = new TramStop {Name = "Centraal Station", AvgTimeToNextDestination = 134};
            var VaartscheRijn2 = new TramStop {Name = "Vaartsche Rijn", AvgTimeToNextDestination = 243};
            var GalgenWaard2 = new TramStop {Name = "Galgenwaard", AvgTimeToNextDestination = 59};
            var KrommeRijn2 = new TramStop {Name = "Kromme Rijn", AvgTimeToNextDestination = 59};
            var Padualaan2 = new TramStop { Name = "Padualaan", AvgTimeToNextDestination = 100 };
            var Heidelberglaan2 = new TramStop { Name = "Heidelberglaan", AvgTimeToNextDestination = 60 };
            var UMC2 = new TramStop { Name = "UMC", AvgTimeToNextDestination = 82 };
            var WKZ2 = new TramStop { Name = "WKZ", AvgTimeToNextDestination = 78 };
            var PRDeUithof2 = new TramStop { Name = "P+R De Uithof", AvgTimeToNextDestination = q };

            PRDeUithof.NextStop = WKZ;
            WKZ.NextStop = UMC;
            UMC.NextStop = Heidelberglaan;
            Heidelberglaan.NextStop = Padualaan;
            Padualaan.NextStop = KrommeRijn;
            KrommeRijn.NextStop = GalgenWaard;
            GalgenWaard.NextStop = VaartscheRijn;
            VaartscheRijn.NextStop = CentraalStation;
            CentraalStation.NextStop = CentraalStation2;
            CentraalStation2.NextStop = VaartscheRijn2;
            VaartscheRijn2.NextStop = GalgenWaard2;
            GalgenWaard2.NextStop = KrommeRijn2;
            KrommeRijn2.NextStop = Padualaan2;
            Padualaan2.NextStop = Heidelberglaan2;
            Heidelberglaan2.NextStop = UMC2;
            UMC2.NextStop = WKZ2;
            WKZ2.NextStop = PRDeUithof2;
            PRDeUithof2.NextStop = PRDeUithof;

            _start = PRDeUithof;
        }
        
        public static void Run(int maxTime, int f, int q)
        {
            InitializeRoute(q);
            var trams = (34 + q)/(60/f);
            while (_time < maxTime)
            {
                //Handel trams af
                foreach (var tram in Trams)
                {
                    //TODO events zoals breken van trams toevoegen
                    tram.Drive(_time);
                }
                //Voeg trams toe als nodig
                if (trams >= Trams.Count)
                {
                    if (_time%(60/f*60) == 0)
                    {
                        var tram = new Tram { Destination = _start };
                        tram.Start(_time);
                        Trams.Add((tram));
                    }
                }
                //TODO Passagiers aan tramhaltes toevoegens
                _time += 1;
            }
        }

    }


}
