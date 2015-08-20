
# TankardDB

Experimenting a simple document store for client app scenarios.

## Status

**This is a Proof-of-Concept under development.**

### Major TODOs

- [x] Basic operation: Insert
- [x] Basic operation: Select
- [ ] Basic operation: Update
- [ ] Basic operation: Delete
- [ ] FileStore
- [ ] Benchmark: prove it is faster that other libs
- [ ] Indexing

See [TODO](TODO.md).


## Goals

### Simplicity

Provide a super-simple way to store and retreive C# objects. Serialization is done using Json.NET and file access is designed for speed.

You get two IStore classes to work in-memory or on disk. You can implement the interface to store someplace else.

Very few operations are permitted to keep the lib lightweight.

### Speed

Very few operations allows to perform them very quickly. We are focusing on that.

### Portability

It compiles as PCL (.net 4.5, WP SL 8, WP 8.1, W 8).


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
    // open a database using a file store
    var target = new Tankard(new FileStore("c:\\Path\\To\\Database\\Folder"));
    
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




