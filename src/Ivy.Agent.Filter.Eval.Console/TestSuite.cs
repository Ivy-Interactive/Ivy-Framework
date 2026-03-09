using Ivy.Agent.Filter;

namespace Ivy.Agent.Filter.Eval.Console;

public record TestSuite(
    string Name,
    FieldMeta[] Fields,
    TestCase[] Tests
);

public record TestCase(
    string Filter,
    string[] Expected
);
