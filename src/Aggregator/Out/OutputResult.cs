namespace Aggregator.Out;

public class OutputResult
{
    public Dictionary<string, object> Data { get; }

    public OutputResult(string root, Dictionary<string, object> data)
    {
        this.Data = data;
        this.Data.Add("Root", root);
    }

}
