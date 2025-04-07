using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Xunit;
using Duo.Helpers;
using Microsoft.UI.Dispatching;

namespace TestMessi.Helpers
{
    // The MockDispatcherQueue now implements Duo.Helpers.IDispatcherQueue 
    public class MockDispatcherQueue : Duo.Helpers.IDispatcherQueue
    {
        public bool TryEnqueue(Action callback)
        {
            // Execute immediately for testing
            callback?.Invoke();
            return true;
        }
    }

    public class DebounceHelperTests
    {
        private static readonly FieldInfo ActiveTimersField = typeof(DebounceHelper)
            .GetField("_activeTimers", BindingFlags.NonPublic | BindingFlags.Static);

        private Dictionary<string, System.Timers.Timer> GetActiveTimers()
        {
            return (Dictionary<string, System.Timers.Timer>)ActiveTimersField.GetValue(null);
        }

        // Since DebounceHelper relies on DispatcherQueue.GetForCurrentThread() which might be null in tests
        // and requires Windows UI thread, we'll focus on testing the timer cancellation logic 
        // which doesn't depend on the dispatcher execution

        [Fact]
        public void DefaultQueue_GetterAndSetter_WorksCorrectly()
        {
            // Test the setter
            var mockQueue = new MockDispatcherQueue();
            DebounceHelper.DefaultQueue = mockQueue;
            
            // Test the getter
            Assert.Same(mockQueue, DebounceHelper.DefaultQueue);
        }

        [Fact]
        public void Debounce_CreatesNewTimer_ForEachUniqueKey()
        {
            // Arrange
            Action action = () => { };
            string key1 = "key1";
            string key2 = "key2";
            var mockQueue = new MockDispatcherQueue();

            // Act
            DebounceHelper.Debounce(action, 1000, key1, mockQueue);
            DebounceHelper.Debounce(action, 1000, key2, mockQueue);

            // Assert
            var timers = GetActiveTimers();
            Assert.True(timers.ContainsKey(key1), "Timer for key1 should be created");
            Assert.True(timers.ContainsKey(key2), "Timer for key2 should be created");
        }

        [Fact]
        public void Debounce_CreatesTimerWithCorrectDelay()
        {
            // Arrange
            Action action = () => { };
            string key = "testTimerDelay";
            int delay = 500;
            var mockQueue = new MockDispatcherQueue();

            // Act
            DebounceHelper.Debounce(action, delay, key, mockQueue);

            // Assert
            var timers = GetActiveTimers();
            Assert.True(timers.ContainsKey(key), "Timer should be created with the specified key");
            Assert.Equal(delay, timers[key].Interval);
        }

        [Fact]
        public void Debounce_CancelsExistingTimer_WhenCalledWithSameKey()
        {
            // Arrange
            Action action = () => { };
            string key = "testCancelTimer";
            var mockQueue = new MockDispatcherQueue();
            
            // Act - First call
            DebounceHelper.Debounce(action, 1000, key, mockQueue);
            var timer1 = GetActiveTimers()[key];
            
            // Second call with same key
            DebounceHelper.Debounce(action, 2000, key, mockQueue);
            var timer2 = GetActiveTimers()[key];

            // Assert
            Assert.NotSame(timer1, timer2);
            Assert.Equal(2000, timer2.Interval);
        }

        [Fact]
        public void Debounce_UsesMethodHashCodeAsKey_WhenKeyIsNull()
        {
            // Arrange
            Action action = () => { };
            string expectedKey = action.GetHashCode().ToString();
            var mockQueue = new MockDispatcherQueue();
            
            // Act
            DebounceHelper.Debounce(action, 1000, null, mockQueue);
            
            // Assert
            var timers = GetActiveTimers();
            Assert.True(timers.ContainsKey(expectedKey), 
                "Timer should be created with key based on method's hash code");
        }

        [Fact]
        public void Debounce_AcceptsDefaultDelay_WhenDelayNotSpecified()
        {
            // Arrange
            Action action = () => { };
            string key = "testDefaultDelay";
            var mockQueue = new MockDispatcherQueue();
            
            // Act
            DebounceHelper.Debounce(action, key: key, dispatcherQueue: mockQueue);
            
            // Assert
            var timers = GetActiveTimers();
            Assert.True(timers.ContainsKey(key), "Timer should be created with the specified key");
            Assert.Equal(200, timers[key].Interval);
        }

        [Fact]
        public void Debounce_RemovesTimerFromDictionary_AfterExecution()
        {
            // Arrange
            var executed = false;
            string key = "testRemoveTimer";
            Action action = () => executed = true;
            var mockQueue = new MockDispatcherQueue();
            
            // Act
            DebounceHelper.Debounce(action, 100, key, mockQueue);
            
            // Wait for timer to execute
            Thread.Sleep(150);
            
            // Assert
            Assert.True(executed, "Action should have been executed");
            var timers = GetActiveTimers();
            Assert.False(timers.ContainsKey(key), 
                "Timer should be removed from dictionary after elapsed event");
        }

        [Fact]
        public void Debounce_MultipleKeys_TracksTimersSeparately()
        {
            // Arrange
            Action action = () => { };
            string key1 = "multiKey1";
            string key2 = "multiKey2";
            var mockQueue = new MockDispatcherQueue();

            // Act
            DebounceHelper.Debounce(action, 100, key1, mockQueue);
            DebounceHelper.Debounce(action, 200, key2, mockQueue);

            // Assert
            var timers = GetActiveTimers();
            Assert.True(timers.ContainsKey(key1), "Timer 1 should be tracked");
            Assert.True(timers.ContainsKey(key2), "Timer 2 should be tracked");
            Assert.Equal(100, timers[key1].Interval);
            Assert.Equal(200, timers[key2].Interval);
        }

        [Fact]
        public void Debounce_ExecutesMethodAfterDelay()
        {
            // Arrange
            bool wasCalled = false;
            Action action = () => wasCalled = true;
            var mockQueue = new MockDispatcherQueue();
            
            // Act
            DebounceHelper.Debounce(action, 100, "test1", mockQueue);
            
            // Wait just long enough for the timer to elapse
            Thread.Sleep(150);
            
            // Assert
            Assert.True(wasCalled, "The action should be called after the timer elapses");
        }
        
        [Fact]
        public void Debounce_MultipleCallsWithSameKey_OnlyExecutesLastOne()
        {
            // Arrange
            int callCount = 0;
            Action action1 = () => callCount += 1;
            Action action2 = () => callCount += 10;
            Action action3 = () => callCount += 100;
            string key = "multiCall";
            var mockQueue = new MockDispatcherQueue();
            
            // Act - Call multiple times with the same key
            DebounceHelper.Debounce(action1, 100, key, mockQueue);
            DebounceHelper.Debounce(action2, 100, key, mockQueue);
            DebounceHelper.Debounce(action3, 100, key, mockQueue);
            
            // Wait for timer to elapse
            Thread.Sleep(150);
            
            // Assert - Only the last action should be executed
            Assert.Equal(100, callCount);
        }
    }
}
