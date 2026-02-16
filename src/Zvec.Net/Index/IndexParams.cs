using Zvec.Net.Types;

namespace Zvec.Net.Index;

/// <summary>
/// Base class for index configuration parameters.
/// </summary>
/// <remarks>
/// Use the factory methods to create instances:
/// <list type="bullet">
/// <item><see cref="Hnsw"/> - HNSW index for fast approximate search</item>
/// <item><see cref="Ivf"/> - IVF index for balanced performance</item>
/// <item><see cref="Flat"/> - Flat index for exact search</item>
/// <item><see cref="Invert"/> - Inverted index for scalar fields</item>
/// </list>
/// </remarks>
public abstract record IndexParams
{
    /// <summary>
    /// Gets the index type.
    /// </summary>
    public IndexType Type { get; init; }

    /// <summary>
    /// Gets the distance metric used for similarity calculation.
    /// </summary>
    public MetricType MetricType { get; init; } = MetricType.Cosine;

    /// <summary>
    /// Gets the quantization type for reducing index size.
    /// </summary>
    public QuantizeType QuantizeType { get; init; } = QuantizeType.Undefined;

    /// <summary>
    /// Protected constructor.
    /// </summary>
    protected IndexParams(IndexType type) => Type = type;

    /// <summary>
    /// Creates HNSW index parameters for fast approximate nearest neighbor search.
    /// </summary>
    /// <param name="m">Number of connections per node (default: 16).</param>
    /// <param name="efConstruction">Size of dynamic candidate list during construction (default: 200).</param>
    /// <param name="metric">Distance metric (default: Cosine).</param>
    /// <returns>HNSW index parameters.</returns>
    public static HnswIndexParams Hnsw(int m = 16, int efConstruction = 200, MetricType metric = MetricType.Cosine)
        => new() { M = m, EfConstruction = efConstruction, MetricType = metric };

    /// <summary>
    /// Creates IVF index parameters for inverted file index.
    /// </summary>
    /// <param name="nLists">Number of clusters (default: 1024).</param>
    /// <param name="nProbe">Number of clusters to search (default: 64).</param>
    /// <param name="metric">Distance metric (default: Cosine).</param>
    /// <returns>IVF index parameters.</returns>
    public static IvfIndexParams Ivf(int nLists = 1024, int nProbe = 64, MetricType metric = MetricType.Cosine)
        => new() { NLists = nLists, NProbe = nProbe, MetricType = metric };

    /// <summary>
    /// Creates flat index parameters for exact brute-force search.
    /// </summary>
    /// <param name="metric">Distance metric (default: Cosine).</param>
    /// <returns>Flat index parameters.</returns>
    public static FlatIndexParams Flat(MetricType metric = MetricType.Cosine)
        => new() { MetricType = metric };

    /// <summary>
    /// Creates inverted index parameters for scalar fields.
    /// </summary>
    /// <param name="enableRangeOptimization">Whether to optimize for range queries.</param>
    /// <returns>Inverted index parameters.</returns>
    public static InvertIndexParams Invert(bool enableRangeOptimization = false)
        => new() { EnableRangeOptimization = enableRangeOptimization };
}

/// <summary>
/// HNSW (Hierarchical Navigable Small World) index parameters.
/// </summary>
/// <remarks>
/// HNSW provides fast approximate nearest neighbor search with configurable accuracy/speed tradeoff.
/// </remarks>
public sealed record HnswIndexParams : IndexParams
{
    /// <summary>
    /// Gets the number of connections per node in the graph.
    /// </summary>
    /// <remarks>
    /// Higher values improve recall but increase memory usage. Typical range: 8-64.
    /// </remarks>
    public int M { get; init; } = 16;

    /// <summary>
    /// Gets the size of dynamic candidate list during index construction.
    /// </summary>
    /// <remarks>
    /// Higher values improve index quality but increase construction time. Typical range: 100-500.
    /// </remarks>
    public int EfConstruction { get; init; } = 200;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public HnswIndexParams() : base(IndexType.Hnsw) { }
}

/// <summary>
/// IVF (Inverted File) index parameters.
/// </summary>
/// <remarks>
/// IVF partitions vectors into clusters for faster search with some accuracy tradeoff.
/// </remarks>
public sealed record IvfIndexParams : IndexParams
{
    /// <summary>
    /// Gets the number of clusters (inverted lists).
    /// </summary>
    /// <remarks>
    /// Higher values improve search speed but may reduce recall. Typical: sqrt(N) where N is document count.
    /// </remarks>
    public int NLists { get; init; } = 1024;

    /// <summary>
    /// Gets the number of clusters to search during query.
    /// </summary>
    /// <remarks>
    /// Higher values improve recall but reduce speed. Typical: 10-100.
    /// </remarks>
    public int NProbe { get; init; } = 64;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public IvfIndexParams() : base(IndexType.Ivf) { }
}

/// <summary>
/// Flat index parameters for exact brute-force search.
/// </summary>
/// <remarks>
/// Flat index provides 100% recall but may be slow for large datasets.
/// </remarks>
public sealed record FlatIndexParams : IndexParams
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public FlatIndexParams() : base(IndexType.Flat) { }
}

/// <summary>
/// Inverted index parameters for scalar fields.
/// </summary>
public sealed record InvertIndexParams
{
    /// <summary>
    /// Gets or sets whether range query optimization is enabled.
    /// </summary>
    public bool EnableRangeOptimization { get; init; } = false;
}
