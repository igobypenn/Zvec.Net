using System.Linq.Expressions;
using Zvec.Net.Index;
using Zvec.Net.Internal;
using Zvec.Net.Models;
using Zvec.Net.Native;
using Zvec.Net.Query;
using Zvec.Net.Schema;
using Zvec.Net.Types;

namespace Zvec.Net;

/// <summary>
/// Internal implementation of the vector query builder.
/// </summary>
internal sealed class VectorQueryBuilder<T> : IVectorQueryBuilder<T> where T : class, IDocument, new()
{
    private readonly Collection<T> _collection;
    private readonly List<VectorQuery> _vectorQueries = new();
    private readonly List<string> _outputFields = new();
    private readonly FilterExpressionTranslator<T> _filterTranslator = new();

    private int _topK = 10;
    private string? _filter;
    private bool _includeVectors = false;
    private IReRanker? _reranker;

    /// <summary>
    /// Initializes a new query builder.
    /// </summary>
    /// <param name="collection">The collection to query.</param>
    public VectorQueryBuilder(Collection<T> collection)
    {
        _collection = collection;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> VectorNearest<TVector>(
        Expression<Func<T, TVector?>> fieldSelector,
        TVector vector,
        double weight = 1.0,
        IndexQueryParam? param = null) where TVector : struct
    {
        var fieldName = GetFieldName(fieldSelector);

        VectorQuery query;

        // Handle different vector types
        var vectorType = typeof(TVector);

        if (vectorType == typeof(float[]))
        {
            var floatArr = (float[])(object)vector;
            query = VectorQuery.ByVector(fieldName, floatArr, weight, param);
        }
        else if (vectorType == typeof(double[]))
        {
            var doubleArr = (double[])(object)vector;
            var asFloat = doubleArr.Select(d => (float)d).ToArray();
            query = VectorQuery.ByVector(fieldName, asFloat, weight, param);
        }
        else if (vectorType == typeof(SparseVector))
        {
            var sparse = (SparseVector)(object)vector;
            query = VectorQuery.BySparseVector(fieldName, sparse, weight, param);
        }
        else
        {
            throw new NotSupportedException($"Vector type {vectorType.Name} is not supported");
        }

        _vectorQueries.Add(query);
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> VectorNearestById<TVector>(
        Expression<Func<T, TVector?>> fieldSelector,
        string documentId,
        double weight = 1.0,
        IndexQueryParam? param = null) where TVector : struct
    {
        var fieldName = GetFieldName(fieldSelector);
        var query = VectorQuery.ById(fieldName, documentId, weight, param);
        _vectorQueries.Add(query);
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _filter = _filterTranslator.Translate(predicate);
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> Where(string filter)
    {
        _filter = filter;
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> TopK(int k)
    {
        if (k <= 0) throw new ArgumentException("TopK must be positive", nameof(k));
        _topK = k;
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> IncludeVectors(bool include = true)
    {
        _includeVectors = include;
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> IncludeFields(params Expression<Func<T, object?>>[] fields)
    {
        foreach (var field in fields)
        {
            var name = GetFieldName(field);
            _outputFields.Add(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> IncludeFields(params string[] fieldNames)
    {
        _outputFields.AddRange(fieldNames);
        return this;
    }

    /// <inheritdoc/>
    public IVectorQueryBuilder<T> Reranker(IReRanker reRanker)
    {
        _reranker = reRanker;
        return this;
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> Execute()
    {
        ValidateQuery();

        var options = new QueryOptions
        {
            TopK = _topK,
            Filter = _filter,
            IncludeVectors = _includeVectors,
            OutputFields = _outputFields.Count > 0 ? _outputFields : null,
            ReRanker = _reranker
        };

        return _collection.ExecuteQuery(_vectorQueries, options);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(Execute, cancellationToken).ConfigureAwait(false);
    }

    private void ValidateQuery()
    {
        if (_vectorQueries.Count == 0)
        {
            var vectorNames = _collection.Schema.Vectors.Select(v => v.Name).ToList();
            throw ThrowHelper.NoVectorQuerySpecified(vectorNames);
        }

        foreach (var query in _vectorQueries)
        {
            query.Validate();
        }
    }

    private static string GetFieldName<TField>(Expression<Func<T, TField>> selector)
    {
        if (selector.Body is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }

        if (selector.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression innerMember)
        {
            return innerMember.Member.Name;
        }

        throw new ArgumentException(
            $"Expression '{selector}' must be a simple member access expression like 'd => d.FieldName'",
            nameof(selector));
    }
}
