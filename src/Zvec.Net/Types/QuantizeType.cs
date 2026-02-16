namespace Zvec.Net.Types;

/// <summary>
/// Quantization types for reducing index size.
/// </summary>
public enum QuantizeType
{
    /// <summary>
    /// No quantization.
    /// </summary>
    Undefined = 0,
    
    /// <summary>
    /// 16-bit floating point quantization.
    /// </summary>
    Fp16 = 1,
    
    /// <summary>
    /// 8-bit integer quantization.
    /// </summary>
    Int8 = 2,
    
    /// <summary>
    /// 4-bit integer quantization.
    /// </summary>
    Int4 = 3,
}
