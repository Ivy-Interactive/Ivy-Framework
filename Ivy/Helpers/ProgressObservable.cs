using System.Reactive.Linq;

namespace Ivy.Helpers;

public static class ProgressObservable
{
    public delegate double EasingFunction(double t);

    private static double LinearEasing(double t) => t;

    public static IObservable<int> Create(double duration = 10, EasingFunction easingFunction = null)
    {
        var easing = easingFunction ?? LinearEasing;
        double updateInterval = (duration * 1000.0) / 100;

        return Observable.Interval(TimeSpan.FromMilliseconds(updateInterval))
            .Take(101)
            .Select(tick =>
            {
                double t = tick / 100.0;
                double progress = easing(t) * 100.0;
                return (int)progress;
            });
    }
}

public static class EasingFunctions
{
    // Calculate the eased value based on the current time 't'
    // 't' is expected to be normalized between 0 and 1
    public static double DualLinearEasing(double t)
    {
        const double transitionPoint = 0.25; // Transition occurs at 25% of the duration
        if (t <= transitionPoint)
        {
            // First phase: steeper linear progression up to the transition point
            return (t / transitionPoint) * 0.75; // Ends at 75% of the value at the transition point
        }
        else
        {
            // Second phase: more gentle linear progression from 75% to 100%
            double startVal = 0.75;
            double endVal = 1.0;
            double phaseDuration = 1.0 - transitionPoint;
            double phaseProgress = (t - transitionPoint) / phaseDuration;
            return startVal + (endVal - startVal) * phaseProgress;
        }
    }
}