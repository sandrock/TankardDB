
namespace TankardDB.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TankardDB.Core.Internals;
    using TankardDB.Core.Stores;

    public class TankardQuery : IEnumerable<ITankardItem>, IEnumerator<ITankardItem>
    {
        private readonly Tankard core;
        private MainIndexRow current;
        private ITankardItem currentValue;
        private IStoreLock storeLock;
        private bool disposed;
        private List<string> readIds;

        internal TankardQuery(Tankard core)
        {
            this.core = core;
        }

        private IStoreLock StoreLock
        {
            get
            {
                if (this.storeLock == null)
                {
                    this.storeLock = this.core.Store.GetReadLock();
                    this.readIds = new List<string>();
                }

                return this.storeLock;
            }
        }

        public IEnumerator<ITankardItem> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        ITankardItem IEnumerator<ITankardItem>.Current
        {
            get
            {
                if (this.currentValue == null)
                {
                    var row = this.current;
                    if (row != null)
                    {
                        var task = this.storeLock.GetObject(row);
                        task.Wait();
                        var serialized = task.Result;
                        this.currentValue = this.core.Deserialize(serialized, row);
                    }
                }

                return this.currentValue;
            }
        }

        object IEnumerator.Current
        {
            get { return this.currentValue; }
        }

        bool IEnumerator.MoveNext()
        {
            this.current = null;
            this.currentValue = null;

            MainIndexRow item;
            do
            {
                var task = this.StoreLock.GetNextMainIndexRow();
                task.Wait();
                this.current = item = task.Result;

                if (item == null)
                {
                    break;
                }
            } while (item.IsDeleted == true || this.readIds.Contains(item.Id, this.core.stringComparer));

            this.currentValue = null;
            return this.current != null;
        }

        void IEnumerator.Reset()
        {
            var item = this.storeLock;
            this.storeLock = null;
            if (item != null)
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases managed and - optionally - unmanaged resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    IDisposable item = this.storeLock;
                    this.storeLock = null;
                    if (item != null)
                    {
                        item.Dispose();
                    }
                }

                this.disposed = true;
            }
        }
    }
}
