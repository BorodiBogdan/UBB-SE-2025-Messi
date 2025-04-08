using TestProject1.Commands;


namespace TestProject1.Commands
{
    using System;
    using System.Windows.Input;
    using Duo.Commands;
    using Xunit;
    public class RelayCommandTests : IDisposable
    {
        private bool executeCalled;
        private bool canExecuteCalled;
        private RelayCommand relayCommand;
        public RelayCommandTests()
        {
            this.executeCalled = false;
            this.canExecuteCalled = false;
        }

        [Fact]
        public void Constructor_ExecuteActionIsNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new RelayCommand(null));
            Assert.Equal("execute", exception.ParamName);
        }

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

        [Fact]
        public void RaiseCanExecuteChanged_NoHandlersAttached_DoesNotThrow()
        {
            // Arrange
            this.relayCommand = new RelayCommand(() => { });

            // Act & Assert
            var exception = Record.Exception(() => this.relayCommand.RaiseCanExecuteChanged());
            Assert.Null(exception);
        }

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

        public void Dispose()
        {
            this.relayCommand = null;
        }
    }
}