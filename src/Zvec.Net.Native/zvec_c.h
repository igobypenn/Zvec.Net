#ifndef ZVEC_C_H
#define ZVEC_C_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

/* ===== Status ===== */
typedef struct {
    int32_t code;
    const char* message;
} zvec_status_t;

/* ===== Types ===== */
typedef int32_t zvec_data_type_t;
typedef int32_t zvec_index_type_t;
typedef int32_t zvec_metric_type_t;
typedef int32_t zvec_quantize_type_t;

/* Data type values */
#define ZVEC_DATA_TYPE_UNDEFINED    0
#define ZVEC_DATA_TYPE_STRING       2
#define ZVEC_DATA_TYPE_BOOL         3
#define ZVEC_DATA_TYPE_INT32        4
#define ZVEC_DATA_TYPE_INT64        5
#define ZVEC_DATA_TYPE_UINT32       6
#define ZVEC_DATA_TYPE_UINT64       7
#define ZVEC_DATA_TYPE_FLOAT        8
#define ZVEC_DATA_TYPE_DOUBLE       9
#define ZVEC_DATA_TYPE_VECTOR_FP16  22
#define ZVEC_DATA_TYPE_VECTOR_FP32  23
#define ZVEC_DATA_TYPE_VECTOR_FP64  24
#define ZVEC_DATA_TYPE_VECTOR_INT8  26
#define ZVEC_DATA_TYPE_VECTOR_INT16 27
#define ZVEC_DATA_TYPE_SPARSE_FP16  30
#define ZVEC_DATA_TYPE_SPARSE_FP32  31

/* Index type values */
#define ZVEC_INDEX_TYPE_UNDEFINED 0
#define ZVEC_INDEX_TYPE_HNSW      1
#define ZVEC_INDEX_TYPE_IVF       3
#define ZVEC_INDEX_TYPE_FLAT      4
#define ZVEC_INDEX_TYPE_INVERT    10

/* Metric type values */
#define ZVEC_METRIC_TYPE_UNDEFINED 0
#define ZVEC_METRIC_TYPE_L2        1
#define ZVEC_METRIC_TYPE_IP        2
#define ZVEC_METRIC_TYPE_COSINE    3

/* Quantize type values */
#define ZVEC_QUANTIZE_TYPE_UNDEFINED 0
#define ZVEC_QUANTIZE_TYPE_FP16      1
#define ZVEC_QUANTIZE_TYPE_INT8      2
#define ZVEC_QUANTIZE_TYPE_INT4      3

/* ===== Handles ===== */
typedef struct zvec_collection_t* zvec_collection_handle_t;
typedef struct zvec_doc_t* zvec_doc_handle_t;
typedef struct zvec_result_t* zvec_result_handle_t;
typedef struct zvec_schema_t* zvec_schema_handle_t;
typedef struct zvec_query_t* zvec_query_handle_t;

/* ===== Field Definition ===== */
typedef struct {
    const char* name;
    int32_t data_type;
    int32_t dimension;
    int nullable;
    int32_t index_type;
    int32_t metric_type;
    int32_t m;
    int32_t ef_construction;
    int32_t n_lists;
    int32_t quantize_type;
} zvec_field_def_t;

/* ===== Collection Options ===== */
typedef struct {
    int32_t segment_max_docs;
    int32_t index_build_parallel;
    int auto_flush;
} zvec_collection_options_t;

/* ===== Query Definition ===== */
typedef struct {
    int32_t topk;
    const char* field_name;
    const float* vector_data;
    size_t vector_len;
    const char* filter;
    int include_vector;
    int include_doc_id;
    const char** output_fields;
    size_t output_fields_count;
    int32_t index_type;
    int32_t ef_search;
    int32_t n_probe;
} zvec_query_def_t;

/* ===== Document ===== */
zvec_doc_handle_t zvec_doc_create();
void zvec_doc_destroy(zvec_doc_handle_t handle);

void zvec_doc_set_pk(zvec_doc_handle_t handle, const char* pk);
const char* zvec_doc_get_pk(zvec_doc_handle_t handle);
double zvec_doc_get_score(zvec_doc_handle_t handle);

/* Scalar setters */
zvec_status_t zvec_doc_set_string(zvec_doc_handle_t handle, const char* field, const char* value);
zvec_status_t zvec_doc_set_int32(zvec_doc_handle_t handle, const char* field, int32_t value);
zvec_status_t zvec_doc_set_int64(zvec_doc_handle_t handle, const char* field, int64_t value);
zvec_status_t zvec_doc_set_float(zvec_doc_handle_t handle, const char* field, float value);
zvec_status_t zvec_doc_set_double(zvec_doc_handle_t handle, const char* field, double value);
zvec_status_t zvec_doc_set_bool(zvec_doc_handle_t handle, const char* field, int value);
zvec_status_t zvec_doc_set_null(zvec_doc_handle_t handle, const char* field);

/* Vector setters */
zvec_status_t zvec_doc_set_vector_f32(zvec_doc_handle_t handle, const char* field, const float* data, size_t len);
zvec_status_t zvec_doc_set_sparse_vector_f32(zvec_doc_handle_t handle, const char* field, 
    const uint32_t* indices, const float* values, size_t len);

/* Vector getters */
size_t zvec_doc_get_vector_f32(zvec_doc_handle_t handle, const char* field, float* out_data, size_t max_len);

/* Field getters */
int zvec_doc_has_field(zvec_doc_handle_t handle, const char* field);
const char* zvec_doc_get_string(zvec_doc_handle_t handle, const char* field);
int64_t zvec_doc_get_int64(zvec_doc_handle_t handle, const char* field);
double zvec_doc_get_double(zvec_doc_handle_t handle, const char* field);
int zvec_doc_get_bool(zvec_doc_handle_t handle, const char* field);

/* ===== Schema Creation ===== */
zvec_schema_handle_t zvec_schema_create(const char* name);
void zvec_schema_destroy(zvec_schema_handle_t handle);

zvec_status_t zvec_schema_add_field(zvec_schema_handle_t handle, const zvec_field_def_t* field_def);
zvec_status_t zvec_schema_add_vector_field(zvec_schema_handle_t handle, const zvec_field_def_t* field_def);

/* ===== Schema (from collection) ===== */
zvec_schema_handle_t zvec_collection_get_schema(zvec_collection_handle_t handle);

const char* zvec_schema_get_name(zvec_schema_handle_t handle);
size_t zvec_schema_get_field_count(zvec_schema_handle_t handle);
size_t zvec_schema_get_vector_count(zvec_schema_handle_t handle);
zvec_field_def_t zvec_schema_get_field(zvec_schema_handle_t handle, size_t index);
zvec_field_def_t zvec_schema_get_vector(zvec_schema_handle_t handle, size_t index);

/* ===== Query ===== */
zvec_query_handle_t zvec_query_create();
void zvec_query_destroy(zvec_query_handle_t handle);

void zvec_query_set_topk(zvec_query_handle_t handle, int32_t topk);
void zvec_query_set_field_name(zvec_query_handle_t handle, const char* field_name);
void zvec_query_set_vector(zvec_query_handle_t handle, const float* data, size_t len);
void zvec_query_set_filter(zvec_query_handle_t handle, const char* filter);
void zvec_query_set_include_vector(zvec_query_handle_t handle, int include);
void zvec_query_set_output_fields(zvec_query_handle_t handle, const char** fields, size_t count);
void zvec_query_set_ef_search(zvec_query_handle_t handle, int32_t ef);
void zvec_query_set_n_probe(zvec_query_handle_t handle, int32_t n_probe);

/* ===== Collection ===== */
zvec_status_t zvec_collection_create_and_open(
    const char* path,
    zvec_schema_handle_t schema,
    const zvec_collection_options_t* options,
    zvec_collection_handle_t* out_handle);

zvec_status_t zvec_collection_open(
    const char* path,
    const zvec_collection_options_t* options,
    zvec_collection_handle_t* out_handle);

void zvec_collection_destroy(zvec_collection_handle_t handle);
zvec_status_t zvec_collection_destroy_data(zvec_collection_handle_t handle);

zvec_status_t zvec_collection_flush(zvec_collection_handle_t handle);
zvec_status_t zvec_collection_optimize(zvec_collection_handle_t handle);

zvec_status_t zvec_collection_create_index(
    zvec_collection_handle_t handle,
    const char* field_name,
    const zvec_field_def_t* index_def);

zvec_status_t zvec_collection_drop_index(
    zvec_collection_handle_t handle,
    const char* field_name);

zvec_status_t zvec_collection_insert(zvec_collection_handle_t handle, zvec_doc_handle_t* docs, size_t count);
zvec_status_t zvec_collection_upsert(zvec_collection_handle_t handle, zvec_doc_handle_t* docs, size_t count);
zvec_status_t zvec_collection_update(zvec_collection_handle_t handle, zvec_doc_handle_t* docs, size_t count);
zvec_status_t zvec_collection_delete(zvec_collection_handle_t handle, const char** ids, size_t count);
zvec_status_t zvec_collection_delete_by_filter(zvec_collection_handle_t handle, const char* filter);

zvec_status_t zvec_collection_query(zvec_collection_handle_t handle, zvec_query_handle_t query, zvec_result_handle_t* out_result);
zvec_status_t zvec_collection_fetch(zvec_collection_handle_t handle, const char** ids, size_t count, zvec_result_handle_t* out_result);

const char* zvec_collection_get_path(zvec_collection_handle_t handle);

/* ===== Result ===== */
void zvec_result_destroy(zvec_result_handle_t handle);
size_t zvec_result_count(zvec_result_handle_t handle);
zvec_doc_handle_t zvec_result_get_doc(zvec_result_handle_t handle, size_t index);

/* ===== Version ===== */
const char* zvec_version();

#ifdef __cplusplus
}
#endif

#endif /* ZVEC_C_H */
