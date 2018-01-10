using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public interface IEvolutionWork
    {
        /// <summary>
        /// Do the evolutionary work on the individuals starting with index start to index end
        /// using the given samples.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="samples"></param>
        void DoWork(int start, int end, Sample[] samples);
    }
}
