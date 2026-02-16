using Zvec.Net.Types;

namespace Zvec.Net.Models;

/// <summary>
/// Represents the result of an operation in zvec.
/// </summary>
/// <remarks>
/// Status is a readonly record struct that indicates whether an operation succeeded or failed.
/// Use <see cref="IsOk"/> to check for success, or <see cref="ThrowIfError"/> to throw on failure.
/// </remarks>
public readonly record struct Status
{
    /// <summary>
    /// Gets the status code indicating success or the type of error.
    /// </summary>
    public StatusCode Code { get; init; }

    /// <summary>
    /// Gets the human-readable message describing the status.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    /// <value><c>true</c> if the operation succeeded; otherwise, <c>false</c>.</value>
    public bool IsOk => Code == StatusCode.Ok;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><c>true</c> if the operation failed; otherwise, <c>false</c>.</value>
    public bool IsError => Code != StatusCode.Ok;

    /// <summary>
    /// Gets a status indicating successful completion.
    /// </summary>
    public static Status Ok => new() { Code = StatusCode.Ok, Message = string.Empty };

    /// <summary>
    /// Creates a status from a code and message.
    /// </summary>
    /// <param name="code">The status code.</param>
    /// <param name="message">The status message.</param>
    /// <returns>A new <see cref="Status"/> instance.</returns>
    public static Status From(StatusCode code, string message) => new() { Code = code, Message = message };

    /// <summary>
    /// Throws a <see cref="ZvecException"/> if this status indicates an error.
    /// </summary>
    /// <exception cref="ZvecException">Thrown when <see cref="IsError"/> is <c>true</c>.</exception>
    public void ThrowIfError()
    {
        if (IsError)
        {
            throw new ZvecException(Code, Message);
        }
    }

    /// <summary>
    /// Returns a string representation of this status.
    /// </summary>
    /// <returns>"OK" for success, or "Code: Message" for errors.</returns>
    public override string ToString() => IsOk ? "OK" : $"{Code}: {Message}";
}
