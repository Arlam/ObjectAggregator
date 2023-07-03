using Aggregator.Models;
using Aggregator.Out;

namespace Aggregator.Services;

public class AggregatorService
{
    private readonly IStorage _storage;
    private readonly LocatorResolver _locatorResolver;

    public AggregatorService(IStorage storage, LocatorResolver locatorResolver)
    {
        this._storage = storage;
        this._locatorResolver = locatorResolver;
    }

    public OutputResult AggregateOutput(RowLocator rootLocator, Table rootTable, Client client)
    {
        var row = this._storage.FindRow(rootLocator, client);
        var data = this.getRowData(row, rootTable, client);
        var output = new OutputResult(rootTable.Name, data);
        return output;
    }

    public OutputResult AggregateOutput(Row row, Table rootTable, Client client)
    {
        var data = this.getRowData(row, rootTable, client);
        var output = new OutputResult(rootTable.Name, data);
        return output;
    }

    private Dictionary<string, object> getRowData(Row row, Table rootTable, Client client)
    {
        if (row == null)
        {
            return null;
        }
        var data = new Dictionary<string, object>();
        foreach (KeyValuePair<string, object> entry in row.Data)
        {
            data.Add(entry.Key, entry.Value);
        }
        foreach (Table table in rootTable.OneToOne)
        {
            var locator = this._locatorResolver.buildLocator(
                row,
                table.Name,
                table.Join.ThisTableDefinition
            );
            var child = this._storage.FindRow(locator, client);
            var rowData = getRowData(child, table, client);
            if (rowData != null)
            {
                data.Add(table.GetFieldName(), rowData);
            }
        }
        foreach (Table table in rootTable.OneToMany)
        {
            var locator = this._locatorResolver.buildLocator(
                row,
                table.Name,
                table.Join.ThisTableDefinition
            );
            var childs = this._storage.FindRows(locator, client);
            var rowsData = new List<Dictionary<string, object>>();
            childs.ForEach(child =>
            {
                var rowData = getRowData(child, table, client);
                if (rowData != null)
                {
                    rowsData.Add(rowData);
                }
            });
            data.Add(table.GetFieldName(), rowsData);
        }
        return data;
    }

    public List<Tuple<Table, Row>> FindRootObjects(RowLocator locator, Table table, Client client)
    {
        List<Row> rows = this._storage.FindRows(locator, client);
        if (table.IsRoot())
        {
            return rows.ConvertAll(row => Tuple.Create(table, row));
        }

        return rows.ConvertAll(
                row =>
                    this._locatorResolver.buildLocator(
                        row,
                        table.Parent.Name,
                        table.Join.ParentTableDefinition
                    )
            )
            .SelectMany(parentLocator => this.FindRootObjects(parentLocator, table.Parent, client))
            .ToList();
    }
}
