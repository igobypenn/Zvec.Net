namespace Zvec.Net.Types;

/// <summary>
/// Index types for vector fields.
/// </summary>
public enum IndexType
{
    /// <summary>
    /// Undefined index type.
    /// </summary>
    Undefined = 0,
    
    /// <summary>
    /// HNSW (Hierarchical Navigable Small World) - fast approximate search.
    /// </summary>
    Hnsw = 1,
    
    /// <summary>
    /// Reserved.
    /// </summary>
    Reserved = 2,
    
    /// <summary>
    /// IVF (Inverted File) - cluster-based search.
    /// </summary>
    Ivf = 3,
    
    /// <summary>
    /// Flat - exact brute-force search.
    /// </summary>
    Flat = 4,
    
    /// <summary>
    /// Inverted index for scalar fields.
    /// </summary>
    Invert = 10,
}
