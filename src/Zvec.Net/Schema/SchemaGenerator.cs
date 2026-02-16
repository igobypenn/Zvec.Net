using System.Reflection;
using Zvec.Net.Attributes;
using Zvec.Net.Exceptions;
using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Schema;

/// <summary>
/// Generates collection schemas from POCO types.
/// </summary>
internal static class SchemaGenerator
{
    /// <summary>
    /// Generates a collection schema from a document type.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <returns>A collection schema.</returns>
    public static CollectionSchema GenerateSchema<T>() where T : IDocument
    {
        var type = typeof(T);
        var fields = new List<FieldSchema>();
        var vectors = new List<VectorSchema>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var keyAttr = prop.GetCustomAttribute<KeyAttribute>();
            var fieldAttr = prop.GetCustomAttribute<FieldAttribute>();
            var vectorAttr = prop.GetCustomAttribute<VectorFieldAttribute>();

            if (keyAttr != null)
                continue;

            if (prop.Name == nameof(IDocument.Id) || prop.Name == nameof(DocumentBase.Score))
                continue;

            if (vectorAttr != null)
            {
                var vectorSchema = CreateVectorSchema(prop, vectorAttr);
                vectors.Add(vectorSchema);
            }
            else if (fieldAttr != null || IsImplicitField(prop))
            {
                var fieldSchema = CreateFieldSchema(prop, fieldAttr);
                fields.Add(fieldSchema);
            }
        }

        return new CollectionSchema(type.Name, fields, vectors);
    }

    private static bool IsImplicitField(PropertyInfo prop)
    {
        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        return type.IsValueType || type == typeof(string);
    }

    private static VectorSchema CreateVectorSchema(PropertyInfo prop, VectorFieldAttribute attr)
    {
        Internal.TypeValidator.ValidateVectorProperty(prop, attr);

        var dataType = attr.Precision.ToDataType();
        var indexParams = CreateIndexParam(attr);

        return new VectorSchema(
            name: prop.Name,
            dataType: dataType,
            dimension: attr.Dimension,
            indexParams: indexParams,
            nullable: attr.Nullable
        );
    }

    private static IndexParams CreateIndexParam(VectorFieldAttribute attr)
    {
        return attr.IndexType switch
        {
            IndexType.Hnsw => IndexParams.Hnsw(
                m: attr.M > 0 ? attr.M : 16,
                efConstruction: attr.EfConstruction > 0 ? attr.EfConstruction : 200,
                metric: attr.MetricType),

            IndexType.Ivf => IndexParams.Ivf(
                nLists: attr.NLists > 0 ? attr.NLists : 1024,
                nProbe: attr.NProbe > 0 ? attr.NProbe : 64,
                metric: attr.MetricType),

            IndexType.Flat => IndexParams.Flat(metric: attr.MetricType),

            _ => IndexParams.Flat(metric: attr.MetricType)
        };
    }

    private static FieldSchema CreateFieldSchema(PropertyInfo prop, FieldAttribute? attr)
    {
        var dataType = InferDataType(prop.PropertyType);
        var nullable = attr?.Nullable ?? IsNullableType(prop.PropertyType);

        InvertIndexParams? indexParams = attr?.Indexed == true
            ? IndexParams.Invert()
            : null;

        return new FieldSchema(prop.Name, dataType, nullable, indexParams);
    }

    private static DataType InferDataType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(string)) return DataType.String;
        if (underlying == typeof(bool)) return DataType.Bool;
        if (underlying == typeof(int)) return DataType.Int32;
        if (underlying == typeof(long)) return DataType.Int64;
        if (underlying == typeof(uint)) return DataType.UInt32;
        if (underlying == typeof(ulong)) return DataType.UInt64;
        if (underlying == typeof(float)) return DataType.Float;
        if (underlying == typeof(double)) return DataType.Double;
        if (underlying == typeof(byte[])) return DataType.Binary;

        if (underlying == typeof(int[])) return DataType.ArrayInt32;
        if (underlying == typeof(long[])) return DataType.ArrayInt64;
        if (underlying == typeof(float[])) return DataType.ArrayFloat;
        if (underlying == typeof(double[])) return DataType.ArrayDouble;
        if (underlying == typeof(string[])) return DataType.ArrayString;
        if (underlying == typeof(bool[])) return DataType.ArrayBool;

        throw new SchemaValidationException(
            $"Cannot infer DataType for property type '{type.Name}'. " +
            $"Add [Field] or [VectorField] attribute explicitly.");
    }

    private static bool IsNullableType(Type type)
    {
        if (Nullable.GetUnderlyingType(type) != null) return true;
        if (!type.IsValueType) return true;
        return false;
    }
}
