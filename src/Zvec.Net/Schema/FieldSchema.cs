using Zvec.Net.Index;
using Zvec.Net.Types;

namespace Zvec.Net.Schema;

/// <summary>
/// Represents the schema of a scalar field in a collection.
/// </summary>
public sealed class FieldSchema
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the data type of the field.
    /// </summary>
    public DataType DataType { get; }

    /// <summary>
    /// Gets a value indicating whether the field can contain null values.
    /// </summary>
    public bool Nullable { get; }

    /// <summary>
    /// Gets the index parameters for this field, if indexed.
    /// </summary>
    public InvertIndexParams? IndexParams { get; }

    /// <summary>
    /// Initializes a new field schema.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="nullable">Whether the field can be null.</param>
    /// <param name="indexParams">Optional index parameters.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty, or when dataType is a vector type.</exception>
    public FieldSchema(string name, DataType dataType, bool nullable = false, InvertIndexParams? indexParams = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Field name cannot be null or empty", nameof(name));

        if (!IsScalarDataType(dataType))
            throw new ArgumentException($"DataType.{dataType} is not a valid scalar type. Use VectorSchema for vector fields.", nameof(dataType));

        Name = name;
        DataType = dataType;
        Nullable = nullable;
        IndexParams = indexParams;
    }

    private static bool IsScalarDataType(DataType type) => type switch
    {
        DataType.Int32 or DataType.Int64 or DataType.UInt32 or DataType.UInt64 or
        DataType.Float or DataType.Double or DataType.String or DataType.Bool or
        DataType.Binary or
        DataType.ArrayInt32 or DataType.ArrayInt64 or DataType.ArrayUInt32 or DataType.ArrayUInt64 or
        DataType.ArrayFloat or DataType.ArrayDouble or DataType.ArrayString or DataType.ArrayBool or
        DataType.ArrayBinary => true,
        _ => false
    };

    /// <inheritdoc/>
    public override string ToString() => $"FieldSchema[{Name}, {DataType}, Nullable={Nullable}]";
}
