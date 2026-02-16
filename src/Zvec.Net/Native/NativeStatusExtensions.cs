using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Native;

internal static class NativeStatusExtensions
{
    public static Status ToStatus(this in NativeStatus status)
    {
        return Status.From((StatusCode)status.Code, status.GetMessage() ?? string.Empty);
    }
    
    public static void ThrowIfError(this in NativeStatus status, string operation)
    {
        if (!status.IsOk)
        {
            throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? $"{operation} failed");
        }
    }
}
