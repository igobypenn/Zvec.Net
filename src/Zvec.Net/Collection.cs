using System.Runtime.InteropServices;
using Zvec.Net.Index;
using Zvec.Net.Internal;
using Zvec.Net.Models;
using Zvec.Net.Native;
using Zvec.Net.Query;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net;

/// <summary>
/// Static factory methods and non-generic collection operations.
/// </summary>
public sealed class Collection : IVectorCollection
{
    private readonly INativeMethods _native;
    private IntPtr _handle;
    private readonly CollectionSchema _schema;
    private volatile bool _disposed;
    
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
    
    // ===== Generic Factory Methods =====
    
    /// <summary>
    /// Creates and opens a new typed vector collection.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="path">The filesystem path where the collection will be stored.</param>
    /// <param name="options">Optional collection configuration.</param>
    /// <returns>A new typed collection instance.</returns>
    public static Collection<T> CreateAndOpen<T>(string path, CollectionOptions? options = null) 
        where T : class, IDocument, new()
    {
        return Collection<T>.CreateAndOpen(path, options);
    }
    
    /// <summary>
    /// Opens an existing typed vector collection.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    /// <param name="path">The filesystem path where the collection is stored.</param>
    /// <param name="options">Optional collection configuration.</param>
    /// <returns>A typed collection instance.</returns>
    public static Collection<T> Open<T>(string path, CollectionOptions? options = null) 
        where T : class, IDocument, new()
    {
        return Collection<T>.Open(path, options);
    }
    
    // ===== Non-Generic Factory Methods =====
    
    /// <summary>
    /// Creates and opens a new collection with an explicit schema.
    /// </summary>
    /// <param name="path">The filesystem path where the collection will be stored.</param>
    /// <param name="schema">The collection schema.</param>
    /// <param name="options">Optional collection configuration.</param>
    /// <returns>A new collection instance.</returns>
    public static Collection CreateAndOpen(string path, CollectionSchema schema, CollectionOptions? options = null)
    {
        return CreateAndOpen(path, schema, options, NativeMethodsWrapper.Instance);
    }
    
    internal static Collection CreateAndOpen(string path, CollectionSchema schema, CollectionOptions? options, INativeMethods native)
    {
        ThrowHelper.ThrowIfNullOrEmpty(path, nameof(path));
        
        var nativeOptions = NativeCollectionOptions.Create(
            options?.SegmentMaxDocs ?? 1_000_000,
            options?.IndexBuildParallel ?? 0,
            options?.AutoFlush ?? true);
        
        var nativeSchemaPtr = CreateNativeSchema(schema, native);
        try
        {
            var status = native.zvec_collection_create_and_open(path, nativeSchemaPtr, in nativeOptions, out var handle);
            
            if (!status.IsOk)
            {
                throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? "Failed to create collection");
            }
            
            return new Collection(handle, schema, native);
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
    /// Opens an existing collection.
    /// </summary>
    /// <param name="path">The filesystem path where the collection is stored.</param>
    /// <param name="options">Optional collection configuration.</param>
    /// <returns>A collection instance.</returns>
    public static Collection Open(string path, CollectionOptions? options = null)
    {
        return Open(path, options, NativeMethodsWrapper.Instance);
    }
    
    internal static Collection Open(string path, CollectionOptions? options, INativeMethods native)
    {
        ThrowHelper.ThrowIfNullOrEmpty(path, nameof(path));
        
        var nativeOptions = NativeCollectionOptions.Create(
            options?.SegmentMaxDocs ?? 1_000_000,
            options?.IndexBuildParallel ?? 0,
            options?.AutoFlush ?? true);
        
        var status = native.zvec_collection_open(path, in nativeOptions, out var handle);
        
        if (!status.IsOk)
        {
            throw new ZvecException((StatusCode)status.Code, status.GetMessage() ?? "Failed to open collection");
        }
        
        var schemaPtr = native.zvec_collection_get_schema(handle);
        var schema = NativeSchemaHelper.ReadSchemaFromNative(native, schemaPtr);
        
        return new Collection(handle, schema, native);
    }
    
    /// <summary>
    /// Flushes pending writes to disk.
    /// </summary>
    public void Flush()
    {
        ThrowIfDisposed();
        _native.zvec_collection_flush(_handle).ThrowIfError("Flush");
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
