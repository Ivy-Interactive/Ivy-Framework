using Ivy.Core.Exceptions;
using Ivy.Core.Helpers;

namespace Ivy.Core.Hooks;

/// <summary>
/// Manages the execution of effects in priority order with deduplication and exception handling.
/// Processes effects asynchronously while maintaining thread safety and proper cleanup.
/// </summary>
/// <param name="exceptionHandler">Handler for exceptions that occur during effect execution.</param>
public class EffectQueue(IExceptionHandler exceptionHandler) : IDisposable
{
    private readonly Lock _syncLock = new();
    private readonly Disposables _disposables = new();
    private readonly Queue<(EffectHook Effect, EffectPriority Priority)> _queue = new();
    private bool _isProcessing;

    /// <summary>
    /// Adds an effect to the queue for execution at the specified priority level.
    /// </summary>
    /// <param name="effect">The effect to enqueue for execution.</param>
    /// <param name="priority">The priority level that determines execution order.</param>
    public void Enqueue(EffectHook effect, EffectPriority priority)
    {
        lock (_syncLock)
        {
            _queue.Enqueue((effect, priority));
        }

        // Start processing if not already in progress
        ProcessQueueAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Processes all queued effects in priority order, ensuring only one processing cycle runs at a time.
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        if (_isProcessing)
            return;

        lock (_syncLock)
        {
            if (_isProcessing)
                return;
            _isProcessing = true;
        }

        try
        {
            while (true)
            {
                lock (_syncLock)
                {
                    if (_queue.Count == 0)
                        break;
                }

                // Process each priority level in order
                await ProcessEffectsForPriority(EffectPriority.StateChange);
                await ProcessEffectsForPriority(EffectPriority.AfterRender);
                await ProcessEffectsForPriority(EffectPriority.AfterInit);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    /// <summary>
    /// Processes all effects for a specific priority level, deduplicating by effect identity.
    /// </summary>
    /// <param name="targetPriority">The priority level to process.</param>
    private async Task ProcessEffectsForPriority(EffectPriority targetPriority)
    {
        List<(EffectHook Effect, EffectPriority Priority)> effectsToProcess;
        Queue<(EffectHook Effect, EffectPriority Priority)> newQueue;

        lock (_syncLock)
        {
            // Get all effects for current priority level
            effectsToProcess = _queue
                .Where(x => x.Priority == targetPriority)
                .ToList();

            // Get only most recent effect per identity to avoid duplicate execution
            var uniqueEffects = effectsToProcess
                .GroupBy(x => x.Effect.Identity)
                .Select(g => g.Last())
                .ToList();

            // Create new queue without the processed effects
            newQueue = new Queue<(EffectHook Effect, EffectPriority Priority)>(_queue.Where(x => x.Priority != targetPriority));
            _queue.Clear();
            foreach (var item in newQueue)
            {
                _queue.Enqueue(item);
            }

            effectsToProcess = uniqueEffects;
        }

        foreach (var (effect, _) in effectsToProcess)
        {
            try
            {
                var task = effect.Handler();
                await task;
                _disposables.Add(task.Result);
                await Task.Yield();
            }
            catch (Exception ex)
            {
                exceptionHandler.HandleException(new EffectException(effect, ex));
            }
        }
    }

    /// <summary>
    /// Disposes the effect queue, clearing all pending effects and disposing tracked resources.
    /// </summary>
    public void Dispose()
    {
        lock (_syncLock)
        {
            _queue.Clear();
        }
        _disposables.Dispose();
    }
}
