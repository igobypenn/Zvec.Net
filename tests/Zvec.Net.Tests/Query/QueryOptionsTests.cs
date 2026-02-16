using Zvec.Net.Index;
using Zvec.Net.Query;

namespace Zvec.Net.Tests.Query;

public class QueryOptionsTests
{
    [Fact]
    public void Default_HasDefaultValues()
    {
        var options = QueryOptions.Default;
        
        Assert.Equal(10, options.TopK);
        Assert.Null(options.Filter);
        Assert.False(options.IncludeVectors);
        Assert.Null(options.OutputFields);
        Assert.Null(options.ReRanker);
    }
    
    [Fact]
    public void WithTopK_SetsTopK()
    {
        var options = QueryOptions.Default.WithTopK(50);
        
        Assert.Equal(50, options.TopK);
    }
    
    [Fact]
    public void WithFilter_SetsFilter()
    {
        var options = QueryOptions.Default.WithFilter("year > 2020");
        
        Assert.Equal("year > 2020", options.Filter);
    }
    
    [Fact]
    public void WithIncludeVectors_SetsFlag()
    {
        var options = QueryOptions.Default.WithIncludeVectors(true);
        
        Assert.True(options.IncludeVectors);
    }
    
    [Fact]
    public void WithOutputFields_SetsFields()
    {
        var options = QueryOptions.Default.WithOutputFields("title", "category");
        
        Assert.Equal(2, options.OutputFields!.Count);
        Assert.Contains("title", options.OutputFields);
        Assert.Contains("category", options.OutputFields);
    }
    
    [Fact]
    public void WithReRanker_SetsReRanker()
    {
        var reranker = new RrfReRanker(50);
        var options = QueryOptions.Default.WithReRanker(reranker);
        
        Assert.Same(reranker, options.ReRanker);
    }
    
    [Fact]
    public void FluentChaining_Works()
    {
        var reranker = new WeightedReRanker(new Dictionary<string, double> { ["a"] = 1.0 });
        
        var options = QueryOptions.Default
            .WithTopK(100)
            .WithFilter("category = 'tech'")
            .WithIncludeVectors()
            .WithOutputFields("title")
            .WithReRanker(reranker);
        
        Assert.Equal(100, options.TopK);
        Assert.Equal("category = 'tech'", options.Filter);
        Assert.True(options.IncludeVectors);
        Assert.Single(options.OutputFields!);
        Assert.Same(reranker, options.ReRanker);
    }
}
