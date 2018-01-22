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
            Stopwatch sw = Stopwatch.StartNew();

            for (int n = 0; n < 100; n++)
            {
                DEHyperParameter hp = new DEHyperParameter();
                hp.Individuals = 100;

                double[] x = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                double[] y = { 0, 1, 2, 2.6, 2, 1, 0, 1, 2, 2.4, 2, 1 };

                Sample[] sampleSet = new Sample[x.Length];
                for (int i = 0; i < x.Length; i++)
                {
                    sampleSet[i] = new Sample(x[i], y[i]);
                }
                FuncMinimizeWrapper wrapper = new FuncMinimizeWrapper(MySinoidFunc, sampleSet);
                Func<double[], double> minFunc = wrapper.Func;

                DEMinimizer min = new DEMinimizer(minFunc, 4, -5, 5);
                DEMinimizerResult result = min.Run(hp, 0.1, 1000);
                Console.WriteLine(result);
            }

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + " ms");
        }
    }
}
