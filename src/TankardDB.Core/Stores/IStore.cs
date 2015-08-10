
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IStore
    {
        Task<long[]> ReserveIds(long count);

        Task AppendObject(ITankardItem item);
    }
}
