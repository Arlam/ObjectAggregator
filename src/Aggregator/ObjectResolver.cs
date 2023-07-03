using Aggregator.Models;

namespace Aggregator;

public class ObjectResolver
{
    private readonly IStorage _storage;

    public ObjectResolver(IStorage storage)
    {
        this._storage = storage;
    }

    public List<Row> FindAll(RowLocator locator, Table table)
    {
        var rows = this._storage.FindRows(locator, new Client("id", new SourceSystem("")));
        rows.ConvertAll(row => toParentLocator(row, table));

        return new List<Row>();
    }

    private RowLocator toParentLocator(Row row, Table table)
    {
        var parentTableName = "";
        var keys = new List<Key>();

        RowLocator rowLocator = new RowLocator(parentTableName, keys);
        return rowLocator;
    }
}
