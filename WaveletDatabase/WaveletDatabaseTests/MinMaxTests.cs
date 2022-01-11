using NUnit.Framework;
using WaveletDatabase.Database;

namespace WaveletDatabaseTests;

[TestFixture]
public class MinMaxTests
{
    [Test]
    public void Contains_is_judged_by_overlap()
    {
        var a = new MinMax<double>(1.0){Max = 2.0};
        
        var b = new MinMax<double>(2.1){Max = 3.0};
        var c = new MinMax<double>(1.5){Max = 2.5};
        var d = new MinMax<double>(1.25){Max = 1.75};
        var e = new MinMax<double>(0.5){Max = 1.5};
        var f = new MinMax<double>(0.0){Max = 0.5};
        
        Assert.That(a.Contains(a), Is.True, "self equality");
        
        Assert.That(a.Contains(b), Is.False, "a -> b");
        Assert.That(b.Contains(a), Is.False, "b -> a");
        
        Assert.That(a.Contains(f), Is.False, "a -> f");
        Assert.That(f.Contains(a), Is.False, "f -> a");
        
        Assert.That(a.Contains(c) && c.Contains(a), Is.True, "a <-> c");
        Assert.That(a.Contains(d) && d.Contains(a), Is.True, "a <-> d");
        Assert.That(a.Contains(e) && e.Contains(a), Is.True, "a <-> e");
    }

    [Test]
    public void contains_value_is_inclusive()
    {
        var a = new MinMax<double>(1.0){Max = 2.0};
        
        Assert.That(a.Contains(0.0), Is.False, "outside lower bounds");
        Assert.That(a.Contains(0.99), Is.False, "outside lower edge");
        
        Assert.That(a.Contains(1.0), Is.True, "lower bound");
        Assert.That(a.Contains(1.01), Is.True, "lower edge");
        Assert.That(a.Contains(1.5), Is.True, "middle");
        Assert.That(a.Contains(1.99), Is.True, "upper edge");
        Assert.That(a.Contains(2.0), Is.True, "upper bound");
        
        Assert.That(a.Contains(2.01), Is.False, "outside upper edge");
        Assert.That(a.Contains(3.0), Is.False, "outside upper bounds");
    }
}