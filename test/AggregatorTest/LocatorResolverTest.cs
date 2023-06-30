using NUnit.Framework;
using Aggregator;
using Aggregator.Relations;
using Aggregator.Models;
using System.Collections.Generic;

namespace AggregatorTest;

public class LocatorResolverTest
{
    private LocatorResolver _locatorResolver;

    [SetUp]
    public void Setup()
    {
        this._locatorResolver = new LocatorResolver();
    }

    [Test]
    public void testCreateLocator()
    {
        var row = new Row(
            new Dictionary<string, object>() { { "patientId", 42 }, { "phone", "911" } }
        );

        var definition = new Dictionary<string, IValueResolver>()
        {
            { "id", new ColumnValueResolver("patientId") },
            { "active", new ConstatntValueResolver(true) }
        };

        var rowLocator = this._locatorResolver.buildLocator(row, "Patient", definition);

        Assert.AreEqual("Patient", rowLocator.TableName);
        Assert.AreEqual(2, rowLocator.Keys.Count);
        Assert.AreEqual("id", rowLocator.Keys[0].ColumnName);
        Assert.AreEqual(42, rowLocator.Keys[0].Value);
        Assert.AreEqual("active", rowLocator.Keys[1].ColumnName);
        Assert.AreEqual(true, rowLocator.Keys[1].Value);
    }
}
