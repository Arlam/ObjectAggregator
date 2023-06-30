namespace Aggregator.Relations;

public record Join
{
    public Dictionary<string, IValueResolver> ParentTableDefinition { get; set; }
    public Dictionary<string, IValueResolver> ThisTableDefinition { get; set; }
}
