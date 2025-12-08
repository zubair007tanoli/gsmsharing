# 🚀 Development Setup Guide
## GSMSharing V2 - Getting Started

**Last Updated:** December 2024  
**Status:** Phase 0 Complete

---

## 📋 Prerequisites

### Required Software
- **.NET SDK 10.0** or higher
  - Download: https://dotnet.microsoft.com/download
  - Verify: `dotnet --version`

- **SQL Server** (Local or Remote)
  - SQL Server 2019+ or SQL Server Express
  - SQL Server Management Studio (SSMS) or Azure Data Studio

- **IDE/Editor**
  - Visual Studio 2022 (Recommended)
  - Visual Studio Code with C# Dev Kit
  - Rider

- **Git** (for version control)
  - Download: https://git-scm.com/downloads

---

## 🔧 Step-by-Step Setup

### Step 1: Clone Repository
```bash
git clone <repository-url>
cd GsmsharingV2
```

### Step 2: Restore Dependencies
```bash
dotnet restore
```

### Step 3: Configure Database Connection

#### Option A: Local Development Database
1. Create local database:
   ```sql
   CREATE DATABASE gsmsharingv3_dev;
   ```

2. Update `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "GsmsharingConnection": "Data Source=localhost;Database=gsmsharingv3_dev;Integrated Security=true;MultipleActiveResultSets=true;Encrypt=false"
     }
   }
   ```

#### Option B: Use Remote Database
1. Update `appsettings.Development.json` with your connection string
2. **Note:** For production, use User Secrets or Azure Key Vault

### Step 4: Set Up User Secrets (Recommended)

For development, use User Secrets instead of hardcoding connection strings:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:GsmsharingConnection" "YOUR_CONNECTION_STRING_HERE"
```

### Step 5: Install Packages
```bash
dotnet restore
```

This will install:
- Entity Framework Core
- ASP.NET Core Identity
- Serilog (logging)
- Other dependencies

### Step 6: Build Project
```bash
dotnet build
```

### Step 7: Run Database Migrations

The application will automatically create tables on first run, or you can run:

```bash
# If using EF Core Migrations (future)
dotnet ef database update
```

Or manually run: `Database/CreateTables.sql`

### Step 8: Run Application
```bash
dotnet run
```

Or in Visual Studio: Press F5

### Step 9: Verify Setup

1. Open browser: `https://localhost:5001` or `http://localhost:5000`
2. Check logs in `logs/` folder
3. Verify database connection in logs
4. Test user registration/login

---

## 🔐 Security Configuration

### Development Environment

1. **User Secrets Setup:**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:GsmsharingConnection" "YOUR_DEV_CONNECTION_STRING"
   ```

2. **Update appsettings.Development.json:**
   - Remove sensitive data
   - Use local database when possible

### Production Environment

1. **Use Environment Variables:**
   ```bash
   export ConnectionStrings__GsmsharingConnection="YOUR_PRODUCTION_CONNECTION_STRING"
   ```

2. **Or Azure Key Vault:**
   - Store connection strings in Azure Key Vault
   - Configure application to read from Key Vault

---

## 📁 Project Structure

```
GsmsharingV2/
├── Controllers/          # MVC Controllers
├── Models/              # Domain Models
├── Views/               # Razor Views
├── Database/            # DbContext & SQL Scripts
├── Repositories/        # Data Access Layer
├── Interfaces/          # Repository Interfaces
├── wwwroot/            # Static Files
├── Properties/         # Launch Settings
├── docs/               # Documentation
└── appsettings.json    # Configuration
```

---

## 🛠️ Development Tools

### Recommended VS Code Extensions
- C# Dev Kit
- SQL Server (mssql)
- GitLens
- REST Client

### Recommended Visual Studio Extensions
- Entity Framework Core Power Tools
- ReSharper (optional)
- CodeMaid (optional)

---

## 🗄️ Database Setup

### Initial Database Setup

1. **Connect to SQL Server:**
   ```bash
   sqlcmd -S localhost -U sa -P YourPassword
   ```

2. **Create Database:**
   ```sql
   CREATE DATABASE gsmsharingv3_dev;
   GO
   USE gsmsharingv3_dev;
   GO
   ```

3. **Run Create Tables Script:**
   - Open `Database/CreateTables.sql`
   - Execute in SSMS or Azure Data Studio

4. **Or Let Application Create Tables:**
   - Application will auto-create tables on first run
   - Check logs for table creation status

### Database Analysis

Run the analysis script to understand database structure:
```sql
-- Execute: Database/AnalysisScripts/DatabaseAnalysis.sql
```

---

## 🧪 Testing Setup

### Run Application
```bash
dotnet run
```

### Check Logs
- Logs are written to: `logs/gsmsharing-YYYYMMDD.txt`
- Console output shows real-time logs
- Check for any errors or warnings

### Test Basic Functionality
1. Register a new user
2. Create a post
3. View posts
4. Check database for data

---

## 🐛 Troubleshooting

### Common Issues

#### 1. Database Connection Failed
**Error:** `Cannot open database "gsmsharingv3"`

**Solutions:**
- Verify SQL Server is running
- Check connection string
- Verify database exists
- Check firewall rules
- Verify SQL Server authentication

#### 2. Package Restore Failed
**Error:** `NU1101: Unable to find package`

**Solutions:**
```bash
dotnet nuget locals all --clear
dotnet restore
```

#### 3. Build Errors
**Error:** Various compilation errors

**Solutions:**
- Clean solution: `dotnet clean`
- Rebuild: `dotnet build`
- Check .NET SDK version: `dotnet --version`

#### 4. Tables Not Created
**Error:** Tables don't exist

**Solutions:**
- Check database connection
- Run `Database/CreateTables.sql` manually
- Check application logs for errors
- Verify database permissions

#### 5. Logging Not Working
**Error:** No log files created

**Solutions:**
- Check `logs/` folder exists
- Verify Serilog configuration in appsettings.json
- Check file permissions
- Verify Serilog packages installed

---

## 📝 Development Workflow

### Daily Workflow
1. Pull latest changes: `git pull`
2. Restore packages: `dotnet restore`
3. Build project: `dotnet build`
4. Run application: `dotnet run`
5. Make changes
6. Test changes
7. Commit changes: `git commit -m "Description"`

### Before Committing
- [ ] Code compiles without errors
- [ ] Application runs successfully
- [ ] No sensitive data in commits
- [ ] Meaningful commit messages

---

## 🔄 Environment Configuration

### Development (appsettings.Development.json)
- Detailed logging (Debug level)
- Local database
- Development features enabled

### Production (appsettings.json)
- Warning level logging
- Production database
- Security features enabled
- HTTPS required

---

## 📊 Monitoring & Logs

### Log Locations
- **File Logs:** `logs/gsmsharing-YYYYMMDD.txt`
- **Console:** Application output
- **Log Level:** Configured in appsettings.json

### Log Levels
- **Debug:** Detailed information (Development only)
- **Information:** General information
- **Warning:** Warning messages
- **Error:** Error messages
- **Fatal:** Critical errors

---

## ✅ Verification Checklist

After setup, verify:

- [ ] Application builds successfully
- [ ] Application runs without errors
- [ ] Database connection works
- [ ] Tables are created
- [ ] Logging is working
- [ ] User registration works
- [ ] User login works
- [ ] Posts can be created
- [ ] Posts can be viewed

---

## 🆘 Getting Help

### Documentation
- Review `docs/` folder for detailed documentation
- Check `PRD.md` for requirements
- Review `ROADMAP.md` for development plan

### Common Commands
```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Clean build artifacts
dotnet clean

# Run tests (when available)
dotnet test
```

---

## 🎯 Next Steps

After setup is complete:

1. Review codebase structure
2. Read PRD and Roadmap
3. Start Phase 1: Architecture Modernization
4. Begin implementing service layer

---

**Last Updated:** December 2024  
**Status:** Phase 0 Complete

---

**End of Development Setup Guide**

