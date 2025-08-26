# Security Audit Dashboard API

ASP.NET Core 8.0 Web API providing backend services for federated authentication, role management, and security event auditing.

## 🏗 Project Structure

```
SecurityAuditDashboard.Api/
├── Controllers/
│   ├── RolesController.cs         # Role management endpoints
│   ├── SecurityEventsController.cs # Security event logging endpoints
│   └── UsersController.cs         # User management endpoints
├── Services/
│   ├── Interfaces/
│   │   ├── ISecurityEventService.cs
│   │   └── IUserService.cs
│   └── Implementations/
│       ├── SecurityEventService.cs # Handles security event logging
│       └── UserService.cs         # Handles user creation and management
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRoleRepository.cs
│   │   ├── ISecurityEventRepository.cs
│   │   └── IUserRepository.cs
│   └── Implementations/
│       ├── RoleRepository.cs      # Role data access
│       ├── SecurityEventRepository.cs # Security event data access
│       └── UserRepository.cs      # User data access
├── Data/
│   ├── Context/
│   │   └── AuditDbContext.cs      # Entity Framework DbContext
│   └── Entities/
│       ├── User.cs                # User entity
│       ├── Role.cs                # Role entity
│       ├── Claim.cs               # Claim entity
│       ├── RoleClaim.cs           # Role-Claim mapping
│       └── SecurityEvent.cs       # Security event entity
├── Authentication/
│   └── AuthenticationConfig.cs    # Auth configuration models
├── GraphQL/                        # GraphQL endpoints (TBD)
└── Program.cs                      # Application entry point
```

## 🔑 Authentication

- **JWT Bearer Token** validation only
- Validates tokens issued by the UI (BFF pattern)
- Expects Okta-issued JWTs with proper audience and issuer

## 🗄 Database

- **SQL Server** via Entity Framework Core
- **Connection String**: Configure in `appsettings.json`
- **Migrations**: Run `dotnet ef database update`

### Entity Relationships
- `User` → has one `Role`
- `Role` → has many `Claims` (through `RoleClaim`)
- `SecurityEvent` → tracks `AuthorUser` and `AffectedUser`

## 📋 Security Events

Three event types are tracked:
1. **LoginSuccess** - User authentication with provider details
2. **Logout** - User sign-out events
3. **RoleAssigned** - Role changes with from/to details

## 🔒 Roles & Permissions

### Predefined Roles:
- **BasicUser**: Default role, no special permissions
- **AuthObserver**: Can view authentication events (`Audit.ViewAuthEvents`)
- **SecurityAuditor**: Can view all events and change roles (`Audit.ViewAuthEvents`, `Audit.RoleChanges`)

## 🚀 Running the API

```bash
# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Run in development
dotnet run

# Run with hot reload
dotnet watch run
```

Default URL: `https://localhost:5120`

## 🧪 Testing

Tests are located in `SecurityAuditDashboard.Api.Tests/`

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 🔧 Configuration

Configure in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SecurityAuditDb;..."
  },
  "Authentication": {
    "Okta": {
      "Authority": "https://your-domain.okta.com/oauth2/default",
      "ClientId": "your-client-id"
    }
  }
}
```

## 📦 Key Dependencies

- **Entity Framework Core** - ORM and database access
- **HotChocolate** - GraphQL server (pending implementation)
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT validation

## 🔄 API Endpoints

### Users
- `POST /api/users/login` - Create/retrieve user on login
- `GET /api/users/{id}` - Get user details
- `PUT /api/users/{id}/role` - Assign role to user

### Security Events
- `POST /api/security-events` - Log security event
- `GET /api/security-events` - Get all events (requires permissions)
- `GET /api/security-events/user/{userId}` - Get user's events

### Roles
- `GET /api/roles` - List all roles
- `GET /api/roles/{id}/claims` - Get role claims