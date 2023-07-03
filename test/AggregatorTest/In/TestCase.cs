using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AggregatorTest.In;

public record TestCase(
    [property: JsonPropertyName("inputRecords")] List<TestInput> InputRecords,
    [property: JsonPropertyName("expectedOutput")] List<Dictionary<String, Object>> ExpectedOutput
);
