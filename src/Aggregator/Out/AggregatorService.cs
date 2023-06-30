using Aggregator.Models;

namespace Aggregator.Out;

public class AggregatorService
{
    private readonly IStorage _storage;
    private readonly LocatorResolver _locatorResolver;

    public AggregatorService(IStorage storage, LocatorResolver locatorResolver)
    {
        this._storage = storage;
        this._locatorResolver = locatorResolver;
    }

    public OutputResult aggregateOutput(RowLocator rootLocator, Table rootTable, Client client)
    {
        var row = this._storage.FindRow(rootLocator, client);
        var data = this.getRowData(row, rootTable, client);
        var output = new OutputResult(data);
        return output;
    }

    private Dictionary<string, object> getRowData(Row row, Table rootTable, Client client)
    {
        if (row == null)
        {
            return null;
        }
        var data = row.Data;
        foreach (Table table in rootTable.OneToOne)
        {
            var locator = this._locatorResolver.buildLocator(row, table.Name, table.Join.ThisTableDefinition);
            var child = this._storage.FindRow(locator, client);
            data.Add(table.Name, getRowData(child, table, client));
        }
        foreach (Table table in rootTable.OneToMany)
        {
            var locator = this._locatorResolver.buildLocator(row, table.Name, table.Join.ThisTableDefinition);
            var childs = this._storage.FindRows(locator, client);
            var rowsData = new List<Dictionary<string, object>>();
            childs.ForEach(child =>
            {
                rowsData.Add(getRowData(child, table, client));
            });
            data.Add(table.Name, rowsData);
        }
        return data;
    }
}
