using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    class DEMinimizerWork : IEvolutionWork
    {
        protected int dim;
        protected double[][] population;   // Our population of individuals (agents)
        protected double[] cost; // The 1-fitness (cost) of every individual
        protected Random rng;
        protected DEHyperParameter param;
        protected DEVariant DE;
        protected DEMinimizer min;
        protected int start, end;

        public DEMinimizerWork(DEMinimizer min, DEVariant DE, double[][] population, double[] cost, int dim, Random rng, DEHyperParameter param, int start, int end)
        {
            this.min = min;
            this.DE = DE;
            this.dim = dim;
            this.population = population;
            this.cost = cost;
            this.rng = rng;
            this.param = param;
            this.start = start;
            this.end = end;
        }

        public void DoWork()
        {
            int minCostIdx = start;

            // For every agent in the population
            for (int xIdx = start; xIdx < end; xIdx++)
            {
                int aIdx = RandomUtils.PickRandomAgent(param.Individuals, new int[] { xIdx }, rng);
                int bIdx = RandomUtils.PickRandomAgent(param.Individuals, new int[] { xIdx, aIdx }, rng);
                int cIdx = RandomUtils.PickRandomAgent(param.Individuals, new int[] { xIdx, aIdx, bIdx }, rng);

                double[] x = population[xIdx];
                double[] a = population[aIdx];
                double[] b = population[bIdx];
                double[] c = population[cIdx];

                int j = rng.Next(dim);
                double[] y = new double[dim];

                for (int k = 0; k < dim; k++)
                {
                    if (rng.NextDouble() < param.CR || k == dim - 1)
                    {
                        y[j] = DE.MutateVectorElement(param, a[j], b[j], c[j], 
                            min.MinRange[j], min.MaxRange[j], rng);
                    }
                    else
                    {
                        y[j] = x[j];
                    }
                    j = (j + 1) % dim;
                }

                // Test our candidate
                double yCost = min.MinFunc(y);
                if (cost[xIdx] > yCost)
                {
                    population[xIdx] = y;
                    cost[xIdx] = yCost;
                    if (minCostIdx == -1 || cost[xIdx] <= cost[minCostIdx])
                    {
                        minCostIdx = xIdx;
                    }
                }
            }

            // Communicate our local "best" indidividual to the Optimizer
            min.UpdateBestCandidate(minCostIdx);
        }

    }
}
