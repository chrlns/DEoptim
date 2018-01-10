using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public interface IEvolutionWorker
    {
        int[] SliceStrategy(int populationSize);

        void Submit(IEvolutionWork work);
    }
}
