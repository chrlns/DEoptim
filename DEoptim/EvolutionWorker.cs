using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DEoptim
{
    class EvolutionWorker : IEvolutionWorker
    {
        private ConcurrentQueue<IEvolutionWork> queue;
        private Thread[] worker;

        public EvolutionWorker(int numThreads)
        {
            this.queue = new ConcurrentQueue<IEvolutionWork>();
            this.worker = new Thread[numThreads];
        }

        /// <summary>
        /// Fire a QueueEmptyEvent to notify listeners that the workers have
        /// finished their work.
        /// </summary>
        protected void FireQueueEmptyEvent()
        {

        }

        public void Join()
        {
            for (int num = 0; num < worker.Length; num++)
            {
                worker[num].Join();
            }
        }

        public void Run()
        {
            IEvolutionWork work;
            while(queue.TryDequeue(out work))
            {
                work.DoWork();
            }
        }

        public void Start()
        {
            for (int num = 0; num < worker.Length; num++)
            {
                worker[num] = new Thread(Run);
                worker[num].Start();
            }
        }

        public int[] SliceStrategy(int populationSize)
        {
            // We assume that is useful to split the work into parts so
            // that every thread has at least one slices of work to do
            int[] slices = new int[worker.Length * 2];          // Example: 4 workers -> slices.Length = 8
            int oneSliceSize = populationSize / worker.Length;  // Example: oneSliceSize = 1000 / 8 = 125

            slices[0] = 0;
            slices[1] = oneSliceSize; // Example: 125
            for (int i = 2; i < slices.Length; i += 2)
            {
                slices[i] = slices[i - 1]; // Example: 125
                slices[i + 1] = slices[i] + oneSliceSize; // Example: 125 + 125 = 500
            }

            return slices;
        }

        public void Submit(IEvolutionWork work)
        {
            queue.Enqueue(work);
        }
    }
}
