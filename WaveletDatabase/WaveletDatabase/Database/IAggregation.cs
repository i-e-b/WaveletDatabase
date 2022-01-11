namespace WaveletDatabase.Database;

public interface IAggregation<TSum, in TValue> where TSum: IAggregateValue
{
    /// <summary>
    /// Aggregate a new value into the existing sum
    /// </summary>
    TSum Add(TSum? existing, TValue newValue);
    
    /// <summary>
    /// Returns true if 'haystack' contains 'needle'
    /// </summary>
    /// <param name="needle">A value or range we want to find</param>
    /// <param name="haystack">The aggregate data being tested</param>
    bool Contains(TSum needle, TSum haystack);
}

public interface IAggregateValue
{
    /// <summary>
    /// Other will be an instance of the same aggregate type, or the individual value type
    /// Return true if we consider 'other' is contained in this
    /// </summary>
    bool Contains(object other);
}
