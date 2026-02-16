#include "zvec_c.h"

#include <cstring>
#include <string>
#include <vector>
#include <memory>

// zvec headers
#include <zvec/db/collection.h>
#include <zvec/db/doc.h>
#include <zvec/db/schema.h>
#include <zvec/db/status.h>
#include <zvec/db/query_params.h>
#include <zvec/db/index_params.h>

using namespace zvec;

// Internal structures wrapping zvec objects
struct zvec_collection_t {
    Collection::Ptr ptr;
    std::string path_cache;
};

struct zvec_doc_t {
    Doc doc;
    std::string pk_cache;
    std::vector<float> vector_cache;
};

struct zvec_result_t {
    std::vector<zvec_doc_t> docs;
};

struct zvec_schema_t {
    CollectionSchema schema;
    std::string name_cache;
};

struct zvec_query_t {
    VectorQuery query;
    std::string field_name_cache;
    std::string filter_cache;
    std::vector<std::string> output_fields_cache;
    std::vector<const char*> output_fields_ptrs;
    std::vector<float> vector_cache;
};

// Helper: convert zvec Status to C status
static zvec_status_t to_c_status(const Status& s) {
    if (s.ok()) {
        return {0, nullptr};
    }
    return {static_cast<int32_t>(s.code()), s.c_str()};
}

static zvec_status_t ok_status() {
    return {0, nullptr};
}

// Helper: convert C field_def to IndexParams
static IndexParams::Ptr create_index_params(const zvec_field_def_t* def) {
    if (!def) return nullptr;
    
    IndexType index_type = static_cast<IndexType>(def->index_type);
    MetricType metric_type = static_cast<MetricType>(def->metric_type);
    QuantizeType quantize_type = static_cast<QuantizeType>(def->quantize_type);
    
    switch (index_type) {
        case IndexType::HNSW:
            return std::make_shared<HnswIndexParams>(
                metric_type, 
                def->m > 0 ? def->m : 16,
                def->ef_construction > 0 ? def->ef_construction : 200,
                quantize_type
            );
        case IndexType::IVF:
            return std::make_shared<IVFIndexParams>(
                metric_type,
                def->n_lists > 0 ? def->n_lists : 1024,
                10, false, quantize_type
            );
        case IndexType::FLAT:
            return std::make_shared<FlatIndexParams>(metric_type, quantize_type);
        case IndexType::INVERT:
            return std::make_shared<InvertIndexParams>();
        default:
            return nullptr;
    }
}

// Helper: convert C field_def to FieldSchema
static FieldSchema::Ptr create_field_schema(const zvec_field_def_t* def) {
    if (!def || !def->name) return nullptr;
    
    DataType data_type = static_cast<DataType>(def->data_type);
    auto field = std::make_shared<FieldSchema>(std::string(def->name), data_type, def->dimension, def->nullable != 0);
    
    // Add index params if specified
    if (def->index_type != ZVEC_INDEX_TYPE_UNDEFINED) {
        auto index_params = create_index_params(def);
        if (index_params) {
            field->set_index_params(index_params);
        }
    }
    
    return field;
}

extern "C" {

// ===== Version =====
const char* zvec_version() {
    return "0.2.0";
}

// ===== Document =====
zvec_doc_handle_t zvec_doc_create() {
    return new zvec_doc_t();
}

void zvec_doc_destroy(zvec_doc_handle_t handle) {
    delete handle;
}

void zvec_doc_set_pk(zvec_doc_handle_t handle, const char* pk) {
    if (handle && pk) {
        handle->doc.set_pk(std::string(pk));
        handle->pk_cache = pk;
    }
}

const char* zvec_doc_get_pk(zvec_doc_handle_t handle) {
    if (!handle) return nullptr;
    handle->pk_cache = handle->doc.pk();
    return handle->pk_cache.c_str();
}

double zvec_doc_get_score(zvec_doc_handle_t handle) {
    return handle ? handle->doc.score() : 0.0;
}

zvec_status_t zvec_doc_set_string(zvec_doc_handle_t handle, const char* field, const char* value) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set<std::string>(field, value ? std::string(value) : std::string());
    return ok_status();
}

zvec_status_t zvec_doc_set_int32(zvec_doc_handle_t handle, const char* field, int32_t value) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set<int32_t>(field, value);
    return ok_status();
}

zvec_status_t zvec_doc_set_int64(zvec_doc_handle_t handle, const char* field, int64_t value) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set<int64_t>(field, value);
    return ok_status();
}

zvec_status_t zvec_doc_set_float(zvec_doc_handle_t handle, const char* field, float value) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set<float>(field, value);
    return ok_status();
}

zvec_status_t zvec_doc_set_double(zvec_doc_handle_t handle, const char* field, double value) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set<double>(field, value);
    return ok_status();
}

zvec_status_t zvec_doc_set_bool(zvec_doc_handle_t handle, const char* field, int value) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set<bool>(field, value != 0);
    return ok_status();
}

zvec_status_t zvec_doc_set_null(zvec_doc_handle_t handle, const char* field) {
    if (!handle || !field) return {2, "null argument"};
    handle->doc.set_null(field);
    return ok_status();
}

zvec_status_t zvec_doc_set_vector_f32(zvec_doc_handle_t handle, const char* field, const float* data, size_t len) {
    if (!handle || !field || !data) return {2, "null argument"};
    std::vector<float> vec(data, data + len);
    handle->doc.set<std::vector<float>>(field, std::move(vec));
    return ok_status();
}

zvec_status_t zvec_doc_set_sparse_vector_f32(zvec_doc_handle_t handle, const char* field, 
    const uint32_t* indices, const float* values, size_t len) {
    if (!handle || !field || !indices || !values) return {2, "null argument"};
    std::pair<std::vector<uint32_t>, std::vector<float>> sparse;
    sparse.first.assign(indices, indices + len);
    sparse.second.assign(values, values + len);
    handle->doc.set<decltype(sparse)>(field, std::move(sparse));
    return ok_status();
}

size_t zvec_doc_get_vector_f32(zvec_doc_handle_t handle, const char* field, float* out_data, size_t max_len) {
    if (!handle || !field || !out_data) return 0;
    auto result = handle->doc.get<std::vector<float>>(field);
    if (result.has_value()) {
        const auto& vec = result.value();
        size_t copy_len = std::min(max_len, vec.size());
        std::memcpy(out_data, vec.data(), copy_len * sizeof(float));
        return copy_len;
    }
    return 0;
}

int zvec_doc_has_field(zvec_doc_handle_t handle, const char* field) {
    if (!handle || !field) return 0;
    return handle->doc.has(field) ? 1 : 0;
}

const char* zvec_doc_get_string(zvec_doc_handle_t handle, const char* field) {
    if (!handle || !field) return nullptr;
    auto result = handle->doc.get<std::string>(field);
    if (result.has_value()) {
        handle->pk_cache = result.value();
        return handle->pk_cache.c_str();
    }
    return nullptr;
}

int64_t zvec_doc_get_int64(zvec_doc_handle_t handle, const char* field) {
    if (!handle || !field) return 0;
    auto result = handle->doc.get<int64_t>(field);
    return result.has_value() ? result.value() : 0;
}

double zvec_doc_get_double(zvec_doc_handle_t handle, const char* field) {
    if (!handle || !field) return 0.0;
    auto result = handle->doc.get<double>(field);
    return result.has_value() ? result.value() : 0.0;
}

int zvec_doc_get_bool(zvec_doc_handle_t handle, const char* field) {
    if (!handle || !field) return 0;
    auto result = handle->doc.get<bool>(field);
    return result.has_value() ? (result.value() ? 1 : 0) : 0;
}

// ===== Schema Creation =====
zvec_schema_handle_t zvec_schema_create(const char* name) {
    if (!name) return nullptr;
    auto* schema = new zvec_schema_t();
    schema->schema = CollectionSchema(std::string(name));
    return schema;
}

void zvec_schema_destroy(zvec_schema_handle_t handle) {
    delete handle;
}

zvec_status_t zvec_schema_add_field(zvec_schema_handle_t handle, const zvec_field_def_t* field_def) {
    if (!handle || !field_def) return {2, "null argument"};
    
    auto field = create_field_schema(field_def);
    if (!field) return {2, "invalid field definition"};
    
    auto status = handle->schema.add_field(field);
    return to_c_status(status);
}

zvec_status_t zvec_schema_add_vector_field(zvec_schema_handle_t handle, const zvec_field_def_t* field_def) {
    if (!handle || !field_def) return {2, "null argument"};
    
    auto field = create_field_schema(field_def);
    if (!field) return {2, "invalid field definition"};
    
    // Ensure it's a vector type
    if (!FieldSchema::is_vector_field(field->data_type())) {
        return {2, "not a vector field type"};
    }
    
    auto status = handle->schema.add_field(field);
    return to_c_status(status);
}

// ===== Schema (from collection) =====
zvec_schema_handle_t zvec_collection_get_schema(zvec_collection_handle_t handle) {
    if (!handle || !handle->ptr) return nullptr;
    
    auto result = handle->ptr->Schema();
    if (result.has_value()) {
        auto* schema = new zvec_schema_t();
        schema->schema = result.value();
        return schema;
    }
    return nullptr;
}

const char* zvec_schema_get_name(zvec_schema_handle_t handle) {
    if (!handle) return nullptr;
    handle->name_cache = handle->schema.name();
    return handle->name_cache.c_str();
}

size_t zvec_schema_get_field_count(zvec_schema_handle_t handle) {
    if (!handle) return 0;
    return handle->schema.fields().size();
}

size_t zvec_schema_get_vector_count(zvec_schema_handle_t handle) {
    if (!handle) return 0;
    return handle->schema.vector_fields().size();
}

zvec_field_def_t zvec_schema_get_field(zvec_schema_handle_t handle, size_t index) {
    zvec_field_def_t def = {nullptr, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    if (!handle) return def;
    
    auto fields = handle->schema.fields();
    if (index >= fields.size()) return def;
    
    const auto& field = fields[index];
    def.name = field->name().c_str();
    def.data_type = static_cast<int32_t>(field->data_type());
    def.dimension = field->dimension();
    def.nullable = field->nullable() ? 1 : 0;
    def.index_type = static_cast<int32_t>(field->index_type());
    
    return def;
}

zvec_field_def_t zvec_schema_get_vector(zvec_schema_handle_t handle, size_t index) {
    zvec_field_def_t def = {nullptr, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    if (!handle) return def;
    
    auto vectors = handle->schema.vector_fields();
    if (index >= vectors.size()) return def;
    
    const auto& field = vectors[index];
    def.name = field->name().c_str();
    def.data_type = static_cast<int32_t>(field->data_type());
    def.dimension = field->dimension();
    def.nullable = field->nullable() ? 1 : 0;
    def.index_type = static_cast<int32_t>(field->index_type());
    
    // Get metric type from index params if available
    if (field->index_params()) {
        auto* vec_params = dynamic_cast<const VectorIndexParams*>(field->index_params().get());
        if (vec_params) {
            def.metric_type = static_cast<int32_t>(vec_params->metric_type());
            def.quantize_type = static_cast<int32_t>(vec_params->quantize_type());
        }
        auto* hnsw_params = dynamic_cast<const HnswIndexParams*>(field->index_params().get());
        if (hnsw_params) {
            def.m = hnsw_params->m();
            def.ef_construction = hnsw_params->ef_construction();
        }
        auto* ivf_params = dynamic_cast<const IVFIndexParams*>(field->index_params().get());
        if (ivf_params) {
            def.n_lists = ivf_params->n_list();
        }
    }
    
    return def;
}

// ===== Query =====
zvec_query_handle_t zvec_query_create() {
    return new zvec_query_t();
}

void zvec_query_destroy(zvec_query_handle_t handle) {
    delete handle;
}

void zvec_query_set_topk(zvec_query_handle_t handle, int32_t topk) {
    if (handle) handle->query.topk_ = topk;
}

void zvec_query_set_field_name(zvec_query_handle_t handle, const char* field_name) {
    if (handle && field_name) {
        handle->field_name_cache = field_name;
        handle->query.field_name_ = handle->field_name_cache;
    }
}

void zvec_query_set_vector(zvec_query_handle_t handle, const float* data, size_t len) {
    if (handle && data) {
        handle->vector_cache.assign(data, data + len);
        // Convert float vector to string for zvec
        handle->query.query_vector_.assign(
            reinterpret_cast<const char*>(handle->vector_cache.data()),
            len * sizeof(float)
        );
    }
}

void zvec_query_set_filter(zvec_query_handle_t handle, const char* filter) {
    if (handle && filter) {
        handle->filter_cache = filter;
        handle->query.filter_ = handle->filter_cache;
    }
}

void zvec_query_set_include_vector(zvec_query_handle_t handle, int include) {
    if (handle) handle->query.include_vector_ = include != 0;
}

void zvec_query_set_output_fields(zvec_query_handle_t handle, const char** fields, size_t count) {
    if (handle && fields) {
        handle->output_fields_cache.clear();
        handle->output_fields_ptrs.clear();
        for (size_t i = 0; i < count; i++) {
            handle->output_fields_cache.push_back(fields[i]);
        }
        handle->query.output_fields_ = handle->output_fields_cache;
    }
}

void zvec_query_set_ef_search(zvec_query_handle_t handle, int32_t ef) {
    if (handle && handle->query.query_params_) {
        auto* hnsw = dynamic_cast<HnswQueryParams*>(handle->query.query_params_.get());
        if (hnsw) hnsw->set_ef(ef);
    }
}

void zvec_query_set_n_probe(zvec_query_handle_t handle, int32_t n_probe) {
    if (handle && handle->query.query_params_) {
        auto* ivf = dynamic_cast<IVFQueryParams*>(handle->query.query_params_.get());
        if (ivf) ivf->set_nprobe(n_probe);
    }
}

// ===== Collection =====
zvec_status_t zvec_collection_create_and_open(
    const char* path,
    zvec_schema_handle_t schema,
    const zvec_collection_options_t* options,
    zvec_collection_handle_t* out)
{
    if (!path || !out) {
        return {2, "null argument"};
    }
    if (!schema) {
        return {2, "null schema"};
    }
    
    (void)options;  // CollectionOptions has different fields in zvec
    
    auto result = Collection::CreateAndOpen(std::string(path), schema->schema, CollectionOptions{});
    
    if (result.has_value()) {
        auto* col = new zvec_collection_t();
        col->ptr = result.value();
        col->path_cache = path;
        *out = col;
        return ok_status();
    }
    
    return to_c_status(result.error());
}

zvec_status_t zvec_collection_open(
    const char* path,
    const zvec_collection_options_t* options,
    zvec_collection_handle_t* out)
{
    if (!path || !out) {
        return {2, "null argument"};
    }
    
    (void)options;  // CollectionOptions has different fields in zvec
    
    auto result = Collection::Open(std::string(path), CollectionOptions{});
    
    if (result.has_value()) {
        auto* col = new zvec_collection_t();
        col->ptr = result.value();
        col->path_cache = path;
        *out = col;
        return ok_status();
    }
    
    return to_c_status(result.error());
}

void zvec_collection_destroy(zvec_collection_handle_t handle) {
    if (handle) {
        handle->ptr.reset();
        delete handle;
    }
}

zvec_status_t zvec_collection_destroy_data(zvec_collection_handle_t handle) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    return to_c_status(handle->ptr->Destroy());
}

zvec_status_t zvec_collection_flush(zvec_collection_handle_t handle) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    return to_c_status(handle->ptr->Flush());
}

zvec_status_t zvec_collection_optimize(zvec_collection_handle_t handle) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    return to_c_status(handle->ptr->Optimize());
}

zvec_status_t zvec_collection_create_index(
    zvec_collection_handle_t handle,
    const char* field_name,
    const zvec_field_def_t* index_def)
{
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!field_name) return {2, "null field_name"};
    if (!index_def) return {2, "null index_def"};
    
    auto index_params = create_index_params(index_def);
    if (!index_params) return {2, "invalid index definition"};
    
    return to_c_status(handle->ptr->CreateIndex(std::string(field_name), index_params));
}

zvec_status_t zvec_collection_drop_index(
    zvec_collection_handle_t handle,
    const char* field_name)
{
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!field_name) return {2, "null field_name"};
    
    return to_c_status(handle->ptr->DropIndex(std::string(field_name)));
}

zvec_status_t zvec_collection_insert(zvec_collection_handle_t handle, zvec_doc_handle_t* docs, size_t count) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!docs || count == 0) return ok_status();
    
    std::vector<Doc> zvec_docs;
    zvec_docs.reserve(count);
    for (size_t i = 0; i < count; i++) {
        zvec_docs.push_back(docs[i]->doc);
    }
    
    auto result = handle->ptr->Insert(zvec_docs);
    if (result.has_value()) {
        for (const auto& s : result.value()) {
            if (!s.ok()) return to_c_status(s);
        }
        return ok_status();
    }
    return to_c_status(result.error());
}

zvec_status_t zvec_collection_upsert(zvec_collection_handle_t handle, zvec_doc_handle_t* docs, size_t count) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!docs || count == 0) return ok_status();
    
    std::vector<Doc> zvec_docs;
    zvec_docs.reserve(count);
    for (size_t i = 0; i < count; i++) {
        zvec_docs.push_back(docs[i]->doc);
    }
    
    auto result = handle->ptr->Upsert(zvec_docs);
    if (result.has_value()) {
        for (const auto& s : result.value()) {
            if (!s.ok()) return to_c_status(s);
        }
        return ok_status();
    }
    return to_c_status(result.error());
}

zvec_status_t zvec_collection_update(zvec_collection_handle_t handle, zvec_doc_handle_t* docs, size_t count) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!docs || count == 0) return ok_status();
    
    std::vector<Doc> zvec_docs;
    zvec_docs.reserve(count);
    for (size_t i = 0; i < count; i++) {
        zvec_docs.push_back(docs[i]->doc);
    }
    
    auto result = handle->ptr->Update(zvec_docs);
    if (result.has_value()) {
        for (const auto& s : result.value()) {
            if (!s.ok()) return to_c_status(s);
        }
        return ok_status();
    }
    return to_c_status(result.error());
}

zvec_status_t zvec_collection_delete(zvec_collection_handle_t handle, const char** ids, size_t count) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!ids || count == 0) return ok_status();
    
    std::vector<std::string> pks;
    pks.reserve(count);
    for (size_t i = 0; i < count; i++) {
        pks.push_back(std::string(ids[i]));
    }
    
    auto result = handle->ptr->Delete(pks);
    if (result.has_value()) {
        for (const auto& s : result.value()) {
            if (!s.ok()) return to_c_status(s);
        }
        return ok_status();
    }
    return to_c_status(result.error());
}

zvec_status_t zvec_collection_delete_by_filter(zvec_collection_handle_t handle, const char* filter) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!filter) return {2, "null filter"};
    
    return to_c_status(handle->ptr->DeleteByFilter(std::string(filter)));
}

zvec_status_t zvec_collection_query(zvec_collection_handle_t handle, zvec_query_handle_t query, zvec_result_handle_t* out) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!query) return {2, "null query"};
    if (!out) return {2, "null out"};
    
    auto result = handle->ptr->Query(query->query);
    if (result.has_value()) {
        auto* res = new zvec_result_t();
        for (const auto& doc_ptr : result.value()) {
            if (doc_ptr) {
                zvec_doc_t d;
                d.doc = *doc_ptr;
                d.pk_cache = doc_ptr->pk();
                res->docs.push_back(std::move(d));
            }
        }
        *out = res;
        return ok_status();
    }
    
    return to_c_status(result.error());
}

zvec_status_t zvec_collection_fetch(zvec_collection_handle_t handle, const char** ids, size_t count, zvec_result_handle_t* out) {
    if (!handle || !handle->ptr) return {2, "null handle"};
    if (!out) return {2, "null out"};
    
    std::vector<std::string> pks;
    if (ids && count > 0) {
        pks.reserve(count);
        for (size_t i = 0; i < count; i++) {
            pks.push_back(std::string(ids[i]));
        }
    }
    
    auto result = handle->ptr->Fetch(pks);
    if (result.has_value()) {
        auto* res = new zvec_result_t();
        for (const auto& [pk, doc_ptr] : result.value()) {
            if (doc_ptr) {
                zvec_doc_t d;
                d.doc = *doc_ptr;
                d.pk_cache = pk;
                res->docs.push_back(std::move(d));
            }
        }
        *out = res;
        return ok_status();
    }
    
    return to_c_status(result.error());
}

const char* zvec_collection_get_path(zvec_collection_handle_t handle) {
    if (!handle || !handle->ptr) return nullptr;
    
    auto result = handle->ptr->Path();
    if (result.has_value()) {
        handle->path_cache = result.value();
        return handle->path_cache.c_str();
    }
    return nullptr;
}

// ===== Result =====
void zvec_result_destroy(zvec_result_handle_t handle) {
    delete handle;
}

size_t zvec_result_count(zvec_result_handle_t handle) {
    return handle ? handle->docs.size() : 0;
}

zvec_doc_handle_t zvec_result_get_doc(zvec_result_handle_t handle, size_t index) {
    if (!handle || index >= handle->docs.size()) return nullptr;
    return &handle->docs[index];
}

}  // extern "C"
