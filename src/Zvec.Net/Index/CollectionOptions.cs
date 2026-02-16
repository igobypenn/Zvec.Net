using Zvec.Net.Types;

namespace Zvec.Net.Index;

/// <summary>
/// Configuration options for creating or opening a collection.
/// </summary>
public sealed class CollectionOptions
{
    /// <summary>
    /// Gets or sets the maximum number of documents per segment.
    /// </summary>
    /// <remarks>
    /// Default is 1,000,000. Larger values mean fewer segments but more memory usage.
    /// </remarks>
    public int SegmentMaxDocs { get; set; } = 1_000_000;

    /// <summary>
    /// Gets or sets the number of parallel threads for index building.
    /// </summary>
    /// <remarks>
    /// Default is 0 (auto-detect based on CPU cores).
    /// </remarks>
    public int IndexBuildParallel { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether changes are automatically flushed after write operations.
    /// </summary>
    /// <remarks>
    /// Default is true. Set to false for bulk load performance, then call Flush() manually.
    /// </remarks>
    public bool AutoFlush { get; set; } = true;
}
