using System.Runtime.InteropServices;
using Zvec.Net.Native;

namespace Zvec.Net.Tests.Native;

public class NativeLibraryTests
{
    private static readonly bool NativeLibraryAvailable;

    static NativeLibraryTests()
    {
        // Check if native library is available
        try
        {
            NativeLibraryAvailable = NativeLibrary.TryLoad("zvec_native", out _);
        }
        catch
        {
            NativeLibraryAvailable = false;
        }
    }

    [Fact]
    public void NativeLibrary_LoadsAndReturnsVersion()
    {
        if (!NativeLibraryAvailable) return;

        var versionPtr = NativeMethods.zvec_version();
        Assert.NotEqual(IntPtr.Zero, versionPtr);

        var version = Marshal.PtrToStringUTF8(versionPtr);
        Assert.Equal("0.2.0", version);
    }

    [Fact]
    public void NativeDoc_CreateAndDestroy()
    {
        if (!NativeLibraryAvailable) return;

        var docPtr = NativeMethods.zvec_doc_create();
        Assert.NotEqual(IntPtr.Zero, docPtr);

        NativeMethods.zvec_doc_destroy(docPtr);
    }

    [Fact]
    public void NativeDoc_SetAndGetPk()
    {
        if (!NativeLibraryAvailable) return;

        var docPtr = NativeMethods.zvec_doc_create();
        Assert.NotEqual(IntPtr.Zero, docPtr);

        try
        {
            NativeMethods.zvec_doc_set_pk(docPtr, "test-id-123");

            var pkPtr = NativeMethods.zvec_doc_get_pk(docPtr);
            Assert.NotEqual(IntPtr.Zero, pkPtr);

            var pk = Marshal.PtrToStringUTF8(pkPtr);
            Assert.Equal("test-id-123", pk);
        }
        finally
        {
            NativeMethods.zvec_doc_destroy(docPtr);
        }
    }

    [Fact]
    public void NativeDoc_SetAndGetScalarFields()
    {
        if (!NativeLibraryAvailable) return;

        var docPtr = NativeMethods.zvec_doc_create();
        Assert.NotEqual(IntPtr.Zero, docPtr);

        try
        {
            // String field
            var status = NativeMethods.zvec_doc_set_string(docPtr, "title", "Hello World");
            Assert.True(status.IsOk);

            var strPtr = NativeMethods.zvec_doc_get_string(docPtr, "title");
            var str = Marshal.PtrToStringUTF8(strPtr);
            Assert.Equal("Hello World", str);

            // Int64 field
            status = NativeMethods.zvec_doc_set_int64(docPtr, "count", 42L);
            Assert.True(status.IsOk);

            var count = NativeMethods.zvec_doc_get_int64(docPtr, "count");
            Assert.Equal(42L, count);

            // Double field
            status = NativeMethods.zvec_doc_set_double(docPtr, "score", 3.14159);
            Assert.True(status.IsOk);

            var score = NativeMethods.zvec_doc_get_double(docPtr, "score");
            Assert.Equal(3.14159, score, 5);

            // Bool field
            status = NativeMethods.zvec_doc_set_bool(docPtr, "active", 1);
            Assert.True(status.IsOk);

            var active = NativeMethods.zvec_doc_get_bool(docPtr, "active");
            Assert.Equal(1, active);

            // Has field check
            Assert.Equal(1, NativeMethods.zvec_doc_has_field(docPtr, "title"));
            Assert.Equal(0, NativeMethods.zvec_doc_has_field(docPtr, "nonexistent"));
        }
        finally
        {
            NativeMethods.zvec_doc_destroy(docPtr);
        }
    }

    [Fact]
    public void NativeDoc_SetVector()
    {
        if (!NativeLibraryAvailable) return;

        var docPtr = NativeMethods.zvec_doc_create();
        Assert.NotEqual(IntPtr.Zero, docPtr);

        try
        {
            var vector = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };

            var handle = GCHandle.Alloc(vector, GCHandleType.Pinned);
            try
            {
                unsafe
                {
                    var ptr = (float*)handle.AddrOfPinnedObject();
                    var status = NativeMethods.zvec_doc_set_vector_f32(docPtr, "embedding",
                        in *ptr, (nuint)vector.Length);
                    Assert.True(status.IsOk);
                }
            }
            finally
            {
                handle.Free();
            }

            Assert.Equal(1, NativeMethods.zvec_doc_has_field(docPtr, "embedding"));
        }
        finally
        {
            NativeMethods.zvec_doc_destroy(docPtr);
        }
    }

    [Fact]
    public void NativeCollection_OpenNonexistentPath_ReturnsError()
    {
        if (!NativeLibraryAvailable) return;

        var options = NativeCollectionOptions.Create();
        var status = NativeMethods.zvec_collection_open("/nonexistent/path/that/does/not/exist", in options, out var handle);

        Assert.NotEqual(0, status.Code);
    }
}
