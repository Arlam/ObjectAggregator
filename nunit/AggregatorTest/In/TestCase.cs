using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AggregatorTest.In;

public record TestCase(List<TestInput> inputRecords, List<Dictionary<String, Object>> expectedOutput);
