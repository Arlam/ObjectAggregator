using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AggregatorTest.In;

public record TestEvent(List<String> pk, String table);
