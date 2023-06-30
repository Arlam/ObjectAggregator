using Aggregator.Models;
using Aggregator.Relations;

namespace Aggregator;

public class LocatorResolver
{
    public RowLocator buildLocator(
        Row row,
        string tableName,
        Dictionary<string, IValueResolver> definitions
    )
    {
        var keys = new List<Key>();
        foreach (KeyValuePair<string, IValueResolver> entry in definitions)
        {
            var columnName = entry.Key;
            var fkValue = entry.Value.GetValue(row);
            keys.Add(new Key(columnName, fkValue));
        }

        return new RowLocator(tableName, keys);
    }
}
