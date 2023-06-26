using Xunit;
using System.IO;

namespace AggregatorTest;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {

        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo("./test-cases");
        System.Console.WriteLine("===========================");
        FileInfo[] files = dir.GetFiles("*.json", System.IO.SearchOption.AllDirectories);
        System.Console.WriteLine(files);
        foreach(FileInfo file in files) {
            System.Console.WriteLine(file);
        }

        System.Console.WriteLine("===========================");
        // Assert.Pass();
        // throw new NotImplementedException("Not fully implemented.");
    }
}