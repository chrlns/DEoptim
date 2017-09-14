using System;
using System.Collections.Generic;
using System.Linq;

namespace DEoptim
{
    /// <summary>
    /// Optimizes parameters of a non-linear function using 
    /// Differential Evolution algorithm.
    /// </summary>
    public class Optimizer
    {
        /// <summary>
        /// Function to be minimized.
        /// double func(double[] parameter, double[] variables)
        /// </summary>
        protected Func<double[], double[], double> function;

        protected int iterationsUsed;
        public int IterationsUsed => iterationsUsed;

        protected int numParameters;

        protected double[][] population;
        protected double[] cost;

        protected double F = 0.8;
        public double FWeight
        {
            get
            {
                return F;
            }
            set
            {
                F = value;
            }
        }

        protected int minCostIdx = 0;
        protected double[] lowerLimit, upperLimit;

        public Optimizer(
            Func<double[], double[], double> function, int numParameters,
            double lowerLimit, double upperLimit, int populationSize = 100)
        {
            double[] upper = new double[numParameters];
            double[] lower = new double[numParameters];
            for (int n = 0; n < numParameters; n++)
            {
                lower[n] = lowerLimit;
                upper[n] = upperLimit;
            }
            Init(function, numParameters, lower, upper, populationSize);
        }

        public Optimizer(
            Func<double[], double[], double> function, int numParameters,
            double[] lowerLimit, double[] upperLimit, int populationSize = 100)
        {
            Init(function, numParameters, lowerLimit, upperLimit, populationSize);
        }

        private void Init(Func<double[], double[], double> function, int numParameters,
            double[] lowerLimit, double[] upperLimit, int populationSize = 100)
        { 
            this.function = function;
            this.numParameters = numParameters;
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;

            // Initialize populations
            Random rng = new Random();
            population = new double[populationSize][];
            cost = new double[populationSize];
            for (int n = 0; n < populationSize; n++)
            {
                population[n] = new double[numParameters];
                RandomizeAgent(population[n], lowerLimit, upperLimit, rng);
            }
        }

        /// <summary>
        /// Inits the parameter array (agent) with randomized values.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lowerLimit"></param>
        /// <param name="upperLimit"></param>
        /// <param name="rng"></param>
        private void RandomizeAgent(double[] agent, double[] lowerLimit, double[] upperLimit, Random rng)
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
        public double RandomizeDouble(double lowerLimit, double upperLimit, Random rng)
        {
            double range = upperLimit - lowerLimit;
            return rng.NextDouble() * range + lowerLimit;
        }

        /// <summary>
        /// Evaluates the given individual (agent).
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="sampleSet"></param>
        /// <returns></returns>
        public double Cost(double[] agent, Sample[] sampleSet)
        {
            double cost = 0;

            foreach (Sample sample in sampleSet)
            {
                double err = sample.Outcome - function(agent, sample.Variables);
                cost += err * err;
            }

            return cost;
        }

        /// <summary>
        /// Selects a random individual index.
        /// </summary>
        /// <param name="except"></param>
        /// <param name="rng"></param>
        /// <returns></returns>
        private int PickRandomAgent(int[] except, Random rng)
        {
            int idx = rng.Next(population.Length);
            while (Array.IndexOf(except, idx) >= 0)
                idx = rng.Next(population.Length);
            return idx;
        }

        /// <summary>
        /// Checks if v is within the specified limits. If it violates the interval
        /// boundaries, a newly randomized value is returned. Otherwise v is returned.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rng"></param>
        /// <param name="idx"></param>
        /// <returns>v if within bounds, a randomized value otherwise.</returns>
        private double EnsureBounds(double v, int idx, Random rng)
        {
            if (v < lowerLimit[idx] || v > upperLimit[idx])
                return RandomizeDouble(lowerLimit[idx], upperLimit[idx], rng);
            else
                return v;
        }

        /// <summary>
        /// Wipes a random fraction of the population.
        /// </summary>
        /// <param name="retainFactor">
        /// Normalized [0..1] value indicating which percentage of the
        /// population must be wiped. Setting retainFactor to 0 will reset
        /// every individual of the population.
        /// </param>
        public void WipePopulation(float retainFactor)
        {
            Random rng = new Random();
            int numWipes = population.Length - (int)(population.Length * retainFactor);
            while (numWipes > 0)
            {
                int n = rng.Next(population.Length);
                if (cost[n] < double.MaxValue - 1)
                {
                    RandomizeAgent(population[n], lowerLimit, upperLimit, rng);
                    cost[n] = double.MaxValue;
                    numWipes--;
                }
            }
        }

        /// <summary>
        /// Performs the algorithm with the given sample set and parameters.
        /// </summary>
        /// <param name="sampleSet"></param>
        /// <param name="iterations"></param>
        /// <param name="CR">Crossover probability, default: 0.9</param>
        /// <param name="minCostThreshold">Minimum cost abort condition, default: 0.0</param>
        /// <returns></returns>
        public double[] Run(Sample[] sampleSet, int iterations = 100, float CR = 0.9f, float minCostThreshold = 0.0f)
        {
            Random rng = new Random();

            // Initialize cost array for sample set and current population
            for (int i = 0; i < population.Length; i++)
            {
                cost[i] = Cost(population[i], sampleSet);
                if (cost[minCostIdx] > cost[i])
                {
                    minCostIdx = i;
                }
            }

            for (int i = 1; i <= iterations; i++)
            {
                // For every agent in the population
                for (int xIdx = 0; xIdx < population.Length; xIdx++)
                {
                    int aIdx = PickRandomAgent(new int[]{ xIdx }, rng);
                    int bIdx = PickRandomAgent(new int[] { xIdx, aIdx }, rng);
                    int cIdx = PickRandomAgent(new int[] { xIdx, aIdx, bIdx }, rng);

                    double[] x = population[xIdx];
                    double[] a = population[aIdx];
                    double[] b = population[bIdx];
                    double[] c = population[cIdx];

                    int R = rng.Next(numParameters);
                    double[] y = new double[numParameters];
                    // Storn and Price (1997) choose j start randomly 
                    // We choose a random R instead
                    for (int j = 0; j < numParameters; j++) 
                    {
                        if (R == j || rng.NextDouble() < CR)
                        {
                            y[j] = EnsureBounds(a[j] + F * (b[j] - c[j]), j, rng);
                            
                        }
                        else
                        {
                            y[j] = x[j];
                        }
                    }

                    // Test our candidate
                    double yCost = Cost(y, sampleSet);
                    if (cost[xIdx] > yCost)
                    {
                        population[xIdx] = y;
                        cost[xIdx] = yCost;
                        if (cost[xIdx] <= cost[minCostIdx])
                        {
                            minCostIdx = xIdx;
                            if (cost[minCostIdx] <= minCostThreshold)
                            {
                                iterationsUsed = i;
                                return population[minCostIdx];
                            }
                        }
                    }
                }
            }

            iterationsUsed = iterations;
            return population[minCostIdx];
        }

    }
}
