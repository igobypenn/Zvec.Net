using Zvec.Net.Index;

namespace Zvec.Net.Tests.Index;

public class CollectionOptionsTests
{
    [Fact]
    public void Default_HasDefaultValues()
    {
        var options = new CollectionOptions();
        
        Assert.Equal(1_000_000, options.SegmentMaxDocs);
        Assert.Equal(0, options.IndexBuildParallel);
        Assert.True(options.AutoFlush);
    }
    
    [Fact]
    public void SegmentMaxDocs_CanBeSet()
    {
        var options = new CollectionOptions { SegmentMaxDocs = 500_000 };
        
        Assert.Equal(500_000, options.SegmentMaxDocs);
    }
    
    [Fact]
    public void IndexBuildParallel_CanBeSet()
    {
        var options = new CollectionOptions { IndexBuildParallel = 8 };
        
        Assert.Equal(8, options.IndexBuildParallel);
    }
    
    [Fact]
    public void AutoFlush_CanBeSet()
    {
        var options = new CollectionOptions { AutoFlush = false };
        
        Assert.False(options.AutoFlush);
    }
    
    [Fact]
    public void AllProperties_CanBeSet()
    {
        var options = new CollectionOptions
        {
            SegmentMaxDocs = 100_000,
            IndexBuildParallel = 4,
            AutoFlush = false
        };
        
        Assert.Equal(100_000, options.SegmentMaxDocs);
        Assert.Equal(4, options.IndexBuildParallel);
        Assert.False(options.AutoFlush);
    }
}
