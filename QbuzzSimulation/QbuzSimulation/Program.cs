using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace QbuzzSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = "input-data.csv";

            string path = Path.Combine(Environment.CurrentDirectory, @"Data\Input\", filename);
            string outputPath = Path.Combine(Environment.CurrentDirectory, @"Data\Output");

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            // Every time period is from t:00:00 until t:59:59.
            string[] periods = { "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21" };

            string[][][][] input = LoadModel(path, periods);
            var table = PassengerRates.ConvertInput(input);

            foreach (float rateMultiplier in new float[]{ 1.0f, 1.5f, 2.0f})
            {
                var multipliedTable = new List<TramStopRate>();
                foreach (TramStopRate stopRate in table)
                {
                    TramStopRate multipliedStopRate = new TramStopRate(stopRate.Name, stopRate.Route,
                                                                       stopRate.RateIn * rateMultiplier, stopRate.RateOut * rateMultiplier, stopRate.TimeEnd);
                    multipliedTable.Add(multipliedStopRate);
                }


                // All settings are an array of time, frequency, switching time q and the amount of trams.
                // 55800 seconds is 15.5 hours.
                int[] frequencies = { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30 };
                int[] amountOfTrams = { 8, 10, 12, 14, 16, 18, 20, 22 };

                int[][] settings = new int[frequencies.Length * amountOfTrams.Length][];

                for (int i = 0; i < frequencies.Length; i++)
                {
                    for (int j = 0; j < amountOfTrams.Length; j++)
                    {
                        settings[i * amountOfTrams.Length + j] = new int[] { 55800, frequencies[i], 300, amountOfTrams[j] };
                    }
                }

                // Perform this amount of runs per setting.
                int numberOfRuns = 20;


                foreach (int[] setting in settings)
                {
                    string[] results = new string[numberOfRuns];

                    int time = setting[0];
                    int frequency = setting[1];
                    int q = setting[2];
                    int trams = setting[3];
                    string settingString = rateMultiplier.ToString() + "_" + time.ToString() + "-" + frequency.ToString() + "-" + q.ToString() + "-" + trams.ToString();

                    for (int run = 1; run <= numberOfRuns; run++)
                    {
                        var system = new System(time, frequency, q, trams, multipliedTable);
                        system.Run(false);

                        Console.WriteLine();
                        system.Export(outputPath, settingString, run);
                        results[run - 1] = system.Results();
                    }

                    var measurementsCsv = Path.Combine(outputPath, "measurements_" + settingString + ".csv");
                    File.WriteAllLines(measurementsCsv, results);
                }
            }


            //string[] artificialFilenames = { "input-data-passengers-01.csv", "input-data-passengers-02.csv", "input-data-passengers-03.csv", "input-data-passengers-04.csv", "input-data-passengers-06.csv", "input-data-passengers-15.csv", "input-data-passengers-25.csv" };
            //foreach (string artificialFilename in artificialFilenames)
            //{
            //    string[][][][] artificialInput = LoadArtificialModel(path);
            //    var artificialTable = PassengerRates.ConvertArtificialInput(input);

            //    // Settings for the artificial models.
            //    int[] frequencies = { 9, 15 };
            //    int[] amountOfTrams = { 10, 16 };

            //    int[][] settings = new int[frequencies.Length * amountOfTrams.Length][];

            //    for (int i = 0; i < frequencies.Length; i++)
            //    {
            //        for (int j = 0; j < amountOfTrams.Length; j++)
            //        {
            //            settings[i * amountOfTrams.Length + j] = new int[] { 55800, frequencies[i], 300, amountOfTrams[j] };
            //        }
            //    }

            //    // Perform this amount of runs per setting.
            //    int numberOfRuns = 20;


            //    foreach (int[] setting in settings)
            //    {
            //        string[] results = new string[numberOfRuns];

            //        int time = setting[0];
            //        int frequency = setting[1];
            //        int q = setting[2];
            //        int trams = setting[3];
            //        string settingString = time.ToString() + "-" + frequency.ToString() + "-" + q.ToString() + "-" + trams.ToString();

            //        for (int run = 1; run <= numberOfRuns; run++)
            //        {
            //            var system = new System(time, frequency, q, trams, table);
            //            system.Run(false);

            //            Console.WriteLine();
            //            system.Export(outputPath, settingString, run);
            //            results[run - 1] = system.Results();
            //        }

            //        var measurementsCsv = Path.Combine(outputPath, artificialFilename.Remove(artificialFilename.Length - 4) + "_measurements_" + settingString + ".csv");
            //        File.WriteAllLines(measurementsCsv, results);
            //    }
            //}

            Console.ReadLine();
        }

        /// Load an input model CSV file into the following structure:
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
        public static string[][][][] LoadModel(string path, string[] periods)
        {
            string[][] input = File.ReadAllLines(path).Select(l => l.Split(';').ToArray()).ToArray();
            // Skip the header row.
            input = input.Skip(1).ToArray();

            string[] stops = { "P+R Uithof", "WKZ", "UMC", "Heidelberglaan", "Padualaan", "Kromme Rijn", "Galgenwaard", "Vaartscherijn", "Centraal Station Centrumzijde" };
            // Direction 0 is from P+R Uithof to Centraal Station Centrumzijde, direction 1 is in the opposite direction.
            string[] directions = { "0", "1" };

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


        /// Load an artificial input model CSV file into the following structure:
        ///     stops   (all stops in the model, from P+R Uithof (0) to Centraal Station (8))
        ///         directions  (either 0 starting at P+R Uithof, or 1 starting at Centraal Station)
        ///             periods (the driving day from 6-21, where 0 corresponds with 6)
        ///                 passengers in/out   (either 0 for in, or 1 for out)           
        public static string[][][][] LoadArtificialModel(string path)
        {
            string[] periods = { "6", "7", "7", "9", "9", "9", "9", "9", "9", "9", "16", "16", "18", "18", "18", "18" };

            string[][][][] model = LoadModel(path, periods);

            return model;
        }
    }
}
