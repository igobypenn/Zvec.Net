using Zvec.Net.Types;

namespace Zvec.Net.Exceptions;

/// <summary>
/// Exception thrown when schema validation fails.
/// </summary>
public sealed class SchemaValidationException : ZvecException
{
    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the validation failure.</param>
    public SchemaValidationException(string message)
        : base(StatusCode.InvalidSchema, message)
    {
    }
}
