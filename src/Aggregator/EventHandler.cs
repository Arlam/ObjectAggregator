namespace Aggregator;

using System;
using System.Collections.Generic;
using Aggregator;
using Aggregator.Services;
using Aggregator.Models;
using Aggregator.Out;
using static Aggregator.Relations.Condition;

public class EventHandler
{
    private readonly Output _output;
    private readonly LocatorResolver _locatorResolver;
    private readonly AggregatorService _aggregatorService;
    private readonly IStorage _storage;
    private readonly Dictionary<string, List<Table>> _tables;

    public EventHandler(IStorage storage, Output output)
    {
        this._storage = storage;
        this._output = output;
        this._tables = CreateTables();
        this._locatorResolver = new LocatorResolver();
        this._aggregatorService = new AggregatorService(this._storage, this._locatorResolver);
    }

    public void OnEvent(Event evntObj)
    {
        var locator = evntObj.Locator;
        var client = evntObj.Client;
        var updatedRow = this._storage.FindRow(locator, client);
        var tableName = evntObj.Locator.TableName;

        this.GetTables(client)
            .SelectMany(schema => this.GetAffectedTables(schema, tableName))
            .SelectMany(table => this._aggregatorService.FindRootObjects(locator, table, client))
            .ToList()
            .ConvertAll(
                pair => this._aggregatorService.AggregateOutput(pair.Item2, pair.Item1, client)
            )
            .ForEach(result => this._output.Publish(result, client));
    }

    private List<Table> GetTables(Client client)
    {
        if (this._tables.ContainsKey(client.sourceSystem.Name))
        {
            return this._tables[client.sourceSystem.Name];
        }
        return new List<Table>();
    }

    private List<Table> GetAffectedTables(Table table, string tableName)
    {
        var matched = table
            .Relations()
            .SelectMany(child => this.GetAffectedTables(child, tableName))
            .ToList();
        if (string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
        {
            matched.Add(table);
        }
        return matched;
    }

    private Dictionary<string, List<Table>> CreateTables()
    {
        var integrityTables = new List<Table>
        {
            Table
                .Of("Patient")
                .JoinOneToOne(Table.Of("Address"))
                .On(
                    Eq("Address.PersonId", "Patient.Id")
                        .And(EqConst("Address.Discriminator", "Patient"))
                )
                .JoinOneToOne(Table.Of("ContactInfo"))
                .On(Eq("Patient.Id", "ContactInfo.PatientId")),
            Table
                .Of("Physician")
                .JoinOneToOne(Table.Of("Principal"))
                .On(Eq("Physician.PrincipalId", "Principal.PrincipalId"))
                .JoinOneToOne(Table.Of("ProviderGroup"))
                .On(Eq("Physician.ProviderGroupId", "ProviderGroup.ProviderGroupId"))
                .JoinOneToMany(
                    Table
                        .Of("PhysicianPhysicianOrganization")
                        .JoinOneToOne(Table.Of("PhysicianOrganization"))
                        .On(
                            Eq(
                                "PhysicianPhysicianOrganization.PhysicianOrganizationId",
                                "PhysicianOrganization.Id"
                            )
                        )
                )
                .On(Eq("Physician.PhysicianId", "PhysicianPhysicianOrganization.PhysicianId")),
            Table
                .Of("CostRecord")
                .JoinOneToOne(Table.Of("PatientEncounter"))
                .On(
                    Eq("CostRecord.DomainRecordId", "PatientEncounter.Id")
                        .And(EqConst("CostRecord.Discriminator", "encounter"))
                )
                .JoinOneToOne(
                    Table
                        .Of("PatientLab")
                        .JoinOneToOne(Table.Of("PatientLabDetail"))
                        .On(Eq("PatientLab.PatientLabDetailId", "PatientLabDetail.Id"))
                )
                .On(
                    Eq("CostRecord.DomainRecordId", "PatientLab.Id")
                        .And(EqConst("CostRecord.Discriminator", "lab"))
                ),
            Table
                .Of("PatientEncounter")
                .JoinOneToOne(Table.Of("PatientEncounterDetail"))
                .On(Eq("PatientEncounter.PatientEncounterDetailId", "PatientEncounterDetail.Id"))
                .JoinOneToMany(
                    Table
                        .Of("PatientEncounterDiagnosis")
                        .JoinOneToMany(Table.Of("PatientEncounterDiagnosisDetail"))
                        .On(
                            Eq(
                                "PatientEncounterDiagnosis.Id",
                                "PatientEncounterDiagnosisDetail.PatientEncounterDiagnosisId"
                            )
                        )
                )
                .On(Eq("PatientEncounter.Id", "PatientEncounterDiagnosis.PatientEncounterId")),
        };
        var primeSuteTables = new List<Table>()
        {
            Table
                .Of("DictionaryEntry")
                .JoinOneToOne(
                    Table
                        .Of("Dictionary")
                        .JoinOneToMany(
                            Table
                                .Of("ApplicationDictionary")
                                .JoinOneToOne(Table.Of("Application"))
                                .On(Eq("ApplicationDictionary.ApplicationId", "Application.Id"))
                        )
                        .On(Eq("Dictionary.Name", "ApplicationDictionary.DictionaryName"))
                )
                .On(Eq("DictionaryEntry.DictionaryName", "Dictionary.Name"))
                .JoinOneToMany(Table.Of("DictionaryEntryMapping").As("DictionaryEntryMapping1"))
                .On(
                    Eq("DictionaryEntry.DictionaryName", "DictionaryEntryMapping.DictionaryName1")
                        .And(Eq("DictionaryEntry.Code", "DictionaryEntryMapping.Code1"))
                )
                .JoinOneToMany(Table.Of("DictionaryEntryMapping").As("DictionaryEntryMapping2"))
                .On(
                    Eq("DictionaryEntry.DictionaryName", "DictionaryEntryMapping.DictionaryName2")
                        .And(Eq("DictionaryEntry.Code", "DictionaryEntryMapping.Code2"))
                )
        };

        Dictionary<string, List<Table>> tables = new Dictionary<string, List<Table>>();
        tables.Add("intergy", integrityTables);
        tables.Add("prime_suite", primeSuteTables);
        return tables;
    }
}
