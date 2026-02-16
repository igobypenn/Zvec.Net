namespace Zvec.Net.Models;

/// <summary>
/// Represents a dynamic document that can hold arbitrary fields and vectors.
/// </summary>
/// <remarks>
/// Use this class when you need to work with documents whose schema is not known at compile time.
/// For strongly-typed documents, derive from <see cref="DocumentBase"/> instead.
/// </remarks>
public sealed class Doc
{
    /// <summary>
    /// Gets or sets the unique identifier for this document.
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the similarity score when this document is returned from a search.
    /// </summary>
    public double? Score { get; init; }
    
    /// <summary>
    /// Gets the scalar fields in this document.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Fields { get; init; } = new Dictionary<string, object?>();
    
    /// <summary>
    /// Gets the vector fields in this document.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Vectors { get; init; } = new Dictionary<string, object?>();
    
    /// <summary>
    /// Determines whether the document contains the specified scalar field.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns><c>true</c> if the field exists; otherwise, <c>false</c>.</returns>
    public bool HasField(string name) => Fields.ContainsKey(name);
    
    /// <summary>
    /// Determines whether the document contains the specified vector field.
    /// </summary>
    /// <param name="name">The name of the vector field.</param>
    /// <returns><c>true</c> if the vector field exists; otherwise, <c>false</c>.</returns>
    public bool HasVector(string name) => Vectors.ContainsKey(name);
    
    /// <summary>
    /// Gets the value of a scalar field.
    /// </summary>
    /// <typeparam name="T">The type of the field value.</typeparam>
    /// <param name="name">The name of the field.</param>
    /// <returns>The field value, or default if not found.</returns>
    public T? Field<T>(string name) => Fields.TryGetValue(name, out var value) ? (T?)value : default;
    
    /// <summary>
    /// Gets the value of a scalar field as an object.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The field value, or null if not found.</returns>
    public object? Field(string name) => Fields.TryGetValue(name, out var value) ? value : null;
    
    /// <summary>
    /// Gets a value type from a vector field.
    /// </summary>
    /// <typeparam name="T">The type of the vector (e.g., float for sparse vector element).</typeparam>
    /// <param name="name">The name of the vector field.</param>
    /// <returns>The value, or default if not found.</returns>
    public T? Vector<T>(string name) where T : struct => 
        Vectors.TryGetValue(name, out var value) ? (T?)value : null;
    
    /// <summary>
    /// Gets a float array from a vector field.
    /// </summary>
    /// <param name="name">The name of the vector field.</param>
    /// <returns>The float array, or null if not found.</returns>
    public float[]? Vector(string name) => Vectors.TryGetValue(name, out var value) ? value as float[] : null;
    
    /// <summary>
    /// Gets the names of all scalar fields.
    /// </summary>
    public IReadOnlyList<string> FieldNames => Fields.Keys.ToList();
    
    /// <summary>
    /// Gets the names of all vector fields.
    /// </summary>
    public IReadOnlyList<string> VectorNames => Vectors.Keys.ToList();
    
    /// <summary>
    /// Creates a document with an ID and optional single vector.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="vector">The optional vector data.</param>
    /// <param name="vectorFieldName">The name of the vector field (default: "embedding").</param>
    /// <returns>A new <see cref="Doc"/> instance.</returns>
    public static Doc Create(string id, float[]? vector = null, string? vectorFieldName = "embedding")
    {
        var vectors = new Dictionary<string, object?>();
        if (vector != null && vectorFieldName != null)
        {
            vectors[vectorFieldName] = vector;
        }
        
        return new Doc
        {
            Id = id,
            Vectors = vectors
        };
    }
    
    /// <summary>
    /// Creates a document with fields and optional vectors.
    /// </summary>
    /// <param name="id">The document ID.</param>
    /// <param name="fields">The scalar fields.</param>
    /// <param name="vectors">The optional vector fields.</param>
    /// <returns>A new <see cref="Doc"/> instance.</returns>
    public static Doc Create(string id, Dictionary<string, object?> fields, Dictionary<string, object?>? vectors = null)
    {
        return new Doc
        {
            Id = id,
            Fields = fields,
            Vectors = vectors ?? new Dictionary<string, object?>()
        };
    }
    
    /// <inheritdoc/>
    public override string ToString()
    {
        var scoreStr = Score.HasValue ? $", Score={Score.Value:F4}" : "";
        return $"Doc[Id={Id}{scoreStr}, Fields={Fields.Count}, Vectors={Vectors.Count}]";
    }
}
