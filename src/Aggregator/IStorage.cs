using Aggregator.Models;

namespace Aggregator;

public interface IStorage
{
    Row FindRow(RowLocator locator, Client client)
    {
        var rows = FindRows(locator, client);
        if (rows.Count == 1)
        {
            return rows[0];
        }
        if (rows.Count == 0)
        {
            return null;
        }
        throw new Exception("Expected one record but found " + rows.Count);
    }

    List<Row> FindRows(RowLocator locator, Client client);
}
