
namespace TankardDB.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;
    using TankardDB.Core.Stores;

    public class TestStore : IStore
    {
        internal Func<long, Task<long[]>> ReserveIdsDelegate { get; set; }
        internal Func<string, byte[], Task<MainIndexRow>> AppendObjectDelegate { get; set; }
        internal Func<MainIndexRow, Task> AppendMainIndexDelegate { get; set; }
        internal Func<string, Task<MainIndexRow>> SeekLatestMainIndexDelegate { get; set; }
        internal Func<MainIndexRow, Task<byte[]>> GetObjectDelegate { get; set; }

        internal int ReserveIdsCount { get; set; }
        internal int AppendObjectCount { get; set; }
        internal int AppendMainIndexCount { get; set; }
        public int SeekLatestMainIndexCount { get; set; }
        public int GetObjectCount { get; set; }

        public async Task<long[]> ReserveIds(long count)
        {
            this.ReserveIdsCount += 1;
            return await this.ReserveIdsDelegate(count);
        }

        public async Task<MainIndexRow> AppendObject(string id, byte[] data)
        {
            this.AppendObjectCount += 1;
            return await this.AppendObjectDelegate(id, data);
        }

        public async Task AppendMainIndex(MainIndexRow row)
        {
            this.AppendMainIndexCount += 1;
            await this.AppendMainIndexDelegate(row);
        }

        public async Task<MainIndexRow> SeekLatestMainIndex(string id)
        {
            this.SeekLatestMainIndexCount += 1;
            return await this.SeekLatestMainIndexDelegate(id);
        }

        public async Task<byte[]> GetObject(MainIndexRow row)
        {
            this.GetObjectCount += 1;
            return await this.GetObjectDelegate(row);
        }

        public Task<MainIndexRow[]> SeekLatestMainIndex(string[] ids)
        {
            throw new NotImplementedException();
        }

        public Task<byte[][]> GetObjects(MainIndexRow[] rows)
        {
            throw new NotImplementedException();
        }
    }
}
