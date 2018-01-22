using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public interface IEvolutionWorker
    {
        void Join();

        int[] SliceStrategy(int populationSize);

        void Submit(IEvolutionWork work);

        void Start();
    }
}
