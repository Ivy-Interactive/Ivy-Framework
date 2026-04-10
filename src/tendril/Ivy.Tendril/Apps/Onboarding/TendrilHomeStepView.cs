using System.Text.RegularExpressions;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Onboarding;

public class TendrilHomeStepView(IState<int> stepperIndex) : ViewBase
{
    public override object Build()
    {
        var folderPath = UseState<string?>(
            Environment.GetEnvironmentVariable("TENDRIL_HOME")
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".tendril"
            )
        );
        var error = UseState<string?>(null);
        var config = UseService<IConfigService>();

        return Layout.Vertical()
               | Text.H2("Tendril Data Location")
               | Text.Muted("This folder will store your plans, inbox, trash, and other Tendril data.")
               | (error.Value != null ? Text.Danger(error.Value) : null!)
               | folderPath.ToFolderInput("Select Tendril data folder...", mode: FolderInputMode.FullPath)
                   .WithField().Label("Tendril Home")
               | new Button("Next").Primary().Large().Icon(Icons.ArrowRight, Align.Right)
                   .OnClick(() =>
                   {
                       if (string.IsNullOrEmpty(folderPath.Value))
                       {
                           error.Set("Please provide a valid path");
                           return;
                       }

                       try
                       {
                           var tendrilHome = folderPath.Value;
                           tendrilHome = Environment.ExpandEnvironmentVariables(tendrilHome);

                           if (tendrilHome.StartsWith("~"))
                           {
                               var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                               if (tendrilHome == "~") tendrilHome = home;
                               else if (tendrilHome.StartsWith("~/") || tendrilHome.StartsWith("~\\"))
                                   tendrilHome = Path.Combine(home, tendrilHome.Substring(2));
                           }
                           else if (tendrilHome.StartsWith("$"))
                           {
                               var match = Regex.Match(tendrilHome, @"^\$([A-Za-z_][A-Za-z0-9_]*)");
                               if (match.Success)
                               {
                                   var varName = match.Groups[1].Value;
                                   var varValue = Environment.GetEnvironmentVariable(varName);
                                   if (!string.IsNullOrEmpty(varValue))
                                       tendrilHome = varValue + tendrilHome.Substring(match.Length);
                               }
                           }

                           if (!Path.IsPathRooted(tendrilHome)) tendrilHome = Path.GetFullPath(tendrilHome);
                           tendrilHome = Path.GetFullPath(tendrilHome);

                           config.SetPendingTendrilHome(tendrilHome);
                           error.Set(null);
                           stepperIndex.Set(stepperIndex.Value + 1);
                       }
                       catch (Exception ex)
                       {
                           error.Set($"Invalid path: {ex.Message}");
                       }
                   });
    }
}
