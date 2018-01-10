using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    class DEOptimizerWork : IEvolutionWork
    {
        protected int numParameters;
        protected double[][] population;   // Our population of individuals (agents)
        protected double[] populationCost; // The 1-fitness (cost) of every individual
        protected Random rng;
        protected DEOptimizerParameter optParam;

        protected DEOptimizer opt;

        public DEOptimizerWork(DEOptimizer opt, double[][] population, double[] cost, int numParameters, Random rng, DEOptimizerParameter optParam)
        {
            this.opt = opt;
            this.numParameters = numParameters;
            this.population = population;
            this.populationCost = cost;
            this.rng = rng;
            this.optParam = optParam;
        }

        public void DoWork(int start, int end, Sample[] sampleSet)
        {
            int minCostIdx = start;

            // For every agent in the population
            for (int xIdx = start; xIdx <= end; xIdx++)
            {
                int aIdx = PickRandomAgent(new int[] { xIdx }, rng);
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
                    if (R == j || rng.NextDouble() < optParam.CR)
                    {
                        y[j] = EnsureBounds(a[j] + optParam.F * (b[j] - c[j]), j, rng);

                    }
                    else
                    {
                        y[j] = x[j];
                    }
                }

                // Test our candidate
                double yCost = opt.Cost(y, sampleSet);
                if (populationCost[xIdx] > yCost)
                {
                    population[xIdx] = y;
                    populationCost[xIdx] = yCost;
                    if (populationCost[xIdx] <= populationCost[minCostIdx])
                    {
                        minCostIdx = xIdx;
                    }
                }
            }

            // Communicate our local "best" indidividual to the Optimizer
            opt.UpdateBestCandidate(minCostIdx);
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
            if (v < opt.LowerLimit[idx] || v > opt.UpperLimit[idx])
                return RandomUtils.RandomizeDouble(opt.LowerLimit[idx], opt.UpperLimit[idx], rng);
            else
                return v;
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
    }
}
