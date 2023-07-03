using NUnit.Framework;
using Aggregator;
using Aggregator.Models;
using Aggregator.Services;
using AggregatorTest.Service;
using System.Collections.Generic;
using static Aggregator.Relations.Condition;
using System;

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
        var output = this._aggregatorService.AggregateOutput(locator, table, client);
        var actualData = output.Data;
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

        var output = this._aggregatorService.AggregateOutput(patientLocator, patient, client);
        var actualData = output.Data;

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

        var output = this._aggregatorService.AggregateOutput(patientLocator, patient, client);
        var actualData = output.Data;

        Assert.AreEqual(44, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        Assert.IsFalse(actualData.ContainsKey("Address"));
    }

    [Test]
    public void testAggreagate_WithOneToMany()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToMany(Table.Of("PatientEncounter"))
            .On(
                Eq("Patient.id", "PatientEncounter.patientId")
                    .And(EqConst("PatientEncounter.datePerformed", "2012-01-22"))
            );

        var patientLocator = this.CreatePatient(42, "FirstName", "LastName");
        var addressLocator1 = this.CreatePatientEncounter(1, 41, 1, "2012-01-21");
        var addressLocator2 = this.CreatePatientEncounter(2, 42, 2, "2012-01-22");
        var addressLocator3 = this.CreatePatientEncounter(3, 43, 3, "2012-01-23");
        var addressLocator4 = this.CreatePatientEncounter(4, 42, 4, "2012-01-24");
        var addressLocator5 = this.CreatePatientEncounter(5, 42, 5, "2012-01-22");

        var output = this._aggregatorService.AggregateOutput(patientLocator, patient, client);
        var actualData = output.Data;

        Assert.AreEqual(42, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        var actualPatientEncounters =
            (List<Dictionary<string, object>>)actualData["PatientEncounter"];
        Assert.AreEqual(2, actualPatientEncounters.Count);
        Assert.AreEqual("2012-01-22", actualPatientEncounters[0]["datePerformed"]);
        Assert.AreEqual(2, actualPatientEncounters[0]["id"]);
        Assert.AreEqual("2012-01-22", actualPatientEncounters[1]["datePerformed"]);
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
        var addressLocator1 = this.CreatePatientEncounter(1, 41, 1, "2012-01-22");
        var addressLocator2 = this.CreatePatientEncounter(2, 42, 2, "2012-01-22");
        var addressLocator3 = this.CreatePatientEncounter(3, 43, 3, "2012-01-22");
        var addressLocator4 = this.CreatePatientEncounter(4, 42, 4, "2012-01-22");
        var addressLocator5 = this.CreatePatientEncounter(5, 42, 5, "2012-01-22");

        var output = this._aggregatorService.AggregateOutput(patientLocator, patient, client);
        var actualData = output.Data;

        Assert.AreEqual(44, actualData["id"]);
        Assert.AreEqual("FirstName", actualData["firstName"]);
        Assert.AreEqual("LastName", actualData["lastName"]);
        var actualPatientEncounters =
            (List<Dictionary<string, object>>)actualData["PatientEncounter"];
        Assert.AreEqual(0, actualPatientEncounters.Count);
    }

    [Test]
    public void TestFindRootObjects()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToOne(
                Table
                    .Of("PatientEncounter")
                    .JoinOneToOne(Table.Of("PatientEncounterDetail"))
                    .On(
                        Eq("PatientEncounter.patientEncounterDetailId", "PatientEncounterDetail.id")
                    )
            )
            .On(Eq("Patient.id", "PatientEncounter.patientId"));

        var patientEncounterDetail = patient
            .Relations()
            .Find(table => table.Name.Equals("PatientEncounter"))
            .Relations()
            .Find(table => table.Name.Equals("PatientEncounterDetail"));

        var patientLocator = this.CreatePatient(41, "FirstName", "LastName");
        var addressLocator1 = this.CreatePatientEncounter(1, 41, 11, "2012-01-22");
        var addressLocator2 = this.CreatePatientEncounter(2, 42, 12, "2012-01-22");
        var encounterDetail12 = this.CreatePatientEncounterDetail(12, "12123");
        var encounterDetail11 = this.CreatePatientEncounterDetail(11, "12123");

        var root = this._aggregatorService.FindRootObjects(
            encounterDetail11,
            patientEncounterDetail,
            client
        );
        Assert.AreEqual(1, root.Count);
        Assert.AreEqual("FirstName", root[0].Item2.Data["firstName"]);
        Assert.AreEqual(patient, root[0].Item1);
    }

    [Test]
    public void TestFindRootObjects_Multiple()
    {
        var client = new Client("id", new SourceSystem("source-a"));
        var patient = Table
            .Of("Patient")
            .JoinOneToOne(
                Table
                    .Of("PatientEncounter")
                    .JoinOneToOne(Table.Of("PatientEncounterDetail"))
                    .On(
                        Eq("PatientEncounter.patientEncounterDetailId", "PatientEncounterDetail.id")
                    )
            )
            .On(Eq("Patient.id", "PatientEncounter.patientId"));

        var patientEncounterDetail = patient
            .Relations()
            .Find(table => table.Name.Equals("PatientEncounter"))
            .Relations()
            .Find(table => table.Name.Equals("PatientEncounterDetail"));

        var patientLocator = this.CreatePatient(41, "FirstName", "LastName");
        var addressLocator1 = this.CreatePatientEncounter(1, 41, 11, "2012-01-22");
        var addressLocator2 = this.CreatePatientEncounter(2, 42, 12, "2012-01-22");
        var encounterDetail12 = this.CreatePatientEncounterDetail(12, "12123");
        var encounterDetail11 = this.CreatePatientEncounterDetail(11, "12123");

        var root = this._aggregatorService.FindRootObjects(
            encounterDetail11,
            patientEncounterDetail,
            client
        );
        Assert.AreEqual(1, root.Count);
        Assert.AreEqual("FirstName", root[0].Item2.Data["firstName"]);
        Assert.AreEqual(patient, root[0].Item1);
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

    private RowLocator CreatePatientEncounter(
        int encounterId,
        int patientId,
        int patientEncounterDetailId,
        string datePerformed
    )
    {
        var keys = new List<Key>() { new Key("id", encounterId) };
        var locator = new RowLocator("PatientEncounter", keys);
        var row = new Row(
            new Dictionary<string, object>()
            {
                { "id", encounterId },
                { "patientId", patientId },
                { "patientEncounterDetailId", patientEncounterDetailId },
                { "datePerformed", datePerformed }
            }
        );
        this._storage.PutRow("source-a", locator, row);
        return locator;
    }

    private RowLocator CreatePatientEncounterDetail(int id, string providerNPI)
    {
        var keys = new List<Key>() { new Key("id", id) };
        var locator = new RowLocator("PatientEncounterDetail", keys);
        var row = new Row(
            new Dictionary<string, object>() { { "id", id }, { "providerNPI", providerNPI } }
        );
        this._storage.PutRow("source-a", locator, row);
        return locator;
    }

    private RowLocator CreatePatientEncounterDiagnosis(int id, string providerNPI)
    {
        var keys = new List<Key>() { new Key("id", id) };
        var locator = new RowLocator("PatientEncounterDetail", keys);
        var row = new Row(
            new Dictionary<string, object>() { { "id", id }, { "providerNPI", providerNPI } }
        );
        this._storage.PutRow("source-a", locator, row);
        return locator;
    }
}
