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
            Optimizer optim = new Optimizer(MySinoidFunc, new double[] { 0, 0, 0, 0 }, 10, -5, 5);

            for (int m = 1; m < 100; m += 5)
            {
                double[] x = new double[20];
                double[] y = new double[x.Length];
                for (int n = 0; n < x.Length; n++)
                {
                    x[n] = m + n;
                    y[n] = -1.180 * Math.Sin(1.03 * x[n] + 0.67) + 1.40;
                }

                optim.WipePopulation(0.5f);

                Sample[] sampleSet = new Sample[x.Length];
                for (int i = 0; i < x.Length; i++)
                {
                    sampleSet[i] = new Sample(x[i], y[i]);
                }

                double[] best = optim.Run(sampleSet);
                double cost = optim.Cost(best, sampleSet);

                Assert.IsTrue(cost < 0.0001);
            }
        }
    }
}
