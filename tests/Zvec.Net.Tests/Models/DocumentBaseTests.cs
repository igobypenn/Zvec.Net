using Zvec.Net.Index;
using Zvec.Net.Models;
using Zvec.Net.Types;

namespace Zvec.Net.Tests.Models;

public class DocumentBaseTests
{
    private class TestDoc : DocumentBase
    {
        public TestDoc() : base() { }
        public TestDoc(string id) : base(id) { }
    }
    
    [Fact]
    public void Constructor_Default_SetsEmptyId()
    {
        var doc = new TestDoc();
        
        Assert.Equal(string.Empty, doc.Id);
    }
    
    [Fact]
    public void Constructor_WithId_SetsId()
    {
        var doc = new TestDoc("doc123");
        
        Assert.Equal("doc123", doc.Id);
    }
    
    [Fact]
    public void Id_CanBeSet()
    {
        var doc = new TestDoc();
        
        doc.Id = "new-id";
        
        Assert.Equal("new-id", doc.Id);
    }
    
    [Fact]
    public void Score_CanBeSet()
    {
        var doc = new TestDoc { Score = 0.95 };
        
        Assert.Equal(0.95, doc.Score);
    }
    
    [Fact]
    public void Score_DefaultIsNull()
    {
        var doc = new TestDoc();
        
        Assert.Null(doc.Score);
    }
}
