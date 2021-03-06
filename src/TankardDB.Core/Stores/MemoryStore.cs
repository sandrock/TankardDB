﻿
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;

    public sealed class MemoryStore : IStore
    {
        private readonly List<Range> occupiedIdRanges = new List<Range>();
        private readonly ReaderWriterLockSlim occupiedIdLock = new ReaderWriterLockSlim();

        private readonly List<byte[]> objectStore = new List<byte[]>();
        private readonly ReaderWriterLockSlim objectLock = new ReaderWriterLockSlim();

        private readonly List<string> mainIndex = new List<string>();
        private readonly ReaderWriterLockSlim mainIndexLock = new ReaderWriterLockSlim();

        private readonly SekvapLanguage lang = new SekvapLanguage();

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

        public async Task<MainIndexRow> AppendObject(string id, byte[] data)
        {
            return await Task.Run(() =>
            {
                this.objectLock.EnterWriteLock();
                try
                {
                    this.objectStore.Add(data);
                    var index = this.objectStore.IndexOf(data);
                    var result = new MainIndexRow(id, index, data.Length, null);
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
            if (item == null)
                throw new ArgumentNullException("item");

            await Task.Run(() =>
            {
                this.mainIndexLock.EnterWriteLock();
                try
                {
                    var data = item.ToSekvap(this.lang);
                    this.mainIndex.Add(data);
                }
                finally
                {
                    this.mainIndexLock.ExitWriteLock();
                }
            });
        }

        public async Task<MainIndexRow> SeekLatestMainIndex(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("The value cannot be empty", "id");

            return await Task.Run(() =>
            {
                this.mainIndexLock.EnterReadLock();
                try
                {
                    for (int i = 0; i < this.mainIndex.Count; i++)
                    {
                        var itemAsString = this.mainIndex[this.mainIndex.Count - i - 1];
                        var itemData = this.lang.Parse(itemAsString);
                        var row = new MainIndexRow(itemData);
                        if (row.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                        {
                            return row;
                        }
                    }

                    return null;
                }
                finally
                {
                    this.mainIndexLock.ExitReadLock();
                }
            });
        }

        public async Task<MainIndexRow[]> SeekLatestMainIndex(string[] ids)
        {
            if (ids == null)
                throw new ArgumentNullException("ids");

            return await Task.Run(() =>
            {
                this.mainIndexLock.EnterReadLock();
                try
                {
                    var result = new MainIndexRow[ids.Length];
                    for (int i = 0; i < this.mainIndex.Count; i++)
                    {
                        var itemAsString = this.mainIndex[this.mainIndex.Count - i - 1];
                        var itemData = this.lang.Parse(itemAsString);
                        var row = new MainIndexRow(itemData);

                        for (int j = 0; j < ids.Length; j++)
                        {
                            var id = ids[j];
                            if (id != null && row.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                            {
                                if (result[j] == null)
                                {
                                    result[j] = row;

                                    if (result.All(r => r != null))
                                    {
                                        return result;
                                    }
                                }
                            }
                        }
                    }

                    return result;
                }
                finally
                {
                    this.mainIndexLock.ExitReadLock();
                }
            });
        }

        public async Task<byte[]> GetObject(MainIndexRow row)
        {
            if (row == null)
                throw new ArgumentNullException("row");

            return await Task.Run(() =>
            {
                this.objectLock.EnterReadLock();
                try
                {
                    var bytes = this.objectStore[checked((int)row.ObjectStoreBeginIndex)];
                    Debug.Assert(bytes.Length == row.ObjectStoreLength, "MemoryStore.GetObject: bytes.Length != row.ObjectStoreLength");
                    return bytes;
                }
                finally
                {
                    this.objectLock.ExitReadLock();
                }
            });
        }

        public async Task<byte[][]> GetObjects(MainIndexRow[] rows)
        {
            if (rows == null)
                throw new ArgumentNullException("rows");

            return await Task.Run(() =>
            {
                this.objectLock.EnterReadLock();
                try
                {
                    var result = new byte[rows.Length][];
                    for (int i = 0; i < rows.Length; i++)
                    {
                        var row = rows[i];
                        if (row != null)
                        {
                            var bytes = this.objectStore[checked((int)row.ObjectStoreBeginIndex)];
                            Debug.Assert(bytes.Length == row.ObjectStoreLength, "MemoryStore.GetObject: bytes.Length != row.ObjectStoreLength");
                            result[i] = bytes;
                        }
                    }

                    return result;
                }
                finally
                {
                    this.objectLock.ExitReadLock();
                }
            });
        }

        public IStoreLock GetReadLock()
        {
            var storeLock = new StoreLock(this, () =>
            {
                this.mainIndexLock.ExitReadLock();
                this.objectLock.ExitReadLock();
            });
            this.mainIndexLock.EnterReadLock();
            this.objectLock.EnterReadLock();
            return storeLock;
        }

        internal MainIndexRow ReadMainIndex(int index)
        {
            if (index >= this.mainIndex.Count)
                return null;

            var itemAsString = this.mainIndex[this.mainIndex.Count - index - 1];
            var itemData = this.lang.Parse(itemAsString);
            var row = new MainIndexRow(itemData);
            return row;
        }

        internal byte[] ReadObjectStore(long index, long length)
        {
            var bytes = this.objectStore[checked((int)index)];
            Debug.Assert(bytes.Length == length, "MemoryStore.ReadObjectStore: bytes.Length != length");
            return bytes;
        }

        private sealed class StoreLock : IStoreLock
        {
            private readonly MemoryStore store;
            private Action disposeAction;
            private int mainIndexIndex = -1;

            public StoreLock(MemoryStore store, Action disposeAction)
            {
                this.store = store;
                this.disposeAction = disposeAction;
            }

            public void Dispose()
            {
                var action = this.disposeAction;
                this.disposeAction = null;
                if (action != null)
                {
                    action();
                }
            }

            public async Task<MainIndexRow> GetNextMainIndexRow()
            {
                var index = ++this.mainIndexIndex;
                var item = this.store.ReadMainIndex(index);
                return item;
            }

            public async Task<byte[]> GetObject(MainIndexRow row)
            {
                var result = this.store.ReadObjectStore(row.ObjectStoreBeginIndex.Value, row.ObjectStoreLength.Value);
                return result;
            }
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
