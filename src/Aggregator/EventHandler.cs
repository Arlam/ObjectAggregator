namespace Aggregator;

using Aggregator.Models;
using Aggregator.Out;

public class EventHandler
{
    private readonly Output _output;

    public EventHandler(Output output)
    {
        this._output = output;
    }

    public void OnEvent(Event evntObj)
    {
        this._output.Publish(new RootAggregate(), evntObj.Client);
    }

    private List<Table> CreateTables()
    {
        var tables = new List<Table>();

        Table.Of("Patient");
        

        return tables;
    }
}
