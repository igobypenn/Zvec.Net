namespace Zvec.Net.Native;

internal interface INativeMethods
{
    // Library Info
    IntPtr zvec_version();
    
    // Document lifecycle
    IntPtr zvec_doc_create();
    void zvec_doc_destroy(IntPtr handle);
    
    // Document PK
    void zvec_doc_set_pk(IntPtr handle, string pk);
    IntPtr zvec_doc_get_pk(IntPtr handle);
    double zvec_doc_get_score(IntPtr handle);
    
    // Scalar setters
    NativeStatus zvec_doc_set_string(IntPtr handle, string field, string? value);
    NativeStatus zvec_doc_set_int32(IntPtr handle, string field, int value);
    NativeStatus zvec_doc_set_int64(IntPtr handle, string field, long value);
    NativeStatus zvec_doc_set_float(IntPtr handle, string field, float value);
    NativeStatus zvec_doc_set_double(IntPtr handle, string field, double value);
    NativeStatus zvec_doc_set_bool(IntPtr handle, string field, int value);
    NativeStatus zvec_doc_set_null(IntPtr handle, string field);
    
    // Vector setters
    NativeStatus zvec_doc_set_vector_f32(IntPtr handle, string field, in float data, nuint len);
    NativeStatus zvec_doc_set_sparse_vector_f32(IntPtr handle, string field, in uint indices, in float values, nuint len);
    
    // Vector getters
    nuint zvec_doc_get_vector_f32(IntPtr handle, string field, out float outData, nuint maxLen);
    
    // Field getters
    int zvec_doc_has_field(IntPtr handle, string field);
    IntPtr zvec_doc_get_string(IntPtr handle, string field);
    long zvec_doc_get_int64(IntPtr handle, string field);
    double zvec_doc_get_double(IntPtr handle, string field);
    int zvec_doc_get_bool(IntPtr handle, string field);
    
    // Schema creation
    IntPtr zvec_schema_create(string name);
    void zvec_schema_destroy(IntPtr handle);
    NativeStatus zvec_schema_add_field(IntPtr handle, in NativeFieldDef fieldDef);
    NativeStatus zvec_schema_add_vector_field(IntPtr handle, in NativeFieldDef fieldDef);
    
    // Schema from collection
    IntPtr zvec_collection_get_schema(IntPtr handle);
    IntPtr zvec_schema_get_name(IntPtr handle);
    nuint zvec_schema_get_field_count(IntPtr handle);
    nuint zvec_schema_get_vector_count(IntPtr handle);
    NativeFieldDef zvec_schema_get_field(IntPtr handle, nuint index);
    NativeFieldDef zvec_schema_get_vector(IntPtr handle, nuint index);
    
    // Query
    IntPtr zvec_query_create();
    void zvec_query_destroy(IntPtr handle);
    void zvec_query_set_topk(IntPtr handle, int topk);
    void zvec_query_set_field_name(IntPtr handle, string fieldName);
    void zvec_query_set_vector(IntPtr handle, in float data, nuint len);
    void zvec_query_set_filter(IntPtr handle, string filter);
    void zvec_query_set_include_vector(IntPtr handle, int include);
    void zvec_query_set_output_fields(IntPtr handle, IntPtr fields, nuint count);
    void zvec_query_set_ef_search(IntPtr handle, int ef);
    void zvec_query_set_n_probe(IntPtr handle, int nProbe);
    
    // Collection
    NativeStatus zvec_collection_create_and_open(string path, IntPtr schema, in NativeCollectionOptions options, out IntPtr outHandle);
    NativeStatus zvec_collection_open(string path, in NativeCollectionOptions options, out IntPtr outHandle);
    void zvec_collection_destroy(IntPtr handle);
    NativeStatus zvec_collection_destroy_data(IntPtr handle);
    NativeStatus zvec_collection_flush(IntPtr handle);
    NativeStatus zvec_collection_optimize(IntPtr handle);
    NativeStatus zvec_collection_create_index(IntPtr handle, string fieldName, in NativeFieldDef indexDef);
    NativeStatus zvec_collection_drop_index(IntPtr handle, string fieldName);
    NativeStatus zvec_collection_insert(IntPtr handle, IntPtr[] docs, nuint count);
    NativeStatus zvec_collection_upsert(IntPtr handle, IntPtr[] docs, nuint count);
    NativeStatus zvec_collection_update(IntPtr handle, IntPtr[] docs, nuint count);
    NativeStatus zvec_collection_delete(IntPtr handle, string[] ids, nuint count);
    NativeStatus zvec_collection_delete_by_filter(IntPtr handle, string filter);
    NativeStatus zvec_collection_query(IntPtr handle, IntPtr query, out IntPtr outResult);
    NativeStatus zvec_collection_fetch(IntPtr handle, string[] ids, nuint count, out IntPtr outResult);
    IntPtr zvec_collection_get_path(IntPtr handle);
    
    // Result
    void zvec_result_destroy(IntPtr handle);
    nuint zvec_result_count(IntPtr handle);
    IntPtr zvec_result_get_doc(IntPtr handle, nuint index);
}
