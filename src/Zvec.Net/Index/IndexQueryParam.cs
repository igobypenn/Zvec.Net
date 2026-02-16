namespace Zvec.Net.Index;

/// <summary>
/// Base class for query-time index parameters.
/// </summary>
public abstract record IndexQueryParam
{
    /// <summary>
    /// Creates HNSW query parameters.
    /// </summary>
    /// <param name="ef">Size of dynamic candidate list during search (default: 64).</param>
    /// <returns>HNSW query parameters.</returns>
    public static HnswQueryParam Hnsw(int ef = 64) => new() { Ef = ef };

    /// <summary>
    /// Creates IVF query parameters.
    /// </summary>
    /// <param name="nProbe">Number of clusters to search (default: 64).</param>
    /// <returns>IVF query parameters.</returns>
    public static IvfQueryParam Ivf(int nProbe = 64) => new() { NProbe = nProbe };
}

/// <summary>
/// Query-time parameters for HNSW index.
/// </summary>
public sealed record HnswQueryParam : IndexQueryParam
{
    /// <summary>
    /// Gets or sets the size of dynamic candidate list during search.
    /// </summary>
    /// <remarks>
    /// Higher values improve recall but reduce speed. Should be >= topk.
    /// </remarks>
    public int Ef { get; init; } = 64;
}

/// <summary>
/// Query-time parameters for IVF index.
/// </summary>
public sealed record IvfQueryParam : IndexQueryParam
{
    /// <summary>
    /// Gets or sets the number of clusters to search.
    /// </summary>
    /// <remarks>
    /// Higher values improve recall but reduce speed.
    /// </remarks>
    public int NProbe { get; init; } = 64;
}
