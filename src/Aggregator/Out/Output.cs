using Aggregator.Models;

namespace Aggregator.Out;

public interface Output
{
    void Publish(RootAggregate agg, Client client);
}
