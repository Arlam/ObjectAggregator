using System.Collections.Generic;
using Aggregator;
using Aggregator.Models;

namespace AggregatorTest.Service;

public class LocalStorage : IStorage
{
    private IDictionary<string, Dictionary<string, List<Row>>> _cache =
        new Dictionary<string, Dictionary<string, List<Row>>>();

    public void PutRow(string sourceSystem, RowLocator locator, Row row)
    {
        var tables = GetOrCreateDefaultAndGet<string, Dictionary<string, List<Row>>>(
            this._cache,
            sourceSystem,
            new Dictionary<string, List<Row>>()
        );

        var rows = GetOrCreateDefaultAndGet<string, List<Row>>(
            tables,
            locator.TableName,
            new List<Row>()
        );

        var rowIndex = rows.FindIndex(row => this.AllMatches(row, locator.Keys));
        if (rowIndex == -1)
        {
            rows.Add(row);
        }
        else
        {
            rows[rowIndex] = row;
        }
    }

    private static TValue GetOrCreateDefaultAndGet<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue
    )
    {
        TValue value;
        if (dictionary.TryGetValue(key, out value))
        {
            return value;
        }
        else
        {
            dictionary.Add(key, defaultValue);
            return defaultValue;
        }
    }

    private bool AllMatches(Row row, List<Key> columns)
    {
        foreach (Key column in columns)
        {
            var columnExists = row.Data.ContainsKey(column.ColumnName);
            var same = columnExists && column.Value.Equals(row.Data[column.ColumnName]);
            if (!same)
            {
                return false;
            }
        }
        return true;
    }

    public List<Row> FindRows(RowLocator locator, Client client)
    {
        if (!this._cache.ContainsKey(client.sourceSystem.Name))
        {
            return new List<Row>();
        }
        var tables = this._cache[client.sourceSystem.Name];
        if (!tables.ContainsKey(locator.TableName))
        {
            return new List<Row>();
        }
        return tables[locator.TableName].FindAll(row =>
        {
            var isEqual = true;
            locator.Keys.ForEach(key =>
            {
                if (!row.Data[key.ColumnName].Equals(key.Value))
                {
                    isEqual = false;
                }
            });
            return isEqual;
        });
    }
}
