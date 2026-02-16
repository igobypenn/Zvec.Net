using System.Runtime.InteropServices;
using Zvec.Net.Native;

namespace Zvec.Net.Tests.Native;

public class IntegrationTests : IDisposable
{
    private static readonly bool NativeLibraryAvailable;
    private readonly string _testDir;
    
    static IntegrationTests()
    {
        try
        {
            NativeLibraryAvailable = NativeLibrary.TryLoad("zvec_native", out _);
        }
        catch
        {
            NativeLibraryAvailable = false;
        }
    }
    
    public IntegrationTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"zvec_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
    }
    
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, recursive: true);
            }
        }
        catch { }
    }
    
    [Fact]
    public void CreateAndOpenCollection_WithSchema_Succeeds()
    {
        if (!NativeLibraryAvailable) return;
        
        var schemaPtr = NativeMethods.zvec_schema_create("test_collection");
        Assert.NotEqual(IntPtr.Zero, schemaPtr);
        
        try
        {
            var fieldDef = new NativeFieldDef
            {
                Name = Marshal.StringToHGlobalAnsi("title"),
                DataType = 2,
                Nullable = 1
            };
            
            try
            {
                var addStatus = NativeMethods.zvec_schema_add_field(schemaPtr, in fieldDef);
                Assert.True(addStatus.IsOk, $"Failed to add field: {addStatus.GetMessage()}");
            }
            finally
            {
                Marshal.FreeHGlobal(fieldDef.Name);
            }
            
            var vectorDef = new NativeFieldDef
            {
                Name = Marshal.StringToHGlobalAnsi("embedding"),
                DataType = 23,
                Dimension = 128,
                Nullable = 0,
                IndexType = 1,
                MetricType = 3,
                M = 16,
                EfConstruction = 200
            };
            
            try
            {
                var addVecStatus = NativeMethods.zvec_schema_add_vector_field(schemaPtr, in vectorDef);
                Assert.True(addVecStatus.IsOk, $"Failed to add vector field: {addVecStatus.GetMessage()}");
            }
            finally
            {
                Marshal.FreeHGlobal(vectorDef.Name);
            }
            
            var options = NativeCollectionOptions.Create();
            var createStatus = NativeMethods.zvec_collection_create_and_open(
                Path.Combine(_testDir, "test_col"),
                schemaPtr,
                in options,
                out var collectionPtr);
            
            Assert.True(createStatus.IsOk, $"Failed to create collection: {createStatus.GetMessage()}");
            Assert.NotEqual(IntPtr.Zero, collectionPtr);
            
            NativeMethods.zvec_collection_destroy(collectionPtr);
        }
        finally
        {
            NativeMethods.zvec_schema_destroy(schemaPtr);
        }
    }
    
    [Fact]
    public void InsertAndFetchDocuments_Succeeds()
    {
        if (!NativeLibraryAvailable) return;
        
        var schemaPtr = NativeMethods.zvec_schema_create("insert_test");
        Assert.NotEqual(IntPtr.Zero, schemaPtr);
        
        try
        {
            var titleField = new NativeFieldDef
            {
                Name = Marshal.StringToHGlobalAnsi("title"),
                DataType = 2,
                Nullable = 1
            };
            NativeMethods.zvec_schema_add_field(schemaPtr, in titleField);
            Marshal.FreeHGlobal(titleField.Name);
            
            var vecField = new NativeFieldDef
            {
                Name = Marshal.StringToHGlobalAnsi("vec"),
                DataType = 23,
                Dimension = 4,
                Nullable = 0,
                IndexType = 4
            };
            NativeMethods.zvec_schema_add_vector_field(schemaPtr, in vecField);
            Marshal.FreeHGlobal(vecField.Name);
            
            var options = NativeCollectionOptions.Create();
            var createStatus = NativeMethods.zvec_collection_create_and_open(
                Path.Combine(_testDir, "insert_test"),
                schemaPtr,
                in options,
                out var collectionPtr);
            
            Assert.True(createStatus.IsOk);
            
            try
            {
                var docPtr = NativeMethods.zvec_doc_create();
                Assert.NotEqual(IntPtr.Zero, docPtr);
                
                try
                {
                    NativeMethods.zvec_doc_set_pk(docPtr, "doc1");
                    NativeMethods.zvec_doc_set_string(docPtr, "title", "Test Document");
                    
                    var vector = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };
                    var handle = GCHandle.Alloc(vector, GCHandleType.Pinned);
                    NativeStatus vecStatus;
                    try
                    {
                        unsafe
                        {
                            var ptr = (float*)handle.AddrOfPinnedObject();
                            vecStatus = NativeMethods.zvec_doc_set_vector_f32(docPtr, "vec", in *ptr, 4);
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                    
                    Assert.True(vecStatus.IsOk);
                    
                    var docPtrs = new IntPtr[] { docPtr };
                    var insertStatus = NativeMethods.zvec_collection_insert(collectionPtr, docPtrs, 1);
                    Assert.True(insertStatus.IsOk, $"Insert failed: {insertStatus.GetMessage()}");
                }
                finally
                {
                    NativeMethods.zvec_doc_destroy(docPtr);
                }
                
                var flushStatus = NativeMethods.zvec_collection_flush(collectionPtr);
                Assert.True(flushStatus.IsOk);
                
                var ids = new string[] { "doc1" };
                var fetchStatus = NativeMethods.zvec_collection_fetch(collectionPtr, ids, 1, out var resultPtr);
                Assert.True(fetchStatus.IsOk);
                Assert.NotEqual(IntPtr.Zero, resultPtr);
                
                try
                {
                    var count = NativeMethods.zvec_result_count(resultPtr);
                    Assert.Equal((nuint)1, count);
                    
                    var fetchedDocPtr = NativeMethods.zvec_result_get_doc(resultPtr, 0);
                    Assert.NotEqual(IntPtr.Zero, fetchedDocPtr);
                    
                    var pkPtr = NativeMethods.zvec_doc_get_pk(fetchedDocPtr);
                    var pk = Marshal.PtrToStringUTF8(pkPtr);
                    Assert.Equal("doc1", pk);
                    
                    var titlePtr = NativeMethods.zvec_doc_get_string(fetchedDocPtr, "title");
                    var title = Marshal.PtrToStringUTF8(titlePtr);
                    Assert.Equal("Test Document", title);
                }
                finally
                {
                    NativeMethods.zvec_result_destroy(resultPtr);
                }
            }
            finally
            {
                NativeMethods.zvec_collection_destroy(collectionPtr);
            }
        }
        finally
        {
            NativeMethods.zvec_schema_destroy(schemaPtr);
        }
    }
    
    [Fact]
    public void QueryWithVector_ReturnsResults()
    {
        if (!NativeLibraryAvailable) return;
        
        var schemaPtr = NativeMethods.zvec_schema_create("query_test");
        
        try
        {
            var vecField = new NativeFieldDef
            {
                Name = Marshal.StringToHGlobalAnsi("embedding"),
                DataType = 23,
                Dimension = 4,
                Nullable = 0,
                IndexType = 4,
                MetricType = 2
            };
            NativeMethods.zvec_schema_add_vector_field(schemaPtr, in vecField);
            Marshal.FreeHGlobal(vecField.Name);
            
            var options = NativeCollectionOptions.Create();
            var createStatus = NativeMethods.zvec_collection_create_and_open(
                Path.Combine(_testDir, "query_test"),
                schemaPtr,
                in options,
                out var collectionPtr);
            
            Assert.True(createStatus.IsOk);
            
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    var docPtr = NativeMethods.zvec_doc_create();
                    NativeMethods.zvec_doc_set_pk(docPtr, $"doc{i}");
                    
                    var vector = new float[] { i * 0.1f, i * 0.2f, i * 0.3f, i * 0.4f };
                    var handle = GCHandle.Alloc(vector, GCHandleType.Pinned);
                    try
                    {
                        unsafe
                        {
                            var ptr = (float*)handle.AddrOfPinnedObject();
                            NativeMethods.zvec_doc_set_vector_f32(docPtr, "embedding", in *ptr, 4);
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                    
                    var docPtrs = new IntPtr[] { docPtr };
                    NativeMethods.zvec_collection_insert(collectionPtr, docPtrs, 1);
                    NativeMethods.zvec_doc_destroy(docPtr);
                }
                
                NativeMethods.zvec_collection_flush(collectionPtr);
                
                var queryPtr = NativeMethods.zvec_query_create();
                Assert.NotEqual(IntPtr.Zero, queryPtr);
                
                try
                {
                    NativeMethods.zvec_query_set_topk(queryPtr, 3);
                    NativeMethods.zvec_query_set_field_name(queryPtr, "embedding");
                    
                    var queryVector = new float[] { 0.2f, 0.4f, 0.6f, 0.8f };
                    var qHandle = GCHandle.Alloc(queryVector, GCHandleType.Pinned);
                    try
                    {
                        unsafe
                        {
                            var ptr = (float*)qHandle.AddrOfPinnedObject();
                            NativeMethods.zvec_query_set_vector(queryPtr, in *ptr, 4);
                        }
                    }
                    finally
                    {
                        qHandle.Free();
                    }
                    
                    var queryStatus = NativeMethods.zvec_collection_query(collectionPtr, queryPtr, out var resultPtr);
                    Assert.True(queryStatus.IsOk, $"Query failed: {queryStatus.GetMessage()}");
                    
                    try
                    {
                        var count = NativeMethods.zvec_result_count(resultPtr);
                        Assert.True(count >= 1 && count <= 3, $"Expected 1-3 results, got {count}");
                    }
                    finally
                    {
                        NativeMethods.zvec_result_destroy(resultPtr);
                    }
                }
                finally
                {
                    NativeMethods.zvec_query_destroy(queryPtr);
                }
            }
            finally
            {
                NativeMethods.zvec_collection_destroy(collectionPtr);
            }
        }
        finally
        {
            NativeMethods.zvec_schema_destroy(schemaPtr);
        }
    }
}
