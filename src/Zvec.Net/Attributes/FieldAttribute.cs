using Zvec.Net.Types;

namespace Zvec.Net.Attributes;

/// <summary>
/// Marks a property as a scalar field in a document.
/// </summary>
/// <remarks>
/// Supported types: string, int, long, float, double, bool, and their nullable variants.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FieldAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether the field can be null.
    /// </summary>
    public bool Nullable { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the field should be indexed.
    /// </summary>
    public bool Indexed { get; init; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldAttribute"/> class.
    /// </summary>
    public FieldAttribute() { }

    /// <summary>
    /// Initializes a new instance with specified nullability.
    /// </summary>
    /// <param name="nullable">Whether the field can be null.</param>
    public FieldAttribute(bool nullable)
    {
        Nullable = nullable;
    }
}
