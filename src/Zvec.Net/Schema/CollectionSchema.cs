using Zvec.Net.Exceptions;
using Zvec.Net.Index;
using Zvec.Net.Types;

namespace Zvec.Net.Schema;

/// <summary>
/// Represents the schema of a vector collection.
/// </summary>
/// <remarks>
/// A collection schema defines the scalar fields and vector fields that documents in the collection can contain.
/// Schemas can be created explicitly or generated from POCO types using <see cref="SchemaGenerator"/>.
/// </remarks>
public sealed class CollectionSchema
{
    /// <summary>
    /// Gets the name of the collection.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Gets the scalar field definitions.
    /// </summary>
    public IReadOnlyList<FieldSchema> Fields { get; }
    
    /// <summary>
    /// Gets the vector field definitions.
    /// </summary>
    public IReadOnlyList<VectorSchema> Vectors { get; }
    
    /// <summary>
    /// Initializes a new collection schema.
    /// </summary>
    /// <param name="name">The collection name.</param>
    /// <param name="fields">The scalar field definitions.</param>
    /// <param name="vectors">The vector field definitions.</param>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="SchemaValidationException">Thrown when field names are duplicated.</exception>
    public CollectionSchema(
        string name,
        IEnumerable<FieldSchema>? fields = null,
        IEnumerable<VectorSchema>? vectors = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Collection name cannot be null or empty", nameof(name));
        
        var fieldList = fields?.ToList() ?? new List<FieldSchema>();
        var vectorList = vectors?.ToList() ?? new List<VectorSchema>();
        
        ValidateUniqueNames(fieldList, vectorList);
        
        Name = name;
        Fields = fieldList.AsReadOnly();
        Vectors = vectorList.AsReadOnly();
    }
    
    private static void ValidateUniqueNames(IList<FieldSchema> fields, IList<VectorSchema> vectors)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var field in fields)
        {
            if (!names.Add(field.Name))
                throw new SchemaValidationException($"Duplicate field name: '{field.Name}'");
        }
        
        foreach (var vector in vectors)
        {
            if (!names.Add(vector.Name))
                throw new SchemaValidationException($"Duplicate field/vector name: '{vector.Name}'");
        }
    }
    
    /// <summary>
    /// Gets a scalar field by name (case-insensitive).
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The field schema, or null if not found.</returns>
    public FieldSchema? GetField(string name) => 
        Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
    
    /// <summary>
    /// Gets a vector field by name (case-insensitive).
    /// </summary>
    /// <param name="name">The vector field name.</param>
    /// <returns>The vector schema, or null if not found.</returns>
    public VectorSchema? GetVector(string name) => 
        Vectors.FirstOrDefault(v => string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));
    
    /// <summary>
    /// Determines whether a scalar field exists (case-insensitive).
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns><c>true</c> if the field exists; otherwise, <c>false</c>.</returns>
    public bool HasField(string name) => GetField(name) != null;
    
    /// <summary>
    /// Determines whether a vector field exists (case-insensitive).
    /// </summary>
    /// <param name="name">The vector field name.</param>
    /// <returns><c>true</c> if the vector field exists; otherwise, <c>false</c>.</returns>
    public bool HasVector(string name) => GetVector(name) != null;
    
    /// <inheritdoc/>
    public override string ToString() => $"CollectionSchema[{Name}, Fields={Fields.Count}, Vectors={Vectors.Count}]";
}
