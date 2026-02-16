namespace Zvec.Net.Types;

/// <summary>
/// Distance metrics for similarity calculation.
/// </summary>
public enum MetricType
{
    /// <summary>
    /// Undefined metric type.
    /// </summary>
    Undefined = 0,
    
    /// <summary>
    /// Euclidean distance (L2 norm).
    /// </summary>
    L2 = 1,
    
    /// <summary>
    /// Inner product (dot product).
    /// </summary>
    Ip = 2,
    
    /// <summary>
    /// Cosine similarity.
    /// </summary>
    Cosine = 3,
}
