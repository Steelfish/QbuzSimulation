using System;

namespace QbuzzSimulation
{
    static class RandomDistribution
    {
        public static Random rand = new Random();

        /// <summary>
        /// Generate a next Gaussian or normally distributed number based on a mean and standard deviation.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="standardDeviation"></param>
        /// <returns></returns>
        public static double GenerateNextGaussian(double mean, double standardDeviation)
        {
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();

            double randomStandardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randomNormal = mean + standardDeviation * randomStandardNormal;
            return randomNormal;
        }

        /// <summary>
        /// Generate a next item based on the weights of each item.
        /// Uses a weighted selection wheel.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int GenerateNextEmpirical(double[] items)
        {
            if (items.Length == 1)
                return 0;
            Random rnd = new Random();
            double randomValue = rnd.NextDouble();

            int n = items.Length;

            double total = 0;
            foreach (double item in items)
            {
                total += item;
            }

            double[] probabilities = new double[n];
            for (int i = 0; i < n; i++)
            {
                probabilities[i] = items[i] / total;
            }

            int index = 0;
            double sum = 0;
            while (randomValue <= sum && index < n)
            {
                sum += probabilities[index];
                index++;
            }
            return index;
        }

        /// <summary>
        /// Generate a next Chaucy number based on a location parameter mu and scale parameter c.
        /// https://math.stackexchange.com/questions/484395/how-to-generate-a-cauchy-random-variable
        /// </summary>
        /// <param name="c"></param>
        /// <param name="mu"></param>
        /// <returns></returns>
        public static int GenerateNextCauchy(float mu, float c)
        {
            double randomValue = rand.NextDouble();

            double number = c * Math.Tan((randomValue - 0.5) * Math.PI) + mu;
            return (int)Math.Round(number, 0);
        }

        /// <summary>
        /// Generate a next Poisson number based on an average rate parameter.
        /// http://preshing.com/20111007/how-to-generate-random-timings-for-a-poisson-process/
        /// </summary>
        /// <param name="rateParameter"></param>
        /// <returns></returns>
        public static int GenerateNextPoisson(double rateParameter)
        {
            double randomValue = rand.NextDouble();

            double number = -Math.Log(1.0f - randomValue) / rateParameter;
            return (int)Math.Round(number, 0);
        }


    }
}
