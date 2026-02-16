using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Zvec.Net.Attributes;
using Zvec.Net.Index;
using Zvec.Net.Internal;
using Zvec.Net.Models;
using Zvec.Net.Native;
using Zvec.Net.Query;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net;

/// <summary>
/// A typed vector collection for storing and querying documents with vector embeddings.
/// </summary>
/// <typeparam name="T">The document type, must implement IDocument and have a parameterless constructor.</typeparam>
public sealed class Collection<T> : IVectorCollection<T> where T : class, IDocument, new()
{
    private readonly INativeMethods _native;
    private IntPtr _handle;
    private readonly CollectionSchema _schema;
    private volatile bool _disposed;
    
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> VectorPropertyCache = new();
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> FieldPropertyCache = new();
    
    private Collection(IntPtr handle, CollectionSchema schema, INativeMethods native)
    {
        _handle = handle;
        _schema = schema;
        _native = native;
    }
    
    /// <summary>
    /// Gets the filesystem path where the collection is stored.
    /// </summary>
    public string Path
    {
        get
        {
            ThrowIfDisposed();
            var ptr = _native.zvec_collection_get_path(_handle);
            return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
        }
    }
    
    /// <summary>
    /// Gets the schema definition for this collection.
    /// </summary>
    public CollectionSchema Schema => _schema;
    
    /// <summary>
    /// Gets statistics about the collection.
    /// </summary>
    public CollectionStats Stats
    {
        get
        {
            ThrowIfDisposed();
            return new CollectionStats();
        }
    }
    
    // ===== Factory Methods =====
    
    /// <summary>
    /// Creates and opens a new vector collection at the specified path.
    /// </summary>
    /// <param name="path">The filesystem path where the collection will be stored.</param>
    /// <param name="options">Optional collection configuration.</param>
    /// <returns>A new collection instance ready for operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when path is null or empty.</exception>
    /// <exception cref="ZvecException">Thrown when collection creation fails.</exception>
    public static Collection<T> CreateAndOpen(string path, CollectionOptions? options = null)
    {
        return CreateAndOpen(path, options, NativeMethodsWrapper.Instance);
    }
    
    internal static Collection<T> CreateAndOpen(string path, CollectionOptions? options, INativeMethods native)
    {
        ThrowHelper.ThrowIfNullOrEmpty(path, nameof(path));
        
        var schema = SchemaGenerator.GenerateSchema<T>();
        var nativeOptions = CreateNativeOptions(options);
        
        var nativeSchemaPtr = CreateNativeSchema(schema, native);
        try
        {
            var status = native.zvec_collection_create_and_open(path, nativeSchemaPtr, in nativeOptions, out var handle);
            
            if (!status.IsOk)
            {
                throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? "Failed to create collection");
            }
            
            return new Collection<T>(handle, schema, native);
        }
        finally
        {
            if (nativeSchemaPtr != IntPtr.Zero)
            {
                native.zvec_schema_destroy(nativeSchemaPtr);
            }
        }
    }
    
    /// <summary>
    /// Opens an existing vector collection at the specified path.
    /// </summary>
    /// <param name="path">The filesystem path where the collection is stored.</param>
    /// <param name="options">Optional collection configuration.</param>
    /// <returns>A collection instance ready for operations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when path is null or empty.</exception>
    /// <exception cref="ZvecException">Thrown when opening the collection fails.</exception>
    public static Collection<T> Open(string path, CollectionOptions? options = null)
    {
        return Open(path, options, NativeMethodsWrapper.Instance);
    }
    
    internal static Collection<T> Open(string path, CollectionOptions? options, INativeMethods native)
    {
        ThrowHelper.ThrowIfNullOrEmpty(path, nameof(path));
        
        var nativeOptions = CreateNativeOptions(options);
        var status = native.zvec_collection_open(path, in nativeOptions, out var handle);
        
        if (!status.IsOk)
        {
            throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? "Failed to open collection");
        }
        
        var schemaPtr = native.zvec_collection_get_schema(handle);
        var schema = NativeSchemaHelper.ReadSchemaFromNative(native, schemaPtr);
        
        return new Collection<T>(handle, schema, native);
    }
    
    // ===== Insert =====
    
    /// <summary>
    /// Inserts one or more documents into the collection.
    /// </summary>
    /// <param name="documents">The documents to insert.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Insert(params T[] documents) => Insert(documents.AsEnumerable());
    
    /// <summary>
    /// Inserts documents into the collection.
    /// </summary>
    /// <param name="documents">The documents to insert.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Insert(IEnumerable<T> documents)
    {
        ThrowIfDisposed();
        var docList = documents.ToList();
        if (docList.Count == 0) return Status.Ok;
        
        return ExecuteDocumentOperation(docList, (handle, docs, count) => 
            _native.zvec_collection_insert(handle, docs, count));
    }
    
    /// <summary>
    /// Asynchronously inserts documents into the collection.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="documents">The documents to insert.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Status> InsertAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Insert(documents), cancellationToken);
    }
    
    // ===== Upsert =====
    
    /// <summary>
    /// Upserts one or more documents into the collection (insert or update).
    /// </summary>
    /// <param name="documents">The documents to upsert.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Upsert(params T[] documents) => Upsert(documents.AsEnumerable());
    
    /// <summary>
    /// Upserts documents into the collection (insert or update).
    /// </summary>
    /// <param name="documents">The documents to upsert.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Upsert(IEnumerable<T> documents)
    {
        ThrowIfDisposed();
        var docList = documents.ToList();
        if (docList.Count == 0) return Status.Ok;
        
        return ExecuteDocumentOperation(docList, (handle, docs, count) => 
            _native.zvec_collection_upsert(handle, docs, count));
    }
    
    /// <summary>
    /// Asynchronously upserts documents into the collection.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="documents">The documents to upsert.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Status> UpsertAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Upsert(documents), cancellationToken);
    }
    
    // ===== Update =====
    
    /// <summary>
    /// Updates one or more existing documents in the collection.
    /// </summary>
    /// <param name="documents">The documents to update.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Update(params T[] documents) => Update(documents.AsEnumerable());
    
    /// <summary>
    /// Updates existing documents in the collection.
    /// </summary>
    /// <param name="documents">The documents to update.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Update(IEnumerable<T> documents)
    {
        ThrowIfDisposed();
        var docList = documents.ToList();
        if (docList.Count == 0) return Status.Ok;
        
        return ExecuteDocumentOperation(docList, (handle, docs, count) => 
            _native.zvec_collection_update(handle, docs, count));
    }
    
    /// <summary>
    /// Asynchronously updates existing documents in the collection.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="documents">The documents to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Status> UpdateAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Update(documents), cancellationToken);
    }
    
    // ===== Delete =====
    
    /// <summary>
    /// Deletes documents by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of documents to delete.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Delete(params string[] ids) => Delete(ids.AsEnumerable());
    
    /// <summary>
    /// Deletes documents by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of documents to delete.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status Delete(IEnumerable<string> ids)
    {
        ThrowIfDisposed();
        var idList = ids.ToList();
        if (idList.Count == 0) return Status.Ok;
        
        var idArray = idList.ToArray();
        var status = _native.zvec_collection_delete(_handle, idArray, (nuint)idArray.Length);
        
        return status.ToStatus();
    }
    
    /// <summary>
    /// Asynchronously deletes documents by their IDs.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="ids">The IDs of documents to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Status> DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Delete(ids), cancellationToken);
    }
    
    /// <summary>
    /// Deletes documents matching a filter expression.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>A status indicating success or failure.</returns>
    public Status DeleteByFilter(string filter)
    {
        ThrowIfDisposed();
        var status = _native.zvec_collection_delete_by_filter(_handle, filter);
        return status.ToStatus();
    }
    
    /// <summary>
    /// Asynchronously deletes documents matching a filter expression.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="filter">The filter expression.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<Status> DeleteByFilterAsync(string filter, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DeleteByFilter(filter), cancellationToken);
    }
    
    // ===== Query =====
    
    /// <summary>
    /// Creates a new query builder for vector similarity search.
    /// </summary>
    /// <returns>A query builder instance.</returns>
    public IVectorQueryBuilder<T> Query()
    {
        ThrowIfDisposed();
        return new VectorQueryBuilder<T>(this);
    }
    
    /// <summary>
    /// Executes a vector similarity query.
    /// </summary>
    /// <param name="vectorQuery">The vector query to execute.</param>
    /// <param name="options">Optional query options.</param>
    /// <returns>A list of matching documents.</returns>
    public IReadOnlyList<T> Query(VectorQuery vectorQuery, QueryOptions? options = null)
    {
        ThrowIfDisposed();
        options ??= QueryOptions.Default;
        
        vectorQuery.Validate();
        
        return ExecuteQuery(new[] { vectorQuery }, options);
    }
    
    /// <summary>
    /// Asynchronously executes a vector similarity query.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="vectorQuery">The vector query to execute.</param>
    /// <param name="options">Optional query options.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<IReadOnlyList<T>> QueryAsync(VectorQuery vectorQuery, QueryOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Query(vectorQuery, options), cancellationToken);
    }
    
    internal IReadOnlyList<T> ExecuteQuery(IReadOnlyList<VectorQuery> vectorQueries, QueryOptions options)
    {
        ThrowIfDisposed();
        
        if (vectorQueries.Count == 0)
        {
            return new List<T>();
        }
        
        var firstQuery = vectorQueries[0];
        
        var queryPtr = _native.zvec_query_create();
        if (queryPtr == IntPtr.Zero)
        {
            throw new ZvecException(StatusCode.InternalError, "Failed to create query");
        }
        
        try
        {
            BuildNativeQuery(queryPtr, firstQuery, options);
            return ExecuteNativeQuery(queryPtr);
        }
        finally
        {
            _native.zvec_query_destroy(queryPtr);
        }
    }
    
    private void BuildNativeQuery(IntPtr queryPtr, VectorQuery vectorQuery, QueryOptions options)
    {
        _native.zvec_query_set_topk(queryPtr, options.TopK);
        _native.zvec_query_set_field_name(queryPtr, vectorQuery.FieldName);
        
        SetQueryVector(queryPtr, vectorQuery.Vector);
        
        if (!string.IsNullOrEmpty(options.Filter))
        {
            _native.zvec_query_set_filter(queryPtr, options.Filter);
        }
        
        _native.zvec_query_set_include_vector(queryPtr, options.IncludeVectors ? 1 : 0);
        
        SetQueryOutputFields(queryPtr, options.OutputFields);
        SetQueryParamOptions(queryPtr, vectorQuery.Param);
    }
    
    private void SetQueryVector(IntPtr queryPtr, float[]? vector)
    {
        if (vector == null || vector.Length == 0) return;
        
        var handle = GCHandle.Alloc(vector, GCHandleType.Pinned);
        try
        {
            unsafe
            {
                var ptr = (float*)handle.AddrOfPinnedObject();
                _native.zvec_query_set_vector(queryPtr, in *ptr, (nuint)vector.Length);
            }
        }
        finally
        {
            handle.Free();
        }
    }
    
    private void SetQueryOutputFields(IntPtr queryPtr, IReadOnlyList<string>? outputFields)
    {
        if (outputFields == null || outputFields.Count == 0) return;
        
        var fieldPtrs = new IntPtr[outputFields.Count];
        try
        {
            for (int i = 0; i < outputFields.Count; i++)
            {
                fieldPtrs[i] = Marshal.StringToHGlobalAnsi(outputFields[i]);
            }
            
            unsafe
            {
                fixed (IntPtr* ptr = fieldPtrs)
                {
                    _native.zvec_query_set_output_fields(queryPtr, (IntPtr)ptr, (nuint)fieldPtrs.Length);
                }
            }
        }
        finally
        {
            foreach (var ptr in fieldPtrs)
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
    }
    
    private void SetQueryParamOptions(IntPtr queryPtr, IndexQueryParam? param)
    {
        switch (param)
        {
            case HnswQueryParam hnswParam:
                _native.zvec_query_set_ef_search(queryPtr, hnswParam.Ef);
                break;
            case IvfQueryParam ivfParam:
                _native.zvec_query_set_n_probe(queryPtr, ivfParam.NProbe);
                break;
        }
    }
    
    private IReadOnlyList<T> ExecuteNativeQuery(IntPtr queryPtr)
    {
        var status = _native.zvec_collection_query(_handle, queryPtr, out var resultPtr);
        
        if (!status.IsOk)
        {
            throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? "Query failed");
        }
        
        try
        {
            return ReadResults(resultPtr);
        }
        finally
        {
            _native.zvec_result_destroy(resultPtr);
        }
    }
    
    // ===== Fetch =====
    
    /// <summary>
    /// Fetches documents by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of documents to fetch.</param>
    /// <returns>A dictionary mapping IDs to documents.</returns>
    public IReadOnlyDictionary<string, T> Fetch(params string[] ids) => Fetch(ids.AsEnumerable());
    
    /// <summary>
    /// Fetches documents by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of documents to fetch.</param>
    /// <returns>A dictionary mapping IDs to documents.</returns>
    public IReadOnlyDictionary<string, T> Fetch(IEnumerable<string> ids)
    {
        ThrowIfDisposed();
        var idList = ids.ToList();
        if (idList.Count == 0) return new Dictionary<string, T>();
        
        var idArray = idList.ToArray();
        var status = _native.zvec_collection_fetch(_handle, idArray, (nuint)idArray.Length, out var resultPtr);
        
        if (!status.IsOk)
        {
            throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? "Fetch failed");
        }
        
        try
        {
            var results = ReadResults(resultPtr);
            return results.ToDictionary(d => d.Id);
        }
        finally
        {
            _native.zvec_result_destroy(resultPtr);
        }
    }
    
    /// <summary>
    /// Asynchronously fetches documents by their IDs.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="ids">The IDs of documents to fetch.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task<IReadOnlyDictionary<string, T>> FetchAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => Fetch(ids), cancellationToken);
    }
    
    // ===== DDL =====
    
    /// <summary>
    /// Flushes pending writes to disk.
    /// </summary>
    public void Flush()
    {
        ThrowIfDisposed();
        _native.zvec_collection_flush(_handle).ThrowIfError("Flush");
    }
    
    /// <summary>
    /// Asynchronously flushes pending writes to disk.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(Flush, cancellationToken);
    }
    
    /// <summary>
    /// Creates an index on a vector field for faster similarity search.
    /// </summary>
    /// <param name="fieldName">The name of the vector field.</param>
    /// <param name="indexParams">The index parameters.</param>
    public void CreateIndex(string fieldName, IndexParams indexParams)
    {
        ThrowIfDisposed();
        var nativeFieldDef = NativeFieldDef.Create(fieldName, (int)indexParams.Type);
        _native.zvec_collection_create_index(_handle, fieldName, in nativeFieldDef).ThrowIfError("CreateIndex");
    }
    
    /// <summary>
    /// Asynchronously creates an index on a vector field.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="fieldName">The name of the vector field.</param>
    /// <param name="indexParams">The index parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task CreateIndexAsync(string fieldName, IndexParams indexParams, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CreateIndex(fieldName, indexParams), cancellationToken);
    }
    
    /// <summary>
    /// Drops an index from a field.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    public void DropIndex(string fieldName)
    {
        ThrowIfDisposed();
        _native.zvec_collection_drop_index(_handle, fieldName).ThrowIfError("DropIndex");
    }
    
    /// <summary>
    /// Asynchronously drops an index from a field.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DropIndexAsync(string fieldName, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DropIndex(fieldName), cancellationToken);
    }
    
    /// <summary>
    /// Optimizes the collection for better query performance.
    /// </summary>
    public void Optimize()
    {
        ThrowIfDisposed();
        _native.zvec_collection_optimize(_handle).ThrowIfError("Optimize");
    }
    
    /// <summary>
    /// Asynchronously optimizes the collection.
    /// </summary>
    /// <remarks>
    /// This method wraps the synchronous operation in Task.Run. The underlying native library
    /// does not provide true async I/O. Use this for offloading to background threads, not for
    /// improving I/O scalability.
    /// </remarks>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OptimizeAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(Optimize, cancellationToken);
    }
    
    /// <summary>
    /// Destroys the collection and deletes all data from disk.
    /// </summary>
    public void Destroy()
    {
        ThrowIfDisposed();
        _native.zvec_collection_destroy_data(_handle).ThrowIfError("Destroy");
    }
    
    /// <summary>
    /// Disposes the collection, releasing the native handle.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_handle != IntPtr.Zero)
        {
            _native.zvec_collection_destroy(_handle);
            _handle = IntPtr.Zero;
        }
    }
    
    private void ThrowIfDisposed()
    {
        if (_disposed) throw ThrowHelper.CollectionDisposed();
    }
    
    // ===== Helper Methods =====
    
    private static NativeCollectionOptions CreateNativeOptions(CollectionOptions? options)
    {
        return NativeCollectionOptions.Create(
            options?.SegmentMaxDocs ?? 1_000_000,
            options?.IndexBuildParallel ?? 0,
            options?.AutoFlush ?? true);
    }
    
    private Status ExecuteDocumentOperation(IReadOnlyList<T> documents, Func<IntPtr, IntPtr[], nuint, NativeStatus> operation)
    {
        var handles = CreateNativeDocs(documents);
        try
        {
            var status = operation(_handle, handles, (nuint)handles.Length);
            return status.ToStatus();
        }
        finally
        {
            foreach (var h in handles)
            {
                _native.zvec_doc_destroy(h);
            }
        }
    }
    
    private IReadOnlyList<T> ReadResults(IntPtr resultPtr)
    {
        var count = (int)_native.zvec_result_count(resultPtr);
        var results = new List<T>(count);
        
        for (int i = 0; i < count; i++)
        {
            var docPtr = _native.zvec_result_get_doc(resultPtr, (nuint)i);
            if (docPtr != IntPtr.Zero)
            {
                var doc = ReadDocument(docPtr);
                results.Add(doc);
            }
        }
        
        return results;
    }
    
    private T ReadDocument(IntPtr docPtr)
    {
        var doc = new T();
        
        var pkPtr = _native.zvec_doc_get_pk(docPtr);
        if (pkPtr != IntPtr.Zero)
        {
            doc.Id = Marshal.PtrToStringUTF8(pkPtr) ?? string.Empty;
        }
        
        var score = _native.zvec_doc_get_score(docPtr);
        
        if (typeof(DocumentBase).IsAssignableFrom(typeof(T)))
        {
            var scoreProp = typeof(T).GetProperty(nameof(DocumentBase.Score));
            scoreProp?.SetValue(doc, score);
        }
        
        var fieldProps = GetFieldProperties();
        foreach (var (name, prop) in fieldProps)
        {
            if (_native.zvec_doc_has_field(docPtr, name) != 0)
            {
                var value = ReadFieldValue(docPtr, name, prop.PropertyType);
                if (value != null)
                {
                    prop.SetValue(doc, value);
                }
            }
        }
        
        return doc;
    }
    
    private object? ReadFieldValue(IntPtr docPtr, string fieldName, Type propertyType)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        
        if (underlying == typeof(string))
        {
            var ptr = _native.zvec_doc_get_string(docPtr, fieldName);
            return ptr != IntPtr.Zero ? Marshal.PtrToStringUTF8(ptr) : null;
        }
        
        if (underlying == typeof(int))
        {
            return (int)_native.zvec_doc_get_int64(docPtr, fieldName);
        }
        
        if (underlying == typeof(long))
        {
            return _native.zvec_doc_get_int64(docPtr, fieldName);
        }
        
        if (underlying == typeof(float))
        {
            return (float)_native.zvec_doc_get_double(docPtr, fieldName);
        }
        
        if (underlying == typeof(double))
        {
            return _native.zvec_doc_get_double(docPtr, fieldName);
        }
        
        if (underlying == typeof(bool))
        {
            return _native.zvec_doc_get_bool(docPtr, fieldName) != 0;
        }
        
        return null;
    }
    
    private IntPtr[] CreateNativeDocs(IReadOnlyList<T> documents)
    {
        var handles = new IntPtr[documents.Count];
        var fieldProps = GetFieldProperties();
        var vectorProps = GetVectorProperties();
        
        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];
            var handle = _native.zvec_doc_create();
            
            _native.zvec_doc_set_pk(handle, doc.Id);
            
            foreach (var (name, prop) in fieldProps)
            {
                var value = prop.GetValue(doc);
                SetFieldValue(handle, name, value, prop.PropertyType);
            }
            
            foreach (var (name, prop) in vectorProps)
            {
                var value = prop.GetValue(doc);
                SetVectorValue(handle, name, value, prop);
            }
            
            handles[i] = handle;
        }
        
        return handles;
    }
    
    private void SetFieldValue(IntPtr docPtr, string fieldName, object? value, Type type)
    {
        if (value == null)
        {
            _native.zvec_doc_set_null(docPtr, fieldName);
            return;
        }
        
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        
        if (underlying == typeof(string))
        {
            _native.zvec_doc_set_string(docPtr, fieldName, (string)value);
        }
        else if (underlying == typeof(int))
        {
            _native.zvec_doc_set_int32(docPtr, fieldName, (int)value);
        }
        else if (underlying == typeof(long))
        {
            _native.zvec_doc_set_int64(docPtr, fieldName, (long)value);
        }
        else if (underlying == typeof(float))
        {
            _native.zvec_doc_set_float(docPtr, fieldName, (float)value);
        }
        else if (underlying == typeof(double))
        {
            _native.zvec_doc_set_double(docPtr, fieldName, (double)value);
        }
        else if (underlying == typeof(bool))
        {
            _native.zvec_doc_set_bool(docPtr, fieldName, (bool)value ? 1 : 0);
        }
    }
    
    private void SetVectorValue(IntPtr docPtr, string fieldName, object? value, PropertyInfo prop)
    {
        if (value == null) return;
        
        var attr = prop.GetCustomAttribute<VectorFieldAttribute>();
        if (attr == null) return;
        
        if (attr.Precision == VectorPrecision.Float32 && value is float[] f32Arr && f32Arr.Length > 0)
        {
            unsafe
            {
                fixed (float* ptr = f32Arr)
                {
                    _native.zvec_doc_set_vector_f32(docPtr, fieldName, in *ptr, (nuint)f32Arr.Length);
                }
            }
        }
    }
    
    private Dictionary<string, PropertyInfo> GetFieldProperties()
    {
        return FieldPropertyCache.GetOrAdd(typeof(T), t =>
        {
            var result = new Dictionary<string, PropertyInfo>();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<KeyAttribute>() != null) continue;
                if (prop.Name == nameof(IDocument.Id)) continue;
                if (prop.Name == nameof(DocumentBase.Score)) continue;
                if (prop.GetCustomAttribute<VectorFieldAttribute>() != null) continue;
                
                result[prop.Name] = prop;
            }
            return result;
        });
    }
    
    private Dictionary<string, PropertyInfo> GetVectorProperties()
    {
        return VectorPropertyCache.GetOrAdd(typeof(T), t =>
        {
            var result = new Dictionary<string, PropertyInfo>();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<VectorFieldAttribute>() != null)
                {
                    result[prop.Name] = prop;
                }
            }
            return result;
        });
    }
    
    private static IntPtr CreateNativeSchema(CollectionSchema schema, INativeMethods native)
    {
        var schemaPtr = native.zvec_schema_create(schema.Name);
        if (schemaPtr == IntPtr.Zero)
        {
            throw new ZvecException(StatusCode.InternalError, "Failed to create native schema");
        }
        
        try
        {
            foreach (var field in schema.Fields)
            {
                var fieldDef = NativeFieldDef.FromFieldSchema(field);
                try
                {
                    var status = native.zvec_schema_add_field(schemaPtr, in fieldDef);
                    if (!status.IsOk)
                    {
                        throw new ZvecException((StatusCode)status.Code, 
                            $"Failed to add field '{field.Name}': {status.GetMessage()}");
                    }
                }
                finally
                {
                    fieldDef.Free();
                }
            }
            
            foreach (var vector in schema.Vectors)
            {
                var fieldDef = NativeFieldDef.FromVectorSchema(vector);
                try
                {
                    var status = native.zvec_schema_add_vector_field(schemaPtr, in fieldDef);
                    if (!status.IsOk)
                    {
                        throw new ZvecException((StatusCode)status.Code,
                            $"Failed to add vector field '{vector.Name}': {status.GetMessage()}");
                    }
                }
                finally
                {
                    fieldDef.Free();
                }
            }
            
            return schemaPtr;
        }
        catch
        {
            native.zvec_schema_destroy(schemaPtr);
            throw;
        }
    }
}
