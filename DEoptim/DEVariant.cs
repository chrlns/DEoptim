using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public abstract class DEVariant
    {
        public static string DEbest2bin = "DE/best/2/bin";
        public static string DErand1bin = "DE/rand/1/bin";

        public static DEVariant Default()
        {
            return From(DErand1bin);
        }

        public static DEVariant From(string variant)
        {
            // TODO Implement more variants
            return new VariantDErand1bin();
        }

        /// <summary>
        /// Checks if v is within the specified limits. If it violates the interval
        /// boundaries, a newly randomized value is returned. Otherwise v is returned.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rng"></param>
        /// <param name="idx"></param>
        /// <returns>v if within bounds, a randomized value otherwise.</returns>
        protected double EnsureBounds(double v, double min, double max, Random rng)
        {
            if (v < min || v > max)
                return RandomUtils.RandomizeDouble(min, max, rng);
            else
                return v;
        }

        public abstract double MutateVectorElement(DEHyperParameter hp, double a, double b, double c, double min, double max, Random rng);
    }
}
