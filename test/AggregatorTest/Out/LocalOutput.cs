using System.Collections.Generic;
using Aggregator.Models;
using Aggregator.Out;

namespace AggregatorTest.Out;

public class LocalOutput : Output
{
    public List<OutputResult> State {get; } = new List<OutputResult>();



    public void Publish(OutputResult agg, Client client)
    {
        this.State.Add(agg);
    }
}
