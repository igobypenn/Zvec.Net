using System.Linq.Expressions;
using Zvec.Net.Models;
using Zvec.Net.Query;

namespace Zvec.Net.Tests.Query;

public class FilterExpressionTranslatorTests
{
    private readonly FilterExpressionTranslator<TestDoc> _translator = new();
    
    [Fact]
    public void Translate_EqualityComparison()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Category == "tech";
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("(Category == 'tech')", result);
    }
    
    [Fact]
    public void Translate_NumericComparison()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Year > 2020;
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("(Year > 2020)", result);
    }
    
    [Fact]
    public void Translate_AndAlso()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Category == "tech" && d.Year >= 2020;
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("((Category == 'tech') && (Year >= 2020))", result);
    }
    
    [Fact]
    public void Translate_OrElse()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Category == "tech" || d.Category == "science";
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("((Category == 'tech') || (Category == 'science'))", result);
    }
    
    [Fact]
    public void Translate_ComplexExpression()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Category == "tech" && d.Year >= 2020 && d.Price < 100;
        
        var result = _translator.Translate(expr);
        
        Assert.Contains("Category == 'tech'", result);
        Assert.Contains("Year >= 2020", result);
        Assert.Contains("Price < 100", result);
    }
    
    [Fact]
    public void Translate_StartsWith()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Title!.StartsWith("Hello");
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("Title HAS_PREFIX 'Hello'", result);
    }
    
    [Fact]
    public void Translate_EndsWith()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Title!.EndsWith("World");
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("Title HAS_SUFFIX 'World'", result);
    }
    
    [Fact]
    public void Translate_Contains()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Title!.Contains("test");
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("Title CONTAIN_ANY 'test'", result);
    }
    
    [Fact]
    public void Translate_EscapesSingleQuotes()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.Category == "it's";
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("(Category == 'it\\'s')", result);
    }
    
    [Fact]
    public void Translate_BooleanValue()
    {
        Expression<Func<TestDoc, bool>> expr = d => d.IsActive == true;
        
        var result = _translator.Translate(expr);
        
        Assert.Equal("(IsActive == true)", result);
    }
    
    private class TestDoc : IDocument
    {
        public string Id { get; set; } = "";
        public string? Title { get; set; }
        public string? Category { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }
        public bool IsActive { get; set; }
    }
}
