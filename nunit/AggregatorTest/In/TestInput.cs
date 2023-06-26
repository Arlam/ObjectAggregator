using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AggregatorTest.In;
public record TestInput(TestEvent testEvent, Dictionary<String, Object> stateRecord);
