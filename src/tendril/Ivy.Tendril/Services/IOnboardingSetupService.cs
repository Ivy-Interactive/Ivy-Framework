namespace Ivy.Tendril.Services;

public interface IOnboardingSetupService
{
    Task CompleteSetupAsync(string tendrilHome);
}
