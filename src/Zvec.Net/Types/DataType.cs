namespace Zvec.Net.Types;

/// <summary>
/// Data types supported by zvec fields.
/// </summary>
public enum DataType : uint
{
    /// <summary>
    /// Undefined or unknown type.
    /// </summary>
    Undefined = 0,
    
    /// <summary>
    /// Binary data (byte array).
    /// </summary>
    Binary = 1,
    
    /// <summary>
    /// UTF-8 string.
    /// </summary>
    String = 2,
    
    /// <summary>
    /// Boolean value.
    /// </summary>
    Bool = 3,
    
    /// <summary>
    /// 32-bit signed integer.
    /// </summary>
    Int32 = 4,
    
    /// <summary>
    /// 64-bit signed integer.
    /// </summary>
    Int64 = 5,
    
    /// <summary>
    /// 32-bit unsigned integer.
    /// </summary>
    UInt32 = 6,
    
    /// <summary>
    /// 64-bit unsigned integer.
    /// </summary>
    UInt64 = 7,
    
    /// <summary>
    /// 32-bit floating point.
    /// </summary>
    Float = 8,
    
    /// <summary>
    /// 64-bit floating point.
    /// </summary>
    Double = 9,
    
    /// <summary>
    /// 32-bit binary vector.
    /// </summary>
    VectorBinary32 = 20,
    
    /// <summary>
    /// 64-bit binary vector.
    /// </summary>
    VectorBinary64 = 21,
    
    /// <summary>
    /// 16-bit floating point vector.
    /// </summary>
    VectorFp16 = 22,
    
    /// <summary>
    /// 32-bit floating point vector.
    /// </summary>
    VectorFp32 = 23,
    
    /// <summary>
    /// 64-bit floating point vector.
    /// </summary>
    VectorFp64 = 24,
    
    /// <summary>
    /// 4-bit integer vector.
    /// </summary>
    VectorInt4 = 25,
    
    /// <summary>
    /// 8-bit integer vector.
    /// </summary>
    VectorInt8 = 26,
    
    /// <summary>
    /// 16-bit integer vector.
    /// </summary>
    VectorInt16 = 27,
    
    /// <summary>
    /// 16-bit sparse floating point vector.
    /// </summary>
    SparseVectorFp16 = 30,
    
    /// <summary>
    /// 32-bit sparse floating point vector.
    /// </summary>
    SparseVectorFp32 = 31,
    
    /// <summary>
    /// Binary array.
    /// </summary>
    ArrayBinary = 40,
    
    /// <summary>
    /// String array.
    /// </summary>
    ArrayString = 41,
    
    /// <summary>
    /// Boolean array.
    /// </summary>
    ArrayBool = 42,
    
    /// <summary>
    /// 32-bit integer array.
    /// </summary>
    ArrayInt32 = 43,
    
    /// <summary>
    /// 64-bit integer array.
    /// </summary>
    ArrayInt64 = 44,
    
    /// <summary>
    /// 32-bit unsigned integer array.
    /// </summary>
    ArrayUInt32 = 45,
    
    /// <summary>
    /// 64-bit unsigned integer array.
    /// </summary>
    ArrayUInt64 = 46,
    
    /// <summary>
    /// 32-bit floating point array.
    /// </summary>
    ArrayFloat = 47,
    
    /// <summary>
    /// 64-bit floating point array.
    /// </summary>
    ArrayDouble = 48,
}
