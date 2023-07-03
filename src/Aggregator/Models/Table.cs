using Aggregator.Relations;

namespace Aggregator.Models;

public class Table
{
    public string Name { get; }
    public string _alias;
    public List<Table> OneToOne { get; } = new List<Table>();
    public List<Table> OneToMany { get; } = new List<Table>();
    public Join Join { get; set; }

    public Table Parent { get; set; }

    private Table(string name)
    {
        this.Name = name;
    }

    public Relation JoinOneToOne(Table child)
    {
        return new Relation(this, child, RelationType.ONE_TO_ONE);
    }

    public Relation JoinOneToMany(Table child)
    {
        return new Relation(this, child, RelationType.ONE_TO_MANY);
    }

    public bool IsRoot()
    {
        return this.Parent == null;
    }

    public Table As(string alias)
    {
        this._alias = alias;
        return this;
    }

    public string GetFieldName()
    {
        if (this._alias != null)
        {
            return this._alias;
        }
        return this.Name;
    }

    public List<Table> Relations()
    {
        return new List<Table>().Concat(this.OneToOne).Concat(this.OneToMany).ToList();
    }

    public static Table Of(string name)
    {
        return new Table(name);
    }
}
