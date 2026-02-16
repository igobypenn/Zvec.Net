using Zvec.Net.Types;

namespace Zvec.Net.Models;

/// <summary>
/// Interface for document types that can be stored in a vector collection.
/// </summary>
public interface IDocument
{
    /// <summary>
    /// Gets or sets the unique identifier for this document.
    /// </summary>
    string Id { get; set; }
}

/// <summary>
/// Base class for documents with optional score tracking.
/// </summary>
public abstract class DocumentBase : IDocument
{
    /// <summary>
    /// Gets or sets the unique identifier for this document.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets the similarity score when this document is returned from a vector search.
    /// </summary>
    /// <remarks>
    /// The score is only populated for documents returned from query operations.
    /// For insert/update operations, this value will be null.
    /// </remarks>
    public double? Score { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBase"/> class.
    /// </summary>
    protected DocumentBase() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBase"/> class with the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier for this document.</param>
    protected DocumentBase(string id)
    {
        Id = id;
    }
}
