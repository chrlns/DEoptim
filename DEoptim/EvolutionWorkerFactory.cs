using System;
using System.Collections.Generic;
using System.Text;

namespace DEoptim
{
    public sealed class EvolutionWorkerFactory
    {
        public static IEvolutionWorker createDefault()
        {
            return createSingleThreaded();
        }

        public static IEvolutionWorker createSingleThreaded()
        {
            return createMultiThreaded(1);
        }

        public static IEvolutionWorker createMultiThreaded()
        {
            return createMultiThreaded(Environment.ProcessorCount);
        }

        public static IEvolutionWorker createMultiThreaded(int numThreads)
        {
            return new EvolutionWorker(numThreads);
        }
    }
}
