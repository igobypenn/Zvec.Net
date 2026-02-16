namespace Zvec.Net.Attributes;

/// <summary>
/// Marks a property as the primary key for a document.
/// </summary>
/// <remarks>
/// Only one property per document can have this attribute.
/// The property must be of type <see cref="string"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class KeyAttribute : Attribute
{
}
