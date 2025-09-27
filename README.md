# Sinking

🔄 **A powerful 2-way synchronization service for seamlessly integrating issues and workflows between Jira, GitHub, and Azure DevOps.**

![Sinking Homepage](https://github.com/user-attachments/assets/5616031d-a6f9-4f53-9397-f81201ab7e23)

## 🚀 Overview

Sinking is a comprehensive .NET 9 web application that enables bidirectional synchronization of issues, work items, and workflows across multiple project management platforms. Whether you're working with Jira tickets, GitHub issues, or Azure DevOps work items, Sinking provides a unified interface to keep your teams in sync.

### ✨ Key Features

- **🔐 Secure Token Management**: Encrypted storage of Personal Access Tokens for all supported systems
- **🔄 Automated Sync Jobs**: Configure scheduled synchronization with custom field mappings and cron expressions
- **📊 Real-time Monitoring**: Monitor job execution, view detailed failure logs, and manage running tasks
- **🎯 Flexible Field Mapping**: Custom field mapping between different systems with support for complex transformations
- **📈 Comprehensive Logging**: Detailed change logs and audit trails for all synchronization activities
- **🔒 Enterprise Security**: Built with ASP.NET Identity, secure cookie authentication, and encrypted data storage

### 🏗️ Supported Systems

| Platform | Features Supported |
|----------|-------------------|
| **Jira** | Issues, Comments, Attachments, Custom Fields, Labels, Status, Priority |
| **GitHub** | Issues, Comments, Labels, Assignees, Milestones |
| **Azure DevOps** | Work Items, Comments, Attachments, Tags, Custom Fields |

## 🛠️ Architecture

Sinking follows a clean architecture approach with the following key components:

```text
src/
├── Sinking.Core/              # Core business logic and domain models
│   ├── Models.cs              # Issue comments and attachments models
│   ├── UnifiedIssue.cs        # Unified issue representation
│   ├── Enums.cs               # Status, priority, and system enums
│   ├── IssueDiff.cs           # Issue comparison and change detection
│   ├── SourceSystemMappers.cs # Mapping between different systems
│   └── FieldNames.cs          # Field name constants for different platforms
├── Sinking.Web/               # ASP.NET Core Blazor Server web application
│   ├── Components/            # Blazor components and pages
│   ├── Data/                  # Entity Framework models and DbContext
│   ├── Services/              # Application services (encryption, logging)
│   └── Program.cs             # Application startup and configuration
└── Sinking.Core.Tests/        # Unit tests (86 tests covering core functionality)
```

### 🏛️ Core Domain Models

**UnifiedIssue**: Central abstraction that represents issues from any source system
- Supports conversion from/to Jira, GitHub, and Azure DevOps formats
- Handles comments, attachments, labels, and custom fields
- Provides issue comparison and diff functionality

**PersonalAccessToken**: Securely encrypted API tokens for external systems
- AES encryption for token storage
- Support for multiple tokens per system
- Connection testing and validation

**SyncJob**: Configuration for automated synchronization
- Bidirectional sync between any supported systems  
- Custom field mappings and transformations
- Cron-based scheduling with execution monitoring

## 📦 Installation & Setup

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 (recommended) or VS Code with C# extension

### 🚀 Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/radaiko/Sinking.git
   cd Sinking/src
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the tests** (optional but recommended)
   ```bash
   dotnet test
   ```

5. **Start the application**
   ```bash
   cd Sinking.Web
   dotnet run
   ```

6. **Open your browser** and navigate to `https://localhost:5001` or `http://localhost:5000`

### 🔧 Configuration

The application uses SQLite by default with the following configuration in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=Data/app.db;Cache=Shared"
  },
  "Encryption": {
    "Key": "DefaultKey32BytesLongForSinkingApp",
    "IV": "DefaultIV16Bytes"
  }
}
```

⚠️ **Security Note**: For production deployment, make sure to:
- Change the default encryption key and IV
- Use a secure database connection string
- Configure proper HTTPS certificates
- Review and update security headers

## 💻 Usage

### 1. Account Setup
- Navigate to the application and create an account
- The first registered user gets administrator privileges

### 2. Configure Access Tokens
![Token Management](https://github.com/user-attachments/assets/b9190a46-6bf7-442f-a25e-58ad94dcf485)

Add Personal Access Tokens for your systems:
- **Jira**: Create a token in your Atlassian account settings
- **GitHub**: Generate a Personal Access Token with appropriate repo permissions
- **Azure DevOps**: Create a Personal Access Token with Work Items read/write access

### 3. Create Sync Jobs
Configure synchronization jobs between systems:
- Select source and target systems
- Define project/repository mappings
- Set up field mappings for custom transformations
- Configure sync schedule using cron expressions

### 4. Monitor Synchronization
- View real-time job execution status
- Analyze failure logs and error details
- Review change logs and audit trails

## 🔧 Development

### Running Tests
```bash
cd src
dotnet test
```
All 86 tests should pass, covering:
- UnifiedIssue conversion and mapping
- Field difference detection
- Source system integrations
- Token encryption/decryption

### Project Structure
- **Sinking.Core**: Platform-agnostic business logic
- **Sinking.Web**: Blazor Server UI and web services  
- **Sinking.Core.Tests**: Comprehensive unit test suite

### Database Migrations
The application uses Entity Framework Core with SQLite:
```bash
cd src/Sinking.Web
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes and add tests
4. Ensure all tests pass: `dotnet test`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) and [Blazor Server](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- UI components powered by [Bootstrap](https://getbootstrap.com/)
- Icons by [Bootstrap Icons](https://icons.getbootstrap.com/)

## 📞 Support

For questions, issues, or feature requests, please:
- Create an issue in this repository
- Check existing issues for similar problems
- Provide detailed information about your environment and use case

---

**Made with ❤️ for seamless project management integration**