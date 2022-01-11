using NUnit.Framework;
using WaveletDatabase.Database;

namespace WaveletDatabaseTests;

[TestFixture]
public class BasicTests
{
    [Test]
    public void can_create_a_series_with_a_pyramid_height_and_aggregation__and_add_to_it()
    {
        var aggregate = new MinMaxAggregation(0.0);
        
        // 5 levels = 32 buckets per top-level value
        // This implies the 'page' size if any bucket in the given position is occupied
        var subject = new DbCore<MinMax<double>, double>(5, aggregate);
        
        // Not sure about the position. Go 'raw' for the moment and worry about it later
        subject.WriteEntry(position:0, value: 1.3, log: "This is any object that goes with the entry");
        
        // Read a slice through pyramids
        var sequence = subject.ReadRange(startPosition: 0, endPosition: 64, level: 0);
        Assert.That(sequence.Count, Is.EqualTo(1), "Level 0 sequence length");
        
        sequence = subject.ReadRange(startPosition: 0, endPosition: 64, level: 2);
        Assert.That(sequence.Count, Is.EqualTo(4), "Level 2 sequence length");
        
        sequence = subject.ReadRange(startPosition: 0, endPosition: 64, level: 4);
        Assert.That(sequence.Count, Is.EqualTo(16), "Level 4 sequence length");
        
        sequence = subject.ReadRange(startPosition: 0, endPosition: 64, level: 5);
        Assert.That(sequence.Count, Is.EqualTo(32), "Level 5 sequence length");
    }
    
    [Test]
    public void aggregation_levels_with_min_max()
    {
        var aggregate = new MinMaxAggregation(0.0);
        
        var subject = new DbCore<MinMax<double>, double>(5, aggregate);
        
        
        subject.WriteEntry(position:0, value: 1.3, log: "This is any object that goes with the entry");
        subject.WriteEntry(position:10, value: -1.3);
        subject.WriteEntry(position:20, value: 100);
        subject.WriteEntry(position:30, value: -20);
        
        subject.WriteEntry(position:31, value: -40);
        //---- change of block ----
        subject.WriteEntry(position:32, value: 200);
        
        subject.WriteEntry(position:40, value: 100);
        subject.WriteEntry(position:50, value: -100);
        
        // Read a slice through pyramids
        var sequence = subject.ReadRange(startPosition: 0, endPosition: 64, level: 0);
        Assert.That(sequence.Count, Is.EqualTo(2), "Level 0 sequence length");
        Assert.That(sequence[0].Min, Is.EqualTo(-40), "Block 0 min");
        Assert.That(sequence[0].Max, Is.EqualTo(100), "Block 0 max");
        
        Assert.That(sequence[1].Min, Is.EqualTo(-100), "Block 1 min");
        Assert.That(sequence[1].Max, Is.EqualTo(200),  "Block 1 max");
    }

    [Test]
    public void find_by_value_range()
    {
        var aggregate = new MinMaxAggregation(0.0);
        
        var subject = new DbCore<MinMax<double>, double>(5, aggregate);
        
        
        subject.WriteEntry(position:10, value: -1.3, "out of range - 0");
        
        subject.WriteEntry(position:20, value: 100, "find me - 1");
        subject.WriteEntry(position:30, value: -20, "no - 2");
        subject.WriteEntry(position:31, value: -40, "no - 3");
        subject.WriteEntry(position:32, value: 200, "no - 4");
        subject.WriteEntry(position:40, value: 100, "find me - 5");
        
        subject.WriteEntry(position:50, value: -100, "out of range - 6");
        
        // Find all value:log pairs that fit in both time and value
        var valueRange = new MinMax<double>(50, 150);
        var found = subject.FindInRange(startPosition: 20, endPosition: 40, valueRange: valueRange);
        
        Assert.That(found.Count, Is.EqualTo(2));
        Assert.That(found[0].Log, Is.EqualTo("find me - 1"), "first log");
        Assert.That(found[0].Value, Is.EqualTo(100), "first value");
    }
}