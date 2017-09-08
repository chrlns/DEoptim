using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DEoptim;

namespace DEoptimTests
{
    [TestClass]
    public class OptimizerTest
    {
        private static double MySinoidFunc(double[] coeff, double[] vars)
        {
            double x = vars[0];
            return coeff[0] * Math.Sin(coeff[1] * x + coeff[2]) + coeff[3];
        }

        [TestMethod]
        public void TestSinoidOptimization()
        {
            Optimizer optim = new Optimizer(MySinoidFunc, new double[] { 0, 0, 0, 0 }, 100, -5, 5);

            double[] x = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            double[] y = { 0, 1, 2, 2.6, 2, 1, 0, 1, 2, 2.4, 2, 1 };

            Sample[] sampleSet = new Sample[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                sampleSet[i] = new Sample(x[i], y[i]);
            }

            double[] best = optim.Run(sampleSet);
            double cost = optim.Cost(best, sampleSet);
            Console.WriteLine(best);
        }
    }
}
