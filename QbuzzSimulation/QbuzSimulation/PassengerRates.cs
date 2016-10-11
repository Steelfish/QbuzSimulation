using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QbuzzSimulation
{
    public static class PassengerRates
    {
        public static List<TramStopRate> ConvertArtificialInput(string[][][][] input)
        {
            var result = new List<TramStopRate>();
            var resultPeriodes = Enumerable.Range(0, 62).Select(x => (x + 1) * 900).ToArray();
            string[] stops = { "P+R De Uithof", "WKZ", "UMC", "Heidelberglaan", "Padualaan", "Kromme Rijn", "Galgenwaard", "Vaartsche Rijn", "Centraal Station" };
            for (var i = 0; i < stops.Length; i++)
            {
                var period = 0;
                for (var j = 1; j <= 16; j++)
                {
                    double factor;
                    if (i == 1)
                        factor = 3600;
                    else if (i <= 3)
                        factor = 7200;
                    else if (i <= 10)
                        factor = 25200;
                    else if (i <= 12)
                        factor = 7200;
                    else
                        factor = 12600;
                    while (period < 62 && resultPeriodes[period] <= j * 3600)
                    {
                        result.Add(new TramStopRate(stops[i], 1, double.Parse(input[i][0][j - 1][0]) / factor, double.Parse(input[i][0][j - 1][1]), resultPeriodes[period]));
                        result.Add(new TramStopRate(stops[i], 2, double.Parse(input[i][1][j - 1][0]) / factor, double.Parse(input[i][1][j - 1][1]), resultPeriodes[period]));
                        period++;
                    }
                }
                while(period < 62)
                {
                    result.Add(new TramStopRate(stops[i], 1, double.Parse(input[i][0][15][0]) / 12600, double.Parse(input[i][0][15][1]), resultPeriodes[period]));
                    result.Add(new TramStopRate(stops[i], 2, double.Parse(input[i][1][15][0]) / 12600, double.Parse(input[i][1][15][1]), resultPeriodes[period]));
                    period++;
                }
            }
            return result;
        }
    }

    public class TramStopRate
    {
        public int TimeEnd { get; }
        public string Name { get; }
        public int Route { get; }
        public double RateIn { get; }
        public double RateOut { get; }

        public TramStopRate(string name, int route, double rateIn, double rateOut, int time)
        {
            Name = name;
            Route = route;
            RateIn = rateIn;
            RateOut = rateOut;
            TimeEnd = time;
        }
    }
}
