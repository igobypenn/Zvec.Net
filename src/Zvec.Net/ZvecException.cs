using Zvec.Net.Types;

namespace Zvec.Net;

/// <summary>
/// Exception thrown when a zvec operation fails.
/// </summary>
public class ZvecException : Exception
{
    /// <summary>
    /// Gets the status code indicating the type of error.
    /// </summary>
    public StatusCode StatusCode { get; }
    
    /// <summary>
    /// Initializes a new instance with the specified status code and message.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="message">The error message.</param>
    public ZvecException(StatusCode statusCode, string message) 
        : base($"[{statusCode}] {message}")
    {
        StatusCode = statusCode;
    }
    
    /// <summary>
    /// Initializes a new instance with status code, message, and inner exception.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ZvecException(StatusCode statusCode, string message, Exception innerException) 
        : base($"[{statusCode}] {message}", innerException)
    {
        StatusCode = statusCode;
    }
}
