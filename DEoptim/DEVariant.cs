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

        public abstract double MutateVectorElement(DEHyperParameter hp, double a, double b, double c);
    }
}
