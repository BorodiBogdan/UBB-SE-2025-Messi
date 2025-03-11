## UBB-SE-2025-Messi (Messi > Ronaldo)

> 🌍 Duolingo-like  Learning App

A modern learning application inspired by Duolingo, built with a clean MVVM architecture.

## 📋 Project Structure

```
UBB-SE-2025-Messi/
├── 📱 Duo/                           # Main application directory
│   ├── 🖼️ Views/                     # UI components (View layer)
│   │   ├── Pages/                    # Main application pages
│   │   └── Components/               # Reusable UI components
│   │
│   ├── 📊 ViewModels/                # View models (ViewModel layer)
│   │   ├── Base/                     # Base view model classes
│   │   ├── MainViewModel.cs          # Main view model
│   │   ├── LoginViewModel.cs         # Login view model
│   │   ├── PostViewModel.cs          # Post view model
│   │   ├── PostListViewModel.cs      # Post list view model
│   │   ├── PostCreationViewModel.cs  # Post creation view model
│   │   ├── CommentViewModel.cs       # Comment view model
│   │   ├── CommentCreationViewModel.cs # Comment creation view model
│   │   ├── CategoryViewModel.cs      # Category view model
│   │   └── SearchViewModel.cs        # Search view model
│   │
│   ├── 📦 Models/                    # Data models (Model layer)
│   │   ├── User.cs                   # User model
│   │   ├── Post.cs                   # Post model
│   │   ├── PostHashtag.cs            # Post hashtag model
│   │   ├── Comment.cs                # Comment model
│   │   ├── Category.cs               # Category model
│   │   └── Hashtag.cs                # Hashtag model
│   │
│   ├── 🔄 Services/                  # Business logic services
│   │   ├── UserService.cs            # User-related services
│   │   ├── PostService.cs            # Post-related services
│   │   ├── CommentService.cs         # Comment-related services
│   │   ├── CategoryService.cs        # Category-related services
│   │   ├── SearchService.cs          # Search functionality
│   │   └── MarkdownService.cs        # Markdown processing
│   │
│   ├── 🗄️ Repositories/              # Data access layer
│   │   ├── UserRepository.cs         # User data access
│   │   ├── PostRepository.cs         # Post data access
│   │   ├── CommentRepository.cs      # Comment data access
│   │   ├── CategoryRepository.cs     # Category data access
│   │   └── HashtagRepository.cs      # Hashtag data access
│   │
│   ├── 🧰 Helpers/                   # Helper utilities
│   │
│   ├── 🗄️ Data/                      # Data layer
│   │   └── 📊 Queries/               # Database queries
│   │
│   ├── 🎨 Resources/                 # Application resources
│   │   ├── 🖼️ Images/                # Image assets
│   │   └── 🎭 Styles/                # Style definitions
│   │
│   ├── 🖼️ Assets/                    # Application assets
│   │
│   ├── ⚙️ Properties/                # Project properties
│   │
│   ├── 📄 App.xaml                   # Application definition
│   ├── 📄 App.xaml.cs                # Application code-behind
│   ├── 📄 MainWindow.xaml            # Main window definition
│   ├── 📄 MainWindow.xaml.cs         # Main window code-behind
│   ├── 📄 Duo.csproj                 # Project file
│   └── 📄 Duo.sln                    # Solution file
│
├── 📝 planning/                      # Project planning documents
│   ├── 📊 usecase diagram.jpeg       # Use case diagram image
│   ├── 📊 uscase diagram.drawio      # Use case diagram source
│   ├── 📄 requirements.pdf           # Project requirements
│   ├── 📊 diagram.png                # Architecture diagram image
│   └── 📊 diagram.mdj                # Architecture diagram source
```