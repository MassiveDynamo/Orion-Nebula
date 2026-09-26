using System.Threading;

namespace Data.Journal
{
    public class ImportMetrics
    {
        private long _processed;
        private long _failed;
        private long _retries;

        public void IncrementProcessed(int count = 1) => Interlocked.Add(ref _processed, count);
        public void IncrementFailed(int count = 1) => Interlocked.Add(ref _failed, count);
        public void IncrementRetries(int count = 1) => Interlocked.Add(ref _retries, count);

        public long Processed => Interlocked.Read(ref _processed);
        public long Failed => Interlocked.Read(ref _failed);
        public long Retries => Interlocked.Read(ref _retries);
    }
}
