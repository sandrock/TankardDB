
namespace TankardDB.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TankardDB.Core.Stores;

    public class Tankard
    {
        private readonly IStore store;

        private readonly ReaderWriterLockSlim idsLock = new ReaderWriterLockSlim();
        private readonly List<long> idsList = new List<long>();
        private long idsReserveSize = 8L;
        private long assignedIds = 0L;

        public Tankard(IStore store)
        {
            this.store = store;
        }

        public TankardQuery Query
        {
            get
            {
                return new TankardQuery(this);
            }
        }

        public async Task<ITankardItem> Insert(ITankardItem item)
        {
            var setName = this.GetSetName(item);
            var id = await this.GetNextId(setName);
            item.Id = id;
            await this.store.AppendObject(item);
            // update ID index
            // update indexes
            throw new NotImplementedException();
        }

        private string GetSetName(ITankardItem item)
        {
            var type = item.GetType();
            if (type.GenericTypeArguments.Length > 0)
                throw new InvalidOperationException("Generic types are not supported.");

            return type.Name;
        }

        private async Task<string> GetNextId(string setName)
        {
            // try get reserved id
            this.idsLock.EnterWriteLock();
            try
            {
                if (this.idsList.Count > 0)
                {
                    var index = 0;
                    var id = this.idsList[index];
                    this.idsList.RemoveAt(index);
                    this.assignedIds++;
                    return this.ConcatId(setName, id);
                }
            }
            finally
            {
                this.idsLock.ExitWriteLock();
            }

            // no reserved id, create some more
            var newList = await this.store.ReserveIds(this.idsReserveSize);

            this.idsLock.EnterWriteLock();
            try
            {
                this.idsList.AddRange(newList);
                var index = 0;
                var id = this.idsList[index];
                this.idsList.RemoveAt(index);
                this.assignedIds++;
                return this.ConcatId(setName, id);
            }
            finally
            {
                this.idsLock.ExitWriteLock();
            }
        }

        private string ConcatId(string setName, long id)
        {
            return setName + "-" + id;
        }
    }
}
