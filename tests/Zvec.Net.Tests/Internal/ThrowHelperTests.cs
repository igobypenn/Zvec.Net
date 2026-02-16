using Zvec.Net.Internal;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Internal;

public class ThrowHelperTests
{
    [Fact]
    public void ThrowIfNull_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ThrowHelper.ThrowIfNull(null, "param"));
    }

    [Fact]
    public void ThrowIfNull_WithValue_DoesNotThrow()
    {
        ThrowHelper.ThrowIfNull("value", "param");
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WithNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ThrowHelper.ThrowIfNullOrEmpty(null, "param"));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WithEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ThrowHelper.ThrowIfNullOrEmpty("", "param"));
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WithValue_DoesNotThrow()
    {
        ThrowHelper.ThrowIfNullOrEmpty("value", "param");
    }

    [Fact]
    public void CollectionDisposed_ReturnsInvalidOperationException()
    {
        var ex = ThrowHelper.CollectionDisposed();

        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("disposed", ex.Message.ToLower());
    }

    [Fact]
    public void NoVectorQuerySpecified_WithVectors_ReturnsInvalidOperationException()
    {
        var vectors = new List<string> { "embedding1", "embedding2" };

        var ex = ThrowHelper.NoVectorQuerySpecified(vectors);

        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("embedding1", ex.Message);
        Assert.Contains("embedding2", ex.Message);
    }

    [Fact]
    public void UnsupportedExpression_ReturnsNotSupportedException()
    {
        var ex = ThrowHelper.UnsupportedExpression("test expression");

        Assert.IsType<NotSupportedException>(ex);
        Assert.Contains("test expression", ex.Message);
    }

    [Fact]
    public void PlatformNotSupported_ReturnsNotSupportedException()
    {
        var ex = ThrowHelper.PlatformNotSupported("windows-x86");

        Assert.IsType<NotSupportedException>(ex);
        Assert.Contains("windows-x86", ex.Message);
    }
}
