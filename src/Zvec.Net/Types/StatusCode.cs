namespace Zvec.Net.Types;

/// <summary>
/// Status codes returned by zvec operations.
/// </summary>
public enum StatusCode : int
{
    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Unknown error occurred.
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// Invalid argument provided.
    /// </summary>
    InvalidArgument = 2,

    /// <summary>
    /// Resource not found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Resource already exists.
    /// </summary>
    AlreadyExists = 4,

    /// <summary>
    /// Internal error occurred.
    /// </summary>
    InternalError = 5,

    /// <summary>
    /// I/O error occurred.
    /// </summary>
    IOError = 6,

    /// <summary>
    /// Schema validation failed.
    /// </summary>
    InvalidSchema = 7,

    /// <summary>
    /// Index operation failed.
    /// </summary>
    IndexError = 8,
}
