using System;
using System.IO;
using System.Linq;

namespace QbuzzSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO Input van .csv's accepteren

            //Run simulatie met tijd = 12 uur, f = 5, q = 5, t = 1
            var system = new System(43200, 1, 5, 1);
            system.Run();

            //TODO output uit systeem events genereren

            Console.ReadLine();
        }

        /// Load a CSV file into an array of rows and columns.
        /// Source: https://stackoverflow.com/questions/23292089/csv-file-contents-to-two-dimensional-array
        public static string[][] LoadCsv(string filename)
        {
            return File.ReadAllLines(filename).Select(l => l.Split(',').ToArray()).ToArray();
        }
    }
}
