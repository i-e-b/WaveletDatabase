namespace WaveletDatabase.Database;

public class MinMaxAggregation : IAggregation<MinMax<double>, double>
{
    private readonly double _baselineValue;

    public MinMaxAggregation(double baselineValue)
    {
        _baselineValue = baselineValue;
    }
    
    public MinMax<double> Add(MinMax<double>? existing, double newValue)
    {
        var dataPoint = existing ?? new MinMax<double>(_baselineValue);
        if (newValue > dataPoint.Max) dataPoint.Max = newValue;
        if (newValue < dataPoint.Min) dataPoint.Min = newValue;
        return dataPoint;
    }

    public bool Contains(MinMax<double> needle, MinMax<double> haystack)
    {
        return haystack.Contains(needle);
    }
}

/// <summary>
/// Upper and lower bounds of double values. Contains returns for any overlap in the range
/// </summary>
public class MinMax<T> : IAggregateValue where T : notnull, IComparable<T>
{
    public MinMax(T baseline)
    {
        Min = baseline;
        Max = baseline;
    }
    
    public MinMax(T min, T max)
    {
        Min = min;
        Max = max;
    }
    
    public T Min { get; set; }
    public T Max { get; set; }
    
    public bool Contains(object other)
    {
        return other switch
        {
            MinMax<T> pal => pal.Min.CompareTo(Max) <= 0 && pal.Max.CompareTo(Min) >= 0,
            T val => Max.CompareTo(val) >= 0 && Min.CompareTo(val) <= 0,
            _ => false
        };
    }
}