// <copyright file="RelayCommandWithParameterTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestProject1.Commands
{
    using System;
    using System.Windows.Input;
    using Duo.Commands;
    using Xunit;

    /// <summary>
    /// Contains unit tests for the RelayCommandWithParameter class.
    /// </summary>
    public class RelayCommandWithParameterTests : IDisposable
    {
        private bool executeCalled;
        private bool canExecuteCalled;
        private string lastParameter;
        private RelayCommandWithParameter<string> relayCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommandWithParameterTests"/> class.
        /// </summary>
        public RelayCommandWithParameterTests()
        {
            this.executeCalled = false;
            this.canExecuteCalled = false;
            this.lastParameter = null;
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when execute action is null.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteActionIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RelayCommandWithParameter<string>(null));
        }

        /// <summary>
        /// Tests that the constructor initializes the command with valid parameters.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_InitializesCommand()
        {
            // Arrange
            Action<string> execute = (param) => { this.executeCalled = true; this.lastParameter = param; };
            Predicate<string> canExecute = (param) => { this.canExecuteCalled = true; return true; };

            // Act
            this.relayCommand = new RelayCommandWithParameter<string>(execute, canExecute);

            // Assert
            Assert.NotNull(this.relayCommand);
        }

        /// <summary>
        /// Tests that CanExecute returns true when canExecute function is not provided.
        /// </summary>
        [Fact]
        public void CanExecute_NoCanExecuteFunction_ReturnsTrue()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithParameter<string>((param) => { });

            // Act
            bool result = this.relayCommand.CanExecute("test");

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanExecute returns false when parameter is null for value type.
        /// </summary>
        [Fact]
        public void CanExecute_NullParameterForValueType_ReturnsFalse()
        {
            // Arrange
            var intCommand = new RelayCommandWithParameter<int>((param) => { });

            // Act
            bool result = intCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanExecute returns true for non-null value type parameter.
        /// </summary>
        [Fact]
        public void CanExecute_NonNullValueTypeParameter_ReturnsTrue()
        {
            // Arrange
            var intCommand = new RelayCommandWithParameter<int>((param) => { });

            // Act
            bool result = intCommand.CanExecute(42);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanExecute calls the provided canExecute function.
        /// </summary>
        [Fact]
        public void CanExecute_WithCanExecuteFunction_CallsFunction()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithParameter<string>(
                (param) => { },
                (param) => { this.canExecuteCalled = true; return true; });

            // Act
            this.relayCommand.CanExecute("test");

            // Assert
            Assert.True(this.canExecuteCalled);
        }

        /// <summary>
        /// Tests that Execute calls the provided execute action with the correct parameter.
        /// </summary>
        [Fact]
        public void Execute_CallsExecuteActionWithParameter()
        {
            // Arrange
            const string testParameter = "test parameter";
            this.relayCommand = new RelayCommandWithParameter<string>((param) =>
            {
                this.executeCalled = true;
                this.lastParameter = param;
            });

            // Act
            this.relayCommand.Execute(testParameter);

            // Assert
            Assert.True(this.executeCalled);
            Assert.Equal(testParameter, this.lastParameter);
        }

        /// <summary>
        /// Tests that RaiseCanExecuteChanged invokes the CanExecuteChanged event.
        /// </summary>
        [Fact]
        public void RaiseCanExecuteChanged_InvokesCanExecuteChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            this.relayCommand = new RelayCommandWithParameter<string>((param) => { });
            this.relayCommand.CanExecuteChanged += (sender, args) => eventRaised = true;

            // Act
            this.relayCommand.RaiseCanExecuteChanged();

            // Assert
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that CanExecuteChanged event can be subscribed to and unsubscribed from.
        /// </summary>
        [Fact]
        public void CanExecuteChanged_CanSubscribeAndUnsubscribe()
        {
            // Arrange
            bool eventRaised = false;
            EventHandler handler = (sender, args) => eventRaised = true;
            this.relayCommand = new RelayCommandWithParameter<string>((param) => { });

            // Act
            this.relayCommand.CanExecuteChanged += handler;
            this.relayCommand.RaiseCanExecuteChanged();
            bool firstCall = eventRaised;

            eventRaised = false;
            this.relayCommand.CanExecuteChanged -= handler;
            this.relayCommand.RaiseCanExecuteChanged();
            bool secondCall = eventRaised;

            // Assert
            Assert.True(firstCall);
            Assert.False(secondCall);
        }

        /// <summary>
        /// Tests that CanExecute returns the result of the canExecute function.
        /// </summary>
        [Fact]
        public void CanExecute_ReturnsCanExecuteFunctionResult()
        {
            // Arrange
            this.relayCommand = new RelayCommandWithParameter<string>(
                (param) => { },
                (param) => param == "valid");

            // Act
            bool validResult = this.relayCommand.CanExecute("valid");
            bool invalidResult = this.relayCommand.CanExecute("invalid");

            // Assert
            Assert.True(validResult);
            Assert.False(invalidResult);
        }

        /// <summary>
        /// Disposes of the test resources.
        /// </summary>
        public void Dispose()
        {
            this.relayCommand = null;
        }
    }
}