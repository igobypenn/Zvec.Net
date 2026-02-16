using Zvec.Net.Types;

namespace Zvec.Net.Exceptions;

/// <summary>
/// Exception thrown when an unsupported expression is used in a filter.
/// </summary>
public sealed class UnsupportedExpressionException : ZvecException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the unsupported expression.</param>
    public UnsupportedExpressionException(string message) 
        : base(StatusCode.InvalidArgument, message)
    {
    }
}
