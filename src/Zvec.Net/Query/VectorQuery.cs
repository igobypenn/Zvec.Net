using Zvec.Net.Index;
using Zvec.Net.Models;

namespace Zvec.Net.Query;

/// <summary>
/// Represents a vector similarity query.
/// </summary>
/// <remarks>
/// Create instances using the factory methods:
/// <list type="bullet">
/// <item><see cref="ByVector"/> - Query by vector values</item>
/// <item><see cref="ById"/> - Query by document ID (uses document's vector)</item>
/// <item><see cref="BySparseVector"/> - Query by sparse vector</item>
/// </list>
/// </remarks>
public sealed record VectorQuery
{
    /// <summary>
    /// Gets the name of the vector field to search.
    /// </summary>
    public string FieldName { get; init; }

    /// <summary>
    /// Gets the document ID for ID-based queries.
    /// </summary>
    public string? DocumentId { get; init; }

    /// <summary>
    /// Gets the query vector for vector-based queries.
    /// </summary>
    public float[]? Vector { get; init; }

    /// <summary>
    /// Gets the sparse query vector for sparse queries.
    /// </summary>
    public SparseVector? SparseVector { get; init; }

    /// <summary>
    /// Gets the weight for multi-vector queries.
    /// </summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>
    /// Gets the index-specific query parameters.
    /// </summary>
    public IndexQueryParam? Param { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is an ID-based query.
    /// </summary>
    public bool HasId => !string.IsNullOrEmpty(DocumentId);

    /// <summary>
    /// Gets a value indicating whether this is a vector-based query.
    /// </summary>
    public bool HasVector => Vector != null || SparseVector != null;

    /// <summary>
    /// Gets a value indicating whether this is a sparse vector query.
    /// </summary>
    public bool IsSparse => SparseVector != null;

    /// <summary>
    /// Initializes a new vector query for the specified field.
    /// </summary>
    /// <param name="fieldName">The vector field name.</param>
    public VectorQuery(string fieldName)
    {
        FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
    }

    /// <summary>
    /// Creates a query that finds vectors similar to the specified document's vector.
    /// </summary>
    /// <param name="fieldName">The vector field name.</param>
    /// <param name="documentId">The ID of the document whose vector to use.</param>
    /// <param name="weight">Weight for multi-vector queries.</param>
    /// <param name="param">Optional index query parameters.</param>
    /// <returns>A new <see cref="VectorQuery"/> instance.</returns>
    public static VectorQuery ById(string fieldName, string documentId, double weight = 1.0, IndexQueryParam? param = null)
    {
        if (string.IsNullOrEmpty(documentId))
            throw new ArgumentException("Document ID cannot be null or empty", nameof(documentId));

        return new VectorQuery(fieldName)
        {
            DocumentId = documentId,
            Weight = weight,
            Param = param
        };
    }

    /// <summary>
    /// Creates a query that finds vectors similar to the specified vector.
    /// </summary>
    /// <param name="fieldName">The vector field name.</param>
    /// <param name="vector">The query vector.</param>
    /// <param name="weight">Weight for multi-vector queries.</param>
    /// <param name="param">Optional index query parameters.</param>
    /// <returns>A new <see cref="VectorQuery"/> instance.</returns>
    public static VectorQuery ByVector(string fieldName, float[] vector, double weight = 1.0, IndexQueryParam? param = null)
    {
        if (vector == null || vector.Length == 0)
            throw new ArgumentException("Vector cannot be null or empty", nameof(vector));

        return new VectorQuery(fieldName)
        {
            Vector = vector,
            Weight = weight,
            Param = param
        };
    }

    /// <summary>
    /// Creates a query that finds sparse vectors similar to the specified sparse vector.
    /// </summary>
    /// <param name="fieldName">The sparse vector field name.</param>
    /// <param name="sparseVector">The sparse query vector.</param>
    /// <param name="weight">Weight for multi-vector queries.</param>
    /// <param name="param">Optional index query parameters.</param>
    /// <returns>A new <see cref="VectorQuery"/> instance.</returns>
    public static VectorQuery BySparseVector(string fieldName, SparseVector sparseVector, double weight = 1.0, IndexQueryParam? param = null)
    {
        return new VectorQuery(fieldName)
        {
            SparseVector = sparseVector,
            Weight = weight,
            Param = param
        };
    }

    /// <summary>
    /// Validates the query for correctness.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the query is invalid.</exception>
    internal void Validate()
    {
        if (string.IsNullOrEmpty(FieldName))
            throw new ArgumentException("Field name cannot be empty");

        if (HasId && HasVector)
            throw new ArgumentException("Cannot specify both document ID and vector");

        if (!HasId && !HasVector)
            throw new ArgumentException("Must specify either document ID or vector");
    }
}
