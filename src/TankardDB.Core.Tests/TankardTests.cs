
namespace TankardDB.Core.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TankardDB.Core.Stores;

    [TestClass]
    public class TankardTests
    {
        public class AClass : ITankardItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        [TestClass]
        public class InsertMethod
        {
            [TestMethod]
            public async Task GeneratesId1()
            {
                var target = new Tankard(new MemoryStore());
                var item = new AClass();
                await target.Insert(item);
                Assert.AreEqual("AClass-1", item.Id);
            }

            [TestMethod]
            public async Task GeneratesId2()
            {
                var target = new Tankard(new MemoryStore());
                var item = new AClass();
                await target.Insert(item);
                item = new AClass();
                await target.Insert(item);
                Assert.AreEqual("AClass-2", item.Id);
            }

            [TestMethod]
            public async Task ObjectStoreGetsCalled()
            {
                var item = new AClass();
                var store = new Mock<IStore>();
                store.Setup(s => s.ReserveIds(It.IsAny<long>())).ReturnsAsync(new long[] { 1, }).Verifiable();
                store.Setup(s => s.AppendObject(It.Is<ITankardItem>(x => x == item))).Verifiable();
                var target = new Tankard(store.Object);
                await target.Insert(item);
                store.VerifyAll();
            }
        }
    }
}
