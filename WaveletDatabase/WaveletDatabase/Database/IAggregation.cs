namespace WaveletDatabase.Database;

public interface IAggregation<TSum, in TValue>
{
    TSum Add(TSum? existing, TValue newValue);
}

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
}

public class MinMax<T> where T:notnull
{
    public MinMax(T baseline)
    {
        Min = baseline;
        Max = baseline;
    }
    
    public T Min { get; set; }
    public T Max { get; set; }
}