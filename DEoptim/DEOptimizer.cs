using System;
using System.Collections.Generic;
using System.Linq;

namespace DEoptim
{
    /// <summary>
    /// Optimizes parameters of a non-linear function using 
    /// Differential Evolution algorithm.
    /// </summary>
    public class DEOptimizer
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

        protected DEOptimizerParameter optParam;

        protected int minCostIdx = 0;
        protected double[] lowerLimit, upperLimit;

        /// <summary>
        /// The lower limit for every parameter.
        /// </summary>
        public double[] LowerLimit => lowerLimit;

        /// <summary>
        /// The upper limit for every parameter.
        /// </summary>
        public double[] UpperLimit => upperLimit;

        public DEOptimizer(
            Func<double[], double[], double> function, int numParameters,
            double lowerLimit, double upperLimit, DEOptimizerParameter optParam)
        {
            double[] upper = new double[numParameters];
            double[] lower = new double[numParameters];
            for (int n = 0; n < numParameters; n++)
            {
                lower[n] = lowerLimit;
                upper[n] = upperLimit;
            }
            Init(function, numParameters, lower, upper, optParam);
        }

        public DEOptimizer(
            Func<double[], double[], double> function, int numParameters,
            double[] lowerLimit, double[] upperLimit, DEOptimizerParameter optParam)
        {
            Init(function, numParameters, lowerLimit, upperLimit, optParam);
        }

        private void Init(Func<double[], double[], double> function, int numParameters,
            double[] lowerLimit, double[] upperLimit, DEOptimizerParameter optParam)
        { 
            this.function = function;
            this.numParameters = numParameters;
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;
            this.optParam = optParam;

            // Initialize populations
            Random rng = new Random();
            population = new double[optParam.Individuals][];
            cost = new double[optParam.Individuals];
            for (int n = 0; n < optParam.Individuals; n++)
            {
                population[n] = new double[numParameters];
                RandomUtils.RandomizeAgent(population[n], lowerLimit, upperLimit, rng);
            }
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
        /// Performs the algorithm with the given sample set and parameters.
        /// </summary>
        /// <param name="sampleSet"></param>
        /// <param name="iterations"></param>
        /// <param name="minCostThreshold">Minimum cost abort condition, default: 0.0</param>
        /// <returns></returns>
        public double[] Run(IEvolutionWorker worker, Sample[] sampleSet, int iterations = 100, float minCostThreshold = 0.0f)
        {
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
                    // Create DEOptimizerWork instances
                    
                }
            }

            iterationsUsed = iterations;
            return population[minCostIdx];
        }

        private readonly object lockUpdateBestCandidate = new object();

        public void UpdateBestCandidate(int idx)
        {
            lock(lockUpdateBestCandidate)
            {
                if (cost[idx] < cost[minCostIdx])
                {
                    minCostIdx = idx;
                }
            }
        }
    }

}
