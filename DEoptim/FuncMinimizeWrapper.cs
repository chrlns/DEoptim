using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    /// <summary>
    /// 
    /// </summary>
    public class FuncMinimizeWrapper
    {
        protected Func<double[], double[], double> func;
        protected Sample[] sampleSet;

        public FuncMinimizeWrapper(Func<double[], double[], double> func, Sample[] sampleSet)
        {
            this.func = func;
            this.sampleSet = sampleSet;
        }

        public double Func(double[] agent)
        {
            double cost = 0;

            foreach (Sample sample in sampleSet)
            {
                double err = sample.Outcome - func(agent, sample.Variables);
                cost += err * err;
            }

            return cost;
        }
    }
}
