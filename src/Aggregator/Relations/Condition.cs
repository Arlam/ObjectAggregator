using Aggregator.Models;

namespace Aggregator.Relations;

public class Condition
{
    //TODO: Change to Dictionary<string, List<IValueResolver>> to support multiple conditions for the same column
    public Dictionary<string, Dictionary<string, IValueResolver>> Definitions { get; }

    public Condition()
    {
        this.Definitions = new Dictionary<string, Dictionary<string, IValueResolver>>();
    }

    public Condition And(Condition condition)
    {
        foreach (var table in condition.Definitions)
        {
            foreach (var column in table.Value)
            {
                this.Definitions[table.Key].Add(column.Key, column.Value);
            }
        }

        return this;
    }

    /**
    * {param name="field1"} "TableName1.ColumnName"
    * {param name="field2"} "TableName2.ColumnName"
    **/
    public static Condition Eq(string column1, string column2)
    {
        var tableColumn1 = column1.Split("."); 
        var tableColumn2 = column2.Split(".");

        var condition = new Condition();
        condition.Definitions.Add(tableColumn1[0], new Dictionary<string, IValueResolver>());
        condition.Definitions.Add(tableColumn2[0], new Dictionary<string, IValueResolver>());

        condition.Definitions[tableColumn1[0]].Add(tableColumn1[1], new ColumnValueResolver(tableColumn2[1]));
        condition.Definitions[tableColumn2[0]].Add(tableColumn2[1], new ColumnValueResolver(tableColumn1[1]));
        return condition;
    }

    public static Condition EqConst(string fullColumnName, object constant)
    {
        var tableColumn = fullColumnName.Split("."); //TODO: handle different cases

        var condition = new Condition();
        condition.Definitions.Add(tableColumn[0], new Dictionary<string, IValueResolver>());
        condition.Definitions[tableColumn[0]].Add(
            tableColumn[1],
            new ConstatntValueResolver(constant)
        );
        return condition;
    }
}
