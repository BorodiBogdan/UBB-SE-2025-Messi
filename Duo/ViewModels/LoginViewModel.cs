using System;
using System.Windows.Input;
using Duo.Models;
using Duo.Services;
using Microsoft.UI.Xaml;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Duo.Commands;
using Duo.ViewModels.Base;
using Duo.Helpers;
using Duo.Services.Interfaces;

namespace Duo.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private string _username = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _hasError = false;

        public event EventHandler? LoginSuccessful;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    HasError = !string.IsNullOrEmpty(value);
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            private set => SetProperty(ref _hasError, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            LoginCommand = new RelayCommand(Login, CanLogin);
        }

        private bool CanLogin() => !string.IsNullOrWhiteSpace(Username);

        private void Login()
        {
            try
            {
                ErrorMessage = string.Empty;

                try
                {
                    ValidationHelper.ValidateUsername(Username);
                    _userService.setUser(Username);
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = ex.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
