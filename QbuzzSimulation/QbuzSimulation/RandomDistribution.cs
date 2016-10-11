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
        public static double GenerateNextGaussian(float mean, float standardDeviation)
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
        public static double GenerateNextEmpirical(float[] items)
        {
            double randomValue = rand.NextDouble();

            int n = items.Length;

            float total = 0;
            foreach (float item in items)
            {
                total += item;
            }

            float[] probabilities = new float[n];
            for (int i = 0; i < n; i++)
            {
                probabilities[i] = items[i] / total;
            }

            int index = 0;
            float sum = probabilities[0];
            while (randomValue <= sum)
            {
                index++;
                sum += probabilities[index];
            }
            return items[index];
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
        public static int GenerateNextPoisson(float rateParameter)
        {
            double randomValue = rand.NextDouble();

            double number = -Math.Log(1.0f - randomValue) / rateParameter;
            return (int)Math.Round(number, 0);
        }
    }
}
