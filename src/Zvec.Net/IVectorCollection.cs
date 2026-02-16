using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Query;
using Zvec.Net.Schema;

namespace Zvec.Net;

public interface IVectorCollection : IDisposable
{
    string Path { get; }
    CollectionSchema Schema { get; }
    CollectionStats Stats { get; }
}

public interface IVectorCollection<T> : IVectorCollection where T : IDocument
{
    Status Insert(params T[] documents);
    Status Insert(IEnumerable<T> documents);
    Task<Status> InsertAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
    
    Status Upsert(params T[] documents);
    Status Upsert(IEnumerable<T> documents);
    Task<Status> UpsertAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
    
    Status Update(params T[] documents);
    Status Update(IEnumerable<T> documents);
    Task<Status> UpdateAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
    
    Status Delete(params string[] ids);
    Status Delete(IEnumerable<string> ids);
    Task<Status> DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
    
    Status DeleteByFilter(string filter);
    Task<Status> DeleteByFilterAsync(string filter, CancellationToken cancellationToken = default);
    
    IVectorQueryBuilder<T> Query();
    
    IReadOnlyList<T> Query(VectorQuery vectorQuery, QueryOptions? options = null);
    Task<IReadOnlyList<T>> QueryAsync(VectorQuery vectorQuery, QueryOptions? options = null, CancellationToken cancellationToken = default);
    
    IReadOnlyDictionary<string, T> Fetch(params string[] ids);
    IReadOnlyDictionary<string, T> Fetch(IEnumerable<string> ids);
    Task<IReadOnlyDictionary<string, T>> FetchAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
    
    void Flush();
    Task FlushAsync(CancellationToken cancellationToken = default);
    
    void CreateIndex(string fieldName, IndexParams indexParams);
    Task CreateIndexAsync(string fieldName, IndexParams indexParams, CancellationToken cancellationToken = default);
    
    void DropIndex(string fieldName);
    Task DropIndexAsync(string fieldName, CancellationToken cancellationToken = default);
    
    void Optimize();
    Task OptimizeAsync(CancellationToken cancellationToken = default);
    
    void Destroy();
}
