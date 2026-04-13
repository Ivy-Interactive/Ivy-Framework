using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Onboarding;

public class CodingAgentStepView(
    IState<int> stepperIndex,
    IReadOnlyDictionary<string, bool> checkResults,
    IReadOnlyDictionary<string, HealthCheckStatus?>? healthResults) : ViewBase
{
    private static readonly (string Label, string Name)[] AgentOptions = [("Claude", "claude"), ("Codex", "codex"), ("Gemini", "gemini")];
    private readonly (string Label, string Name)[] _installedOptions = GetInstalledOptions(checkResults, healthResults);

    private static (string Label, string Name)[] GetInstalledOptions(
        IReadOnlyDictionary<string, bool> checkResults,
        IReadOnlyDictionary<string, HealthCheckStatus?>? healthResults)
    {
        var installed = AgentOptions.Where(a => checkResults.ContainsKey(a.Name) && checkResults[a.Name]).ToArray();

        if (healthResults != null)
        {
            var healthy = installed.Where(a => healthResults.TryGetValue(a.Name, out var h) && h == HealthCheckStatus.Authenticated).ToArray();
            if (healthy.Length > 0)
                return healthy;
        }

        return installed.Length == 0 ? AgentOptions : installed;
    }

    public override object Build()
    {
        var config = UseService<IConfigService>();
        var selectedAgent = UseState(() =>
        {
            return _installedOptions.Any(a => a.Name == config.Settings.CodingAgent)
                ? _installedOptions.First(a => a.Name == config.Settings.CodingAgent).Label
                : _installedOptions.First().Label;
        });

        return Layout.Vertical().Margin(0, 0, 0, 20)
                | Text.H2("Choose Your Coding Agent")
                | Text.Muted("You can change this later under Settings.")
                | selectedAgent.ToSelectInput(_installedOptions.Select(a => a.Label).ToArray())
                   .Variant(SelectInputVariant.Toggle)
                   .WithField()
                   .Label("Coding Agent")
                | new Button("Continue").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .OnClick(() =>
                   {
                       var agent = _installedOptions.First(a => a.Label == selectedAgent.Value).Name;
                       config.Settings.CodingAgent = agent;
                       config.SetPendingCodingAgent(agent);
                       stepperIndex.Set(stepperIndex.Value + 1);
                   });
    }
}
