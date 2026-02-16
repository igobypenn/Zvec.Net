using Zvec.Net.Types;

namespace Zvec.Net.Internal;

internal static class ThrowHelper
{
    public static void ThrowIfNull(object? argument, string paramName)
    {
        if (argument is null)
            throw new ArgumentNullException(paramName);
    }

    public static void ThrowIfNullOrEmpty(string? argument, string paramName)
    {
        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException("Value cannot be null or empty.", paramName);
    }

    public static InvalidOperationException CollectionDisposed() =>
        new("Collection has been disposed.");

    public static InvalidOperationException NoVectorQuerySpecified(IReadOnlyList<string> availableVectors)
    {
        var vectorList = string.Join(", ", availableVectors);
        return new InvalidOperationException(
            $"No vector query specified. Call VectorNearest() before Execute().\n" +
            $"Available vector fields: {vectorList}\n" +
            $"Example: .VectorNearest(d => d.{availableVectors[0]}, yourVector)");
    }

    public static NotSupportedException UnsupportedExpression(string description) =>
        new($"Unsupported expression: {description}");

    public static NotSupportedException PlatformNotSupported(string platform) =>
        new($"Platform '{platform}' is not supported. Supported platforms: linux-x64, osx-arm64.");
}
