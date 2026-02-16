using Zvec.Net.Exceptions;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void ZvecException_WithStatusCodeAndMessage_SetsBoth()
    {
        var ex = new ZvecException(StatusCode.InvalidArgument, "Invalid value");
        
        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Contains("Invalid value", ex.Message);
        Assert.Contains("InvalidArgument", ex.Message);
    }
    
    [Fact]
    public void ZvecException_WithInnerException_SetsInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ZvecException(StatusCode.InternalError, "outer", inner);
        
        Assert.Contains("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
    
    [Theory]
    [InlineData(StatusCode.Ok)]
    [InlineData(StatusCode.InvalidArgument)]
    [InlineData(StatusCode.NotFound)]
    [InlineData(StatusCode.InternalError)]
    [InlineData(StatusCode.AlreadyExists)]
    public void ZvecException_VariousStatusCodes_SetsCode(StatusCode code)
    {
        var ex = new ZvecException(code, "test");
        
        Assert.Equal(code, ex.StatusCode);
    }
    
    [Fact]
    public void SchemaValidationException_WithMessage_ContainsMessage()
    {
        var ex = new SchemaValidationException("Invalid schema");
        
        Assert.Contains("Invalid schema", ex.Message);
    }
    
    [Fact]
    public void UnsupportedExpressionException_WithMessage_ContainsMessage()
    {
        var ex = new UnsupportedExpressionException("Bad expression");
        
        Assert.Contains("Bad expression", ex.Message);
    }
}
