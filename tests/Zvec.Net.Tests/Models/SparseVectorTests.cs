using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Models;

public class SparseVectorTests
{
    [Fact]
    public void Constructor_ValidParams_CreatesVector()
    {
        var indices = new uint[] { 0, 5, 10 };
        var values = new float[] { 0.1f, 0.2f, 0.3f };

        var vector = new SparseVector(indices, values);

        Assert.Equal(3, vector.Count);
    }

    [Fact]
    public void Indexer_ValidIndex_ReturnsValue()
    {
        var indices = new uint[] { 0, 5, 10 };
        var values = new float[] { 0.1f, 0.2f, 0.3f };
        var vector = new SparseVector(indices, values);

        Assert.Equal(0.1f, vector[0]);
        Assert.Equal(0.2f, vector[5]);
        Assert.Equal(0.3f, vector[10]);
    }

    [Fact]
    public void Indexer_MissingIndex_ReturnsZero()
    {
        var indices = new uint[] { 0, 5 };
        var values = new float[] { 0.1f, 0.2f };
        var vector = new SparseVector(indices, values);

        Assert.Equal(0f, vector[1]);
        Assert.Equal(0f, vector[100]);
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        var indices = new uint[] { 0, 5, 10, 15 };
        var values = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };

        var vector = new SparseVector(indices, values);

        Assert.Equal(4, vector.Count);
    }

    [Fact]
    public void FromDictionary_CreatesVector()
    {
        var dict = new Dictionary<uint, float>
        {
            [0] = 0.1f,
            [5] = 0.5f,
            [10] = 1.0f
        };

        var vector = SparseVector.FromDictionary(dict);

        Assert.Equal(3, vector.Count);
        Assert.Equal(0.1f, vector[0]);
        Assert.Equal(0.5f, vector[5]);
        Assert.Equal(1.0f, vector[10]);
    }

    [Fact]
    public void ToDictionary_ReturnsCorrectDictionary()
    {
        var indices = new uint[] { 0, 5 };
        var values = new float[] { 0.1f, 0.2f };
        var vector = new SparseVector(indices, values);

        var dict = vector.ToDictionary();

        Assert.Equal(2, dict.Count);
        Assert.Equal(0.1f, dict[0]);
        Assert.Equal(0.2f, dict[5]);
    }

    [Fact]
    public void Indices_ReturnsCorrectIndices()
    {
        var indices = new uint[] { 0, 5, 10 };
        var values = new float[] { 0.1f, 0.2f, 0.3f };
        var vector = new SparseVector(indices, values);

        var result = vector.Indices;

        Assert.Equal(indices, result);
    }

    [Fact]
    public void Values_ReturnsCorrectValues()
    {
        var indices = new uint[] { 0, 5, 10 };
        var values = new float[] { 0.1f, 0.2f, 0.3f };
        var vector = new SparseVector(indices, values);

        var result = vector.Values;

        Assert.Equal(values, result);
    }

    [Fact]
    public void Empty_CreatesEmptyVector()
    {
        var vector = SparseVector.Empty;

        Assert.Equal(0, vector.Count);
    }

    [Fact]
    public void Equals_SameVectors_ReturnsTrue()
    {
        var v1 = new SparseVector(new uint[] { 0, 5 }, new float[] { 0.1f, 0.2f });
        var v2 = new SparseVector(new uint[] { 0, 5 }, new float[] { 0.1f, 0.2f });

        Assert.Equal(v1, v2);
    }

    [Fact]
    public void Equals_DifferentVectors_ReturnsFalse()
    {
        var v1 = new SparseVector(new uint[] { 0, 5 }, new float[] { 0.1f, 0.2f });
        var v2 = new SparseVector(new uint[] { 0, 5 }, new float[] { 0.1f, 0.3f });

        Assert.NotEqual(v1, v2);
    }

    [Fact]
    public void ToString_ContainsCount()
    {
        var vector = new SparseVector(new uint[] { 0 }, new float[] { 0.1f });

        var str = vector.ToString();

        Assert.Contains("1", str);
    }
}
