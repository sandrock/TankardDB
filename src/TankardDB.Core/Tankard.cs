
namespace TankardDB.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;
    using TankardDB.Core.Serialization;
    using TankardDB.Core.Stores;

    public class Tankard
    {
        private readonly IStore store;

        private readonly ReaderWriterLockSlim idsLock = new ReaderWriterLockSlim();
        private readonly List<long> idsList = new List<long>();
        private readonly DefaultTankardSerializer serializer;
        private long idsReserveSize = 1L;
        private long assignedIds = 0L;
        internal StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

        public Tankard(IStore store)
        {
            this.store = store;
            this.serializer = new DefaultTankardSerializer();
        }

        public TankardQuery Query
        {
            get
            {
                return new TankardQuery(this);
            }
        }

        internal IStore Store
        {
            get { return this.store; }
        }

        public async Task Insert(ITankardItem item)
        {
            var setName = this.GetSetName(item);
            
            var id = await this.GetNextId(setName);
            item.Id = id;

            var serialized = this.serializer.Serialize(item);
            
            MainIndexRow objectIndex;
            objectIndex = await this.store.AppendObject(id, serialized);
            
            // update ID index
            objectIndex.Type = item.GetType().AssemblyQualifiedName;
            await this.store.AppendMainIndex(objectIndex);

            // update indexes
            //throw new NotImplementedException();
        }

        public async Task<ITankardItem> GetById(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("The value cannot be empty", "id");

            // Seek MainIndex until desired id(s) are found
            var row = await this.store.SeekLatestMainIndex(id);
            if (row == null)
                return null;

            // Seek ObjectStore until object(s) are retreived
            byte[] serialized = await this.store.GetObject(row);
            var type = Type.GetType(row.Type, true);

            var value = this.serializer.Deserialize(serialized, type);
            return (ITankardItem)value;
        }

        public async Task<ITankardItem[]> GetById(string[] ids)
        {
            if (ids == null)
                throw new ArgumentNullException("ids");

            // Seek MainIndex until desired id(s) are found
            var rows = await this.store.SeekLatestMainIndex(ids);
            if (rows == null)
                return null;

            // Seek ObjectStore until object(s) are retreived
            byte[][] serializeds = await this.store.GetObjects(rows);

            var result = new ITankardItem[serializeds.Length];
            for (int i = 0; i < serializeds.Length; i++)
            {
                if (serializeds[i] != null)
                {
                    var type = Type.GetType(rows[i].Type);
                    result[i] = (ITankardItem)this.serializer.Deserialize(serializeds[i], type);
                }
            }
            
            return result;
        }

        internal ITankardItem Deserialize(byte[] bytes, MainIndexRow row)
        {
            var type = Type.GetType(row.Type);
            var obj = this.serializer.Deserialize(bytes, type);
            return (ITankardItem)obj;
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
                else
                {
                    if (this.assignedIds > this.idsReserveSize)
                    {
                        this.idsReserveSize = this.assignedIds;
                    }
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

        public async Task<T[]> GetById<T>(string[] ids)
        {
            var result = await this.GetById(ids);
            var array = new T[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                array[i] = (T)result[i];
            }

            return array;
        }

        public async Task<T> GetById<T>(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("The value cannot be empty", "id");

            var result = await this.GetById<T>(new string[] { id, });
            return result[0];
        }
    }
}
