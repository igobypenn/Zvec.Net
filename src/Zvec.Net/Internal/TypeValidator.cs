using System.Reflection;
using Zvec.Net.Attributes;
using Zvec.Net.Exceptions;
using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Internal;

internal static class TypeValidator
{
    public static void ValidateVectorProperty(PropertyInfo prop, VectorFieldAttribute attr)
    {
        ValidateVectorType(prop, attr);
        
        if (attr.Dimension <= 0 && !attr.Precision.IsSparse())
        {
            throw new SchemaValidationException(
                $"Property '{GetTypeName(prop)}.{prop.Name}' has invalid dimension.\n" +
                $"  [VectorField(dimension: {attr.Dimension}, precision: VectorPrecision.{attr.Precision})]\n" +
                $"  Dimension must be > 0 for dense vectors. Use dimension: 0 only for sparse vectors.");
        }
    }
    
    private static void ValidateVectorType(PropertyInfo prop, VectorFieldAttribute attr)
    {
        var actualType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
        var expectedType = GetExpectedVectorType(attr.Precision);
        
        if (actualType != expectedType)
        {
            throw new SchemaValidationException(
                $"Type mismatch on property '{GetTypeName(prop)}.{prop.Name}':\n" +
                $"  [VectorField(precision: VectorPrecision.{attr.Precision})] requires property type '{expectedType.Name}'\n" +
                $"  but property is declared as '{prop.PropertyType.Name}'\n" +
                $"  Solution: change property type to '{expectedType.Name}?' or update the precision attribute.");
        }
    }
    
    private static Type GetExpectedVectorType(VectorPrecision precision) => precision switch
    {
        VectorPrecision.Float64 => typeof(double[]),
        VectorPrecision.Float32 => typeof(float[]),
        VectorPrecision.Float16 => typeof(Half[]),
        VectorPrecision.Int8 => typeof(sbyte[]),
        VectorPrecision.SparseFloat32 => typeof(SparseVector),
        VectorPrecision.SparseFloat16 => typeof(SparseVector),
        _ => throw new ArgumentOutOfRangeException(nameof(precision), precision, "Unknown precision")
    };
    
    private static string GetTypeName(PropertyInfo prop) => prop.DeclaringType?.Name ?? "<unknown>";
}
