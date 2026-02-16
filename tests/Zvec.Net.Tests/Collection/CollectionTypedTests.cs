using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Native;
using Zvec.Net.Query;
using Zvec.Net.Schema;
using Zvec.Net.Tests.Mocks;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Collection;

public class CollectionTypedTests : IDisposable
{
    private readonly MockNativeMethods _mock;
    private readonly Collection<Article> _collection;
    private readonly string _testPath;
    
    public CollectionTypedTests()
    {
        _mock = new MockNativeMethods();
        _testPath = $"/tmp/test_{Guid.NewGuid():N}";
        
        var schema = SchemaGenerator.GenerateSchema<Article>();
        _collection = Collection<Article>.CreateAndOpen(_testPath, null, _mock);
    }
    
    public void Dispose()
    {
        _collection?.Dispose();
    }
    
    // ===== Insert Tests =====
    
    [Fact]
    public void Insert_SingleDocument_ReturnsOk()
    {
        var doc = new Article { Id = "doc1", Title = "Test", Year = 2024 };
        
        var status = _collection.Insert(doc);
        
        Assert.True(status.IsOk);
        Assert.Contains(_mock.Collections.Values, c => c.Documents.ContainsKey("doc1"));
    }
    
    [Fact]
    public void Insert_MultipleDocuments_StoresAll()
    {
        var docs = new[]
        {
            new Article { Id = "doc1", Title = "First" },
            new Article { Id = "doc2", Title = "Second" },
            new Article { Id = "doc3", Title = "Third" }
        };
        
        var status = _collection.Insert(docs);
        
        Assert.True(status.IsOk);
        var collection = _mock.Collections.Values.First();
        Assert.Equal(3, collection.Documents.Count);
    }
    
    [Fact]
    public void Insert_EmptyList_ReturnsOk()
    {
        var status = _collection.Insert(Array.Empty<Article>());
        
        Assert.True(status.IsOk);
    }
    
    [Fact]
    public void Insert_WithVector_StoresVector()
    {
        var doc = new Article
        {
            Id = "doc1",
            Title = "Test",
            Embedding = new float[] { 1.0f, 2.0f, 3.0f }
        };
        
        var status = _collection.Insert(doc);
        
        Assert.True(status.IsOk);
        // Verify the document was stored
        var collection = _mock.Collections.Values.FirstOrDefault();
        Assert.NotNull(collection);
        Assert.True(collection.Documents.ContainsKey("doc1"));
    }
    
    // ===== Upsert Tests =====
    
    [Fact]
    public void Upsert_NewDocument_StoresDocument()
    {
        var doc = new Article { Id = "doc1", Title = "Test" };
        
        var status = _collection.Upsert(doc);
        
        Assert.True(status.IsOk);
        Assert.Contains(_mock.Collections.Values.First().Documents, d => d.Key == "doc1");
    }
    
    [Fact]
    public void Upsert_ExistingDocument_UpdatesDocument()
    {
        var doc1 = new Article { Id = "doc1", Title = "Original" };
        _collection.Insert(doc1);
        
        var doc2 = new Article { Id = "doc1", Title = "Updated" };
        var status = _collection.Upsert(doc2);
        
        Assert.True(status.IsOk);
    }
    
    // ===== Update Tests =====
    
    [Fact]
    public void Update_ExistingDocument_Succeeds()
    {
        var doc = new Article { Id = "doc1", Title = "Original" };
        _collection.Insert(doc);
        
        var updated = new Article { Id = "doc1", Title = "Updated" };
        var status = _collection.Update(updated);
        
        Assert.True(status.IsOk);
    }
    
    // ===== Delete Tests =====
    
    [Fact]
    public void Delete_ExistingDocument_RemovesDocument()
    {
        var doc = new Article { Id = "doc1", Title = "Test" };
        _collection.Insert(doc);
        
        var status = _collection.Delete("doc1");
        
        Assert.True(status.IsOk);
        Assert.DoesNotContain(_mock.Collections.Values.First().Documents, d => d.Key == "doc1");
    }
    
    [Fact]
    public void Delete_MultipleDocuments_RemovesAll()
    {
        _collection.Insert(
            new Article { Id = "doc1" },
            new Article { Id = "doc2" },
            new Article { Id = "doc3" }
        );
        
        var status = _collection.Delete("doc1", "doc2");
        
        Assert.True(status.IsOk);
        var collection = _mock.Collections.Values.First();
        Assert.Single(collection.Documents);
    }
    
    [Fact]
    public void Delete_EmptyList_ReturnsOk()
    {
        var status = _collection.Delete(Array.Empty<string>());
        
        Assert.True(status.IsOk);
    }
    
    // ===== DeleteByFilter Tests =====
    
    [Fact]
    public void DeleteByFilter_CallsNativeMethod()
    {
        var status = _collection.DeleteByFilter("year > 2020");
        
        Assert.True(status.IsOk);
        Assert.Contains("zvec_collection_delete_by_filter(year > 2020)", _mock.MethodCalls);
    }
    
    // ===== Fetch Tests =====
    
    [Fact]
    public void Fetch_ExistingDocument_ReturnsDocument()
    {
        var doc = new Article { Id = "doc1", Title = "Test", Year = 2024 };
        _collection.Insert(doc);
        
        var result = _collection.Fetch("doc1");
        
        Assert.Single(result);
        Assert.True(result.ContainsKey("doc1"));
    }
    
    [Fact]
    public void Fetch_MultipleDocuments_ReturnsAll()
    {
        _collection.Insert(
            new Article { Id = "doc1", Title = "First" },
            new Article { Id = "doc2", Title = "Second" }
        );
        
        var result = _collection.Fetch("doc1", "doc2");
        
        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public void Fetch_NonExistentDocument_ReturnsEmpty()
    {
        var result = _collection.Fetch("nonexistent");
        
        Assert.Empty(result);
    }
    
    // ===== Flush Tests =====
    
    [Fact]
    public void Flush_CallsNativeMethod()
    {
        _collection.Flush();
        
        Assert.Contains("zvec_collection_flush", _mock.MethodCalls);
    }
    
    // ===== Optimize Tests =====
    
    [Fact]
    public void Optimize_CallsNativeMethod()
    {
        _collection.Optimize();
        
        Assert.Contains("zvec_collection_optimize", _mock.MethodCalls);
    }
    
    // ===== CreateIndex Tests =====
    
    [Fact]
    public void CreateIndex_CallsNativeMethod()
    {
        _collection.CreateIndex("embedding", IndexParams.Flat());
        
        Assert.Contains("zvec_collection_create_index(embedding)", _mock.MethodCalls);
    }
    
    // ===== DropIndex Tests =====
    
    [Fact]
    public void DropIndex_CallsNativeMethod()
    {
        _collection.DropIndex("embedding");
        
        Assert.Contains("zvec_collection_drop_index(embedding)", _mock.MethodCalls);
    }
    
    // ===== Query Tests =====
    
    [Fact]
    public void Query_ReturnsResults()
    {
        _collection.Insert(new Article { Id = "doc1", Title = "Test" });
        
        var vectorQuery = VectorQuery.ByVector("embedding", new float[768]);
        var results = _collection.Query(vectorQuery);
        
        Assert.NotEmpty(results);
    }
    
    // ===== Disposal Tests =====
    
    [Fact]
    public void Dispose_CallsNativeDestroy()
    {
        var collection = Collection<Article>.CreateAndOpen("/tmp/test_dispose", null, _mock);
        
        collection.Dispose();
        
        Assert.Contains("zvec_collection_destroy", _mock.MethodCalls);
    }
    
    [Fact]
    public void AccessAfterDispose_ThrowsInvalidOperationException()
    {
        var collection = Collection<Article>.CreateAndOpen("/tmp/test_dispose2", null, _mock);
        collection.Dispose();
        
        Assert.Throws<InvalidOperationException>(() => collection.Flush());
    }
    
    // ===== Async Tests =====
    
    [Fact]
    public async Task InsertAsync_InsertsDocuments()
    {
        var doc = new Article { Id = "async1", Title = "Async Test" };
        
        var status = await _collection.InsertAsync(new[] { doc });
        
        Assert.True(status.IsOk);
    }
    
    [Fact]
    public async Task QueryAsync_ReturnsResults()
    {
        _collection.Insert(new Article { Id = "doc1" });
        
        var vectorQuery = VectorQuery.ByVector("embedding", new float[768]);
        var results = await _collection.QueryAsync(vectorQuery);
        
        Assert.NotNull(results);
    }
    
    [Fact]
    public async Task FetchAsync_ReturnsDocuments()
    {
        _collection.Insert(new Article { Id = "doc1" });
        
        var result = await _collection.FetchAsync(new[] { "doc1" });
        
        Assert.Single(result);
    }
    
    // ===== Error Handling Tests =====
    
    [Fact]
    public void Open_NonexistentPath_ThrowsException()
    {
        var errorMock = new MockNativeMethods { SimulateErrors = true, ForceErrorCode = 5 };
        
        Assert.Throws<ZvecException>(() => 
            Collection<Article>.Open("/nonexistent", null, errorMock));
    }
    
    // ===== Path Property =====
    
    [Fact]
    public void Path_ReturnsCollectionPath()
    {
        Assert.Equal(_testPath, _collection.Path);
    }
    
    // ===== Schema Property =====
    
    [Fact]
    public void Schema_ReturnsSchema()
    {
        Assert.NotNull(_collection.Schema);
    }
}
