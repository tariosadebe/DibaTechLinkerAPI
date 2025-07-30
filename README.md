# DibaTech Linker API

A mobile-first web service for saving and organizing links with automatic content extraction and categorization.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![SQLite](https://img.shields.io/badge/Database-SQLite-green)
![JWT](https://img.shields.io/badge/Auth-JWT-orange)
![Swagger](https://img.shields.io/badge/API-Swagger-brightgreen)

## 🚀 Features

### ✅ **Core Features Implemented**
- **🔍 Link Processing Engine** - Parse links, extract metadata, categorize automatically
- **📦 Link Repository** - Save, organize, filter, and search links
- **🔗 Link Sharing** - Public sharing with secure tokens
- **🔐 Authentication** - JWT-based authentication with registration/login
- **📁 Folder Organization** - Custom folders with reordering capabilities
- **🔁 Reminder System** - Configurable email and push notification reminders
- **📱 Mobile-First Design** - Optimized for mobile consumption

### 📋 **API Endpoints**

#### Authentication
- `POST /api/account/register` - User registration
- `POST /api/account/login` - User login
- `POST /api/account/refresh-token` - Refresh JWT token
- `POST /api/account/logout` - User logout
- `GET /api/account/profile` - Get user profile
- `PUT /api/account/profile` - Update user profile

#### Link Management
- `POST /api/links/parse` - Parse and extract link metadata
- `POST /api/links/save` - Save parsed link to user collection
- `GET /api/links/mine` - Get user's saved links
- `GET /api/links/{id}` - Get specific link details
- `PATCH /api/links/{id}` - Update link details
- `DELETE /api/links/{id}` - Delete saved link
- `POST /api/links/{id}/share` - Generate sharing token
- `GET /api/links/share/{shareToken}` - Access shared link
- `POST /api/links/{id}/mark-read` - Mark link as read
- `POST /api/links/{id}/toggle-favourite` - Toggle favorite status

#### Folder Organization
- `GET /api/folders` - Get user's folders
- `POST /api/folders` - Create new folder
- `GET /api/folders/{id}` - Get folder details
- `PUT /api/folders/{id}` - Update folder
- `DELETE /api/folders/{id}` - Delete folder
- `POST /api/folders/reorder` - Reorder folders

#### Reminders
- `POST /api/reminders/subscribe` - Configure reminder settings
- `GET /api/reminders/status` - Get current reminder status
- `POST /api/reminders/unsubscribe` - Disable reminders

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0
- **Database**: SQLite (Development) / SQL Server (Production)
- **Authentication**: JWT Bearer Tokens
- **ORM**: Entity Framework Core
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture with Repository Pattern

## 🚦 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (for production) or SQLite (for development)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/tariosadebe/DibaTechLinkerAPI.git
   cd DibaTechLinkerAPI
   ```

2. **Install dependencies**
   ```bash
   cd DibatechLinkerAPI
   dotnet restore
   ```

3. **Update database**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI**
   - Navigate to: `https://localhost:7283/`

### Configuration

Update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=dibatechlinker.db"
  },
  "JwtSettings": {
    "Secret": "your-super-secure-secret-key-here",
    "Issuer": "DibatechLinker",
    "Audience": "DibatechLinkerAPI",
    "TokenExpirationInHours": 24
  }
}
```

## 📖 API Usage Examples

### 1. Register and Login
```bash
# Register
curl -X POST "https://localhost:7283/api/account/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# Login
curl -X POST "https://localhost:7283/api/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

### 2. Parse and Save a Link
```bash
# Parse link
curl -X POST "https://localhost:7283/api/links/parse" \
  -H "Content-Type: application/json" \
  -d '{"url": "https://example.com/article"}'

# Save parsed link
curl -X POST "https://localhost:7283/api/links/save" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "parsedLinkId": 1,
    "customNote": "Interesting article to read later",
    "customTitle": "Custom Title",
    "folderId": null,
    "tags": ["tech", "article"]
  }'
```

### 3. Create and Organize Folders
```bash
# Create folder
curl -X POST "https://localhost:7283/api/folders" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Work Projects",
    "description": "Links related to work",
    "color": "#3498db"
  }'
```

## 🗂️ Project Structure

```
DibatechLinkerAPI/
├── Controllers/          # API Controllers
├── Data/                # Database Context
├── Models/              # Domain Models and DTOs
│   ├── Domain/          # Entity Models
│   └── DTOs/            # Data Transfer Objects
├── Services/            # Business Logic
│   ├── Interfaces/      # Service Interfaces
│   └── Implementations/ # Service Implementations
├── Migrations/          # EF Core Migrations
└── Program.cs           # Application Entry Point
```

## 🔒 Security Features

- JWT Bearer Token Authentication
- Password Requirements (configurable)
- Account Lockout Protection
- CORS Configuration
- HTTPS Redirection
- Secure Cookie Settings

## 📊 Database Schema

The application uses Entity Framework Core with the following main entities:
- **ApplicationUser** - User accounts with preferences
- **ParsedLink** - Cached link metadata
- **SavedLink** - User's saved links with personal notes
- **Folder** - Organization folders
- **Category** - Automatic content categorization

## 🚀 Deployment

### Development
```bash
dotnet run --environment Development
```

### Production
1. Update `appsettings.Production.json` with production database connection
2. Deploy to your preferred hosting platform (Azure, AWS, etc.)
3. Ensure SSL certificates are properly configured

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Contact

**DibaTech.ng**
- Email: contact@dibatech.ng
- GitHub: [@tariosadebe](https://github.com/tariosadebe)

## 🙏 Acknowledgments

- Built with ASP.NET Core
- Powered by Entity Framework Core
- API Documentation by Swagger/OpenAPI
- Authentication via JWT

---
**Made with ❤️ by DibaTech.ng**
