namespace Zvec.Net.Query;

/// <summary>
/// Interface for reranking strategies in multi-vector queries.
/// </summary>
/// <remarks>
/// Rerankers combine results from multiple vector queries into a single ranked list.
/// </remarks>
public interface IReRanker
{
    /// <summary>
    /// Gets the name of the reranker.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Converts the reranker to a filter expression string.
    /// </summary>
    /// <returns>A filter expression representation.</returns>
    string ToFilterExpression();
}

/// <summary>
/// Reciprocal Rank Fusion (RRF) reranker.
/// </summary>
/// <remarks>
/// RRF combines ranked lists by averaging their reciprocal ranks.
/// The formula is: score(d) = sum(1 / (k + rank(d))) for each ranked list.
/// </remarks>
public sealed class RrfReRanker : IReRanker
{
    /// <summary>
    /// Gets the name of this reranker ("RRF").
    /// </summary>
    public string Name => "RRF";
    
    /// <summary>
    /// Gets the k parameter for RRF calculation.
    /// </summary>
    /// <remarks>
    /// Higher values smooth the ranking. Default is 60.
    /// </remarks>
    public double K { get; }
    
    /// <summary>
    /// Initializes a new RRF reranker with the specified k parameter.
    /// </summary>
    /// <param name="k">The k parameter (must be positive). Default is 60.</param>
    /// <exception cref="ArgumentException">Thrown when k is not positive.</exception>
    public RrfReRanker(double k = 60.0)
    {
        if (k <= 0)
            throw new ArgumentException("K must be positive", nameof(k));
        K = k;
    }
    
    /// <inheritdoc/>
    public string ToFilterExpression() => $"rrf(k={K})";
    
    /// <inheritdoc/>
    public override string ToString() => $"RrfReRanker(K={K})";
}

/// <summary>
/// Weighted reranker for multi-vector queries.
/// </summary>
/// <remarks>
/// Combines scores from multiple vector queries using specified weights.
/// Weights must sum to 1.0.
/// </remarks>
public sealed class WeightedReRanker : IReRanker
{
    /// <summary>
    /// Gets the name of this reranker ("Weighted").
    /// </summary>
    public string Name => "Weighted";
    
    /// <summary>
    /// Gets the weight assignments for each vector field.
    /// </summary>
    public IReadOnlyDictionary<string, double> Weights { get; }
    
    /// <summary>
    /// Initializes a new weighted reranker with the specified weights.
    /// </summary>
    /// <param name="weights">Weights for each vector field. Must sum to 1.0.</param>
    /// <exception cref="ArgumentException">Thrown when weights are invalid.</exception>
    public WeightedReRanker(IDictionary<string, double> weights)
    {
        if (weights == null || weights.Count == 0)
            throw new ArgumentException("Weights cannot be null or empty", nameof(weights));
        
        var sum = weights.Values.Sum();
        if (Math.Abs(sum - 1.0) > 0.001)
            throw new ArgumentException($"Weights must sum to 1.0, but sum is {sum}", nameof(weights));
        
        Weights = new Dictionary<string, double>(weights);
    }
    
    /// <inheritdoc/>
    public string ToFilterExpression()
    {
        var parts = Weights.Select(kv => $"{kv.Key}:{kv.Value:F4}");
        return $"weighted({string.Join(",", parts)})";
    }
    
    /// <inheritdoc/>
    public override string ToString() => $"WeightedReRanker[{Weights.Count} fields]";
}
