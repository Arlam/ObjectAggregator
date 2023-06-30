using NUnit.Framework;
using Aggregator;
using Aggregator.Models;
using Aggregator.Out;
using AggregatorTest.Storage;
using System.Collections.Generic;
using static Aggregator.Relations.Condition;

namespace AggregatorTest.Out;

public class AggregatorServiceTest
{
    private LocalStorage _storage;
    private LocatorResolver _locatorResolver;
    private AggregatorService _aggregatorService;

    [SetUp]
    public void Setup()
    {
        this._storage = new LocalStorage();
        this._locatorResolver = new LocatorResolver();
        this._aggregatorService = new AggregatorService(this._storage, this._locatorResolver);
    }

    [Test]
    public void testAggreagate_NoChildren()
    {
        var locator = this.CreatePatient(42, "FirstName", "LastName");

        var table = Table.Of("Patient");

        var client = new Client("id", new SourceSystem("source-a"));
        var output = this._aggregatorService.aggregateOutput(locator, table, client);
        var actualData = output.GetData();
        Assert.AreEqual(42, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
    }

    [Test]
    public void testAggreagate_WithOneToOne()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToOne(Table.Of("Address"))
            .On(Eq("Patient.id", "Address.patientId"));

        var patientLocator = this.CreatePatient(42, "FirstName", "LastName");
        var addressLocator1 = this.CreateAddress(1, 41, "Street-1");
        var addressLocator2 = this.CreateAddress(2, 42, "Street-2");
        var addressLocator3 = this.CreateAddress(3, 43, "Street-3");

        var output = this._aggregatorService.aggregateOutput(patientLocator, patient, client);
        var actualData = output.GetData();

        Assert.AreEqual(42, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        var actualAddress = (Dictionary<string, object>)actualData["Address"];
        Assert.AreEqual(2, actualAddress["id"]);
        Assert.AreEqual("Street-2", actualAddress["street"]);
    }

    [Test]
    public void testAggreagate_WithOneToOneButNoChildYet()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToOne(Table.Of("Address"))
            .On(Eq("Patient.id", "Address.patientId"));
        var patientLocator = this.CreatePatient(44, "FirstName", "LastName");
        var addressLocator1 = this.CreateAddress(1, 41, "Street-1");
        var addressLocator2 = this.CreateAddress(2, 42, "Street-2");
        var addressLocator3 = this.CreateAddress(3, 43, "Street-3");

        var output = this._aggregatorService.aggregateOutput(patientLocator, patient, client);
        var actualData = output.GetData();

        Assert.AreEqual(44, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        Assert.IsNull(actualData["Address"]);
    }

    [Test]
    public void testAggreagate_WithOneToMany()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToMany(Table.Of("PatientEncounter"))
            .On(Eq("Patient.id", "PatientEncounter.patientId").And(EqConst("PatientEncounter.code", "99232")));

        var patientLocator = this.CreatePatient(42, "FirstName", "LastName");
        var addressLocator1 = this.CreatePatientEncounter(1, 41, "99231");
        var addressLocator2 = this.CreatePatientEncounter(2, 42, "99232");
        var addressLocator3 = this.CreatePatientEncounter(3, 43, "99233");
        var addressLocator4 = this.CreatePatientEncounter(4, 42, "99234");
        var addressLocator5 = this.CreatePatientEncounter(5, 42, "99232");

        var output = this._aggregatorService.aggregateOutput(patientLocator, patient, client);
        var actualData = output.GetData();

        Assert.AreEqual(42, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        var actualPatientEncounters =
            (List<Dictionary<string, object>>)actualData["PatientEncounter"];
        Assert.AreEqual(2, actualPatientEncounters.Count);
        Assert.AreEqual("99232", actualPatientEncounters[0]["code"]);
        Assert.AreEqual(2, actualPatientEncounters[0]["id"]);
        Assert.AreEqual("99232", actualPatientEncounters[1]["code"]);
        Assert.AreEqual(5, actualPatientEncounters[1]["id"]);
    }

    [Test]
    public void testAggreagate_WithOneToManyButNoChildYet()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToMany(Table.Of("PatientEncounter"))
            .On(Eq("Patient.id", "PatientEncounter.patientId"));

        var patientLocator = this.CreatePatient(44, "FirstName", "LastName");
        var addressLocator1 = this.CreatePatientEncounter(1, 41, "99231");
        var addressLocator2 = this.CreatePatientEncounter(2, 42, "99232");
        var addressLocator3 = this.CreatePatientEncounter(3, 43, "99233");
        var addressLocator4 = this.CreatePatientEncounter(4, 42, "99234");
        var addressLocator5 = this.CreatePatientEncounter(5, 42, "99232");

        var output = this._aggregatorService.aggregateOutput(patientLocator, patient, client);
        var actualData = output.GetData();

        Assert.AreEqual(44, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        var actualPatientEncounters =
            (List<Dictionary<string, object>>)actualData["PatientEncounter"];
        Assert.AreEqual(0, actualPatientEncounters.Count);
    }

    private RowLocator CreatePatient(int patientId, string fName, string lName)
    {
        var keys = new List<Key>() { new Key("id", patientId) };
        var locator = new RowLocator("Patient", keys);
        var row = new Row(
            new Dictionary<string, object>()
            {
                { "id", patientId },
                { "firstName", fName },
                { "lastName", lName }
            }
        );
        this._storage.PutRow("source-a", locator, row);
        return locator;
    }

    private RowLocator CreateAddress(int addressId, int patientId, string streetName)
    {
        var keys = new List<Key>() { new Key("id", addressId) };
        var locator = new RowLocator("Address", keys);
        var row = new Row(
            new Dictionary<string, object>()
            {
                { "id", addressId },
                { "patientId", patientId },
                { "street", streetName }
            }
        );
        this._storage.PutRow("source-a", locator, row);
        return locator;
    }

    private RowLocator CreatePatientEncounter(int encounterId, int patientId, string code)
    {
        var keys = new List<Key>() { new Key("id", encounterId) };
        var locator = new RowLocator("PatientEncounter", keys);
        var row = new Row(
            new Dictionary<string, object>()
            {
                { "id", encounterId },
                { "patientId", patientId },
                { "code", code }
            }
        );
        this._storage.PutRow("source-a", locator, row);
        return locator;
    }
}
