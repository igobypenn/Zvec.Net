using Zvec.Net.Exceptions;
using Zvec.Net.Index;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Schema;

public class SchemaTests
{
    [Fact]
    public void CollectionSchema_CreateWithFieldsAndVectors_Succeeds()
    {
        var fields = new[]
        {
            new FieldSchema("title", DataType.String),
            new FieldSchema("year", DataType.Int32, nullable: true)
        };
        
        var vectors = new[]
        {
            VectorSchema.Float32("embedding", 768)
        };
        
        var schema = new CollectionSchema("articles", fields, vectors);
        
        Assert.Equal("articles", schema.Name);
        Assert.Equal(2, schema.Fields.Count);
        Assert.Single(schema.Vectors);
    }
    
    [Fact]
    public void CollectionSchema_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CollectionSchema(""));
    }
    
    [Fact]
    public void CollectionSchema_NullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CollectionSchema(null!));
    }
    
    [Fact]
    public void CollectionSchema_DuplicateFieldName_ThrowsSchemaValidationException()
    {
        var fields = new[]
        {
            new FieldSchema("title", DataType.String),
            new FieldSchema("title", DataType.String)
        };
        
        Assert.Throws<SchemaValidationException>(() => new CollectionSchema("test", fields));
    }
    
    [Fact]
    public void CollectionSchema_DuplicateVectorAndFieldName_ThrowsSchemaValidationException()
    {
        var fields = new[] { new FieldSchema("embedding", DataType.String) };
        var vectors = new[] { VectorSchema.Float32("embedding", 128) };
        
        Assert.Throws<SchemaValidationException>(() => new CollectionSchema("test", fields, vectors));
    }
    
    [Fact]
    public void CollectionSchema_GetField_ReturnsCorrectField()
    {
        var fields = new[] { new FieldSchema("Title", DataType.String) };
        var schema = new CollectionSchema("test", fields);
        
        var field = schema.GetField("title");
        
        Assert.NotNull(field);
        Assert.Equal("Title", field!.Name);
    }
    
    [Fact]
    public void CollectionSchema_GetField_CaseInsensitive()
    {
        var fields = new[] { new FieldSchema("Title", DataType.String) };
        var schema = new CollectionSchema("test", fields);
        
        var field = schema.GetField("TITLE");
        
        Assert.NotNull(field);
    }
    
    [Fact]
    public void CollectionSchema_GetField_Nonexistent_ReturnsNull()
    {
        var schema = new CollectionSchema("test");
        
        var field = schema.GetField("nonexistent");
        
        Assert.Null(field);
    }
    
    [Fact]
    public void CollectionSchema_HasField_ReturnsCorrectBool()
    {
        var fields = new[] { new FieldSchema("title", DataType.String) };
        var schema = new CollectionSchema("test", fields);
        
        Assert.True(schema.HasField("title"));
        Assert.False(schema.HasField("nonexistent"));
    }
    
    [Fact]
    public void CollectionSchema_GetVector_ReturnsCorrectVector()
    {
        var vectors = new[] { VectorSchema.Float32("Embedding", 768) };
        var schema = new CollectionSchema("test", null, vectors);
        
        var vector = schema.GetVector("embedding");
        
        Assert.NotNull(vector);
        Assert.Equal("Embedding", vector!.Name);
    }
    
    [Fact]
    public void CollectionSchema_HasVector_ReturnsCorrectBool()
    {
        var vectors = new[] { VectorSchema.Float32("embedding", 768) };
        var schema = new CollectionSchema("test", null, vectors);
        
        Assert.True(schema.HasVector("embedding"));
        Assert.False(schema.HasVector("nonexistent"));
    }
    
    [Fact]
    public void CollectionSchema_EmptyCollections_Succeeds()
    {
        var schema = new CollectionSchema("test");
        
        Assert.Empty(schema.Fields);
        Assert.Empty(schema.Vectors);
    }
    
    [Fact]
    public void CollectionSchema_ToString_ContainsName()
    {
        var schema = new CollectionSchema("my_collection");
        
        var str = schema.ToString();
        
        Assert.Contains("my_collection", str);
    }
}

public class FieldSchemaTests
{
    [Fact]
    public void Create_ValidParams_Succeeds()
    {
        var field = new FieldSchema("title", DataType.String, nullable: true);
        
        Assert.Equal("title", field.Name);
        Assert.Equal(DataType.String, field.DataType);
        Assert.True(field.Nullable);
    }
    
    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FieldSchema("", DataType.String));
    }
    
    [Fact]
    public void Create_NullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FieldSchema(null!, DataType.String));
    }
    
    [Fact]
    public void Create_VectorDataType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FieldSchema("vec", DataType.VectorFp32));
    }
    
    [Theory]
    [InlineData(DataType.Int32)]
    [InlineData(DataType.Int64)]
    [InlineData(DataType.Float)]
    [InlineData(DataType.Double)]
    [InlineData(DataType.String)]
    [InlineData(DataType.Bool)]
    public void Create_ScalarDataTypes_Succeed(DataType dataType)
    {
        var field = new FieldSchema("field", dataType);
        Assert.Equal(dataType, field.DataType);
    }
    
    [Fact]
    public void ToString_ContainsName()
    {
        var field = new FieldSchema("myField", DataType.String);
        
        var str = field.ToString();
        
        Assert.Contains("myField", str);
    }
}

public class VectorSchemaTests
{
    [Fact]
    public void Float32_CreatesCorrectSchema()
    {
        var schema = VectorSchema.Float32("embedding", 768);
        
        Assert.Equal("embedding", schema.Name);
        Assert.Equal(DataType.VectorFp32, schema.DataType);
        Assert.Equal(768, schema.Dimension);
        Assert.False(schema.IsSparse);
    }
    
    [Fact]
    public void Float64_CreatesCorrectSchema()
    {
        var schema = VectorSchema.Float64("embedding", 512);
        
        Assert.Equal(DataType.VectorFp64, schema.DataType);
        Assert.Equal(512, schema.Dimension);
    }
    
    [Fact]
    public void Float16_CreatesCorrectSchema()
    {
        var schema = VectorSchema.Float16("embedding", 256);
        
        Assert.Equal(DataType.VectorFp16, schema.DataType);
    }
    
    [Fact]
    public void Int8_CreatesCorrectSchema()
    {
        var schema = VectorSchema.Int8("embedding", 128);
        
        Assert.Equal(DataType.VectorInt8, schema.DataType);
    }
    
    [Fact]
    public void SparseFloat32_CreatesCorrectSchema()
    {
        var schema = VectorSchema.SparseFloat32("keywords");
        
        Assert.Equal(DataType.SparseVectorFp32, schema.DataType);
        Assert.True(schema.IsSparse);
        Assert.Equal(0, schema.Dimension);
    }
    
    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VectorSchema("", DataType.VectorFp32, 128));
    }
    
    [Fact]
    public void Create_NonVectorDataType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VectorSchema("field", DataType.String, 128));
    }
    
    [Fact]
    public void Create_NegativeDimension_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VectorSchema("vec", DataType.VectorFp32, -1));
    }
    
    [Fact]
    public void Create_ZeroDimension_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VectorSchema("vec", DataType.VectorFp32, 0));
    }
    
    [Fact]
    public void Create_WithIndexParams_SetsIndexParams()
    {
        var indexParams = IndexParams.Hnsw(m: 32);
        var schema = new VectorSchema("vec", DataType.VectorFp32, 128, indexParams);
        
        Assert.NotNull(schema.IndexParams);
        Assert.Equal(IndexType.Hnsw, schema.IndexParams!.Type);
    }
    
    [Fact]
    public void Create_DefaultIndexParams_IsFlat()
    {
        var schema = new VectorSchema("vec", DataType.VectorFp32, 128);
        
        Assert.NotNull(schema.IndexParams);
        Assert.Equal(IndexType.Flat, schema.IndexParams!.Type);
    }
    
    [Fact]
    public void ToString_ContainsName()
    {
        var schema = VectorSchema.Float32("myVector", 128);
        
        var str = schema.ToString();
        
        Assert.Contains("myVector", str);
    }
}
