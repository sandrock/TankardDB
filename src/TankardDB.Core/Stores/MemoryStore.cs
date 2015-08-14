
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;

    public class MemoryStore : IStore
    {
        private readonly List<Range> occupiedIdRanges = new List<Range>();
        private readonly ReaderWriterLockSlim occupiedIdLock = new ReaderWriterLockSlim();

        private readonly List<ITankardItem> objectStore = new List<ITankardItem>();
        private readonly ReaderWriterLockSlim objectLock = new ReaderWriterLockSlim();

        private readonly List<string> mainIndex = new List<string>();
        private readonly ReaderWriterLockSlim mainIndexLock = new ReaderWriterLockSlim();

        public async Task<long[]> ReserveIds(long count)
        {
            return await Task.Run(() =>
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
            });
        }

        public async Task<MainIndexRow> AppendObject(ITankardItem item)
        {
            return await Task.Run(() =>
            {
                this.objectLock.EnterWriteLock();
                try
                {
                    this.objectStore.Add(item);
                    var index = this.objectStore.IndexOf(item);
                    var result = new MainIndexRow(item.Id, index, index);
                    return result;
                }
                finally
                {
                    this.objectLock.ExitWriteLock();
                }
            });
        }

        public async Task AppendMainIndex(MainIndexRow item)
        {
            await Task.Run(() =>
            {
                this.mainIndexLock.EnterWriteLock();
                try
                {
                    var data = item.ToSekvap();
                    this.mainIndex.Add(data);
                }
                finally
                {
                    this.mainIndexLock.ExitWriteLock();
                }
            });
        }

        private struct Range
        {
            public Range(long start, long end)
            {
                this.Start = start;
                this.End = end;
            }

            public long Start;
            public long End;
        }
    }
}
