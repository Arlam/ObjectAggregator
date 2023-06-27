using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AggregatorTest.In;
public record TestInput(
    [property: JsonPropertyName("event")] TestEvent TestEvent, 
    [property: JsonPropertyName("stateRecord")] Dictionary<String, Object> StateRecord);
