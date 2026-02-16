using System.Runtime.InteropServices;
using Zvec.Net.Native;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Mocks;

internal sealed class MockNativeMethods : INativeMethods
{
    private int _nextHandleId = 1000;
    private readonly Dictionary<IntPtr, MockCollection> _collections = new();
    private readonly Dictionary<IntPtr, MockDocument> _documents = new();
    private readonly Dictionary<IntPtr, MockQuery> _queries = new();
    private readonly Dictionary<IntPtr, CollectionSchema> _schemas = new();
    private readonly Dictionary<IntPtr, MockResult> _results = new();

    public IReadOnlyDictionary<IntPtr, MockCollection> Collections => _collections;
    public IReadOnlyDictionary<IntPtr, MockDocument> Documents => _documents;
    public List<string> MethodCalls { get; } = new();

    public bool SimulateErrors { get; set; }
    public int? ForceErrorCode { get; set; }
    public string? ForceErrorMessage { get; set; }

    private IntPtr NextHandle()
    {
        return (IntPtr)(_nextHandleId++);
    }

    private NativeStatus Ok() => new() { Code = 0, Message = IntPtr.Zero };

    private NativeStatus Error(int code, string message)
    {
        return new NativeStatus { Code = code, Message = Marshal.StringToHGlobalAnsi(message) };
    }

    private NativeStatus MaybeForceError()
    {
        if (SimulateErrors && ForceErrorCode.HasValue)
        {
            return Error(ForceErrorCode.Value, ForceErrorMessage ?? "Forced error");
        }
        return Ok();
    }

    // ===== Library Info =====

    public IntPtr zvec_version()
    {
        MethodCalls.Add(nameof(zvec_version));
        return Marshal.StringToHGlobalAnsi("0.2.0-mock");
    }

    // ===== Document =====

    public IntPtr zvec_doc_create()
    {
        MethodCalls.Add(nameof(zvec_doc_create));
        var handle = NextHandle();
        _documents[handle] = new MockDocument();
        return handle;
    }

    public void zvec_doc_destroy(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_doc_destroy));
        _documents.Remove(handle);
    }

    public void zvec_doc_set_pk(IntPtr handle, string pk)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_pk)}({pk})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Pk = pk;
        }
    }

    public IntPtr zvec_doc_get_pk(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_doc_get_pk));
        if (_documents.TryGetValue(handle, out var doc))
        {
            return Marshal.StringToHGlobalAnsi(doc.Pk ?? "");
        }
        return IntPtr.Zero;
    }

    public double zvec_doc_get_score(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_doc_get_score));
        if (_documents.TryGetValue(handle, out var doc))
        {
            return doc.Score;
        }
        return 0;
    }

    public NativeStatus zvec_doc_set_string(IntPtr handle, string field, string? value)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_string)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = value;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_int32(IntPtr handle, string field, int value)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_int32)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = value;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_int64(IntPtr handle, string field, long value)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_int64)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = value;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_float(IntPtr handle, string field, float value)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_float)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = value;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_double(IntPtr handle, string field, double value)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_double)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = value;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_bool(IntPtr handle, string field, int value)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_bool)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = value != 0;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_null(IntPtr handle, string field)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_null)}({field})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            doc.Fields[field] = null;
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_vector_f32(IntPtr handle, string field, in float data, nuint len)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_vector_f32)}({field}, {len})");
        if (_documents.TryGetValue(handle, out var doc))
        {
            unsafe
            {
                var vector = new float[(int)len];
                fixed (float* dst = vector)
                fixed (float* src = &data)
                {
                    Buffer.MemoryCopy(src, dst, len * sizeof(float), len * sizeof(float));
                }
                doc.Vectors[field] = vector;
            }
        }
        return Ok();
    }

    public NativeStatus zvec_doc_set_sparse_vector_f32(IntPtr handle, string field, in uint indices, in float values, nuint len)
    {
        MethodCalls.Add($"{nameof(zvec_doc_set_sparse_vector_f32)}({field})");
        return Ok();
    }

    public nuint zvec_doc_get_vector_f32(IntPtr handle, string field, out float outData, nuint maxLen)
    {
        MethodCalls.Add(nameof(zvec_doc_get_vector_f32));
        outData = 0;
        return 0;
    }

    public int zvec_doc_has_field(IntPtr handle, string field)
    {
        MethodCalls.Add(nameof(zvec_doc_has_field));
        if (_documents.TryGetValue(handle, out var doc))
        {
            return doc.Fields.ContainsKey(field) ? 1 : 0;
        }
        return 0;
    }

    public IntPtr zvec_doc_get_string(IntPtr handle, string field)
    {
        MethodCalls.Add(nameof(zvec_doc_get_string));
        if (_documents.TryGetValue(handle, out var doc) && doc.Fields.TryGetValue(field, out var value) && value is string str)
        {
            return Marshal.StringToHGlobalAnsi(str);
        }
        return IntPtr.Zero;
    }

    public long zvec_doc_get_int64(IntPtr handle, string field)
    {
        MethodCalls.Add(nameof(zvec_doc_get_int64));
        if (_documents.TryGetValue(handle, out var doc) && doc.Fields.TryGetValue(field, out var value))
        {
            return Convert.ToInt64(value);
        }
        return 0;
    }

    public double zvec_doc_get_double(IntPtr handle, string field)
    {
        MethodCalls.Add(nameof(zvec_doc_get_double));
        if (_documents.TryGetValue(handle, out var doc) && doc.Fields.TryGetValue(field, out var value))
        {
            return Convert.ToDouble(value);
        }
        return 0;
    }

    public int zvec_doc_get_bool(IntPtr handle, string field)
    {
        MethodCalls.Add(nameof(zvec_doc_get_bool));
        if (_documents.TryGetValue(handle, out var doc) && doc.Fields.TryGetValue(field, out var value) && value is bool b)
        {
            return b ? 1 : 0;
        }
        return 0;
    }

    // ===== Schema =====

    public IntPtr zvec_schema_create(string name)
    {
        MethodCalls.Add($"{nameof(zvec_schema_create)}({name})");
        var handle = NextHandle();
        _schemas[handle] = new CollectionSchema(name);
        return handle;
    }

    public void zvec_schema_destroy(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_schema_destroy));
        _schemas.Remove(handle);
    }

    public NativeStatus zvec_schema_add_field(IntPtr handle, in NativeFieldDef fieldDef)
    {
        MethodCalls.Add(nameof(zvec_schema_add_field));
        var name = Marshal.PtrToStringUTF8(fieldDef.Name);
        if (_schemas.TryGetValue(handle, out var schema) && name != null)
        {
            // Store the field (we'd need to modify CollectionSchema to support mutation for this mock)
        }
        return Ok();
    }

    public NativeStatus zvec_schema_add_vector_field(IntPtr handle, in NativeFieldDef fieldDef)
    {
        MethodCalls.Add(nameof(zvec_schema_add_vector_field));
        return Ok();
    }

    public IntPtr zvec_collection_get_schema(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_collection_get_schema));
        var schemaHandle = NextHandle();
        if (_collections.TryGetValue(handle, out var collection))
        {
            _schemas[schemaHandle] = collection.Schema;
        }
        return schemaHandle;
    }

    public IntPtr zvec_schema_get_name(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_schema_get_name));
        if (_schemas.TryGetValue(handle, out var schema))
        {
            return Marshal.StringToHGlobalAnsi(schema.Name);
        }
        return Marshal.StringToHGlobalAnsi("mock_schema");
    }

    public nuint zvec_schema_get_field_count(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_schema_get_field_count));
        if (_schemas.TryGetValue(handle, out var schema))
        {
            return (nuint)schema.Fields.Count;
        }
        return 0;
    }

    public nuint zvec_schema_get_vector_count(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_schema_get_vector_count));
        if (_schemas.TryGetValue(handle, out var schema))
        {
            return (nuint)schema.Vectors.Count;
        }
        return 0;
    }

    public NativeFieldDef zvec_schema_get_field(IntPtr handle, nuint index)
    {
        MethodCalls.Add(nameof(zvec_schema_get_field));
        return default;
    }

    public NativeFieldDef zvec_schema_get_vector(IntPtr handle, nuint index)
    {
        MethodCalls.Add(nameof(zvec_schema_get_vector));
        return default;
    }

    // ===== Query =====

    public IntPtr zvec_query_create()
    {
        MethodCalls.Add(nameof(zvec_query_create));
        var handle = NextHandle();
        _queries[handle] = new MockQuery();
        return handle;
    }

    public void zvec_query_destroy(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_query_destroy));
        _queries.Remove(handle);
    }

    public void zvec_query_set_topk(IntPtr handle, int topk)
    {
        MethodCalls.Add($"{nameof(zvec_query_set_topk)}({topk})");
        if (_queries.TryGetValue(handle, out var query))
        {
            query.TopK = topk;
        }
    }

    public void zvec_query_set_field_name(IntPtr handle, string fieldName)
    {
        MethodCalls.Add($"{nameof(zvec_query_set_field_name)}({fieldName})");
        if (_queries.TryGetValue(handle, out var query))
        {
            query.FieldName = fieldName;
        }
    }

    public void zvec_query_set_vector(IntPtr handle, in float data, nuint len)
    {
        MethodCalls.Add($"{nameof(zvec_query_set_vector)}({len})");
        if (_queries.TryGetValue(handle, out var query))
        {
            unsafe
            {
                query.Vector = new float[(int)len];
                fixed (float* dst = query.Vector)
                fixed (float* src = &data)
                {
                    Buffer.MemoryCopy(src, dst, len * sizeof(float), len * sizeof(float));
                }
            }
        }
    }

    public void zvec_query_set_filter(IntPtr handle, string filter)
    {
        MethodCalls.Add($"{nameof(zvec_query_set_filter)}({filter})");
        if (_queries.TryGetValue(handle, out var query))
        {
            query.Filter = filter;
        }
    }

    public void zvec_query_set_include_vector(IntPtr handle, int include)
    {
        MethodCalls.Add(nameof(zvec_query_set_include_vector));
    }

    public void zvec_query_set_output_fields(IntPtr handle, IntPtr fields, nuint count)
    {
        MethodCalls.Add(nameof(zvec_query_set_output_fields));
    }

    public void zvec_query_set_ef_search(IntPtr handle, int ef)
    {
        MethodCalls.Add($"{nameof(zvec_query_set_ef_search)}({ef})");
    }

    public void zvec_query_set_n_probe(IntPtr handle, int nProbe)
    {
        MethodCalls.Add($"{nameof(zvec_query_set_n_probe)}({nProbe})");
    }

    // ===== Collection =====

    public NativeStatus zvec_collection_create_and_open(string path, IntPtr schema, in NativeCollectionOptions options, out IntPtr outHandle)
    {
        MethodCalls.Add($"{nameof(zvec_collection_create_and_open)}({path})");

        var error = MaybeForceError();
        if (!error.IsOk)
        {
            outHandle = IntPtr.Zero;
            return error;
        }

        outHandle = NextHandle();
        var schemaForCollection = _schemas.TryGetValue(schema, out var s) ? s : new CollectionSchema("mock");
        _collections[outHandle] = new MockCollection(path, schemaForCollection);
        return Ok();
    }

    public NativeStatus zvec_collection_open(string path, in NativeCollectionOptions options, out IntPtr outHandle)
    {
        MethodCalls.Add($"{nameof(zvec_collection_open)}({path})");

        var error = MaybeForceError();
        if (!error.IsOk)
        {
            outHandle = IntPtr.Zero;
            return error;
        }

        // Check if collection exists at path
        var existing = _collections.FirstOrDefault(c => c.Value.Path == path);
        if (existing.Value != null)
        {
            outHandle = existing.Key;
            return Ok();
        }

        outHandle = NextHandle();
        _collections[outHandle] = new MockCollection(path, new CollectionSchema("mock"));
        return Ok();
    }

    public void zvec_collection_destroy(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_collection_destroy));
        _collections.Remove(handle);
    }

    public NativeStatus zvec_collection_destroy_data(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_collection_destroy_data));
        _collections.Remove(handle);
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_flush(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_collection_flush));
        if (_collections.TryGetValue(handle, out var collection))
        {
            collection.FlushCount++;
        }
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_optimize(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_collection_optimize));
        if (_collections.TryGetValue(handle, out var collection))
        {
            collection.OptimizeCount++;
        }
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_create_index(IntPtr handle, string fieldName, in NativeFieldDef indexDef)
    {
        MethodCalls.Add($"{nameof(zvec_collection_create_index)}({fieldName})");
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_drop_index(IntPtr handle, string fieldName)
    {
        MethodCalls.Add($"{nameof(zvec_collection_drop_index)}({fieldName})");
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_insert(IntPtr handle, IntPtr[] docs, nuint count)
    {
        MethodCalls.Add($"{nameof(zvec_collection_insert)}({count})");
        if (_collections.TryGetValue(handle, out var collection))
        {
            for (int i = 0; i < (int)count; i++)
            {
                if (_documents.TryGetValue(docs[i], out var doc) && doc.Pk != null)
                {
                    collection.Documents[doc.Pk] = doc.Clone();
                }
            }
        }
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_upsert(IntPtr handle, IntPtr[] docs, nuint count)
    {
        MethodCalls.Add($"{nameof(zvec_collection_upsert)}({count})");
        if (_collections.TryGetValue(handle, out var collection))
        {
            for (int i = 0; i < (int)count; i++)
            {
                if (_documents.TryGetValue(docs[i], out var doc) && doc.Pk != null)
                {
                    collection.Documents[doc.Pk] = doc.Clone();
                }
            }
        }
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_update(IntPtr handle, IntPtr[] docs, nuint count)
    {
        MethodCalls.Add($"{nameof(zvec_collection_update)}({count})");
        if (_collections.TryGetValue(handle, out var collection))
        {
            for (int i = 0; i < (int)count; i++)
            {
                if (_documents.TryGetValue(docs[i], out var doc) && doc.Pk != null && collection.Documents.ContainsKey(doc.Pk))
                {
                    collection.Documents[doc.Pk] = doc.Clone();
                }
            }
        }
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_delete(IntPtr handle, string[] ids, nuint count)
    {
        MethodCalls.Add($"{nameof(zvec_collection_delete)}({count})");
        if (_collections.TryGetValue(handle, out var collection))
        {
            for (int i = 0; i < (int)count; i++)
            {
                collection.Documents.Remove(ids[i]);
            }
        }
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_delete_by_filter(IntPtr handle, string filter)
    {
        MethodCalls.Add($"{nameof(zvec_collection_delete_by_filter)}({filter})");
        return MaybeForceError();
    }

    public NativeStatus zvec_collection_query(IntPtr handle, IntPtr query, out IntPtr outResult)
    {
        MethodCalls.Add(nameof(zvec_collection_query));
        outResult = IntPtr.Zero;

        if (!_collections.TryGetValue(handle, out var collection) ||
            !_queries.TryGetValue(query, out var queryObj))
        {
            return Error(2, "Invalid handle");
        }

        var error = MaybeForceError();
        if (!error.IsOk)
        {
            return error;
        }

        // Create a mock result with all documents (in real implementation would do similarity search)
        outResult = NextHandle();
        var result = new MockResult();

        foreach (var doc in collection.Documents.Values)
        {
            result.Documents.Add(doc.Clone());
        }

        _results[outResult] = result;
        return Ok();
    }

    public NativeStatus zvec_collection_fetch(IntPtr handle, string[] ids, nuint count, out IntPtr outResult)
    {
        MethodCalls.Add($"{nameof(zvec_collection_fetch)}({count})");
        outResult = IntPtr.Zero;

        if (!_collections.TryGetValue(handle, out var collection))
        {
            return Error(2, "Invalid handle");
        }

        var error = MaybeForceError();
        if (!error.IsOk)
        {
            return error;
        }

        outResult = NextHandle();
        var result = new MockResult();

        for (int i = 0; i < (int)count; i++)
        {
            if (collection.Documents.TryGetValue(ids[i], out var doc))
            {
                result.Documents.Add(doc.Clone());
            }
        }

        _results[outResult] = result;
        return Ok();
    }

    public IntPtr zvec_collection_get_path(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_collection_get_path));
        if (_collections.TryGetValue(handle, out var collection))
        {
            return Marshal.StringToHGlobalAnsi(collection.Path);
        }
        return IntPtr.Zero;
    }

    // ===== Result =====

    public void zvec_result_destroy(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_result_destroy));
        _results.Remove(handle);
    }

    public nuint zvec_result_count(IntPtr handle)
    {
        MethodCalls.Add(nameof(zvec_result_count));
        if (_results.TryGetValue(handle, out var result))
        {
            return (nuint)result.Documents.Count;
        }
        return 0;
    }

    public IntPtr zvec_result_get_doc(IntPtr handle, nuint index)
    {
        MethodCalls.Add(nameof(zvec_result_get_doc));
        if (_results.TryGetValue(handle, out var result) && (int)index < result.Documents.Count)
        {
            var docHandle = NextHandle();
            _documents[docHandle] = result.Documents[(int)index];
            return docHandle;
        }
        return IntPtr.Zero;
    }
}

internal sealed class MockCollection
{
    public string Path { get; }
    public CollectionSchema Schema { get; }
    public Dictionary<string, MockDocument> Documents { get; } = new();
    public int FlushCount { get; set; }
    public int OptimizeCount { get; set; }

    public MockCollection(string path, CollectionSchema schema)
    {
        Path = path;
        Schema = schema;
    }
}

internal sealed class MockDocument
{
    public string? Pk { get; set; }
    public double Score { get; set; }
    public Dictionary<string, object?> Fields { get; } = new();
    public Dictionary<string, float[]> Vectors { get; } = new();

    public MockDocument Clone()
    {
        var clone = new MockDocument
        {
            Pk = Pk,
            Score = Score
        };
        foreach (var (key, value) in Fields)
        {
            clone.Fields[key] = value;
        }
        foreach (var (key, value) in Vectors)
        {
            clone.Vectors[key] = (float[])value.Clone();
        }
        return clone;
    }
}

internal sealed class MockQuery
{
    public int TopK { get; set; } = 10;
    public string? FieldName { get; set; }
    public float[]? Vector { get; set; }
    public string? Filter { get; set; }
}

internal sealed class MockResult
{
    public List<MockDocument> Documents { get; } = new();
}
