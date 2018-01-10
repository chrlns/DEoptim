using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    class RandomUtils
    {
        /// <summary>
        /// Inits the parameter array (agent) with randomized values.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="rng"></param>
        public static void RandomizeAgent(double[] agent, double[] lowerLimit, double[] upperLimit, Random rng)
        {
            for (int n = 0; n < agent.Length; n++)
            {
                agent[n] = RandomizeDouble(lowerLimit[n], upperLimit[n], rng);
            }
        }

        /// <summary>
        /// Returns a randomized double from the interval [lowerLimit ... upperLimit].
        /// </summary>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        public static double RandomizeDouble(double lowerLimit, double upperLimit, Random rng)
        {
            double range = upperLimit - lowerLimit;
            return rng.NextDouble() * range + lowerLimit;
        }
    }
}
