namespace Ivy.Tendril.Apps.Onboarding;

public class WelcomeStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        return Layout.Vertical().Margin(0, 0, 0, 20)
               | Text.H1("Welcome to Ivy Tendril")
               | Text.Markdown(
                   """
                   >[!NOTE]
                   >Ivy Tendril is a coding orchestrator powered by agents like Claude Code, Codex, or Gemini. It's designed to help you complete large amounts of work quickly. 
                   >
                   >**Please be aware that Tendril will consume lots of tokens rapidly.**
                   """)
               | new Button("Get Started").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .OnClick(() => stepperIndex.Set(stepperIndex.Value + 1));
    }
}
