using System.Runtime.InteropServices;

namespace Zvec.Net.Native;

internal static partial class NativeMethods
{
    private const string LibraryName = "zvec_native";
    
    // ===== Library Info =====
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_version();
    
    // ===== Document =====
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_doc_create();
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_doc_destroy(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_doc_set_pk(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string pk);
    
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_doc_get_pk(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial double zvec_doc_get_score(IntPtr handle);
    
    // Scalar setters
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_string(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, [MarshalAs(UnmanagedType.LPUTF8Str)] string? value);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_int32(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, int value);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_int64(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, long value);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_float(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, float value);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_double(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, double value);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_bool(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, int value);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_null(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field);
    
    // Vector setters
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_vector_f32(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, in float data, nuint len);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_doc_set_sparse_vector_f32(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, in uint indices, in float values, nuint len);
    
    // Vector getters
    [LibraryImport(LibraryName)]
    internal static partial nuint zvec_doc_get_vector_f32(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field, out float outData, nuint maxLen);
    
    // Field getters
    [LibraryImport(LibraryName)]
    internal static partial int zvec_doc_has_field(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field);
    
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_doc_get_string(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field);
    
    [LibraryImport(LibraryName)]
    internal static partial long zvec_doc_get_int64(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field);
    
    [LibraryImport(LibraryName)]
    internal static partial double zvec_doc_get_double(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field);
    
    [LibraryImport(LibraryName)]
    internal static partial int zvec_doc_get_bool(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string field);
    
    // ===== Schema Creation =====
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_schema_create([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_schema_destroy(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_schema_add_field(IntPtr handle, in NativeFieldDef fieldDef);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_schema_add_vector_field(IntPtr handle, in NativeFieldDef fieldDef);
    
    // ===== Schema (from collection) =====
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_collection_get_schema(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_schema_get_name(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial nuint zvec_schema_get_field_count(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial nuint zvec_schema_get_vector_count(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeFieldDef zvec_schema_get_field(IntPtr handle, nuint index);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeFieldDef zvec_schema_get_vector(IntPtr handle, nuint index);
    
    // ===== Query =====
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_query_create();
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_destroy(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_topk(IntPtr handle, int topk);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_field_name(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string fieldName);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_vector(IntPtr handle, in float data, nuint len);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_filter(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string filter);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_include_vector(IntPtr handle, int include);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_output_fields(IntPtr handle, IntPtr fields, nuint count);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_ef_search(IntPtr handle, int ef);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_query_set_n_probe(IntPtr handle, int nProbe);
    
    // ===== Collection =====
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_create_and_open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, IntPtr schema, in NativeCollectionOptions options, out IntPtr outHandle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, in NativeCollectionOptions options, out IntPtr outHandle);
    
    [LibraryImport(LibraryName)]
    internal static partial void zvec_collection_destroy(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_destroy_data(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_flush(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_optimize(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_create_index(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string fieldName, in NativeFieldDef indexDef);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_drop_index(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string fieldName);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_insert(IntPtr handle, IntPtr[] docs, nuint count);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_upsert(IntPtr handle, IntPtr[] docs, nuint count);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_update(IntPtr handle, IntPtr[] docs, nuint count);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_delete(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)] string[] ids, nuint count);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_delete_by_filter(IntPtr handle, [MarshalAs(UnmanagedType.LPUTF8Str)] string filter);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_query(IntPtr handle, IntPtr query, out IntPtr outResult);
    
    [LibraryImport(LibraryName)]
    internal static partial NativeStatus zvec_collection_fetch(IntPtr handle, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)] string[] ids, nuint count, out IntPtr outResult);
    
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_collection_get_path(IntPtr handle);
    
    // ===== Result =====
    [LibraryImport(LibraryName)]
    internal static partial void zvec_result_destroy(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial nuint zvec_result_count(IntPtr handle);
    
    [LibraryImport(LibraryName)]
    internal static partial IntPtr zvec_result_get_doc(IntPtr handle, nuint index);
}
