using System;
using System.Collections.Generic;

namespace DEoptim
{
    public class Optimizer
    {
        /// <summary>
        /// Function to be minimized.
        /// double func(double[] parameter, double[] variables)
        /// </summary>
        protected Func<double[], double[], double> function;
        protected double[] parameter;
        protected List<double[]> population;
        protected double[] cost;
        protected double F = 0.8; // weight [0..2]
        protected int minCostIdx = 0;
        protected double lowerLimit, upperLimit;

        public Optimizer(
            Func<double[], double[], double> function, 
            double[] parameter,
            int populationsSize = 100,
            double lowerLimit = -10.0,
            double upperLimit = 10.0)
        {
            this.function = function;
            this.parameter = parameter;
            this.lowerLimit = lowerLimit;
            this.upperLimit = upperLimit;

            // Initialize populations
            Random rng = new Random();
            population = new List<double[]>(populationsSize);
            cost = new double[populationsSize];
            for (int n = 0; n < populationsSize; n++)
            {
                double[] agent = new double[parameter.Length];
                RandomizeAgent(agent, lowerLimit, upperLimit, rng);
                population.Add(agent);
            }
            
        }

        private void RandomizeAgent(double[] agent, double lowerLimit, double upperLimit, Random rng = null)
        {
            if (rng == null)
                rng = new Random();

            for (int n = 0; n < agent.Length; n++)
            {
                agent[n] = RandomizeDouble(lowerLimit, upperLimit, rng);
            }
        }

        public double RandomizeDouble(double lowerLimit, double upperLimit, Random rng)
        {
            double range = upperLimit - lowerLimit;
            double rand = rng.NextDouble() * range;
            return rand + lowerLimit;
        }

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

        private int PickRandomAgent(int[] except, Random rng)
        {
            int idx = rng.Next(population.Count);
            while (Array.IndexOf(except, idx) >= 0)
                idx = rng.Next(population.Count);
            return idx;
        }

        private double EnsureBounds(double v, Random rng)
        {
            if (v < lowerLimit || v > upperLimit)
                return RandomizeDouble(lowerLimit, upperLimit, rng);
            else
                return v;
        }

        public double[] Run(Sample[] sampleSet, int iterations = 200, float CR = 0.9f)
        {
            Random rng = new Random();

            // Initialize cost array for sample set and current population
            for (int i = 0; i < population.Count; i++)
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
                for (int xIdx = 0; xIdx < population.Count; xIdx++)
                {
                    int aIdx = PickRandomAgent(new int[]{ xIdx }, rng);
                    int bIdx = PickRandomAgent(new int[] { xIdx, aIdx }, rng);
                    int cIdx = PickRandomAgent(new int[] { xIdx, aIdx, bIdx }, rng);

                    double[] x = population[xIdx];
                    double[] a = population[aIdx];
                    double[] b = population[bIdx];
                    double[] c = population[cIdx];

                    int R = rng.Next(parameter.Length);
                    double[] y = new double[parameter.Length];
                    // Storn and Price (1997) choose j start randomly 
                    // We choose a random R instead
                    for (int j = 0; j < parameter.Length; j++) 
                    {
                        if (R == j || rng.NextDouble() < CR)
                        {
                            y[j] = EnsureBounds(a[j] + F * (b[j] - c[j]), rng);
                            
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
                            minCostIdx = xIdx;
                    }
                }
            }

            return population[minCostIdx];
        }

    }
}
