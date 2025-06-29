using Ivy.Apps;
using Ivy.Core;
using Ivy.Test.Apps.FooBaz.Bar;
using Xunit.Abstractions;

namespace Ivy.Test
{
    public class AppHelperTests(ITestOutputHelper output)
    {
        [Fact] void Test1() => Test(typeof(MyApp), "foo-baz/bar/my-app");
        [Fact] void Test2() => Test(typeof(_Index), "foo-baz/bar/_index");

        private void Test(Type type, string expectedId)
        {
            var descriptor = AppHelpers.GetApp(type);
            Assert.Equal(expectedId, descriptor.Id);
        }
    }
}

namespace Ivy.Test.Apps.FooBaz.Bar
{
    [App()]
    public class MyApp : ViewBase
    {
        public override object? Build()
        {
            throw new NotImplementedException();
        }
    }

    [App()]
    public class _Index : ViewBase
    {
        public override object? Build()
        {
            throw new NotImplementedException();
        }
    }
}