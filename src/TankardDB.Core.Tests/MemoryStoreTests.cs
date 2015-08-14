
namespace TankardDB.Core.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TankardDB.Core.Stores;
    using System.Threading.Tasks;

    [TestClass]
    public class MemoryStoreTests
    {
        [TestClass]
        public class ReserveIdsMethod
        {
            [TestMethod]
            public async Task GenFirstId()
            {
                var target = new MemoryStore();
                var firstId = await target.ReserveIds(1L);
                Assert.AreEqual(1L, firstId[0]);
            }

            [TestMethod]
            public async Task GenSecondId()
            {
                var target = new MemoryStore();
                var firstId = await target.ReserveIds(1L);
                var secondId = await target.ReserveIds(1L);
                Assert.AreEqual(2L, secondId[0]);
            }

            [TestMethod]
            public async Task GenThirdId()
            {
                var target = new MemoryStore();
                var firstId = await target.ReserveIds(1L);
                var secondId = await target.ReserveIds(1L);
                var thirdId = await target.ReserveIds(1L);
                Assert.AreEqual(3L, thirdId[0]);
            }

            [TestMethod]
            public async Task GenMultiFirstId()
            {
                var target = new MemoryStore();
                var firstId = await target.ReserveIds(2L);
                Assert.AreEqual(1L, firstId[0]);
                Assert.AreEqual(2L, firstId[1]);
            }

            [TestMethod]
            public async Task GenMultiSecondId()
            {
                var target = new MemoryStore();
                var firstId = await target.ReserveIds(2L);
                var secondId = await target.ReserveIds(2L);
                Assert.AreEqual(3L, secondId[0]);
                Assert.AreEqual(4L, secondId[1]);
            }

            [TestMethod]
            public async Task GenMultiThirdId()
            {
                var target = new MemoryStore();
                var firstId = await target.ReserveIds(2L);
                var secondId = await target.ReserveIds(2L);
                var thirdId = await target.ReserveIds(2L);
                Assert.AreEqual(5L, thirdId[0]);
                Assert.AreEqual(6L, thirdId[1]);
            }
        }
    }
}
