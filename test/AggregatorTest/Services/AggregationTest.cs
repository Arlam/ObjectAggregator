using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Aggregator;
using Aggregator.Models;
using Aggregator.Out;
using AggregatorTest.Out;
using AggregatorTest.In;
using AggregatorTest.Json;
using AggregatorTest.Service;

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
        this._handler = new EventHandler(this._storage, this._output);
    }

    private static IEnumerable<TestCaseData> GetTestCaseDatas()
    {
        DirectoryInfo dir = new DirectoryInfo("./test-cases");
        FileInfo[] files = dir.GetFiles("*.json", System.IO.SearchOption.AllDirectories);
        foreach (FileInfo file in files)
        {
            // if (file.FullName.EndsWith("/patient/intergy/wrongDiscriminator.json"))
            {
                var testCase = new TestCaseData(file);
                testCase.SetArgDisplayNames(file.FullName.Split("test-cases")[1]);
                yield return testCase;
            }
        }
    }

    [Test, TestCaseSource("GetTestCaseDatas")]
    public void TestAggregate(FileInfo fileInfo)
    {
        var names = fileInfo.FullName.Split("/");
        var sourceSystem = names[names.Length - 2];

        string json = new StreamReader(fileInfo.FullName).ReadToEnd();
        var options = new JsonSerializerOptions { IncludeFields = true, };
        options.Converters.Add(new ObjectConverter());

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

        var publishedMessages = this._output.State.ConvertAll(output => output.Data);

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
