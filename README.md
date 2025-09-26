# Sinking

A C# 2-way synchronization application for Jira Cloud, GitHub Issues, and Azure DevOps Work Items.

## Features

- **Bidirectional Sync**: Two-way synchronization between supported platforms
- **Multiple Services**: GitHub Issues, Jira Cloud, Azure DevOps Work Items
- **Scheduled Sync**: Cron-based scheduling for automatic synchronization
- **Real-time Webhooks**: Webhook endpoints for immediate sync triggers
- **Field Mapping**: Configurable field mappings between different platforms
- **User Mapping**: Email/username resolution between systems
- **Conflict Resolution**: Timestamp-based conflict resolution
- **Attachment Support**: Synchronization of file attachments
- **REST API**: JWT-authenticated Web API for configuration and monitoring
- **Error Tracking**: Comprehensive error logging and monitoring
- **Docker Support**: Containerized deployment with Docker Compose

## Architecture

The solution consists of several projects:

- **Sinking.Api**: Web API with JWT authentication and Swagger documentation
- **Sinking.Console**: Background service for scheduled synchronization
- **Sinking.Core**: Core business logic and service interfaces
- **Sinking.Data**: Entity Framework data access layer with SQLite
- **Tests**: Unit and integration tests

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker (optional, for containerized deployment)

### Configuration

1. **Database**: The application uses SQLite by default. The database file will be created automatically at `data/sinking.db`.

2. **JWT Authentication**: Configure the JWT settings in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSecretKeyForJWTTokenGeneration123456789012345"
  },
  "Auth": {
    "Username": "admin",
    "Password": "password123"
  }
}
```

3. **Service Configurations**: Configure your sync jobs through the API with service-specific settings:

#### GitHub Configuration
```json
{
  "AccessToken": "your_github_token",
  "Owner": "repository_owner",
  "Repository": "repository_name",
  "Labels": ["bug", "enhancement"],
  "IncludeClosedIssues": false
}
```

#### Jira Configuration
```json
{
  "BaseUrl": "https://yourcompany.atlassian.net",
  "Email": "your.email@company.com",
  "ApiToken": "your_jira_api_token",
  "ProjectKey": "PROJ",
  "IssueTypes": ["Story", "Bug"],
  "JqlFilter": "assignee = currentUser()"
}
```

#### Azure DevOps Configuration
```json
{
  "OrganizationUrl": "https://dev.azure.com/yourorg",
  "Project": "YourProject",
  "PersonalAccessToken": "your_pat_token",
  "WorkItemType": "User Story",
  "AreaPaths": ["Area1", "Area2"]
}
```

### Running the Application

#### Using .NET CLI

1. **Start the API**:
```bash
cd src/Sinking.Api
dotnet run
```
The API will be available at `https://localhost:8081` and `http://localhost:8080`.
Swagger UI is available at `/swagger`.

2. **Start the Console Scheduler**:
```bash
cd src/Sinking.Console
dotnet run
```

#### Using Docker

1. **Build and run with Docker Compose**:
```bash
docker-compose up --build
```

This will start both the API and the scheduler service.

## API Usage

### Authentication

1. **Get JWT Token**:
```bash
POST /api/auth/login
{
  "username": "admin",
  "password": "password123"
}
```

2. **Use the token** in subsequent requests:
```bash
Authorization: Bearer {your_jwt_token}
```

### Sync Job Management

1. **Create a Sync Job**:
```bash
POST /api/syncjobs
{
  "name": "GitHub to Jira Sync",
  "description": "Sync GitHub issues to Jira",
  "sourceService": 1,
  "targetService": 2,
  "sourceConfiguration": "{...github config...}",
  "targetConfiguration": "{...jira config...}",
  "isEnabled": true,
  "isBidirectional": false,
  "cronSchedule": "0 */15 * * * *",
  "fieldMappings": [
    {
      "sourceField": "title",
      "targetField": "summary"
    }
  ],
  "userMappings": [
    {
      "sourceUser": "github_user",
      "targetUser": "jira_user@company.com"
    }
  ]
}
```

2. **Trigger Manual Sync**:
```bash
POST /api/syncjobs/{id}/sync
```

3. **Get Sync History**:
```bash
GET /api/syncjobs/{id}/runs
```

4. **View Sync Errors**:
```bash
GET /api/errors?jobId={id}
```

## Cron Schedule Examples

- `0 */15 * * * *` - Every 15 minutes
- `0 0 */6 * * *` - Every 6 hours
- `0 0 9 * * MON-FRI` - Every weekday at 9 AM
- `0 30 8 * * *` - Every day at 8:30 AM

## Field Mappings

Configure how fields are mapped between services:

```json
{
  "fieldMappings": [
    {
      "sourceField": "title",
      "targetField": "summary",
      "isRequired": true
    },
    {
      "sourceField": "body",
      "targetField": "description"
    },
    {
      "sourceField": "assignee",
      "targetField": "assignee"
    }
  ]
}
```

## User Mappings

Map users between different systems:

```json
{
  "userMappings": [
    {
      "sourceUser": "john.doe@github.com",
      "targetUser": "john.doe@company.com"
    },
    {
      "sourceUser": "github_username",
      "targetUser": "jira_account_id"
    }
  ]
}
```

## Conflict Resolution

The application uses timestamp-based conflict resolution:

1. **Source Wins**: If the source item was modified after the target, source changes are applied
2. **Target Wins**: If the target item was modified after the source, a conflict is logged but sync continues
3. **No Conflicts**: If both items have the same modification time, no action is taken

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Database Migrations

The application uses Entity Framework Core with SQLite. The database is created automatically on first run.

To add migrations (if you modify the models):

```bash
cd src/Sinking.Data
dotnet ef migrations add YourMigrationName
```

## Monitoring and Troubleshooting

### Logs

The application uses Microsoft.Extensions.Logging. Logs are output to the console by default.

### Error Tracking

All sync errors are stored in the database and can be accessed via:
- API endpoint: `GET /api/errors`
- Direct database query in the `SyncErrors` table

### Health Checks

Monitor the application health by checking:
- API availability: `GET /api/syncjobs` (should return 200 OK after authentication)
- Recent sync runs: `GET /api/syncjobs/{id}/runs`
- Error count: `GET /api/errors`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Security Considerations

- **Secrets Management**: Store API tokens and credentials securely
- **HTTPS**: Always use HTTPS in production
- **JWT Secret**: Use a strong, unique secret key for JWT token generation
- **Database**: Secure the SQLite database file in production environments
- **Rate Limiting**: Consider implementing rate limiting for API endpoints

## Support

For issues, questions, or contributions, please:

1. Check existing GitHub issues
2. Create a new issue with detailed information
3. Include logs and error messages
4. Provide steps to reproduce problems