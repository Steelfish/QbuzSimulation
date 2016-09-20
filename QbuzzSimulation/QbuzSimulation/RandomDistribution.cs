﻿using System;

namespace QbuzzSimulation
{
    static class RandomDistribution
    {
        /// <summary>
        /// Generate a next Gaussian or normally distributed number based on a mean and standard deviation.
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="standardDeviation"></param>
        /// <returns></returns>
        public static double GenerateNextGaussian(float mean, float standardDeviation)
        {
            Random rand = new Random();
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();

            double randomStandardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randomNormal = mean + standardDeviation * randomStandardNormal;
            return randomNormal;
        }

        /// <summary>
        /// Generate a next Poisson number based on an average rate parameter.
        /// http://preshing.com/20111007/how-to-generate-random-timings-for-a-poisson-process/
        /// </summary>
        /// <param name="rateParameter"></param>
        /// <returns></returns>
        public static double GenerateNextPoisson(float rateParameter)
        {
            Random rnd = new Random();
            double randomValue = rnd.NextDouble();

            double number = -Math.Log(1.0f - randomValue) / rateParameter;
            return number;
        }
    }
}