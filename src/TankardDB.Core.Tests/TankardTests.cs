
namespace TankardDB.Core.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using TankardDB.Core.Internals;
    using TankardDB.Core.Stores;

    [TestClass]
    public class TankardTests
    {
        public class AClass : ITankardItem
        {
            public AClass()
            {
            }

            public AClass(string name)
            {
                this.Name = name;
            }

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
                var store = new TestStore();
                store.ReserveIdsDelegate = async x =>
                {
                    return await Task.Run(() => new long[] { 1, });
                };
                store.AppendObjectDelegate = async (id,bytes) =>
                {
                    return await Task.Run(() =>
                    {
                        var row = new MainIndexRow("AClass-1", 1, 0);
                        return row;
                    });
                };
                store.AppendMainIndexDelegate = async x =>
                {
                    await Task.Run(() => { });
                };
                var target = new Tankard(store);
                await target.Insert(item);

                Assert.AreEqual(1, store.ReserveIdsCount, "ReserveIds should have been called 1 time");
                Assert.AreEqual(1, store.AppendObjectCount, "AppendObject should have been called 1 time");
                Assert.AreEqual(1, store.AppendMainIndexCount, "AppendMainIndex should have been called 1 time");
            }
        }

        [TestClass]
        public class GetByIdMethod
        {
            private static Tankard GetTarget()
            {
                return new Tankard(new MemoryStore());
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task Single_Arg0IsNull()
            {
                var target = GetTarget();
                string id = null;
                var result = await target.GetById(id);
            }

            [TestMethod, ExpectedException(typeof(ArgumentException))]
            public async Task Single_Arg0IsEmpty()
            {
                var target = GetTarget();
                string id = string.Empty;
                var result = await target.GetById(id);
            }

            [TestMethod]
            public async Task Single_FirstInsert()
            {
                var target = GetTarget();
                
                AClass insert = new AClass("Hello world");
                await target.Insert(insert);
                string id = insert.Id;

                var objResult = await target.GetById(id);
                AClass result = (AClass)objResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(id, result.Id);
                Assert.AreEqual(insert.Name, result.Name);
            }

            [TestMethod]
            public async Task Single_SecondInsert()
            {
                var target = GetTarget();

                AClass[] inserts = new AClass[]
                {
                    new AClass("Hello world"),
                    new AClass("Poke & mon"),
                };
                await target.Insert(inserts[0]);
                await target.Insert(inserts[1]);

                string expectedId = "AClass-2";
                Assert.AreEqual(expectedId, inserts[1].Id);

                var objResult = await target.GetById(inserts[1].Id);
                AClass result = (AClass)objResult;
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedId, result.Id);
                Assert.AreEqual(inserts[1].Name, result.Name);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public async Task Multiple_Arg0IsNull()
            {
                var target = GetTarget();
                string[] ids = null;
                var result = await target.GetById(ids);
            }

            [TestMethod]
            public async Task Multiple_SecondInsert()
            {
                var target = GetTarget();

                AClass[] inserts = new AClass[]
                {
                    new AClass("Hello world"),
                    new AClass("Poke & mon"),
                };
                await target.Insert(inserts[0]);
                await target.Insert(inserts[1]);

                string[] ids = inserts.Select(x => x.Id).ToArray();

                ITankardItem[] result = await target.GetById(ids);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Length);
                int i = -1;
                Assert.AreEqual(inserts[++i].Name, ((AClass)result[i]).Name);
                Assert.AreEqual(inserts[++i].Name, ((AClass)result[i]).Name);
            }
        }
    }
}
