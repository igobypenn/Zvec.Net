using Zvec.Net.Models;

namespace Zvec.Net.Tests.Models;

public class DocTests
{
    [Fact]
    public void Create_WithId_SetsId()
    {
        var doc = Doc.Create("doc123");
        
        Assert.Equal("doc123", doc.Id);
    }
    
    [Fact]
    public void Create_WithVector_SetsVector()
    {
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        
        var doc = Doc.Create("doc1", vector, "embedding");
        
        Assert.True(doc.HasVector("embedding"));
        Assert.NotNull(doc.Vector("embedding"));
    }
    
    [Fact]
    public void Create_WithFields_SetsFields()
    {
        var fields = new Dictionary<string, object?>
        {
            ["title"] = "Test",
            ["count"] = 42
        };
        
        var doc = Doc.Create("doc1", fields);
        
        Assert.True(doc.HasField("title"));
        Assert.True(doc.HasField("count"));
        Assert.Equal("Test", doc.Field<string>("title"));
        Assert.Equal(42, doc.Field<int>("count"));
    }
    
    [Fact]
    public void Create_WithFieldsAndVectors_SetsBoth()
    {
        var fields = new Dictionary<string, object?> { ["title"] = "Test" };
        var vectors = new Dictionary<string, object?> { ["embedding"] = new float[] { 1.0f } };
        
        var doc = Doc.Create("doc1", fields, vectors);
        
        Assert.True(doc.HasField("title"));
        Assert.True(doc.HasVector("embedding"));
    }
    
    [Fact]
    public void HasField_ExistingField_ReturnsTrue()
    {
        var fields = new Dictionary<string, object?> { ["title"] = "Test" };
        var doc = Doc.Create("doc1", fields);
        
        Assert.True(doc.HasField("title"));
    }
    
    [Fact]
    public void HasField_NonexistentField_ReturnsFalse()
    {
        var doc = Doc.Create("doc1");
        
        Assert.False(doc.HasField("nonexistent"));
    }
    
    [Fact]
    public void HasVector_ExistingVector_ReturnsTrue()
    {
        var doc = Doc.Create("doc1", new float[] { 1.0f }, "embedding");
        
        Assert.True(doc.HasVector("embedding"));
    }
    
    [Fact]
    public void HasVector_NonexistentVector_ReturnsFalse()
    {
        var doc = Doc.Create("doc1");
        
        Assert.False(doc.HasVector("nonexistent"));
    }
    
    [Fact]
    public void Field_Generic_ReturnsCorrectType()
    {
        var fields = new Dictionary<string, object?> { ["count"] = 42 };
        var doc = Doc.Create("doc1", fields);
        
        Assert.Equal(42, doc.Field<int>("count"));
    }
    
    [Fact]
    public void Field_Nonexistent_ReturnsDefault()
    {
        var doc = Doc.Create("doc1");
        
        Assert.Equal(0, doc.Field<int>("nonexistent"));
    }
    
    [Fact]
    public void Vector_ReturnsFloatArray()
    {
        var vector = new float[] { 1.0f, 2.0f, 3.0f };
        var doc = Doc.Create("doc1", vector, "embedding");
        
        var result = doc.Vector("embedding");
        
        Assert.Equal(vector, result);
    }
    
    [Fact]
    public void Vector_Nonexistent_ReturnsNull()
    {
        var doc = Doc.Create("doc1");
        
        Assert.Null(doc.Vector("nonexistent"));
    }
    
    [Fact]
    public void FieldNames_ReturnsAllFieldNames()
    {
        var fields = new Dictionary<string, object?>
        {
            ["title"] = "Test",
            ["count"] = 42
        };
        var doc = Doc.Create("doc1", fields);
        
        var names = doc.FieldNames;
        
        Assert.Equal(2, names.Count);
        Assert.Contains("title", names);
        Assert.Contains("count", names);
    }
    
    [Fact]
    public void VectorNames_ReturnsAllVectorNames()
    {
        var vectors = new Dictionary<string, object?>
        {
            ["embedding1"] = new float[] { 1.0f },
            ["embedding2"] = new float[] { 2.0f }
        };
        var doc = Doc.Create("doc1", new Dictionary<string, object?>(), vectors);
        
        var names = doc.VectorNames;
        
        Assert.Equal(2, names.Count);
        Assert.Contains("embedding1", names);
        Assert.Contains("embedding2", names);
    }
    
    [Fact]
    public void Score_CanBeSet()
    {
        var doc = new Doc { Id = "doc1", Score = 0.95 };
        
        Assert.Equal(0.95, doc.Score);
    }
    
    [Fact]
    public void ToString_ContainsId()
    {
        var doc = Doc.Create("doc123");
        
        var str = doc.ToString();
        
        Assert.Contains("doc123", str);
    }
    
    [Fact]
    public void ToString_WithScore_ContainsScore()
    {
        var doc = new Doc { Id = "doc1", Score = 0.95 };
        
        var str = doc.ToString();
        
        Assert.Contains("0.95", str);
    }
}
