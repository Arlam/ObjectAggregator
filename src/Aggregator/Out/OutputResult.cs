namespace Aggregator.Out;

public class OutputResult
{
    private Dictionary<string, object> _data { get; }

    public OutputResult(Dictionary<string, object> data)
    {
        this._data = data;
    }

    public Dictionary<string, object> GetData()
    {
        return this._data;
    }
}
