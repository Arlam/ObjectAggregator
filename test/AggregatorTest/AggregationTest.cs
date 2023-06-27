using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using AggregatorTest.In;
using AggregatorTest.Out;
using Aggregator;
using Aggregator.Models;
using AggregatorTest.Storage;

namespace AggregatorTest;

public class AggregationTest
{
    private LocalStorage _storage;
    private LocalOutput _output;
    private EventHandler _handler;

    [SetUp]
    public void Setup()
    {
        this._storage = new LocalStorage();
        this._output = new LocalOutput();
        this._handler = new EventHandler(this._output);
    }

    private static IEnumerable<TestCaseData> GetTestCaseDatas()
    {
        DirectoryInfo dir = new DirectoryInfo("./test-cases");
        FileInfo[] files = dir.GetFiles("*.json", System.IO.SearchOption.AllDirectories);
        foreach (FileInfo file in files)
        {
            var testCase = new TestCaseData(file);
            testCase.SetArgDisplayNames(file.Name);
            yield return testCase;
        }
    }

    [Test, TestCaseSource("GetTestCaseDatas")]
    public void TestAggregate(FileInfo fileInfo)
    {
        var names = fileInfo.FullName.Split("/");
        var sourceSystem = names[names.Length - 2];

        string json = new StreamReader(fileInfo.FullName).ReadToEnd();
        var options = new JsonSerializerOptions { IncludeFields = true, };

        TestCase testCase = JsonSerializer.Deserialize<TestCase>(json, options)!;
        testCase.InputRecords.ForEach(record =>
        {
            Event eventObj = CreateEvent(record, sourceSystem);
            this._storage.PutRow(
                sourceSystem,
                new RowLocator(record.TestEvent.Table, GetPKs(record)),
                new Row(record.StateRecord)
            );
            this._handler.OnEvent(eventObj);
        });

        List<Dictionary<string, object>> publishedMessages = this._output.GetState();

        Assert.AreEqual(testCase.ExpectedOutput.Count, publishedMessages.Count);
    }

    private static Event CreateEvent(TestInput record, string sourceSystem)
    {
        var table = record.TestEvent.Table;
        List<Key> keys = GetPKs(record);
        var eventObj = new Event(
            new RowLocator(table, keys),
            new Client("Client-Id", new SourceSystem(sourceSystem))
        );
        return eventObj;
    }

    private static List<Key> GetPKs(TestInput record)
    {
        List<Key> keys = new List<Key>();
        record.TestEvent.PK.ForEach(pk =>
        {
            var keyValue = pk.Split("=");
            keys.Add(new Key(keyValue[0], keyValue[1]));
        });
        return keys;
    }
}
