using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI.Dispatching;

namespace Duo.Helpers
{
    public interface IDispatcherQueue
    {
        bool TryEnqueue(Action callback);
    }

    public class WinUIDispatcherQueue : IDispatcherQueue
    {
        private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

        public WinUIDispatcherQueue(Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        public bool TryEnqueue(Action callback)
        {
            return _dispatcherQueue?.TryEnqueue(() => callback()) ?? false;
        }
    }

    public static class DebounceHelper
    {
        private static Dictionary<string, System.Timers.Timer> _activeTimers = new Dictionary<string, System.Timers.Timer>();
        private static IDispatcherQueue _defaultQueue;

        public static IDispatcherQueue DefaultQueue 
        {
            get => _defaultQueue ?? new WinUIDispatcherQueue(DispatcherQueue.GetForCurrentThread());
            set => _defaultQueue = value;
        }

        public static void Debounce(Action method, int delay = 200, string? key = null, IDispatcherQueue? dispatcherQueue = null) {
          key = key ?? method.GetHashCode().ToString();
          dispatcherQueue ??= DefaultQueue;

          if (_activeTimers.TryGetValue(key, out var existingTimer))
          {
              existingTimer.Stop();
              existingTimer.Dispose();
          }

          var timer = new System.Timers.Timer(delay);
          _activeTimers[key] = timer;

          timer.Elapsed += (sender, e) => {
            timer.Stop();

            if (_activeTimers.ContainsKey(key))
            {
                _activeTimers.Remove(key);
            }

            dispatcherQueue?.TryEnqueue(() => {
                method();
            });

            timer.Dispose();
          };

          timer.Start();
        }
    }
}