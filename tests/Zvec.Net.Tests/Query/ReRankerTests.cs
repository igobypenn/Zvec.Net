using Zvec.Net.Query;

namespace Zvec.Net.Tests.Query;

public class ReRankerTests
{
    [Fact]
    public void RrfReRanker_DefaultK_Is60()
    {
        var reranker = new RrfReRanker();

        Assert.Equal(60.0, reranker.K);
    }

    [Fact]
    public void RrfReRanker_CustomK_SetsK()
    {
        var reranker = new RrfReRanker(k: 100.0);

        Assert.Equal(100.0, reranker.K);
    }

    [Fact]
    public void RrfReRanker_Name_ReturnsRRF()
    {
        var reranker = new RrfReRanker();

        Assert.Equal("RRF", reranker.Name);
    }

    [Fact]
    public void RrfReRanker_ToFilterExpression_ReturnsCorrectFormat()
    {
        var reranker = new RrfReRanker(k: 50.0);

        var expr = reranker.ToFilterExpression();

        Assert.Equal("rrf(k=50)", expr);
    }

    [Fact]
    public void RrfReRanker_ZeroK_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RrfReRanker(k: 0));
    }

    [Fact]
    public void RrfReRanker_NegativeK_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new RrfReRanker(k: -10));
    }

    [Fact]
    public void RrfReRanker_ToString_ContainsK()
    {
        var reranker = new RrfReRanker(k: 42.0);

        var str = reranker.ToString();

        Assert.Contains("42", str);
    }

    [Fact]
    public void WeightedReRanker_ValidWeights_Succeeds()
    {
        var weights = new Dictionary<string, double>
        {
            ["text"] = 0.7,
            ["image"] = 0.3
        };

        var reranker = new WeightedReRanker(weights);

        Assert.Equal(2, reranker.Weights.Count);
        Assert.Equal(0.7, reranker.Weights["text"]);
        Assert.Equal(0.3, reranker.Weights["image"]);
    }

    [Fact]
    public void WeightedReRanker_Name_ReturnsWeighted()
    {
        var weights = new Dictionary<string, double> { ["a"] = 1.0 };
        var reranker = new WeightedReRanker(weights);

        Assert.Equal("Weighted", reranker.Name);
    }

    [Fact]
    public void WeightedReRanker_WeightsNotSumToOne_ThrowsArgumentException()
    {
        var weights = new Dictionary<string, double>
        {
            ["text"] = 0.5,
            ["image"] = 0.3
        };

        Assert.Throws<ArgumentException>(() => new WeightedReRanker(weights));
    }

    [Fact]
    public void WeightedReRanker_EmptyWeights_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new WeightedReRanker(new Dictionary<string, double>()));
    }

    [Fact]
    public void WeightedReRanker_NullWeights_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new WeightedReRanker(null!));
    }

    [Fact]
    public void WeightedReRanker_ToFilterExpression_ReturnsCorrectFormat()
    {
        var weights = new Dictionary<string, double>
        {
            ["text"] = 0.7,
            ["image"] = 0.3
        };
        var reranker = new WeightedReRanker(weights);

        var expr = reranker.ToFilterExpression();

        Assert.StartsWith("weighted(", expr);
        Assert.Contains("text:", expr);
        Assert.Contains("image:", expr);
    }

    [Fact]
    public void WeightedReRanker_ToString_ContainsFieldCount()
    {
        var weights = new Dictionary<string, double>
        {
            ["text"] = 0.5,
            ["image"] = 0.5
        };
        var reranker = new WeightedReRanker(weights);

        var str = reranker.ToString();

        Assert.Contains("2", str);
    }
}
