using Aggregator.Models;

namespace Aggregator.Relations;

public interface IValueResolver
{
    public object GetValue(Row row);
}

public class ColumnValueResolver : IValueResolver
{
    private readonly string _column;

    public ColumnValueResolver(string column)
    {
        this._column = column;
    }

    public object GetValue(Row row)
    {
        return row.Data[this._column];
    }
}

public class ConstatntValueResolver : IValueResolver
{
    private readonly object _constatnt;

    public ConstatntValueResolver(object constatnt)
    {
        this._constatnt = constatnt;
    }

    //TODO: taking unneeded row is not good idea. Has to be changed somehow 

    public object GetValue(Row row)
    {
        return this._constatnt;
    }
}
