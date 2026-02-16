using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Native;
using Zvec.Net.Schema;
using Zvec.Net.Tests.Mocks;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Collection;

public class CollectionTests : IDisposable
{
    private readonly MockNativeMethods _mock;
    private readonly string _testPath;

    public CollectionTests()
    {
        _mock = new MockNativeMethods();
        _testPath = $"/tmp/test_{Guid.NewGuid():N}";
    }

    public void Dispose()
    {
    }

    [Fact]
    public void CreateAndOpen_WithSchema_Succeeds()
    {
        var schema = new CollectionSchema("test",
            new[] { new FieldSchema("title", DataType.String) },
            new[] { VectorSchema.Float32("embedding", 128) });

        var collection = global::Zvec.Net.Collection.CreateAndOpen(_testPath, schema, null, _mock);

        Assert.NotNull(collection);
        Assert.Equal(schema, collection.Schema);
        collection.Dispose();
    }

    [Fact]
    public void Open_ReturnsCollection()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        Assert.NotNull(collection);
        Assert.NotNull(collection.Schema);
        collection.Dispose();
    }

    [Fact]
    public void Path_ReturnsCollectionPath()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        Assert.Equal(_testPath, collection.Path);

        collection.Dispose();
    }

    [Fact]
    public void Schema_ReturnsSchema()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        Assert.NotNull(collection.Schema);

        collection.Dispose();
    }

    [Fact]
    public void Flush_CallsNativeMethod()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        collection.Flush();

        Assert.Contains("zvec_collection_flush", _mock.MethodCalls);

        collection.Dispose();
    }

    [Fact]
    public void Destroy_CallsNativeMethod()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        collection.Destroy();

        Assert.Contains("zvec_collection_destroy_data", _mock.MethodCalls);
    }

    [Fact]
    public void Dispose_CallsNativeDestroy()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        collection.Dispose();

        Assert.Contains("zvec_collection_destroy", _mock.MethodCalls);
    }

    [Fact]
    public void CreateAndOpen_NullPath_ThrowsArgumentNullException()
    {
        var schema = new CollectionSchema("test");

        Assert.Throws<ArgumentException>(() => global::Zvec.Net.Collection.CreateAndOpen(null!, schema));
    }

    [Fact]
    public void Open_NullPath_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentException>(() => global::Zvec.Net.Collection.Open(null!));
    }

    [Fact]
    public void Stats_ReturnsEmptyStats()
    {
        var collection = global::Zvec.Net.Collection.Open(_testPath, null, _mock);

        var stats = collection.Stats;

        Assert.NotNull(stats);

        collection.Dispose();
    }

    // Generic factory method tests - these delegate to Collection<T>

    [Fact]
    public void CreateAndOpen_Generic_Exists()
    {
        // Just verify the method exists and compiles
        var methods = typeof(global::Zvec.Net.Collection).GetMethods();
        var createMethod = methods.FirstOrDefault(m => m.Name == "CreateAndOpen" && m.IsGenericMethodDefinition);
        Assert.NotNull(createMethod);
    }

    [Fact]
    public void Open_Generic_Exists()
    {
        var methods = typeof(global::Zvec.Net.Collection).GetMethods();
        var openMethod = methods.FirstOrDefault(m => m.Name == "Open" && m.IsGenericMethodDefinition);
        Assert.NotNull(openMethod);
    }
}
