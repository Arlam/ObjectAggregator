using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AggregatorTest.In;

public record TestEvent(
    [property: JsonPropertyName("pk")] List<String> PK, 
    [property: JsonPropertyName("table")] String Table);
