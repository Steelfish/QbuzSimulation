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
            string outputPath = Path.Combine(Environment.CurrentDirectory, @"Data\Output");

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            string[][][][] input = LoadArtificialModel(path);
            var table = PassengerRates.ConvertArtificialInput(input);
            //Run simulatie met tijd = 15.5 uur, f = 15, q = 5, t = 8
            var system = new System(55800, 15, 5, 8, table);
            system.Run();

            Console.WriteLine();
            system.Export(outputPath);

            //TODO output uit systeem events genereren

            Console.ReadLine();
        }

        /// Load an artificial input model CSV file into the following structure:
        ///     stops   (all stops in the model, from P+R Uithof (0) to Centraal Station (8))
        ///         directions  (either 0 starting at P+R Uithof, or 1 starting at Centraal Station)
        ///             periods (the driving day from 6-21, where 0 corresponds with 6)
        ///                 passengers in/out   (either 0 for in, or 1 for out)
        ///                 
        /// Example request from this model:
        ///     The amount of passengers out at Heidelberglaan at 10:00 coming from Centraal Station.
        ///         stops = 3
        ///         directions = 1
        ///         periods = 9
        ///         passengers in/out = 1
        ///     model[3][1][9][1]
        public static string[][][][] LoadArtificialModel(string path)
        {
            string[][] input = File.ReadAllLines(path).Select(l => l.Split(';').ToArray()).ToArray();
            // Skip the header row.
            input = input.Skip(1).ToArray();

            string[] stops = { "P+R Uithof", "WKZ", "UMC", "Heidelberglaan", "Padualaan", "Kromme Rijn", "Galgenwaard", "Vaartscherijn", "Centraal Station Centrumzijde" };
            // Direction 0 is from P+R Uithof to Centraal Station Centrumzijde, direction 1 is in the opposite direction.
            string[] directions = { "0", "1" };
            // Every time period is from t:00:00 until t:59:59.
            //string[] periods = {"6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21"};
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
            string[] periods = { "6", "7", "7", "9", "9", "9", "9", "9", "9", "9", "16", "16", "18", "18", "18", "18" };

            // Initialise an empty passenger model sorted by stops, then directions, then periods, and finally passengers in or out.
            string[][][][] model = new string[stops.Length][][][];
            for (int i = 0; i < stops.Length; i++)
            {
                model[i] = new string[directions.Length][][];
                for (int j = 0; j < directions.Length; j++)
                {
                    model[i][j] = new string[periods.Length][];
                    for (int k = 0; k < periods.Length; k++)
                    {
                        model[i][j][k] = new string[2];
                    }
                }
            }

            // Fill the model with the corresponding passenger in/out data.
            foreach (string stop in stops)
            {
                foreach (string direction in directions)
                {
                    for (int k = 0; k < periods.Length; k++)
                    {
                        int index = Array.FindIndex(input, row => input[Array.IndexOf(input, row)][0] == stop && input[Array.IndexOf(input, row)][1] == direction && input[Array.IndexOf(input, row)][2] == periods[k]);

                        string passengersIn = input[index][4];
                        string passengersOut = input[index][5];
                        model[Array.IndexOf(stops, stop)][Array.IndexOf(directions, direction)][k][0] = passengersIn;
                        model[Array.IndexOf(stops, stop)][Array.IndexOf(directions, direction)][k][1] = passengersOut;
                    }
                }
            }

            return model;
        }
    }
}
