using System.Linq.Expressions;
using System.Text;
using Zvec.Net;
using Zvec.Net.Attributes;
using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Query;
using Zvec.Net.Schema;
using Zvec.Net.Types;

public class Article : DocumentBase
{
    [Field] public string? Title { get; set; }
    [Field] public string? Category { get; set; }
    [Field] public int Year { get; set; }
    [Field] public double Price { get; set; }
    [Field] public bool IsActive { get; set; }
    
    [VectorField(dimension: 768, IndexType = IndexType.Hnsw)]
    public float[]? Embedding { get; set; }
}

public class MultimediaDoc : DocumentBase
{
    [Field] public string? Title { get; set; }
    [Field] public string[]? Tags { get; set; }
    
    [VectorField(dimension: 768, precision: VectorPrecision.Float32, IndexType = IndexType.Hnsw)]
    public float[]? TextEmbedding { get; set; }
    
    [VectorField(dimension: 512, precision: VectorPrecision.Float16)]
    public Half[]? ImageEmbedding { get; set; }
    
    [VectorField(precision: VectorPrecision.SparseFloat32)]
    public SparseVector? Keywords { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Zvec .NET Client - Comprehensive Examples        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
        
        Section1_SchemaDefinition();
        Section2_LinqFilterExpressions();
        Section3_VectorSearchPatterns();
        Section4_QueryCustomization();
        Section5_AdvancedFilters();
        Section6_SparseVectors();
        Section7_RerankersAndHybrid();
        
        Console.WriteLine("\n✓ All examples completed!");
    }
    
    static void Section1_SchemaDefinition()
    {
        PrintSection("1. Schema Definition");
        
        Console.WriteLine("   ┌─ POCO with Attributes ─────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  public class Article : DocumentBase");
        Console.WriteLine("   │  {");
        Console.WriteLine("   │      [Field] public string? Title    { get; set; }");
        Console.WriteLine("   │      [Field] public string? Category { get; set; }");
        Console.WriteLine("   │      [Field] public int     Year     { get; set; }");
        Console.WriteLine("   │      [Field] public double  Price    { get; set; }");
        Console.WriteLine("   │      [Field] public bool    IsActive { get; set; }");
        Console.WriteLine("   │");
        Console.WriteLine("   │      [VectorField(dimension: 768, IndexType = IndexType.Hnsw)]");
        Console.WriteLine("   │      public float[]? Embedding { get; set; }");
        Console.WriteLine("   │  }");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Multi-Vector Document ─────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  public class MultimediaDoc : DocumentBase");
        Console.WriteLine("   │  {");
        Console.WriteLine("   │      [VectorField(768, Float32, Hnsw)] float[]? Text;");
        Console.WriteLine("   │      [VectorField(512, Float16)]      Half[]?  Image;");
        Console.WriteLine("   │      [VectorField(SparseFloat32)]     SparseVector? Keywords;");
        Console.WriteLine("   │  }");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
    }
    
    static void Section2_LinqFilterExpressions()
    {
        PrintSection("2. LINQ Filter Expressions");
        
        ShowFilter("Equality", a => a.Category == "tech");
        ShowFilter("Inequality", a => a.Category != "science");
        ShowFilter("Numeric Greater Than", a => a.Year > 2020);
        ShowFilter("Numeric Less Than", a => a.Price < 99.99);
        ShowFilter("Greater or Equal", a => a.Year >= 2022);
        ShowFilter("Less or Equal", a => a.Price <= 50.0);
        ShowFilter("Boolean Filter", a => a.IsActive == true);
        ShowFilter("Boolean Negation", a => a.IsActive == false);
        
        Console.WriteLine();
        PrintSubsection("Logical Combinations");
        ShowFilter("AND", a => a.Category == "tech" && a.Year >= 2020);
        ShowFilter("OR", a => a.Category == "tech" || a.Category == "ai");
        ShowFilter("Complex", a => (a.Category == "tech" || a.Category == "ai") && a.Year > 2022 && a.Price < 100);
        ShowFilter("Negation", a => !(a.Category == "spam"));
        
        Console.WriteLine();
        PrintSubsection("String Operations");
        ShowFilter("StartsWith", a => a.Title!.StartsWith("Introduction"));
        ShowFilter("EndsWith", a => a.Title!.EndsWith("Guide"));
        ShowFilter("Contains", a => a.Title!.Contains("RAG"));
        
        Console.WriteLine();
        PrintSubsection("Null Checks");
        ShowFilter("Is Null", a => a.Category == null);
        ShowFilter("Is Not Null", a => a.Category != null);
        ShowFilter("Combined with Null", a => a.Category != null && a.Year > 2020);
        
        Console.WriteLine();
    }
    
    static void Section3_VectorSearchPatterns()
    {
        PrintSection("3. Vector Search Patterns");
        
        var queryVector = new float[768];
        for (int i = 0; i < 768; i++) queryVector[i] = Random.Shared.NextSingle();
        
        Console.WriteLine("   ┌─ Basic Vector Search ──────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(a => a.Embedding, queryVector)");
        Console.WriteLine("   │      .TopK(10)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Vector Search with Filter ────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(a => a.Embedding, queryVector)");
        Console.WriteLine("   │      .Where(a => a.Category == \"tech\" && a.Year >= 2022)");
        Console.WriteLine("   │      .TopK(20)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   │  Filter: (Category == 'tech') && (Year >= 2022)");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Search by Document ID ────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Use an existing document's embedding as the query");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearestById(a => a.Embedding, \"doc_123\")");
        Console.WriteLine("   │      .TopK(5)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Multi-Vector Hybrid Search ───────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Search across text AND image embeddings");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(d => d.TextEmbedding, textVector, weight: 0.7)");
        Console.WriteLine("   │      .VectorNearest(d => d.ImageEmbedding, imageVector, weight: 0.3)");
        Console.WriteLine("   │      .Reranker(new WeightedReRanker(...))");
        Console.WriteLine("   │      .TopK(10)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
    }
    
    static void Section4_QueryCustomization()
    {
        PrintSection("4. Query Customization");
        
        Console.WriteLine("   ┌─ TopK Configuration ───────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  .TopK(10)    // Return top 10 results");
        Console.WriteLine("   │  .TopK(100)   // Return top 100 results");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Include Vectors in Results ───────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(a => a.Embedding, queryVector)");
        Console.WriteLine("   │      .IncludeVectors(true)   // Return embeddings too");
        Console.WriteLine("   │      .TopK(10)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   │  foreach (var doc in results)");
        Console.WriteLine("   │  {");
        Console.WriteLine("   │      var embedding = doc.Embedding; // Available!");
        Console.WriteLine("   │  }");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Select Specific Fields ───────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Using expression selectors");
        Console.WriteLine("   │  .IncludeFields(a => a.Title, a => a.Category, a => a.Year)");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Using string field names");
        Console.WriteLine("   │  .IncludeFields(\"title\", \"category\", \"year\")");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Method Chaining Pattern ──────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(a => a.Embedding, queryVector)");
        Console.WriteLine("   │      .Where(a => a.Category == \"tech\")");
        Console.WriteLine("   │      .Where(a => a.Year >= 2020)      // Multiple filters");
        Console.WriteLine("   │      .IncludeFields(a => a.Title)");
        Console.WriteLine("   │      .IncludeVectors(false)");
        Console.WriteLine("   │      .TopK(25)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
    }
    
    static void Section5_AdvancedFilters()
    {
        PrintSection("5. Advanced Filter Patterns");
        
        Console.WriteLine("   ┌─ Raw String Filters ───────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // For dynamic or complex filters");
        Console.WriteLine("   │  var filter = $\"Year >= {minYear} && Price < {maxPrice}\";");
        Console.WriteLine("   │");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(a => a.Embedding, queryVector)");
        Console.WriteLine("   │      .Where(filter)   // String-based filter");
        Console.WriteLine("   │      .TopK(10)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Filter Expression Reference ──────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  Operators:");
        Console.WriteLine("   │    ==    Equal to");
        Console.WriteLine("   │    !=    Not equal to");
        Console.WriteLine("   │    >     Greater than");
        Console.WriteLine("   │    >=    Greater than or equal");
        Console.WriteLine("   │    <     Less than");
        Console.WriteLine("   │    <=    Less than or equal");
        Console.WriteLine("   │    &&    Logical AND");
        Console.WriteLine("   │    ||    Logical OR");
        Console.WriteLine("   │    !     Logical NOT");
        Console.WriteLine("   │");
        Console.WriteLine("   │  String Methods:");
        Console.WriteLine("   │    .StartsWith(\"prefix\")   →  HAS_PREFIX");
        Console.WriteLine("   │    .EndsWith(\"suffix\")     →  HAS_SUFFIX");
        Console.WriteLine("   │    .Contains(\"text\")       →  CONTAIN_ANY");
        Console.WriteLine("   │");
        Console.WriteLine("   │  Null Checks:");
        Console.WriteLine("   │    field == null    →  IS NULL");
        Console.WriteLine("   │    field != null    →  IS NOT NULL");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Building Dynamic Filters ─────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Combine multiple conditions programmatically");
        Console.WriteLine("   │  var conditions = new List<string>();");
        Console.WriteLine("   │  if (requireTech) conditions.Add(\"Category == 'tech'\");");
        Console.WriteLine("   │  if (minYear > 0) conditions.Add($\"Year >= {minYear}\");");
        Console.WriteLine("   │  var filter = string.Join(\" && \", conditions);");
        Console.WriteLine("   │");
        Console.WriteLine("   │  collection.Query()");
        Console.WriteLine("   │      .VectorNearest(a => a.Embedding, queryVector)");
        Console.WriteLine("   │      .Where(filter)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
    }
    
    static void Section6_SparseVectors()
    {
        PrintSection("6. Sparse Vectors");
        
        Console.WriteLine("   ┌─ Creating Sparse Vectors ──────────────────────────────────");
        Console.WriteLine("   │");
        
        var sparseVec = SparseVector.FromDictionary(new Dictionary<uint, float>
        {
            [0] = 0.1f,
            [100] = 0.5f,
            [500] = 0.3f
        });
        
        Console.WriteLine("   │  var sparse = SparseVector.FromDictionary(new Dictionary<uint, float>");
        Console.WriteLine("   │  {");
        Console.WriteLine("   │      [0]   = 0.1f,");
        Console.WriteLine("   │      [100] = 0.5f,");
        Console.WriteLine("   │      [500] = 0.3f");
        Console.WriteLine("   │  });");
        Console.WriteLine("   │");
        Console.WriteLine($"   │  Result: {sparseVec}");
        Console.WriteLine($"   │  sparse[100] = {sparseVec[100]}");
        Console.WriteLine($"   │  sparse[50]  = {sparseVec[50]} (default: 0)");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Sparse Vector Search ─────────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Keyword-based semantic search");
        Console.WriteLine("   │  var keywordVector = SparseVector.FromDictionary(keywords);");
        Console.WriteLine("   │");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(d => d.Keywords, keywordVector)");
        Console.WriteLine("   │      .TopK(10)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Hybrid: Dense + Sparse ───────────────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Combine semantic (dense) + keyword (sparse) search");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      .VectorNearest(d => d.TextEmbedding, denseVector, 0.6)");
        Console.WriteLine("   │      .VectorNearest(d => d.Keywords, sparseVector, 0.4)");
        Console.WriteLine("   │      .Reranker(new RrfReRanker(k: 60))");
        Console.WriteLine("   │      .TopK(20)");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
    }
    
    static void Section7_RerankersAndHybrid()
    {
        PrintSection("7. Rerankers & Hybrid Search");
        
        Console.WriteLine("   ┌─ Reciprocal Rank Fusion (RRF) ─────────────────────────────");
        Console.WriteLine("   │");
        
        var rrfReranker = new RrfReRanker(k: 60);
        Console.WriteLine("   │  var rrf = new RrfReRanker(k: 60);");
        Console.WriteLine($"   │  {rrfReranker}");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Usage:");
        Console.WriteLine("   │  collection.Query()");
        Console.WriteLine("   │      .VectorNearest(d => d.TextEmbedding, textVec)");
        Console.WriteLine("   │      .VectorNearest(d => d.ImageEmbedding, imageVec)");
        Console.WriteLine("   │      .Reranker(new RrfReRanker(k: 60))");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Weighted Reranker ────────────────────────────────────────");
        Console.WriteLine("   │");
        
        var weightedReranker = new WeightedReRanker(new Dictionary<string, double>
        {
            ["text"] = 0.7,
            ["image"] = 0.3
        });
        
        Console.WriteLine("   │  var weighted = new WeightedReRanker(new Dictionary<string, double>");
        Console.WriteLine("   │  {");
        Console.WriteLine("   │      [\"text\"]  = 0.7,");
        Console.WriteLine("   │      [\"image\"] = 0.3");
        Console.WriteLine("   │  });");
        Console.WriteLine($"   │  {weightedReranker}");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Usage:");
        Console.WriteLine("   │  collection.Query()");
        Console.WriteLine("   │      .VectorNearest(d => d.TextEmbedding, textVec, 0.7)");
        Console.WriteLine("   │      .VectorNearest(d => d.ImageEmbedding, imageVec, 0.3)");
        Console.WriteLine("   │      .Reranker(new WeightedReRanker(weights))");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
        
        Console.WriteLine("   ┌─ Complete Hybrid Search Example ───────────────────────────");
        Console.WriteLine("   │");
        Console.WriteLine("   │  // Multi-modal search with filtering and reranking");
        Console.WriteLine("   │  var results = collection.Query()");
        Console.WriteLine("   │      // Search multiple vector fields");
        Console.WriteLine("   │      .VectorNearest(d => d.TextEmbedding, textQuery, 0.5)");
        Console.WriteLine("   │      .VectorNearest(d => d.ImageEmbedding, imageQuery, 0.3)");
        Console.WriteLine("   │      .VectorNearest(d => d.Keywords, keywordQuery, 0.2)");
        Console.WriteLine("   │      // Apply filters");
        Console.WriteLine("   │      .Where(d => d.Tags!.Contains(\"featured\"))");
        Console.WriteLine("   │      // Configure results");
        Console.WriteLine("   │      .TopK(50)");
        Console.WriteLine("   │      .IncludeFields(d => d.Title, d => d.Tags)");
        Console.WriteLine("   │      // Rerank results");
        Console.WriteLine("   │      .Reranker(new RrfReRanker(k: 60))");
        Console.WriteLine("   │      .Execute();");
        Console.WriteLine("   │");
        Console.WriteLine("   └─────────────────────────────────────────────────────────────\n");
    }
    
    static void PrintSection(string title)
    {
        Console.WriteLine($"\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  {title,-58}  ║");
        Console.WriteLine($"╚══════════════════════════════════════════════════════════════╝\n");
    }
    
    static void PrintSubsection(string title)
    {
        Console.WriteLine($"   ─── {title} ───");
    }
    
    static void ShowFilter(string name, Expression<Func<Article, bool>> expr)
    {
        var translated = DemoFilterTranslator.Translate(expr);
        Console.WriteLine($"   {name,-25} →  {translated}");
    }
}

internal static class DemoFilterTranslator
{
    public static string Translate(Expression<Func<Article, bool>> predicate)
    {
        return new DemoVisitor().Translate(predicate);
    }
    
    private class DemoVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _filter = new();
        
        public string Translate(Expression<Func<Article, bool>> predicate)
        {
            _filter.Clear();
            Visit(predicate.Body);
            return _filter.ToString();
        }
        
        protected override Expression VisitBinary(BinaryExpression node)
        {
            _filter.Append('(');
            Visit(node.Left);
            
            var op = node.NodeType switch
            {
                ExpressionType.Equal => node.Right is ConstantExpression { Value: null } 
                    ? " IS NULL" 
                    : " == ",
                ExpressionType.NotEqual => node.Right is ConstantExpression { Value: null } 
                    ? " IS NOT NULL" 
                    : " != ",
                ExpressionType.GreaterThan => " > ",
                ExpressionType.GreaterThanOrEqual => " >= ",
                ExpressionType.LessThan => " < ",
                ExpressionType.LessThanOrEqual => " <= ",
                ExpressionType.AndAlso => " && ",
                ExpressionType.OrElse => " || ",
                _ => $" {node.NodeType} "
            };
            
            _filter.Append(op);
            
            if (op is not (" IS NULL" or " IS NOT NULL"))
            {
                Visit(node.Right);
            }
            
            _filter.Append(')');
            return node;
        }
        
        protected override Expression VisitMember(MemberExpression node)
        {
            _filter.Append(node.Member.Name);
            return node;
        }
        
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            _filter.Append(value switch
            {
                string s => $"'{EscapeString(s)}'",
                bool b => b.ToString().ToLowerInvariant(),
                null => "null",
                float f => f.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                double d => d.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                decimal dec => dec.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                _ => value.ToString()
            });
            return node;
        }
        
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                Visit(node.Object!);
                
                switch (node.Method.Name)
                {
                    case "StartsWith":
                        _filter.Append(" HAS_PREFIX ");
                        break;
                    case "EndsWith":
                        _filter.Append(" HAS_SUFFIX ");
                        break;
                    case "Contains":
                        _filter.Append(" CONTAIN_ANY ");
                        break;
                    default:
                        _filter.Append($" {node.Method.Name} ");
                        break;
                }
                
                Visit(node.Arguments[0]);
                return node;
            }
            
            _filter.Append($"[{node.Method.Name}]");
            return node;
        }
        
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                Visit(node.Operand);
                return node;
            }
            
            if (node.NodeType == ExpressionType.Not)
            {
                _filter.Append('!');
                Visit(node.Operand);
                return node;
            }
            
            _filter.Append($"[{node.NodeType}]");
            return node;
        }
        
        private static string EscapeString(string s) => s.Replace("'", "\\'");
    }
}
