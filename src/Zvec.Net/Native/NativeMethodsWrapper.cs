namespace Zvec.Net.Native;

internal sealed class NativeMethodsWrapper : INativeMethods
{
    public static readonly NativeMethodsWrapper Instance = new();
    
    private NativeMethodsWrapper() { }
    
    public IntPtr zvec_version() => NativeMethods.zvec_version();
    
    public IntPtr zvec_doc_create() => NativeMethods.zvec_doc_create();
    public void zvec_doc_destroy(IntPtr handle) => NativeMethods.zvec_doc_destroy(handle);
    
    public void zvec_doc_set_pk(IntPtr handle, string pk) => NativeMethods.zvec_doc_set_pk(handle, pk);
    public IntPtr zvec_doc_get_pk(IntPtr handle) => NativeMethods.zvec_doc_get_pk(handle);
    public double zvec_doc_get_score(IntPtr handle) => NativeMethods.zvec_doc_get_score(handle);
    
    public NativeStatus zvec_doc_set_string(IntPtr handle, string field, string? value) => 
        NativeMethods.zvec_doc_set_string(handle, field, value);
    public NativeStatus zvec_doc_set_int32(IntPtr handle, string field, int value) => 
        NativeMethods.zvec_doc_set_int32(handle, field, value);
    public NativeStatus zvec_doc_set_int64(IntPtr handle, string field, long value) => 
        NativeMethods.zvec_doc_set_int64(handle, field, value);
    public NativeStatus zvec_doc_set_float(IntPtr handle, string field, float value) => 
        NativeMethods.zvec_doc_set_float(handle, field, value);
    public NativeStatus zvec_doc_set_double(IntPtr handle, string field, double value) => 
        NativeMethods.zvec_doc_set_double(handle, field, value);
    public NativeStatus zvec_doc_set_bool(IntPtr handle, string field, int value) => 
        NativeMethods.zvec_doc_set_bool(handle, field, value);
    public NativeStatus zvec_doc_set_null(IntPtr handle, string field) => 
        NativeMethods.zvec_doc_set_null(handle, field);
    
    public NativeStatus zvec_doc_set_vector_f32(IntPtr handle, string field, in float data, nuint len) => 
        NativeMethods.zvec_doc_set_vector_f32(handle, field, in data, len);
    public NativeStatus zvec_doc_set_sparse_vector_f32(IntPtr handle, string field, in uint indices, in float values, nuint len) => 
        NativeMethods.zvec_doc_set_sparse_vector_f32(handle, field, in indices, in values, len);
    
    public nuint zvec_doc_get_vector_f32(IntPtr handle, string field, out float outData, nuint maxLen) => 
        NativeMethods.zvec_doc_get_vector_f32(handle, field, out outData, maxLen);
    
    public int zvec_doc_has_field(IntPtr handle, string field) => NativeMethods.zvec_doc_has_field(handle, field);
    public IntPtr zvec_doc_get_string(IntPtr handle, string field) => NativeMethods.zvec_doc_get_string(handle, field);
    public long zvec_doc_get_int64(IntPtr handle, string field) => NativeMethods.zvec_doc_get_int64(handle, field);
    public double zvec_doc_get_double(IntPtr handle, string field) => NativeMethods.zvec_doc_get_double(handle, field);
    public int zvec_doc_get_bool(IntPtr handle, string field) => NativeMethods.zvec_doc_get_bool(handle, field);
    
    public IntPtr zvec_schema_create(string name) => NativeMethods.zvec_schema_create(name);
    public void zvec_schema_destroy(IntPtr handle) => NativeMethods.zvec_schema_destroy(handle);
    public NativeStatus zvec_schema_add_field(IntPtr handle, in NativeFieldDef fieldDef) => 
        NativeMethods.zvec_schema_add_field(handle, in fieldDef);
    public NativeStatus zvec_schema_add_vector_field(IntPtr handle, in NativeFieldDef fieldDef) => 
        NativeMethods.zvec_schema_add_vector_field(handle, in fieldDef);
    
    public IntPtr zvec_collection_get_schema(IntPtr handle) => NativeMethods.zvec_collection_get_schema(handle);
    public IntPtr zvec_schema_get_name(IntPtr handle) => NativeMethods.zvec_schema_get_name(handle);
    public nuint zvec_schema_get_field_count(IntPtr handle) => NativeMethods.zvec_schema_get_field_count(handle);
    public nuint zvec_schema_get_vector_count(IntPtr handle) => NativeMethods.zvec_schema_get_vector_count(handle);
    public NativeFieldDef zvec_schema_get_field(IntPtr handle, nuint index) => NativeMethods.zvec_schema_get_field(handle, index);
    public NativeFieldDef zvec_schema_get_vector(IntPtr handle, nuint index) => NativeMethods.zvec_schema_get_vector(handle, index);
    
    public IntPtr zvec_query_create() => NativeMethods.zvec_query_create();
    public void zvec_query_destroy(IntPtr handle) => NativeMethods.zvec_query_destroy(handle);
    public void zvec_query_set_topk(IntPtr handle, int topk) => NativeMethods.zvec_query_set_topk(handle, topk);
    public void zvec_query_set_field_name(IntPtr handle, string fieldName) => NativeMethods.zvec_query_set_field_name(handle, fieldName);
    public void zvec_query_set_vector(IntPtr handle, in float data, nuint len) => NativeMethods.zvec_query_set_vector(handle, in data, len);
    public void zvec_query_set_filter(IntPtr handle, string filter) => NativeMethods.zvec_query_set_filter(handle, filter);
    public void zvec_query_set_include_vector(IntPtr handle, int include) => NativeMethods.zvec_query_set_include_vector(handle, include);
    public void zvec_query_set_output_fields(IntPtr handle, IntPtr fields, nuint count) => NativeMethods.zvec_query_set_output_fields(handle, fields, count);
    public void zvec_query_set_ef_search(IntPtr handle, int ef) => NativeMethods.zvec_query_set_ef_search(handle, ef);
    public void zvec_query_set_n_probe(IntPtr handle, int nProbe) => NativeMethods.zvec_query_set_n_probe(handle, nProbe);
    
    public NativeStatus zvec_collection_create_and_open(string path, IntPtr schema, in NativeCollectionOptions options, out IntPtr outHandle) =>
        NativeMethods.zvec_collection_create_and_open(path, schema, in options, out outHandle);
    public NativeStatus zvec_collection_open(string path, in NativeCollectionOptions options, out IntPtr outHandle) =>
        NativeMethods.zvec_collection_open(path, in options, out outHandle);
    public void zvec_collection_destroy(IntPtr handle) => NativeMethods.zvec_collection_destroy(handle);
    public NativeStatus zvec_collection_destroy_data(IntPtr handle) => NativeMethods.zvec_collection_destroy_data(handle);
    public NativeStatus zvec_collection_flush(IntPtr handle) => NativeMethods.zvec_collection_flush(handle);
    public NativeStatus zvec_collection_optimize(IntPtr handle) => NativeMethods.zvec_collection_optimize(handle);
    public NativeStatus zvec_collection_create_index(IntPtr handle, string fieldName, in NativeFieldDef indexDef) =>
        NativeMethods.zvec_collection_create_index(handle, fieldName, in indexDef);
    public NativeStatus zvec_collection_drop_index(IntPtr handle, string fieldName) => NativeMethods.zvec_collection_drop_index(handle, fieldName);
    public NativeStatus zvec_collection_insert(IntPtr handle, IntPtr[] docs, nuint count) => NativeMethods.zvec_collection_insert(handle, docs, count);
    public NativeStatus zvec_collection_upsert(IntPtr handle, IntPtr[] docs, nuint count) => NativeMethods.zvec_collection_upsert(handle, docs, count);
    public NativeStatus zvec_collection_update(IntPtr handle, IntPtr[] docs, nuint count) => NativeMethods.zvec_collection_update(handle, docs, count);
    public NativeStatus zvec_collection_delete(IntPtr handle, string[] ids, nuint count) => NativeMethods.zvec_collection_delete(handle, ids, count);
    public NativeStatus zvec_collection_delete_by_filter(IntPtr handle, string filter) => NativeMethods.zvec_collection_delete_by_filter(handle, filter);
    public NativeStatus zvec_collection_query(IntPtr handle, IntPtr query, out IntPtr outResult) => NativeMethods.zvec_collection_query(handle, query, out outResult);
    public NativeStatus zvec_collection_fetch(IntPtr handle, string[] ids, nuint count, out IntPtr outResult) => NativeMethods.zvec_collection_fetch(handle, ids, count, out outResult);
    public IntPtr zvec_collection_get_path(IntPtr handle) => NativeMethods.zvec_collection_get_path(handle);
    
    public void zvec_result_destroy(IntPtr handle) => NativeMethods.zvec_result_destroy(handle);
    public nuint zvec_result_count(IntPtr handle) => NativeMethods.zvec_result_count(handle);
    public IntPtr zvec_result_get_doc(IntPtr handle, nuint index) => NativeMethods.zvec_result_get_doc(handle, index);
}
