using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Models;

public class StatusTests
{
    [Fact]
    public void Ok_ReturnsSuccessStatus()
    {
        var status = Status.Ok;
        
        Assert.True(status.IsOk);
        Assert.Equal(StatusCode.Ok, status.Code);
        Assert.Equal(string.Empty, status.Message);
    }
    
    [Fact]
    public void From_OkCode_ReturnsOkStatus()
    {
        var status = Status.From(StatusCode.Ok, "");
        
        Assert.True(status.IsOk);
    }
    
    [Fact]
    public void From_ErrorCode_ReturnsErrorStatus()
    {
        var status = Status.From(StatusCode.InvalidArgument, "Invalid value");
        
        Assert.False(status.IsOk);
        Assert.Equal(StatusCode.InvalidArgument, status.Code);
        Assert.Equal("Invalid value", status.Message);
    }
    
    [Fact]
    public void From_EmptyMessage_ReturnsEmptyString()
    {
        var status = Status.From(StatusCode.InternalError, "");
        
        Assert.Equal(string.Empty, status.Message);
    }
    
    [Fact]
    public void IsOk_OkCode_ReturnsTrue()
    {
        var status = new Status { Code = StatusCode.Ok, Message = "OK" };
        
        Assert.True(status.IsOk);
    }
    
    [Fact]
    public void IsOk_ErrorCode_ReturnsFalse()
    {
        var status = new Status { Code = StatusCode.NotFound, Message = "Not found" };
        
        Assert.False(status.IsOk);
    }
    
    [Fact]
    public void IsError_ErrorCode_ReturnsTrue()
    {
        var status = new Status { Code = StatusCode.NotFound, Message = "Not found" };
        
        Assert.True(status.IsError);
    }
    
    [Fact]
    public void IsError_OkCode_ReturnsFalse()
    {
        var status = Status.Ok;
        
        Assert.False(status.IsError);
    }
    
    [Fact]
    public void ThrowIfError_OkStatus_DoesNotThrow()
    {
        var status = Status.Ok;
        
        status.ThrowIfError();
    }
    
    [Fact]
    public void ThrowIfError_ErrorStatus_ThrowsException()
    {
        var status = Status.From(StatusCode.InvalidArgument, "Test error");
        
        var ex = Assert.Throws<ZvecException>(() => status.ThrowIfError());
        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Contains("Test error", ex.Message);
    }
    
    [Fact]
    public void ToString_OkStatus_ReturnsOK()
    {
        var status = Status.Ok;
        
        Assert.Equal("OK", status.ToString());
    }
    
    [Fact]
    public void ToString_ErrorStatus_ReturnsCodeAndMessage()
    {
        var status = Status.From(StatusCode.NotFound, "Not found");
        
        var str = status.ToString();
        
        Assert.Contains("NotFound", str);
        Assert.Contains("Not found", str);
    }
}
