using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    /// <summary>
    /// DE Variant "DE/rand/1/bin"
    /// </summary>
    class VariantDErand1bin : DEVariant
    {
        public override double MutateVectorElement(DEHyperParameter hp, double a, double b, double c, double min, double max, Random rng)
        {
            return EnsureBounds(c + hp.F * (a - b), min, max, rng);
        }
    }
}
