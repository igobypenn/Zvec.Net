using Zvec.Net.Index;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Index;

public class IndexParamsTests
{
    [Fact]
    public void Hnsw_DefaultValues_SetsCorrectDefaults()
    {
        var params_ = IndexParams.Hnsw();

        Assert.Equal(IndexType.Hnsw, params_.Type);
        var hnsw = Assert.IsType<HnswIndexParams>(params_);
        Assert.Equal(16, hnsw.M);
        Assert.Equal(200, hnsw.EfConstruction);
        Assert.Equal(MetricType.Cosine, params_.MetricType);
    }

    [Fact]
    public void Hnsw_CustomValues_SetsCorrectValues()
    {
        var params_ = IndexParams.Hnsw(m: 32, efConstruction: 400, metric: MetricType.Ip);

        Assert.Equal(IndexType.Hnsw, params_.Type);
        var hnsw = Assert.IsType<HnswIndexParams>(params_);
        Assert.Equal(32, hnsw.M);
        Assert.Equal(400, hnsw.EfConstruction);
        Assert.Equal(MetricType.Ip, params_.MetricType);
    }

    [Fact]
    public void Ivf_DefaultValues_SetsCorrectDefaults()
    {
        var params_ = IndexParams.Ivf();

        Assert.Equal(IndexType.Ivf, params_.Type);
        var ivf = Assert.IsType<IvfIndexParams>(params_);
        Assert.Equal(1024, ivf.NLists);
        Assert.Equal(64, ivf.NProbe);
        Assert.Equal(MetricType.Cosine, params_.MetricType);
    }

    [Fact]
    public void Ivf_CustomValues_SetsCorrectValues()
    {
        var params_ = IndexParams.Ivf(nLists: 2048, nProbe: 128, metric: MetricType.L2);

        Assert.Equal(IndexType.Ivf, params_.Type);
        var ivf = Assert.IsType<IvfIndexParams>(params_);
        Assert.Equal(2048, ivf.NLists);
        Assert.Equal(128, ivf.NProbe);
        Assert.Equal(MetricType.L2, params_.MetricType);
    }

    [Fact]
    public void Flat_DefaultValues_SetsCorrectDefaults()
    {
        var params_ = IndexParams.Flat();

        Assert.Equal(IndexType.Flat, params_.Type);
        Assert.Equal(MetricType.Cosine, params_.MetricType);
        Assert.Equal(QuantizeType.Undefined, params_.QuantizeType);
    }

    [Fact]
    public void Flat_CustomMetric_SetsCorrectValue()
    {
        var params_ = IndexParams.Flat(metric: MetricType.Ip);

        Assert.Equal(MetricType.Ip, params_.MetricType);
    }

    [Fact]
    public void Invert_DefaultValues_SetsCorrectDefaults()
    {
        var params_ = IndexParams.Invert();

        Assert.False(params_.EnableRangeOptimization);
    }

    [Fact]
    public void Invert_WithRangeOptimization_SetsCorrectValue()
    {
        var params_ = IndexParams.Invert(enableRangeOptimization: true);

        Assert.True(params_.EnableRangeOptimization);
    }

    [Fact]
    public void QuantizeType_CanBeSet()
    {
        var params_ = IndexParams.Hnsw();

        Assert.Equal(QuantizeType.Undefined, params_.QuantizeType);
    }
}

public class IndexQueryParamTests
{
    [Fact]
    public void Hnsw_DefaultEf_Is64()
    {
        var param = IndexQueryParam.Hnsw();

        Assert.IsType<HnswQueryParam>(param);
        Assert.Equal(64, ((HnswQueryParam)param).Ef);
    }

    [Fact]
    public void Hnsw_CustomEf_SetsCorrectValue()
    {
        var param = IndexQueryParam.Hnsw(ef: 128);

        Assert.Equal(128, ((HnswQueryParam)param).Ef);
    }

    [Fact]
    public void Ivf_DefaultNProbe_Is64()
    {
        var param = IndexQueryParam.Ivf();

        Assert.IsType<IvfQueryParam>(param);
        Assert.Equal(64, ((IvfQueryParam)param).NProbe);
    }

    [Fact]
    public void Ivf_CustomNProbe_SetsCorrectValue()
    {
        var param = IndexQueryParam.Ivf(nProbe: 256);

        Assert.Equal(256, ((IvfQueryParam)param).NProbe);
    }
}
