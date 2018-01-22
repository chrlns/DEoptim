using DEoptim;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class MinimizerTest
{
    // x_j [-5.12, 5.12]
    private static double Sphere(double[] x)
    {
        double sum = 0;
        for (int j = 0; j < 3; j++)
        {
            sum += Math.Pow(x[j], 2);
        }
        return sum;
    }

    // x_j [-2.048, 2.048]
    private static double RosenbrockSaddle(double[] x)
    {
        return 100 * Math.Pow((x[0] * x[0] - x[1]), 2) * Math.Pow(1 - x[1], 2);
    }

    [TestMethod]
    public void TestMinimizeSphere()
    {
        int minGens = int.MaxValue;
        for (int n = 0; n < 10; n++)
        {
            DEMinimizer min = new DEMinimizer(Sphere, 3, -5.12, 5.12);
            DEHyperParameter hp = new DEHyperParameter();

            // Use hyperparameter from Storn and Price paper
            hp.Individuals = 5;
            hp.F = 0.9f;
            hp.CR = 0.1f;

            DEMinimizerResult result = min.Run(hp, 1E-6, 1000);
            Console.WriteLine(result);
            minGens = Math.Min(minGens, result.Generations);
        }
        Assert.IsTrue(minGens * 5 <= 406);
    }
}