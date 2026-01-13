using System.Text.Json;
using Ivy.Apps;
using Ivy.Core;
using Ivy.Shared;
using Ivy.Test.Apps.Foo.Bar;
using Xunit.Abstractions;

namespace Ivy.Test
{
    public class AppRepositoryTests(ITestOutputHelper output)
    {
        [Fact]
        public void Test1()
        {
            var repository = new AppRepository();
            repository.AddFactory(() => [
                AppHelpers.GetApp(typeof(X1)),
                AppHelpers.GetApp(typeof(_Index))
            ]);
            repository.Reload();
            var menuItems = repository.GetMenuItems();
            output.WriteLine(JsonSerializer.Serialize(menuItems, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}

namespace Ivy.Test.Apps.Foo.Bar
{
    [App()]
    public class X1 : ViewBase
    {
        public override object? Build()
        {
            return null;
        }
    }

    [App(order: 99, icon: Icons.Flashlight, groupExpanded: true)]
    public class _Index : ViewBase
    {
        public override object? Build()
        {
            return null;
        }
    }
}


