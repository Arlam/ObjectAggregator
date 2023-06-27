using Aggregator.Models;

namespace Aggregator;

public interface IStorage
{
    List<Row> FindRows(RowLocator Locator, Client Client);
}
