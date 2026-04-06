namespace Ivy.Tendril;

/// <summary>
/// Defines the menu ordering for Tendril apps.
/// Lower values appear first in the menu.
/// </summary>
public static class MenuOrder
{
    public const int Dashboard = 1;
    public const int Recommendations = 10;
    public const int Drafts = 15;
    public const int Jobs = 20;
    public const int Review = 25;
    public const int PullRequests = 27;
    public const int Icebox = 30;
    public const int Claude = 35;
    public const int Trash = 40;
    public const int Help = 50;
    public const int Onboarding = 100;
}
