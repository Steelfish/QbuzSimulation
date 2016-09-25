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
            string filename = "input-data-passengers-01.csv";
            string path = Path.Combine(Environment.CurrentDirectory, @"Data\Input\", filename);
            string[][][] input = LoadArtificialModel(path);

            //Run simulatie met tijd = 12 uur, f = 5, q = 5, t = 1
            var system = new System(43200, 1, 5, 1);
            system.Run();

            //TODO output uit systeem events genereren

           Console.ReadLine();
        }

        /// Load a CSV file into an array of rows and columns.
        /// Source: https://stackoverflow.com/questions/23292089/csv-file-contents-to-two-dimensional-array
        public static string[][] LoadCsv(string path)
        {
            return File.ReadAllLines(path).Select(l => l.Split(';').ToArray()).ToArray();
        }

        /// Load a CSV file into an array of rows and columns.
        public static string[][][] LoadArtificialModel(string path)
        {
            string[][] input = LoadCsv(path);
            // Skip the header row.
            input = input.Skip(1).ToArray();

            string[] stops = ["P+R Uithof", "WKZ", "UMC", "Heidelberglaan", "Padualaan", "Kromme Rijn", "Galgenwaard", "Vaartscherijn", "Centraal Station Centrumzijde"];
            // Direction 0 is from P+R Uithof to Centraal Station Centrumzijde, direction 1 is in the opposite direction.
            string[] directions = ["0", "1"];
            // Every time period is from t:00:00 until t:59:59.
            string[] periods = ["6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21"];
            //TODO all times mapped to:      To read passenger data for that timeslot
            /** 6:
                    6
                7-8:
                    7
                9-15:
                    9
                16-17:
                    16
                18-21:
                    18
            **/
          
            string[][][] model = new string[stops.Length][][];

            foreach (string stop in stops)
            {
                foreach (string direction in directions)
                {
                    foreach (string period in periods)
                    {
                        int index = Array.FindIndex(input, row => row.Contains(stop));
                        int directionIndex = Array.FindIndex(input, row => row.Contains(direction));
                        int periodIndex = Array.FindIndex(input, row => row.Contains(period));

                        string passengersIn = input[index][4];
                        string passengersOut = input[index][5];
                        model[stop][direction][period][in] = passengersIn;
                        model[stop][direction][period][out] = passengersOut;
                    }
                }
            }

            return model;
        }
    }
}
