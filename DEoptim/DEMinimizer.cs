using System;

namespace DEoptim
{
    public class DEMinimizer
    {
        public double[] MaxRange => maxRange;
        public double[] MinRange => minRange;
        public Func<double[], double> MinFunc => minFunc;

        protected int dim;
        protected Func<double[], double> minFunc;
        protected double[] minRange, maxRange;
        protected Random rng = new Random();
        protected DEVariant DE = DEVariant.Default();
        protected double[] cost;
        protected double[][] population;
        protected int minCostIdx = -1;

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

        public DEMinimizerResult Run(DEHyperParameter param, double valueToReach, int numGenerations = int.MaxValue, IEvolutionWorker worker = null)
        {
            if (worker == null)
            {
                worker = EvolutionWorkerFactory.createSingleThreaded();
            }

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
                int[] slices = worker.SliceStrategy(param.Individuals);
                for (int i = 0; i < slices.Length; i += 2)
                {
                    int start = slices[i];
                    int end = slices[i + 1];
                    worker.Submit(new DEMinimizerWork(this, DE, population, cost, dim, rng, param, start, end));
                }

                worker.Start();
                worker.Join();

                if (valueToReach >= cost[minCostIdx])
                {
                    break;
                }
            }

            return new DEMinimizerResult(cost[minCostIdx], population[minCostIdx], g);
        }

        private readonly object lockUpdateBestCandidate = new object();

        public void UpdateBestCandidate(int idx)
        {
            lock(lockUpdateBestCandidate)
            {
                if (minCostIdx < 0 || cost[idx] < cost[minCostIdx])
                {
                    minCostIdx = idx;
                }
            }
        }

    } // end class
}