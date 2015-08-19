
namespace TankardDB.Core.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;

    public interface IStoreLock : IDisposable
    {
        Task<MainIndexRow> GetNextMainIndexRow();
        Task<byte[]> GetObject(MainIndexRow row);
    }
}
