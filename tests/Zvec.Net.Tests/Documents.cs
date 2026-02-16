using Zvec.Net.Attributes;
using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Tests;

public class Article : DocumentBase
{
    [Field]
    public string? Title { get; set; }

    [Field]
    public string? Category { get; set; }

    [Field]
    public int Year { get; set; }

    [Field]
    public double Price { get; set; }

    [VectorField(dimension: 768, IndexType = IndexType.Hnsw)]
    public float[]? Embedding { get; set; }

    public Article() : base() { }

    public Article(string id) : base(id) { }
}

public class MultimediaDoc : DocumentBase
{
    [Field]
    public string? Title { get; set; }

    [Field(Nullable = true)]
    public string? Description { get; set; }

    [VectorField(dimension: 768, precision: VectorPrecision.Float32, IndexType = IndexType.Hnsw)]
    public float[]? TextEmbedding { get; set; }

    [VectorField(dimension: 512, precision: VectorPrecision.Float16)]
    public Half[]? ImageEmbedding { get; set; }

    [VectorField(dimension: 128, precision: VectorPrecision.Int8)]
    public sbyte[]? AudioFingerprint { get; set; }

    public MultimediaDoc() : base() { }

    public MultimediaDoc(string id) : base(id) { }
}

public class SparseDoc : DocumentBase
{
    [Field]
    public string? Content { get; set; }

    [VectorField(precision: VectorPrecision.SparseFloat32)]
    public SparseVector? Keywords { get; set; }

    public SparseDoc() : base() { }

    public SparseDoc(string id) : base(id) { }
}
