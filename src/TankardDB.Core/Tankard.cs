
namespace TankardDB.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TankardDB.Core.Stores;

    public class Tankard
    {
        private readonly IStore store;

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
            var appendResult = await this.store.AppendObject(item);
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

        private Task<string> GetNextId(string setName)
        {
            throw new NotImplementedException();
        }
    }
}
