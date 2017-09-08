using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public struct Sample
    {
        /// <summary>
        /// Values of the predictor.
        /// </summary>
        public double[] Variables;
        public double Outcome;

        public Sample(double[] var, double outcome)
        {
            Variables = var;
            Outcome = outcome;
        }

        public Sample(double var, double outcome)
        {
            Variables = new double[] { var };
            Outcome = outcome;
        }
    }
}
