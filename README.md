# Zvec.Net

A .NET client for [zvec](https://github.com/alibaba/zvec) - a lightweight, lightning-fast, in-process vector database.

[![CI](https://github.com/user/zvec-net/actions/workflows/ci.yml/badge.svg)](https://github.com/user/zvec-net/actions/workflows/ci.yml)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)

## Features

- **POCO-based schema definition** with attributes
- **Multiple vector fields** per document (Float32, Float64, Float16, Int8)
- **Sparse vector support** (SparseFloat32, SparseFloat16)
- **LINQ-style query builder** with expression filters
- **Rerankers** (RRF, Weighted)
- **Async/sync API** for all operations
- **Type-safe** with nullable reference types
- **GitHub Packages** for easy distribution

## Installation

```bash
dotnet add package Zvec.Net --version 0.2.0
```

## Quick Start

### Define a Document

```csharp
using Zvec.Net;
using Zvec.Net.Attributes;
using Zvec.Net.Models;
using Zvec.Net.Types;

public class Article : DocumentBase
{
    [Field] public string? Title { get; set; }
    [Field] public string? Category { get; set; }
    [Field] public int Year { get; set; }
    
    [VectorField(dimension: 768, IndexType = IndexType.Hnsw)]
    public float[]? Embedding { get; set; }
}
```

### Create and Query

```csharp
// Create collection
using var collection = Collection.CreateAndOpen<Article>("./articles_db");

// Insert documents
collection.Insert(new Article
{
    Id = "doc1",
    Title = "Introduction to RAG",
    Category = "tech",
    Year = 2024,
    Embedding = embeddingService.Embed("Introduction to RAG")
});

collection.Flush();

// Query with LINQ-style filters
var results = collection.Query()
    .VectorNearest(a => a.Embedding, queryVector)
    .Where(a => a.Category == "tech" && a.Year >= 2020)
    .TopK(10)
    .Execute();

foreach (var doc in results)
{
    Console.WriteLine($"{doc.Id}: {doc.Score:F4}");
}
```

## Supported Platforms

| Platform | Architecture | Status |
|----------|-------------|--------|
| Linux | x64 | ✅ |
| Linux | ARM64 | ✅ |
| macOS | ARM64 | ✅ |
| Windows | x64 | 🔜 Planned |

## Supported Vector Types

| Precision | C# Type | Zvec DataType |
|-----------|---------|---------------|
| Float32 | `float[]` | VECTOR_FP32 |
| Float64 | `double[]` | VECTOR_FP64 |
| Float16 | `Half[]` | VECTOR_FP16 |
| Int8 | `sbyte[]` | VECTOR_INT8 |
| SparseFloat32 | `SparseVector` | SPARSE_VECTOR_FP32 |

## Building from Source

### Prerequisites
- .NET 8 SDK
- CMake 3.16+
- C++17 compiler

### Build

```bash
# Clone with submodules
git clone --recursive https://github.com/user/zvec-net.git
cd zvec-net

# Build native library
./build-native.sh

# Build .NET library
dotnet build

# Run tests
dotnet test
```

## API Reference

### Collection<T>

```csharp
// Factory methods
Collection.CreateAndOpen<T>(path, options)
Collection.Open<T>(path, options)

// DML
Insert(IEnumerable<T> documents)
Upsert(IEnumerable<T> documents)
Update(IEnumerable<T> documents)
Delete(IEnumerable<string> ids)
DeleteByFilter(string filter)

// DQL
Query() -> IVectorQueryBuilder<T>
Fetch(IEnumerable<string> ids)

// DDL
Flush()
CreateIndex(fieldName, indexParams)
DropIndex(fieldName)
Optimize()
```

### VectorQueryBuilder<T>

```csharp
VectorNearest(field, vector, weight, param)
VectorNearestById(field, documentId, weight, param)
Where(Expression<Func<T, bool>> predicate)
Where(string filter)
TopK(int k)
IncludeVectors(bool include)
Reranker(IReRanker reranker)
Execute() / ExecuteAsync()
```

### Index Types

```csharp
// HNSW - fast approximate search
IndexParams.Hnsw(m: 16, efConstruction: 200, metric: MetricType.Cosine)

// IVF - balanced performance
IndexParams.Ivf(nLists: 1024, nProbe: 64, metric: MetricType.Ip)

// Flat - exact search
IndexParams.Flat(metric: MetricType.L2)
```

### Query Options

```csharp
QueryOptions.Default
    .WithTopK(50)
    .WithFilter("year >= 2020")
    .WithIncludeVectors(true)
    .WithOutputFields("title", "category")
    .WithReRanker(new RrfReRanker(50));
```

## License

Apache License 2.0

## Acknowledgments

Built on [zvec](https://github.com/alibaba/zvec) by Alibaba.
