using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Query;

namespace Zvec.Net.Tests.Query;

public class VectorQueryTests
{
    [Fact]
    public void ByVector_ValidParams_CreatesQuery()
    {
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        
        var query = VectorQuery.ByVector("embedding", vector);
        
        Assert.Equal("embedding", query.FieldName);
        Assert.Equal(vector, query.Vector);
        Assert.Null(query.DocumentId);
        Assert.True(query.HasVector);
        Assert.False(query.HasId);
    }
    
    [Fact]
    public void ByVector_WithWeight_SetsWeight()
    {
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        
        var query = VectorQuery.ByVector("embedding", vector, weight: 0.5);
        
        Assert.Equal(0.5, query.Weight);
    }
    
    [Fact]
    public void ByVector_WithParam_SetsParam()
    {
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        var param = IndexQueryParam.Hnsw(ef: 100);
        
        var query = VectorQuery.ByVector("embedding", vector, param: param);
        
        Assert.NotNull(query.Param);
        Assert.IsType<HnswQueryParam>(query.Param);
    }
    
    [Fact]
    public void ByVector_NullVector_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VectorQuery.ByVector("embedding", null!));
    }
    
    [Fact]
    public void ByVector_EmptyVector_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VectorQuery.ByVector("embedding", Array.Empty<float>()));
    }
    
    [Fact]
    public void ById_ValidParams_CreatesQuery()
    {
        var query = VectorQuery.ById("embedding", "doc123");
        
        Assert.Equal("embedding", query.FieldName);
        Assert.Equal("doc123", query.DocumentId);
        Assert.Null(query.Vector);
        Assert.True(query.HasId);
        Assert.False(query.HasVector);
    }
    
    [Fact]
    public void ById_WithWeight_SetsWeight()
    {
        var query = VectorQuery.ById("embedding", "doc123", weight: 0.8);
        
        Assert.Equal(0.8, query.Weight);
    }
    
    [Fact]
    public void ById_NullId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VectorQuery.ById("embedding", null!));
    }
    
    [Fact]
    public void ById_EmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => VectorQuery.ById("embedding", ""));
    }
    
    [Fact]
    public void BySparseVector_ValidParams_CreatesQuery()
    {
        var sparse = new SparseVector(new uint[] { 1, 2, 3 }, new float[] { 0.1f, 0.2f, 0.3f });
        
        var query = VectorQuery.BySparseVector("keywords", sparse);
        
        Assert.Equal("keywords", query.FieldName);
        Assert.NotNull(query.SparseVector);
        Assert.True(query.HasVector);
        Assert.True(query.IsSparse);
    }
    
    [Fact]
    public void Validate_EmptyFieldName_ThrowsArgumentException()
    {
        var vector = new float[] { 1.0f };
        var query = new VectorQuery("") { Vector = vector };
        
        Assert.Throws<ArgumentException>(() => query.Validate());
    }
    
    [Fact]
    public void Validate_NoVectorOrId_ThrowsArgumentException()
    {
        var query = new VectorQuery("embedding");
        
        Assert.Throws<ArgumentException>(() => query.Validate());
    }
    
    [Fact]
    public void Validate_BothVectorAndId_ThrowsArgumentException()
    {
        var query = new VectorQuery("embedding")
        {
            Vector = new float[] { 1.0f },
            DocumentId = "doc123"
        };
        
        Assert.Throws<ArgumentException>(() => query.Validate());
    }
    
    [Fact]
    public void Validate_ValidQuery_Succeeds()
    {
        var vector = new float[] { 1.0f };
        var query = VectorQuery.ByVector("embedding", vector);
        
        query.Validate();
    }
}
