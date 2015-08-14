
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

        Task<MainIndexRow> AppendObject(string id, byte[] data);

        Task AppendMainIndex(MainIndexRow row);

        Task<MainIndexRow> SeekLatestMainIndex(string id);

        Task<byte[]> GetObject(MainIndexRow row);
    }
}
