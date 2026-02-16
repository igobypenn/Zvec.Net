using Zvec.Net.Types;

namespace Zvec.Net.Tests.Types;

public class TypesTests
{
    [Fact]
    public void DataType_HasExpectedValues()
    {
        Assert.Equal(0, (int)DataType.Undefined);
        Assert.Equal(2, (int)DataType.String);
        Assert.Equal(3, (int)DataType.Bool);
        Assert.Equal(4, (int)DataType.Int32);
        Assert.Equal(5, (int)DataType.Int64);
        Assert.Equal(8, (int)DataType.Float);
        Assert.Equal(9, (int)DataType.Double);
        Assert.Equal(23, (int)DataType.VectorFp32);
        Assert.Equal(31, (int)DataType.SparseVectorFp32);
    }

    [Fact]
    public void IndexType_HasExpectedValues()
    {
        Assert.Equal(0, (int)IndexType.Undefined);
        Assert.Equal(1, (int)IndexType.Hnsw);
        Assert.Equal(3, (int)IndexType.Ivf);
        Assert.Equal(4, (int)IndexType.Flat);
        Assert.Equal(10, (int)IndexType.Invert);
    }

    [Fact]
    public void MetricType_HasExpectedValues()
    {
        Assert.Equal(0, (int)MetricType.Undefined);
        Assert.Equal(1, (int)MetricType.L2);
        Assert.Equal(2, (int)MetricType.Ip);
        Assert.Equal(3, (int)MetricType.Cosine);
    }

    [Fact]
    public void QuantizeType_HasExpectedValues()
    {
        Assert.Equal(0, (int)QuantizeType.Undefined);
        Assert.Equal(1, (int)QuantizeType.Fp16);
        Assert.Equal(2, (int)QuantizeType.Int8);
        Assert.Equal(3, (int)QuantizeType.Int4);
    }

    [Fact]
    public void StatusCode_HasExpectedValues()
    {
        Assert.Equal(0, (int)StatusCode.Ok);
        Assert.Equal(1, (int)StatusCode.Unknown);
        Assert.Equal(2, (int)StatusCode.InvalidArgument);
        Assert.Equal(3, (int)StatusCode.NotFound);
        Assert.Equal(4, (int)StatusCode.AlreadyExists);
        Assert.Equal(5, (int)StatusCode.InternalError);
    }
}

public class VectorPrecisionTests
{
    [Fact]
    public void ToDataType_Float32_ReturnsCorrectDataType()
    {
        var dataType = VectorPrecision.Float32.ToDataType();

        Assert.Equal(DataType.VectorFp32, dataType);
    }

    [Fact]
    public void ToDataType_Float64_ReturnsCorrectDataType()
    {
        var dataType = VectorPrecision.Float64.ToDataType();

        Assert.Equal(DataType.VectorFp64, dataType);
    }

    [Fact]
    public void ToDataType_Float16_ReturnsCorrectDataType()
    {
        var dataType = VectorPrecision.Float16.ToDataType();

        Assert.Equal(DataType.VectorFp16, dataType);
    }

    [Fact]
    public void ToDataType_Int8_ReturnsCorrectDataType()
    {
        var dataType = VectorPrecision.Int8.ToDataType();

        Assert.Equal(DataType.VectorInt8, dataType);
    }

    [Fact]
    public void ToDataType_SparseFloat32_ReturnsCorrectDataType()
    {
        var dataType = VectorPrecision.SparseFloat32.ToDataType();

        Assert.Equal(DataType.SparseVectorFp32, dataType);
    }

    [Fact]
    public void ToDataType_SparseFloat16_ReturnsCorrectDataType()
    {
        var dataType = VectorPrecision.SparseFloat16.ToDataType();

        Assert.Equal(DataType.SparseVectorFp16, dataType);
    }

    [Fact]
    public void IsSparse_WithSparseTypes_ReturnsTrue()
    {
        Assert.True(VectorPrecision.SparseFloat32.IsSparse());
        Assert.True(VectorPrecision.SparseFloat16.IsSparse());
    }

    [Fact]
    public void IsSparse_WithDenseTypes_ReturnsFalse()
    {
        Assert.False(VectorPrecision.Float32.IsSparse());
        Assert.False(VectorPrecision.Float64.IsSparse());
        Assert.False(VectorPrecision.Float16.IsSparse());
        Assert.False(VectorPrecision.Int8.IsSparse());
    }
}
