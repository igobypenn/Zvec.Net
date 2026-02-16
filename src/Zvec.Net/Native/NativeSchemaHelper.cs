using System.Runtime.InteropServices;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net.Native;

internal static class NativeSchemaHelper
{
    public static CollectionSchema ReadSchemaFromNative(INativeMethods native, IntPtr schemaPtr)
    {
        var namePtr = native.zvec_schema_get_name(schemaPtr);
        var name = Marshal.PtrToStringUTF8(namePtr) ?? "unknown";
        
        var fieldCount = (int)native.zvec_schema_get_field_count(schemaPtr);
        var vectorCount = (int)native.zvec_schema_get_vector_count(schemaPtr);
        
        var fields = new List<FieldSchema>();
        for (int i = 0; i < fieldCount; i++)
        {
            var fieldDef = native.zvec_schema_get_field(schemaPtr, (nuint)i);
            var fieldName = Marshal.PtrToStringUTF8(fieldDef.Name) ?? $"field_{i}";
            fields.Add(new FieldSchema(fieldName, (DataType)fieldDef.DataType, fieldDef.Nullable != 0));
        }
        
        var vectors = new List<VectorSchema>();
        for (int i = 0; i < vectorCount; i++)
        {
            var vectorDef = native.zvec_schema_get_vector(schemaPtr, (nuint)i);
            var vectorName = Marshal.PtrToStringUTF8(vectorDef.Name) ?? $"vector_{i}";
            vectors.Add(new VectorSchema(vectorName, (DataType)vectorDef.DataType, vectorDef.Dimension));
        }
        
        return new CollectionSchema(name, fields, vectors);
    }
}
