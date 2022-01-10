namespace WaveletDatabase.Database;

public class DbCore<TSum, TValue>
{
    private readonly int _pyramidHeight;
    private readonly IAggregation<TSum, TValue> _aggregate;
    
    private readonly int _alignment;
    private readonly Dictionary<int, Pyramid<TSum, TValue>> _blocks; // alignment entry => value pyramid

    public DbCore(int pyramidHeight, IAggregation<TSum, TValue> aggregate)
    {
        if (pyramidHeight is < 0 or > 30) throw new ArgumentOutOfRangeException(nameof(pyramidHeight), "pyramid height must be between 0 and 30");
        _pyramidHeight = pyramidHeight;
        _aggregate = aggregate;
        
        _alignment = (int)Math.Pow(2, pyramidHeight);
        _blocks = new Dictionary<int, Pyramid<TSum, TValue>>();
    }

    /// <summary>
    /// Write data to the db at a given position.
    /// If any bucket aligned to the range of 2^(pyramid-height) is occupied, the entire pyramid will be stored.
    /// </summary>
    /// <param name="position">bucket position. This is the 'time' of the time-series</param>
    /// <param name="value">the metric value related to this entry, which will be propagated through the pyramid</param>
    /// <param name="log">Optional: Extra data for the entry. This could be an error to go with a log level etc.</param>
    public void WriteEntry(int position, TValue value, object? log = null)
    {
        var block = position / _alignment;
        var offset = position % _alignment;
        
        if (!_blocks.ContainsKey(block)) _blocks.Add(block, new Pyramid<TSum, TValue>(_alignment, _pyramidHeight, _aggregate));
        
        _blocks[block].Write(offset, value, log);
    }

    /// <summary>
    /// Read all pyramid entries of a given level that inclusively cover a range of positions.
    /// Aggregations are not separated, so extra data may be included at the ends.
    /// Level 0 is the most aggregated entry (always 1 per block). Increasing levels adds more detail.
    /// </summary>
    /// <param name="startPosition">lowest position number that must be included</param>
    /// <param name="endPosition">highest position number that must be included</param>
    /// <param name="level">aggregation level to read. Higher = finer detail</param>
    public List<TSum> ReadRange(int startPosition, int endPosition, int level)
    {
        var minBlock = startPosition / _alignment;
        var minOffset = startPosition % _alignment;
        
        var maxBlock = endPosition / _alignment;
        var maxOffset = endPosition % _alignment;
        
        var result = new List<TSum>();

        for (var block = minBlock; block <= maxBlock; block++)
        {
            if (!_blocks.ContainsKey(block)) continue;
            var left = block == minBlock ? minOffset : 0;
            var right = block == maxBlock ? maxOffset : _alignment-1;
            _blocks[block].ReadRange(result, level, left, right);
        }
        
        return result;
    }
}

/// <summary>
/// A single stack of values
/// </summary>
internal class Pyramid<TSum, TValue>
{
    private readonly int _width;
    private readonly int _height;
    private readonly IAggregation<TSum, TValue> _aggregate;
    
    private readonly TSum[] _stack;
    private readonly Bucket<TValue>?[] _buckets;
    private readonly int _startOfBase;

    public Pyramid(int width, int height, IAggregation<TSum, TValue> aggregate)
    {
        _width = width;
        _height = height;
        _aggregate = aggregate;

        var total = 1;
        var rem = width;
        for (var i = 0; i < height; i++)
        {
            total += rem;
            rem /= 2;
        }
        
        _startOfBase = 1 + total - width;
        _stack = new TSum[total + 1];
        _buckets = new Bucket<TValue>[width];
    }

    public void Write(int offset, TValue value, object? log)
    {
        if (offset < 0 || offset >= _width) throw new Exception("pyramid offset out of range");
        
        // write into bucket
        _buckets[offset] ??= new Bucket<TValue>();
        _buckets[offset]!.Add(value, log);
        
        // aggregate up the pyramid
        var pos = offset + _startOfBase;
        while (pos > 0)
        {
            _stack[pos] = _aggregate.Add(_stack[pos], value);
            pos /= 2;
        }
        _stack[0] = _aggregate.Add(_stack[0], value);
    }

    public void ReadRange(List<TSum> result, int level, int left, int right)
    {
        if (left < 0 || left >= _width) throw new Exception("pyramid left out of range");
        if (right < 0 || right >= _width) throw new Exception("pyramid right out of range");
        if (level < 0 || level > _height) throw new Exception("pyramid level out of range");
        
        var folds = _height - level;

        left += _startOfBase;
        right += _startOfBase;
        for (var i = 0; i < folds; i++)
        {
            left /= 2;
            right /= 2;
        }

        for (var i = left; i <= right; i++)
        {
            result.Add(_stack[i]); // TODO: would be nice to add the actual sample range here
        }
    }
}

internal class Bucket<TValue>
{
    private readonly List<Tuple<TValue, object?>> _allValues;
    public Bucket()
    {
        _allValues = new List<Tuple<TValue, object?>>();
    }

    public void Add(TValue value, object? log)
    {
        _allValues.Add(new Tuple<TValue, object?>(value, log));
    }
}