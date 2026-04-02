using Ivy.Core.Exceptions;
using Ivy.Core.Helpers;

namespace Ivy.Core.Hooks;

public class EffectQueue(IExceptionHandler exceptionHandler) : IAsyncDisposable
{
    private readonly Lock _syncLock = new();
    private readonly AsyncDisposables _disposables = new();
    private readonly Queue<(EffectHook Effect, EffectPriority Priority)> _queue = new();
    private bool _isProcessing;

    public void Enqueue(EffectHook effect, EffectPriority priority)
    {
        lock (_syncLock)
        {
            _queue.Enqueue((effect, priority));
        }

        // Start processing if not already in progress
        ProcessQueueAsync().ConfigureAwait(false);
    }

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
                await ProcessEffectsForPriority(EffectPriority.OnStateChange);
                await ProcessEffectsForPriority(EffectPriority.OnBuild);
                await ProcessEffectsForPriority(EffectPriority.OnMount);
            }
        }
        finally
        {
            bool hasMore;
            lock (_syncLock)
            {
                _isProcessing = false;
                hasMore = _queue.Count > 0;
            }
            if (hasMore)
            {
                _ = ProcessQueueAsync();
            }
        }
    }

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
                var disposable = await task;
                _disposables.Add(disposable);
                await Task.Yield();
            }
            catch (Exception ex)
            {
                exceptionHandler.HandleException(new EffectException(effect, ex));
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        lock (_syncLock)
        {
            _queue.Clear();
        }
        await _disposables.DisposeAsync();
    }
}
