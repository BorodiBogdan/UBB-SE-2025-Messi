using TestProject1.Commands;

// <copyright file="RelayCommandTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TestProject1.Commands
{
    using System;
    using System.Windows.Input;
    using Duo.Commands;
    using Xunit;

    /// <summary>
    /// Contains unit tests for the RelayCommand class.
    /// </summary>
    public class RelayCommandTests : IDisposable
    {
        private bool executeCalled;
        private bool canExecuteCalled;
        private RelayCommand relayCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommandTests"/> class.
        /// </summary>
        public RelayCommandTests()
        {
            this.executeCalled = false;
            this.canExecuteCalled = false;
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when execute action is null.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteActionIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new RelayCommand(null));
            Assert.Equal("execute", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor properly assigns the execute action when it's not null.
        /// </summary>
        [Fact]
        public void Constructor_ExecuteActionNotNull_AssignsAction()
        {
            // Arrange
            Action execute = () => this.executeCalled = true;

            // Act
            this.relayCommand = new RelayCommand(execute);
            this.relayCommand.Execute(null);

            // Assert
            Assert.True(this.executeCalled);
        }

        /// <summary>
        /// Tests that the constructor initializes the command with valid parameters.
        /// </summary>
        [Fact]
        public void Constructor_ValidParameters_InitializesCommand()
        {
            // Arrange
            Action execute = () => this.executeCalled = true;
            Func<bool> canExecute = () => { this.canExecuteCalled = true; return true; };

            // Act
            this.relayCommand = new RelayCommand(execute, canExecute);

            // Assert
            Assert.NotNull(this.relayCommand);

            // Verify both delegates are properly assigned
            this.relayCommand.Execute(null);
            Assert.True(this.executeCalled);

            bool canExecuteResult = this.relayCommand.CanExecute(null);
            Assert.True(this.canExecuteCalled);
            Assert.True(canExecuteResult);
        }

        /// <summary>
        /// Tests that CanExecute returns true when canExecute function is not provided.
        /// </summary>
        [Fact]
        public void CanExecute_NoCanExecuteFunction_ReturnsTrue()
        {
            // Arrange
            this.relayCommand = new RelayCommand(() => { });

            // Act
            bool result = this.relayCommand.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanExecute returns false when canExecute function returns false.
        /// </summary>
        [Fact]
        public void CanExecute_WithCanExecuteFunctionReturnsFalse_ReturnsFalse()
        {
            // Arrange
            this.relayCommand = new RelayCommand(
                () => { },
                () => false);

            // Act
            bool result = this.relayCommand.CanExecute(null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanExecute calls the provided canExecute function.
        /// </summary>
        [Fact]
        public void CanExecute_WithCanExecuteFunction_CallsFunction()
        {
            // Arrange
            this.relayCommand = new RelayCommand(
                () => { },
                () => { this.canExecuteCalled = true; return true; });

            // Act
            this.relayCommand.CanExecute(null);

            // Assert
            Assert.True(this.canExecuteCalled);
        }

        /// <summary>
        /// Tests that Execute calls the provided execute action.
        /// </summary>
        [Fact]
        public void Execute_CallsExecuteAction()
        {
            // Arrange
            this.relayCommand = new RelayCommand(() => this.executeCalled = true);

            // Act
            this.relayCommand.Execute(null);

            // Assert
            Assert.True(this.executeCalled);
        }

        /// <summary>
        /// Tests that RaiseCanExecuteChanged invokes the CanExecuteChanged event.
        /// </summary>
        [Fact]
        public void RaiseCanExecuteChanged_InvokesCanExecuteChangedEvent()
        {
            // Arrange
            bool eventRaised = false;
            this.relayCommand = new RelayCommand(() => { });
            this.relayCommand.CanExecuteChanged += (sender, args) => eventRaised = true;

            // Act
            this.relayCommand.RaiseCanExecuteChanged();

            // Assert
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that RaiseCanExecuteChanged does not throw when no handlers are attached.
        /// </summary>
        [Fact]
        public void RaiseCanExecuteChanged_NoHandlersAttached_DoesNotThrow()
        {
            // Arrange
            this.relayCommand = new RelayCommand(() => { });

            // Act & Assert
            var exception = Record.Exception(() => this.relayCommand.RaiseCanExecuteChanged());
            Assert.Null(exception);
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
            this.relayCommand = new RelayCommand(() => { });

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
        /// Tests that Execute with parameter calls the execute action.
        /// </summary>
        [Fact]
        public void Execute_WithParameter_CallsExecuteAction()
        {
            // Arrange
            this.relayCommand = new RelayCommand(() => this.executeCalled = true);

            // Act
            this.relayCommand.Execute("some parameter");

            // Assert
            Assert.True(this.executeCalled);
        }

        /// <summary>
        /// Tests that CanExecute with parameter calls the canExecute function.
        /// </summary>
        [Fact]
        public void CanExecute_WithParameter_CallsCanExecuteFunction()
        {
            // Arrange
            this.relayCommand = new RelayCommand(
                () => { },
                () => { this.canExecuteCalled = true; return true; });

            // Act
            this.relayCommand.CanExecute("some parameter");

            // Assert
            Assert.True(this.canExecuteCalled);
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