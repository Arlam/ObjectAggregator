using NUnit.Framework;
using System.IO;

namespace AggregatorTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        DirectoryInfo dir = new DirectoryInfo("./test-cases");
        TestContext.Out.WriteLine("=============NUNIT==============");
        FileInfo[] files = dir.GetFiles("*.json", System.IO.SearchOption.AllDirectories);
        System.Console.WriteLine(files);
        foreach(FileInfo file in files) {
            TestContext.WriteLine(file);
        }

        System.Console.WriteLine("===========================");
    }
}