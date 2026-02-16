using System.Reflection;
using Zvec.Net.Attributes;
using Zvec.Net.Exceptions;
using Zvec.Net.Internal;
using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Schema;

public class TypeValidationTests
{
    [Fact]
    public void ValidateVectorProperty_AcceptsCorrectFloat32Type()
    {
        var prop = typeof(Article).GetProperty(nameof(Article.Embedding))!;
        var attr = prop.GetCustomAttribute<VectorFieldAttribute>()!;
        
        TypeValidator.ValidateVectorProperty(prop, attr);
    }
    
    [Fact]
    public void ValidateVectorProperty_AcceptsCorrectFloat16Type()
    {
        var prop = typeof(MultimediaDoc).GetProperty(nameof(MultimediaDoc.ImageEmbedding))!;
        var attr = prop.GetCustomAttribute<VectorFieldAttribute>()!;
        
        TypeValidator.ValidateVectorProperty(prop, attr);
    }
    
    [Fact]
    public void ValidateVectorProperty_AcceptsCorrectInt8Type()
    {
        var prop = typeof(MultimediaDoc).GetProperty(nameof(MultimediaDoc.AudioFingerprint))!;
        var attr = prop.GetCustomAttribute<VectorFieldAttribute>()!;
        
        TypeValidator.ValidateVectorProperty(prop, attr);
    }
    
    [Fact]
    public void ValidateVectorProperty_AcceptsSparseVectorType()
    {
        var prop = typeof(SparseDoc).GetProperty(nameof(SparseDoc.Keywords))!;
        var attr = prop.GetCustomAttribute<VectorFieldAttribute>()!;
        
        TypeValidator.ValidateVectorProperty(prop, attr);
    }
    
    [Fact]
    public void ValidateVectorProperty_ThrowsOnWrongPrecision()
    {
        var prop = typeof(Article).GetProperty(nameof(Article.Embedding))!;
        var wrongAttr = new VectorFieldAttribute(768, VectorPrecision.Float64);
        
        var ex = Assert.Throws<SchemaValidationException>(() => 
            TypeValidator.ValidateVectorProperty(prop, wrongAttr));
        
        Assert.Contains("Type mismatch", ex.Message);
        Assert.Contains("Float64", ex.Message);
        Assert.Contains("Single[]", ex.Message);
    }
    
    [Fact]
    public void ValidateVectorProperty_ThrowsOnZeroDimensionForDenseVector()
    {
        var prop = typeof(Article).GetProperty(nameof(Article.Embedding))!;
        var wrongAttr = new VectorFieldAttribute(0, VectorPrecision.Float32);
        
        var ex = Assert.Throws<SchemaValidationException>(() => 
            TypeValidator.ValidateVectorProperty(prop, wrongAttr));
        
        Assert.Contains("dimension", ex.Message.ToLower());
    }
}
