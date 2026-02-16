using System.Runtime.InteropServices;
using Zvec.Net.Index;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct NativeStatus
{
    public int Code;
    public IntPtr Message;

    public readonly bool IsOk => Code == 0;

    public readonly string? GetMessage()
    {
        return Message != IntPtr.Zero ? Marshal.PtrToStringUTF8(Message) : null;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeFieldDef
{
    public IntPtr Name;
    public int DataType;
    public int Dimension;
    public int Nullable;
    public int IndexType;
    public int MetricType;
    public int M;
    public int EfConstruction;
    public int NLists;
    public int QuantizeType;

    public void Free()
    {
        if (Name != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(Name);
            Name = IntPtr.Zero;
        }
    }

    public static NativeFieldDef Create(string name, int dataType, int dimension = 0, bool nullable = true)
    {
        return new NativeFieldDef
        {
            Name = Marshal.StringToHGlobalAnsi(name),
            DataType = dataType,
            Dimension = dimension,
            Nullable = nullable ? 1 : 0,
            IndexType = (int)Types.IndexType.Undefined,
            MetricType = (int)Types.MetricType.Undefined,
            M = 0,
            EfConstruction = 0,
            NLists = 0,
            QuantizeType = (int)Types.QuantizeType.Undefined
        };
    }

    public static NativeFieldDef FromFieldSchema(FieldSchema field)
    {
        return new NativeFieldDef
        {
            Name = Marshal.StringToHGlobalAnsi(field.Name),
            DataType = (int)field.DataType,
            Dimension = 0,
            Nullable = field.Nullable ? 1 : 0,
            IndexType = field.IndexParams != null ? (int)Types.IndexType.Invert : (int)Types.IndexType.Undefined,
            MetricType = (int)Types.MetricType.Undefined,
            M = 0,
            EfConstruction = 0,
            NLists = 0,
            QuantizeType = (int)Types.QuantizeType.Undefined
        };
    }

    public static NativeFieldDef FromVectorSchema(VectorSchema vector)
    {
        var def = new NativeFieldDef
        {
            Name = Marshal.StringToHGlobalAnsi(vector.Name),
            DataType = (int)vector.DataType,
            Dimension = vector.Dimension,
            Nullable = vector.Nullable ? 1 : 0,
            IndexType = vector.IndexParams != null ? (int)vector.IndexParams.Type : (int)Types.IndexType.Flat,
            MetricType = vector.IndexParams != null ? (int)vector.IndexParams.MetricType : (int)Types.MetricType.Cosine,
            QuantizeType = vector.IndexParams != null ? (int)vector.IndexParams.QuantizeType : (int)Types.QuantizeType.Undefined,
            M = 0,
            EfConstruction = 0,
            NLists = 0
        };

        if (vector.IndexParams is HnswIndexParams hnsw)
        {
            def.M = hnsw.M;
            def.EfConstruction = hnsw.EfConstruction;
        }
        else if (vector.IndexParams is IvfIndexParams ivf)
        {
            def.NLists = ivf.NLists;
        }

        return def;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeCollectionOptions
{
    public int SegmentMaxDocs;
    public int IndexBuildParallel;
    public int AutoFlush;

    public static NativeCollectionOptions Create(int segmentMaxDocs = 1_000_000, int indexBuildParallel = 0, bool autoFlush = true)
    {
        return new NativeCollectionOptions
        {
            SegmentMaxDocs = segmentMaxDocs,
            IndexBuildParallel = indexBuildParallel,
            AutoFlush = autoFlush ? 1 : 0
        };
    }
}
