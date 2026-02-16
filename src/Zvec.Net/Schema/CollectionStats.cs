namespace Zvec.Net.Schema;

public sealed class CollectionStats
{
    public long DocumentCount { get; init; }
    public long TotalSizeBytes { get; init; }
    public int SegmentCount { get; init; }
    public IReadOnlyDictionary<string, long> IndexSizes { get; init; } = new Dictionary<string, long>();
    
    public override string ToString() => 
        $"CollectionStats[Docs={DocumentCount}, Size={TotalSizeBytes}, Segments={SegmentCount}]";
}
