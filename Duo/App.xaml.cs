using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System.Diagnostics;
using Duo.Views;
using Microsoft.Extensions.Configuration;
using Duo.ViewModels;
using Duo.Services;
using Duo.Data;
using Duo.Repositories;

namespace Duo
{

    public partial class App : Application
    {
        public static UserService userService;
        private static IConfiguration _configuration;
        public static DataLink _dataLink;
        public static UserRepository userRepository;
        public static PostRepository _postRepository;
        public static HashtagRepository _hashtagRepository;
        public static CommentRepository _commentRepository;
        public static PostService _postService;
        public static CategoryService _categoryService;
        public static SearchService _searchService;

        public App()
        {
            this.InitializeComponent();

            _configuration = InitializeConfiguration();

            _dataLink = new DataLink(_configuration);

            userRepository = new UserRepository(_dataLink);
            _postRepository = new PostRepository(_dataLink);
            _hashtagRepository = new HashtagRepository(_dataLink);
            _commentRepository = new CommentRepository(_dataLink);
            CategoryRepository categoryRepository = new CategoryRepository(_dataLink);

            userService = new UserService(userRepository);
            _searchService = new SearchService();
            _postService = new PostService(_postRepository, _hashtagRepository, userService, _searchService);
            _categoryService = new CategoryService(categoryRepository);
        }

        private IConfiguration InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {

            var loginViewModel = new LoginViewModel(userService);

            var loginWindow = new LoginWindow(loginViewModel);
            loginWindow.Activate();

            m_loginWindow = loginWindow;

            loginViewModel.LoginSuccessful += LoginViewModel_LoginSuccessful;
        }

        private void LoginViewModel_LoginSuccessful(object? sender, EventArgs e)
        {

            m_window = new MainWindow();
            m_window.Activate();

            m_loginWindow?.Close();
            m_loginWindow = null;
        }

        private Window? m_window;
        private LoginWindow? m_loginWindow;
    }
}
