using Zvec.Net.Exceptions;
using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Schema;

public class SchemaGeneratorTests
{
    [Fact]
    public void GenerateSchema_GeneratesCorrectFieldTypes()
    {
        var schema = SchemaGenerator.GenerateSchema<Article>();
        
        Assert.Equal(nameof(Article), schema.Name);
        Assert.Equal(4, schema.Fields.Count);
        Assert.Single(schema.Vectors);
        
        Assert.NotNull(schema.GetField(nameof(Article.Title)));
        Assert.NotNull(schema.GetField(nameof(Article.Category)));
        Assert.NotNull(schema.GetField(nameof(Article.Year)));
        Assert.NotNull(schema.GetField(nameof(Article.Price)));
        
        Assert.NotNull(schema.GetVector(nameof(Article.Embedding)));
    }
    
    [Fact]
    public void GenerateSchema_SetsCorrectDataTypes()
    {
        var schema = SchemaGenerator.GenerateSchema<Article>();
        
        Assert.Equal(DataType.String, schema.GetField(nameof(Article.Title))!.DataType);
        Assert.Equal(DataType.Int32, schema.GetField(nameof(Article.Year))!.DataType);
        Assert.Equal(DataType.Double, schema.GetField(nameof(Article.Price))!.DataType);
        Assert.Equal(DataType.VectorFp32, schema.GetVector(nameof(Article.Embedding))!.DataType);
    }
    
    [Fact]
    public void GenerateSchema_SetsVectorDimensions()
    {
        var schema = SchemaGenerator.GenerateSchema<Article>();
        
        var embedding = schema.GetVector(nameof(Article.Embedding));
        Assert.NotNull(embedding);
        Assert.Equal(768, embedding.Dimension);
    }
    
    [Fact]
    public void GenerateSchema_SupportsMultipleVectors()
    {
        var schema = SchemaGenerator.GenerateSchema<MultimediaDoc>();
        
        Assert.Equal(3, schema.Vectors.Count);
        
        Assert.NotNull(schema.GetVector(nameof(MultimediaDoc.TextEmbedding)));
        Assert.NotNull(schema.GetVector(nameof(MultimediaDoc.ImageEmbedding)));
        Assert.NotNull(schema.GetVector(nameof(MultimediaDoc.AudioFingerprint)));
    }
    
    [Fact]
    public void GenerateSchema_SetsCorrectVectorPrecisions()
    {
        var schema = SchemaGenerator.GenerateSchema<MultimediaDoc>();
        
        Assert.Equal(DataType.VectorFp32, schema.GetVector(nameof(MultimediaDoc.TextEmbedding))!.DataType);
        Assert.Equal(DataType.VectorFp16, schema.GetVector(nameof(MultimediaDoc.ImageEmbedding))!.DataType);
        Assert.Equal(DataType.VectorInt8, schema.GetVector(nameof(MultimediaDoc.AudioFingerprint))!.DataType);
    }
    
    [Fact]
    public void GenerateSchema_SupportsSparseVectors()
    {
        var schema = SchemaGenerator.GenerateSchema<SparseDoc>();
        
        var keywords = schema.GetVector(nameof(SparseDoc.Keywords));
        Assert.NotNull(keywords);
        Assert.True(keywords.IsSparse);
        Assert.Equal(DataType.SparseVectorFp32, keywords.DataType);
    }
    
    [Fact]
    public void GenerateSchema_ThrowsOnDuplicateNames()
    {
        // This would require a custom document type with duplicate field names
        // For now, we test that the schema validates internally
        Assert.Throws<SchemaValidationException>(() => 
            new CollectionSchema("test", 
                new[] { new FieldSchema("id", DataType.Int64), new FieldSchema("id", DataType.String) }));
    }
}
