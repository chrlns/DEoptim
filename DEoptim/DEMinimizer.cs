using System;

namespace DEoptim
{
    public class DEMinimizer
    {
        protected int dim;
        protected Func<double[], double> minFunc;
        protected double[] minRange, maxRange;
        protected Random rng = new Random();
        protected DEVariant DE = DEVariant.Default();

        public DEMinimizer(Func<double[], double> minFunc, int dim, double minRange, double maxRange)
        {
            this.minFunc = minFunc;
            this.dim = dim;
            this.minRange = new double[dim];
            this.maxRange = new double[dim];
            for (int i = 0; i < dim; i++)
            {
                this.minRange[i] = minRange;
                this.maxRange[i] = maxRange;
            }
        }

        public DEMinimizerResult Run(DEHyperParameter param, double valueToReach, int numGenerations = int.MaxValue)
        {
            double[][] population;
            double[] cost;
            int minCostIdx = -1;
            
            // Initialize populations
            population = new double[param.Individuals][];
            cost = new double[param.Individuals];
            cost[0] = double.MaxValue;
            for (int n = 0; n < param.Individuals; n++)
            {
                population[n] = new double[dim];
                RandomUtils.RandomizeAgent(population[n], minRange, maxRange, rng);
                cost[n] = int.MaxValue;
            }

            int g = 0;
            for (g = 0; g < numGenerations; g++)
            {
                for (int xIdx = 0; xIdx < param.Individuals; xIdx++)
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
                            //y[j] = EnsureBounds(c[j] + param.F * (a[j] - b[j]), j, rng);
                            y[j] = DE.MutateVectorElement(param, a[j], b[j], c[j]);
                        }
                        else
                        {
                            y[j] = x[j];
                        }
                        j = (j + 1) % dim;
                    }

                    // Test our candidate
                    double yCost = minFunc(y);
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

                if (valueToReach >= cost[minCostIdx])
                {
                    break;
                }
            }

            return new DEMinimizerResult(cost[minCostIdx], population[minCostIdx], g);
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
            if (v < minRange[idx] || v > maxRange[idx])
                return RandomUtils.RandomizeDouble(minRange[idx], maxRange[idx], rng);
            else
                return v;
        }

    } // end class
}