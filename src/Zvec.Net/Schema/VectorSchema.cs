using Zvec.Net.Index;
using Zvec.Net.Types;

namespace Zvec.Net.Schema;

/// <summary>
/// Represents the schema of a vector field in a collection.
/// </summary>
public sealed class VectorSchema
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the data type of the vector.
    /// </summary>
    public DataType DataType { get; }
    
    /// <summary>
    /// Gets the dimensionality of the vector.
    /// </summary>
    public int Dimension { get; }
    
    /// <summary>
    /// Gets the index parameters for this vector field.
    /// </summary>
    public IndexParams? IndexParams { get; }
    
    /// <summary>
    /// Gets a value indicating whether the field can contain null values.
    /// </summary>
    public bool Nullable { get; }
    
    /// <summary>
    /// Gets a value indicating whether this is a sparse vector.
    /// </summary>
    public bool IsSparse => DataType is DataType.SparseVectorFp32 or DataType.SparseVectorFp16;
    
    /// <summary>
    /// Initializes a new vector schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="dataType">The vector data type.</param>
    /// <param name="dimension">The vector dimension (0 for sparse vectors).</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <param name="nullable">Whether the field can be null.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty, or when dataType is not a vector type.</exception>
    public VectorSchema(
        string name, 
        DataType dataType, 
        int dimension = 0, 
        IndexParams? indexParams = null,
        bool nullable = true)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Vector field name cannot be null or empty", nameof(name));
        
        if (!IsVectorDataType(dataType))
            throw new ArgumentException($"DataType.{dataType} is not a valid vector type.", nameof(dataType));
        
        if (dimension < 0)
            throw new ArgumentException("Dimension must be >= 0", nameof(dimension));
        
        var isSparse = dataType is DataType.SparseVectorFp32 or DataType.SparseVectorFp16;
        if (dimension == 0 && !isSparse)
            throw new ArgumentException($"Dimension must be > 0 for dense vector type {dataType}", nameof(dimension));
        
        Name = name;
        DataType = dataType;
        Dimension = dimension;
        IndexParams = indexParams ?? IndexParams.Flat();
        Nullable = nullable;
    }
    
    private static bool IsVectorDataType(DataType type) => type switch
    {
        DataType.VectorFp32 or DataType.VectorFp64 or DataType.VectorFp16 or
        DataType.VectorInt8 or DataType.VectorInt16 or DataType.VectorInt4 or
        DataType.VectorBinary32 or DataType.VectorBinary64 or
        DataType.SparseVectorFp32 or DataType.SparseVectorFp16 => true,
        _ => false
    };
    
    /// <summary>
    /// Creates a Float32 vector schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="dimension">The vector dimension.</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <returns>A new <see cref="VectorSchema"/> instance.</returns>
    public static VectorSchema Float32(string name, int dimension, IndexParams? indexParams = null) 
        => new(name, DataType.VectorFp32, dimension, indexParams);
    
    /// <summary>
    /// Creates a Float64 vector schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="dimension">The vector dimension.</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <returns>A new <see cref="VectorSchema"/> instance.</returns>
    public static VectorSchema Float64(string name, int dimension, IndexParams? indexParams = null) 
        => new(name, DataType.VectorFp64, dimension, indexParams);
    
    /// <summary>
    /// Creates a Float16 vector schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="dimension">The vector dimension.</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <returns>A new <see cref="VectorSchema"/> instance.</returns>
    public static VectorSchema Float16(string name, int dimension, IndexParams? indexParams = null) 
        => new(name, DataType.VectorFp16, dimension, indexParams);
    
    /// <summary>
    /// Creates an Int8 vector schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="dimension">The vector dimension.</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <returns>A new <see cref="VectorSchema"/> instance.</returns>
    public static VectorSchema Int8(string name, int dimension, IndexParams? indexParams = null) 
        => new(name, DataType.VectorInt8, dimension, indexParams);
    
    /// <summary>
    /// Creates a SparseFloat32 vector schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <returns>A new <see cref="VectorSchema"/> instance.</returns>
    public static VectorSchema SparseFloat32(string name, IndexParams? indexParams = null) 
        => new(name, DataType.SparseVectorFp32, 0, indexParams);
    
    /// <inheritdoc/>
    public override string ToString() => 
        IsSparse ? $"VectorSchema[{Name}, {DataType}, Sparse]" : $"VectorSchema[{Name}, {DataType}, Dim={Dimension}]";
}
