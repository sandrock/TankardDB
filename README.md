
# TankardDB

Experimenting a simple document store for client app scenarios

## Status

**This is a Proof-of-Concept under development.**

### Major TODOs

- [x] Basic operation: Insert
- [x] Basic operation: Select
- [ ] Basic operation: Update
- [ ] Basic operation: Delete
- [ ] Benchmark: prove it is faster that other libs
- [ ] Indexing

See [TODO](TODO.md).


## Goals

Provide a super-simple way to store and retreive C# objects.

Store in memory or on disk. Or someplace else.


## Why?

sqlite:

- forces you to manage a schema, this is boring
- has good performance
- is easy to backup

homemade serialized list

- has real bad performance when the list grows big


## How?

    // open a database using a temporary in-memory store
    var target = new Tankard(new MemoryStore());
    
    //  insert a few items
    var item = new AClass();
    await target.Insert(item);
    Assert.AreEqual("AClass-1", item.Id);
    var item = new BClass();
    await target.Insert(item);
    Assert.AreEqual("BClass-2", item.Id);
    
    // fetch items of type AClass
    var query = target.Query.OfType<AClass>().ToArray();
    Assert.AreEqual(query.Count(), 1);
    
    

