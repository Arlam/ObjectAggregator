using Aggregator.Relations;

namespace Aggregator.Models;

public class Relation
{
    private Table _parent;
    private Table _child;
    private RelationType _type;

    public Relation(Table parent, Table child, RelationType type)
    {
        this._parent = parent;
        this._child = child;
        this._type = type;
    }

    public Table On(Condition condition)
    {
        switch (this._type)
        {
            case RelationType.ONE_TO_ONE:
                this._parent.OneToOne.Add(this._child);
                break;
            case RelationType.ONE_TO_MANY:
                this._parent.OneToMany.Add(this._child);
                break;
        }
        var join = new Join();
        join.ParentTableDefinition = condition.Definitions[this._parent.Name];
        join.ThisTableDefinition = condition.Definitions[this._child.Name];

        this._child.Join = join;
        this._child.Parent = this._parent;
        return this._parent;
    }
}

public enum RelationType
{
    ONE_TO_ONE,
    ONE_TO_MANY
}
