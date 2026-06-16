using Ivy.Core.Apps;
using Ivy.Test.Apps.FooBaz.Bar;
using Ivy.Test.Apps.Jobs;

namespace Ivy.Test
{
    public class AppHelperTests
    {
        [Fact] void Test1() => Test(typeof(MyApp), "foo-baz/bar/my");
        [Fact] void Test2() => Test(typeof(_Index), "foo-baz/bar/_index");

        // A trailing folder that duplicates the type name is collapsed so an app in an
        // eponymous folder keeps a flat id (Apps/Jobs/JobsApp => "jobs", not "jobs/jobs").
        [Fact] void EponymousFolderIsDeduped() => Test(typeof(JobsApp), "jobs");

        // Genuine hierarchy is preserved when the folder differs from the type.
        [Fact] void DistinctFolderKeepsHierarchy() => Test(typeof(ReviewApp), "jobs/review");

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

namespace Ivy.Test.Apps.Jobs
{
    [App()]
    public class JobsApp : ViewBase
    {
        public override object? Build()
        {
            throw new NotImplementedException();
        }
    }

    [App()]
    public class ReviewApp : ViewBase
    {
        public override object? Build()
        {
            throw new NotImplementedException();
        }
    }
}
