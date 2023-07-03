using Aggregator.Models;

namespace Aggregator.Out;

public interface Output
{
    void Publish(OutputResult agg, Client client);
}
