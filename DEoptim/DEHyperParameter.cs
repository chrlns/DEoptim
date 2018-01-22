using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    /// <summary>
    /// Parameter for the Differential Evolution Optimizer. 
    /// </summary>
    public class DEHyperParameter
    {
        private float crossoverProbability = 0.9f;
        private float f = 0.8f;
        private int individuals = 500;

        public float CR { get => crossoverProbability; set => crossoverProbability = value; }
        public float F { get => f; set => f = value; }

        /// <summary>
        /// Number of individuals used per generation.
        /// </summary>
        public int Individuals { get => individuals; set => individuals = value; }
    }
}
