using Ivy.Filters;

namespace Ivy.Filters.Eval.Console;

public record TestSuite(
    string Name,
    FieldMeta[] Fields,
    TestCase[] Tests
);

public record TestCase(
    string Filter,
    string[] Expected
);
