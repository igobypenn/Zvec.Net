using System.Linq.Expressions;
using Zvec.Net.Index;
using Zvec.Net.Models;

namespace Zvec.Net.Query;

/// <summary>
/// Fluent builder interface for constructing vector similarity queries.
/// </summary>
/// <typeparam name="T">The document type.</typeparam>
/// <remarks>
/// Use this interface to build complex queries with filters, multiple vectors, and reranking.
/// </remarks>
public interface IVectorQueryBuilder<T> where T : IDocument
{
    /// <summary>
    /// Adds a vector query for the specified field.
    /// </summary>
    /// <typeparam name="TVector">The vector type (float[], SparseVector, etc.).</typeparam>
    /// <param name="fieldSelector">Expression selecting the vector field.</param>
    /// <param name="vector">The query vector.</param>
    /// <param name="weight">Weight for multi-vector queries.</param>
    /// <param name="param">Optional index-specific parameters.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> VectorNearest<TVector>(
        Expression<Func<T, TVector?>> fieldSelector,
        TVector vector,
        double weight = 1.0,
        IndexQueryParam? param = null) where TVector : struct;
    
    /// <summary>
    /// Adds a vector query using a document's vector.
    /// </summary>
    /// <typeparam name="TVector">The vector type.</typeparam>
    /// <param name="fieldSelector">Expression selecting the vector field.</param>
    /// <param name="documentId">The ID of the document whose vector to use.</param>
    /// <param name="weight">Weight for multi-vector queries.</param>
    /// <param name="param">Optional index-specific parameters.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> VectorNearestById<TVector>(
        Expression<Func<T, TVector?>> fieldSelector,
        string documentId,
        double weight = 1.0,
        IndexQueryParam? param = null) where TVector : struct;
    
    /// <summary>
    /// Adds a filter expression to the query.
    /// </summary>
    /// <param name="predicate">A predicate expression.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> Where(Expression<Func<T, bool>> predicate);
    
    /// <summary>
    /// Adds a filter string to the query.
    /// </summary>
    /// <param name="filter">A filter expression string.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> Where(string filter);
    
    /// <summary>
    /// Sets the maximum number of results.
    /// </summary>
    /// <param name="k">The maximum number of results.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> TopK(int k);
    
    /// <summary>
    /// Sets whether to include vectors in results.
    /// </summary>
    /// <param name="include">Whether to include vectors.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> IncludeVectors(bool include = true);
    
    /// <summary>
    /// Specifies which fields to include in results.
    /// </summary>
    /// <param name="fields">Expressions selecting the fields.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> IncludeFields(params Expression<Func<T, object?>>[] fields);
    
    /// <summary>
    /// Specifies which fields to include in results by name.
    /// </summary>
    /// <param name="fieldNames">Names of fields to include.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> IncludeFields(params string[] fieldNames);
    
    /// <summary>
    /// Sets the reranker for multi-vector queries.
    /// </summary>
    /// <param name="reRanker">The reranker to use.</param>
    /// <returns>This builder for method chaining.</returns>
    IVectorQueryBuilder<T> Reranker(IReRanker reRanker);
    
    /// <summary>
    /// Executes the query synchronously.
    /// </summary>
    /// <returns>The matching documents.</returns>
    IReadOnlyList<T> Execute();
    
    /// <summary>
    /// Executes the query asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching documents.</returns>
    Task<IReadOnlyList<T>> ExecuteAsync(CancellationToken cancellationToken = default);
}
