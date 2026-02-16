using Zvec.Net.Types;

namespace Zvec.Net.Types;

/// <summary>
/// Vector precision types for embedding fields.
/// </summary>
public enum VectorPrecision
{
    /// <summary>
    /// 64-bit floating point vectors.
    /// </summary>
    Float64,

    /// <summary>
    /// 32-bit floating point vectors (most common).
    /// </summary>
    Float32,

    /// <summary>
    /// 16-bit floating point vectors (memory efficient).
    /// </summary>
    Float16,

    /// <summary>
    /// 8-bit signed integer vectors (quantized).
    /// </summary>
    Int8,

    /// <summary>
    /// Sparse 32-bit floating point vectors.
    /// </summary>
    SparseFloat32,

    /// <summary>
    /// Sparse 16-bit floating point vectors.
    /// </summary>
    SparseFloat16,
}

/// <summary>
/// Extension methods for <see cref="VectorPrecision"/>.
/// </summary>
public static class VectorPrecisionExtensions
{
    /// <summary>
    /// Converts a vector precision to the corresponding data type.
    /// </summary>
    /// <param name="precision">The vector precision.</param>
    /// <returns>The corresponding <see cref="DataType"/>.</returns>
    public static DataType ToDataType(this VectorPrecision precision) => precision switch
    {
        VectorPrecision.Float64 => DataType.VectorFp64,
        VectorPrecision.Float32 => DataType.VectorFp32,
        VectorPrecision.Float16 => DataType.VectorFp16,
        VectorPrecision.Int8 => DataType.VectorInt8,
        VectorPrecision.SparseFloat32 => DataType.SparseVectorFp32,
        VectorPrecision.SparseFloat16 => DataType.SparseVectorFp16,
        _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, null)
    };

    /// <summary>
    /// Determines whether the precision represents a sparse vector.
    /// </summary>
    /// <param name="precision">The vector precision.</param>
    /// <returns><c>true</c> if sparse; otherwise, <c>false</c>.</returns>
    public static bool IsSparse(this VectorPrecision precision) =>
        precision is VectorPrecision.SparseFloat32 or VectorPrecision.SparseFloat16;
}
