using Ivy.NavigationBacon;
using Ivy.Samples.Shared.Apps.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.Samples.Shared.Apps.Hidden;


public record TestNavigationBeaconAppArgs(string id);

[App(isVisible: false)]
[NavigationBeacon(typeof(TestBeacon), nameof(GetNavigationBeacon))]
public class TestNavigationBeaconApp : ViewBase
{
    private List<TestBeacon> listTestBeacon = new List<TestBeacon>()
    {
        new TestBeacon("1","This is TestBeacon 1"),
        new TestBeacon("2","This is TestBeacon 2"),
        new TestBeacon("3","This is TestBeacon 3")
    };

    public static NavigationBeacon<TestBeacon> GetNavigationBeacon()
    {
        return new NavigationBeacon<TestBeacon>(
            testBeacon => new NavigateArgs("hidden/test-navigation-beacon-app", new TestNavigationBeaconAppArgs(testBeacon.id))
        );
    }
    public override object? Build()
    {
        TestNavigationBeaconAppArgs? args = UseArgs<TestNavigationBeaconAppArgs>();
        if (args != null)
        {
            TestBeacon? test = listTestBeacon.FirstOrDefault(t => t.id == args.id);
            if (test != null)
                return Text.H1(test.Name);
        }
        return null;

    }
}

