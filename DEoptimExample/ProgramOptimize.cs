using DEoptim;
using System;
using System.Diagnostics;
using System.Threading;

namespace DEoptimExample
{
    class ProgramOptimize
    {
        private static double MySinoidFunc(double[] coeff, double[] vars)
        {
            double x = vars[0];
            return coeff[0] * Math.Sin(coeff[1] * x + coeff[2]) + coeff[3];
        }

        static void Main(string[] args)
        {
            bool showOutput = false;

            Stopwatch sw = Stopwatch.StartNew();

            for (int n = 0; n < 100; n++)
            {
                OptimizerParameter optimParam = new OptimizerParameter();
                optimParam.Individuals = 100;
                Optimizer optim = new Optimizer(MySinoidFunc, 4, new double[] { -5, -5, -5, -5 }, new double[] { 5, 5, 5, 5 }, optimParam);

                double[] x = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                double[] y = { 0, 1, 2, 2.6, 2, 1, 0, 1, 2, 2.4, 2, 1 };

                Sample[] sampleSet = new Sample[x.Length];
                for (int i = 0; i < x.Length; i++)
                {
                    sampleSet[i] = new Sample(x[i], y[i]);
                }

                double[] best = optim.Run(sampleSet, 1000, 0.25f);
                double cost = optim.Cost(best, sampleSet);
                int iter = optim.IterationsUsed;
                if (showOutput)
                {
                    foreach (double b in best)
                    {
                        Console.Write(b);
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + " ms");
        }
    }
}
