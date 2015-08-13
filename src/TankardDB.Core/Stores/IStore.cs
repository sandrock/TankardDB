
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;

    public interface IStore
    {
        Task<long[]> ReserveIds(long count);

        Task<MainIndexRow> AppendObject(ITankardItem item);

        Task AppendMainIndex(MainIndexRow row);
    }
}
