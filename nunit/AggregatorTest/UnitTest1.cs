using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AggregatorTest.In;

namespace AggregatorTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    private static IEnumerable<TestCaseData> GetTestCaseDatas()
    {
        DirectoryInfo dir = new DirectoryInfo("./test-cases");
        FileInfo[] files = dir.GetFiles("*.json", System.IO.SearchOption.AllDirectories);
        foreach (FileInfo file in files)
        {
            yield return new TestCaseData(file);
        }
    }

    [Test, TestCaseSource("GetTestCaseDatas")]
    public void Test1(FileInfo fileInfo)
    {
        TestContext.Out.WriteLine("=============NUNIT==============");
        TestContext.Out.WriteLine(fileInfo.Name);
        string json = new StreamReader(fileInfo.FullName).ReadToEnd();
        var options = new JsonSerializerOptions
        {
            IncludeFields = true,
        };
        var testCase = JsonSerializer.Deserialize<TestCase>(json, options)!;


        TestContext.Out.WriteLine(testCase);


        System.Console.WriteLine("===========================");
    }
}