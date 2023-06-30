using Aggregator.Relations;

namespace Aggregator.Models;

public class Table
{
    public string Name { get; }
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

    public static Table Of(string name)
    {
        return new Table(name);
    }
}
