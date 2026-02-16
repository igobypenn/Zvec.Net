using Zvec.Net.Attributes;

namespace Zvec.Net.Models;

/// <summary>
/// Represents a sparse vector where only non-zero values are stored.
/// </summary>
/// <remarks>
/// Sparse vectors are useful for high-dimensional data where most values are zero,
/// such as text embeddings using TF-IDF or BM25.
/// </remarks>
public sealed class SparseVector
{
    private readonly uint[] _indices;
    private readonly float[] _values;

    /// <summary>
    /// Initializes a new sparse vector with the specified indices and values.
    /// </summary>
    /// <param name="indices">The indices of non-zero elements. Must be sorted in ascending order.</param>
    /// <param name="values">The values at the corresponding indices.</param>
    /// <exception cref="ArgumentException">Thrown when arrays have different lengths.</exception>
    public SparseVector(uint[] indices, float[] values)
    {
        if (indices.Length != values.Length)
        {
            throw new ArgumentException("Indices and values must have the same length");
        }
        _indices = indices;
        _values = values;
    }

    private SparseVector()
    {
        _indices = Array.Empty<uint>();
        _values = Array.Empty<float>();
    }

    /// <summary>
    /// Gets the number of non-zero elements in this vector.
    /// </summary>
    public int Count => _indices.Length;

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index">The index to look up.</param>
    /// <returns>The value at the index, or 0 if the index is not present.</returns>
    public float this[uint index]
    {
        get
        {
            var idx = Array.BinarySearch(_indices, index);
            return idx >= 0 ? _values[idx] : 0f;
        }
    }

    /// <summary>
    /// Gets the indices of non-zero elements.
    /// </summary>
    public IReadOnlyList<uint> Indices => _indices;

    /// <summary>
    /// Gets the values at the non-zero indices.
    /// </summary>
    public IReadOnlyList<float> Values => _values;

    /// <summary>
    /// Gets an empty sparse vector.
    /// </summary>
    public static SparseVector Empty { get; } = new();

    /// <summary>
    /// Creates a sparse vector from a dictionary of index-value pairs.
    /// </summary>
    /// <param name="values">A dictionary mapping indices to values.</param>
    /// <returns>A new <see cref="SparseVector"/> instance.</returns>
    public static SparseVector FromDictionary(IReadOnlyDictionary<uint, float> values)
    {
        var indices = values.Keys.OrderBy(k => k).ToArray();
        var vals = indices.Select(i => values[i]).ToArray();
        return new SparseVector(indices, vals);
    }

    /// <summary>
    /// Converts this sparse vector to a dictionary.
    /// </summary>
    /// <returns>A dictionary containing all non-zero index-value pairs.</returns>
    public Dictionary<uint, float> ToDictionary()
    {
        var dict = new Dictionary<uint, float>();
        for (int i = 0; i < _indices.Length; i++)
        {
            dict[_indices[i]] = _values[i];
        }
        return dict;
    }

    /// <summary>
    /// Determines whether this sparse vector equals another.
    /// </summary>
    /// <param name="other">The other sparse vector to compare.</param>
    /// <returns><c>true</c> if the vectors have identical indices and values.</returns>
    public bool Equals(SparseVector? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (_indices.Length != other._indices.Length) return false;

        for (int i = 0; i < _indices.Length; i++)
        {
            if (_indices[i] != other._indices[i] ||
                Math.Abs(_values[i] - other._values[i]) > float.Epsilon)
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as SparseVector);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var idx in _indices) hash.Add(idx);
        foreach (var val in _values) hash.Add(val);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString() => $"SparseVector[Count={Count}]";
}
