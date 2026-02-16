using Zvec.Net.Types;

namespace Zvec.Net.Attributes;

/// <summary>
/// Marks a property as a vector field in a document.
/// </summary>
/// <remarks>
/// Supported types: float[], double[], Half[], sbyte[], and <see cref="Models.SparseVector"/>.
/// The dimension must match the actual vector length at runtime.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class VectorFieldAttribute : Attribute
{
    /// <summary>
    /// Gets the dimensionality of the vector.
    /// </summary>
    /// <remarks>
    /// For sparse vectors, this should be 0.
    /// </remarks>
    public int Dimension { get; }

    /// <summary>
    /// Gets the precision of vector elements.
    /// </summary>
    public VectorPrecision Precision { get; init; } = VectorPrecision.Float32;

    /// <summary>
    /// Gets or sets the index type for this vector field.
    /// </summary>
    public IndexType IndexType { get; init; } = IndexType.Flat;

    /// <summary>
    /// Gets or sets the distance metric for similarity calculation.
    /// </summary>
    public MetricType MetricType { get; init; } = MetricType.Cosine;

    /// <summary>
    /// Gets or sets whether the vector field can be null.
    /// </summary>
    public bool Nullable { get; init; } = true;

    /// <summary>
    /// Gets or sets the M parameter for HNSW index (0 = use default).
    /// </summary>
    public int M { get; init; }

    /// <summary>
    /// Gets or sets the EfConstruction parameter for HNSW index (0 = use default).
    /// </summary>
    public int EfConstruction { get; init; }

    /// <summary>
    /// Gets or sets the NLists parameter for IVF index (0 = use default).
    /// </summary>
    public int NLists { get; init; }

    /// <summary>
    /// Gets or sets the NProbe parameter for IVF index (0 = use default).
    /// </summary>
    public int NProbe { get; init; }

    /// <summary>
    /// Gets or sets the quantization type.
    /// </summary>
    public QuantizeType QuantizeType { get; init; } = QuantizeType.Undefined;

    /// <summary>
    /// Initializes a new vector field attribute for dense vectors.
    /// </summary>
    /// <param name="dimension">The dimensionality of the vector.</param>
    /// <param name="precision">The precision of vector elements (default: Float32).</param>
    public VectorFieldAttribute(int dimension, VectorPrecision precision = VectorPrecision.Float32)
    {
        Dimension = dimension;
        Precision = precision;
    }

    /// <summary>
    /// Initializes a new vector field attribute for sparse vectors.
    /// </summary>
    /// <param name="precision">The precision of vector elements.</param>
    public VectorFieldAttribute(VectorPrecision precision)
    {
        Dimension = 0;
        Precision = precision;
    }

    /// <summary>
    /// Initializes a new vector field attribute for sparse vectors (default: SparseFloat32).
    /// </summary>
    public VectorFieldAttribute()
    {
        Dimension = 0;
        Precision = VectorPrecision.SparseFloat32;
    }
}
