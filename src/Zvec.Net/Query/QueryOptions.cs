using Zvec.Net.Index;

namespace Zvec.Net.Query;

/// <summary>
/// Options for configuring vector query behavior.
/// </summary>
public sealed record QueryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// </summary>
    /// <remarks>
    /// Default is 10. Higher values return more results but may be slower.
    /// </remarks>
    public int TopK { get; init; } = 10;
    
    /// <summary>
    /// Gets or sets the filter expression for pre-filtering results.
    /// </summary>
    /// <remarks>
    /// Filter syntax follows zvec filter expression format.
    /// </remarks>
    public string? Filter { get; init; }
    
    /// <summary>
    /// Gets or sets whether to include vectors in the results.
    /// </summary>
    /// <remarks>
    /// Default is false. Set to true if you need the actual vector values in results.
    /// </remarks>
    public bool IncludeVectors { get; init; } = false;
    
    /// <summary>
    /// Gets or sets the fields to include in the results.
    /// </summary>
    /// <remarks>
    /// When null, all fields are returned. Specify fields to reduce response size.
    /// </remarks>
    public IReadOnlyList<string>? OutputFields { get; init; }
    
    /// <summary>
    /// Gets or sets the reranker for multi-vector queries.
    /// </summary>
    public IReRanker? ReRanker { get; init; }
    
    /// <summary>
    /// Gets the default query options.
    /// </summary>
    public static QueryOptions Default => new();
    
    /// <summary>
    /// Creates a copy with a different top-k.
    /// </summary>
    public QueryOptions WithTopK(int topK) => this with { TopK = topK };
    
    /// <summary>
    /// Creates a copy with a different filter.
    /// </summary>
    public QueryOptions WithFilter(string filter) => this with { Filter = filter };
    
    /// <summary>
    /// Creates a copy with include vectors setting.
    /// </summary>
    public QueryOptions WithIncludeVectors(bool include = true) => this with { IncludeVectors = include };
    
    /// <summary>
    /// Creates a copy with specified output fields.
    /// </summary>
    public QueryOptions WithOutputFields(params string[] fields) => this with { OutputFields = fields };
    
    /// <summary>
    /// Creates a copy with a reranker.
    /// </summary>
    public QueryOptions WithReRanker(IReRanker reRanker) => this with { ReRanker = reRanker };
}
