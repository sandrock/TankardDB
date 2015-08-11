
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class MemoryStore : IStore
    {
        private readonly List<Range> occupiedIdRanges = new List<Range>();
        private readonly ReaderWriterLockSlim occupiedIdLock = new ReaderWriterLockSlim();

        private readonly Dictionary<string, ITankardItem> objectStore = new Dictionary<string, ITankardItem>();
        private readonly ReaderWriterLockSlim objectLock = new ReaderWriterLockSlim();


        public async Task<long[]> ReserveIds(long count)
        {
            this.occupiedIdLock.EnterWriteLock();
            try
            {
                Range reservation;
                if (this.occupiedIdRanges.Count > 0)
                {
                    var maxItem = this.occupiedIdRanges.OrderByDescending(r => r.End).First();
                    var from = maxItem.End + 1;
                    reservation = new Range(from, from + count - 1);
                }
                else
                {
                    reservation = new Range(1, count);
                }

                this.occupiedIdRanges.Add(reservation);

                var result = new long[count];
                for (long i = 0L; i < count; i++)
                {
                    result[i] = reservation.Start + i;
                }

                return result;
            }
            finally
            {
                this.occupiedIdLock.ExitWriteLock();
            }
        }

        public async Task AppendObject(ITankardItem item)
        {
            this.objectLock.EnterWriteLock();
            try
            {
                this.objectStore.Add(item.Id, item);
            }
            finally
            {
                this.objectLock.ExitWriteLock();
            }
        }

        private struct Range
        {
            public Range (long start, long end)
            {
                this.Start = start;
                this.End = end;
            }

            public long Start;
            public long End;
        }

    }
}
