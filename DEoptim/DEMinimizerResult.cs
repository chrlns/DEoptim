using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public class DEMinimizerResult
    {
        public readonly double Minimum;
        public readonly double[] Parameter;
        public readonly int Generations;

        public DEMinimizerResult(double min, double[] param, int gen)
        {
            this.Minimum = min;
            this.Parameter = param;
            this.Generations = gen;
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("[min=");
            buf.Append(Minimum);
            buf.Append(", param=[");
            for (int i = 0; i < Parameter.Length; i++)
            {
                buf.Append(Parameter[i]);
                if (i < Parameter.Length - 1)
                    buf.Append(", ");
            }
            buf.Append("], generations=");
            buf.Append(Generations);
            buf.Append("]");
            return buf.ToString();
        }
    }
}
