using System.Collections.Generic;
using Aggregator.Models;
using Aggregator.Out;

namespace AggregatorTest.Out;

public class LocalOutput : Output
{
    private readonly List<Dictionary<string, object>> _state =
        new List<Dictionary<string, object>>();

    public List<Dictionary<string, object>> GetState()
    {
        return this._state;
    }

    public void Publish(RootAggregate agg, Client client)
    {
        this._state.Add(new Dictionary<string, object>());
    }
}
