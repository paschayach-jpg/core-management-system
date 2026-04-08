# Database Connection Check Report

## Current Configuration

### Connection String
- **Server**: 192.168.250.153:2023 (K2dev server)
- **Database**: SCS
- **Authentication**: SQL Server Authentication
- **User ID**: K2pack
- **Connection Timeout**: 30 seconds

Location: [CMS.Presentation/app.config](../CMS.Presentation/app.config)

```xml
<connectionStrings>
    <add name="CmsDb"
         connectionString="Data Source=192.168.250.153,2023;Initial Catalog=SCS;User ID=K2pack;Password=P@ssw0rd;MultipleActiveResultSets=False;Connect Timeout=30;"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

## Configuration Architecture

### 1. **ConfigManager** ([CMS.Infrastructure/ConfigManager.cs](../CMS.Infrastructure/ConfigManager.cs))
- Reads configuration with fallback order:
  1. In-memory cache (to avoid DB round-trips)
  2. app.config appSettings
  3. Database SystemConfig table
- Includes caching layer for performance
- Persists changes to database via stored procedure

### 2. **Data Access Layer** ([CMS.Data/Repositories](../CMS.Data/Repositories/))
- Multiple repositories use SQL Server for data access:
  - UserAccountRepository
  - FolderRepository
  - FileDocumentRepository
  - ApprovalRepository
  - AuditRepository
  - And 19+ more repositories
- All use SqlConnection with stored procedures

### 3. **Additional Configuration**
- **NetworkDrivePath**: `\\150.1.8.23\AppForUser\Cmd_LmportFile`
- **Employee API**: `http://150.1.8.28:13001/api`
- **AD Auth API**: `http://150.1.8.28:7653`

## Testing Database Connection

### Recent Addition: DatabaseConnectionTester
A new utility class has been created: [CMS.Infrastructure/DatabaseConnectionTester.cs](../CMS.Infrastructure/DatabaseConnectionTester.cs)

#### Usage:

```csharp
// Full diagnostic test
DatabaseConnectionTester.TestConnection();

// Quick boolean check
bool isConnected = DatabaseConnectionTester.IsConnected();

// Get connection details
var info = DatabaseConnectionTester.GetConnectionInfo();
Console.WriteLine(info);
// Output: "Connected to 192.168.250.153,2023/SCS as K2pack (v..."
```

#### What the Test Does:
1. ✓ Reads connection string from app.config
2. ✓ Parses connection string details (server, database, user)
3. ✓ Tests basic connectivity to SQL Server
4. ✓ Executes test query (COUNT tables)
5. ✓ Checks for SystemConfig table
6. ✓ Validates required stored procedures:
   - sp_GetSystemConfig
   - sp_UpsertSystemConfig

#### Test Output Example:
```
========== DATABASE CONNECTION TEST ==========
Test Time: 2026-04-08 10:30:45

Step 1: Reading connection string from app.config...
✓ Connection string found
  Provider: System.Data.SqlClient
  Server: 192.168.250.153,2023
  Database: SCS
  User ID: K2pack
  Connect Timeout: 30s

Step 2: Testing basic connection...
✓ Successfully connected to database
  Database State: Open
  Server Version: 15.00.4033

Step 3: Executing test query...
✓ Query executed successfully
  Tables in database: 127

Step 4: Checking for SystemConfig table...
✓ SystemConfig table found

Step 5: Checking for required stored procedures...
✓ sp_GetSystemConfig found
✓ sp_UpsertSystemConfig found

========== TEST COMPLETE ==========
```

## How to Run the Test

### Option 1: From a Console Application
Add reference to CMS.Infrastructure in a test/console project and call:
```csharp
DatabaseConnectionTester.TestConnection();
```

### Option 2: From Unit Tests
Check [CMS.Tests](../CMS.Tests/) for integration test examples.

### Option 3: Manual SQL Connection Test
Open SQL Server Management Studio:
1. Server: 192.168.250.153,2023
2. Authentication: SQL Server Authentication
3. Login: K2pack
4. Password: P@ssw0rd (from app.config)
5. Database: SCS

## Troubleshooting

### Common Issues:

| Issue | Cause | Solution |
|-------|-------|----------|
| "Connection string 'CmsDb' not found" | Missing app.config | Ensure CMS.Presentation/app.config exists |
| "Network path not found" | Server unreachable | Check network connectivity to 192.168.250.153 |
| "Login failed for user 'K2pack'" | Wrong credentials | Verify password in app.config |
| "Connect timeout" | Slow network | Increase Connect Timeout in connection string |
| "Cannot open database 'SCS'" | Database doesn't exist | Verify database name and permissions |

### Required Database Objects:
- ✓ Tables (check with: SELECT COUNT(*) FROM sys.tables)
- ✓ SystemConfig table (required for ConfigManager fallback)
- ✓ Stored procedures (sp_GetSystemConfig, sp_UpsertSystemConfig)
- ✓ Core and repository schema procedures

## Security Notes
⚠️ **WARNING**: Connection string contains credentials in plain text
- User ID: K2pack
- Password: P@ssw0rd
- Consider implementing encrypted configuration in production
- Review [CMS.Infrastructure/ConfigManager.cs](../CMS.Infrastructure/ConfigManager.cs) for secure alternatives

## Related Files
- Configuration: [CMS.Presentation/app.config](../CMS.Presentation/app.config)
- Config Manager: [CMS.Infrastructure/ConfigManager.cs](../CMS.Infrastructure/ConfigManager.cs)
- Data Layer: [CMS.Data/](../CMS.Data/)
- Database Scripts: [DB_Scripts/](../DB_Scripts/)
- Connection Tester: [CMS.Infrastructure/DatabaseConnectionTester.cs](../CMS.Infrastructure/DatabaseConnectionTester.cs) _(NEW)_
