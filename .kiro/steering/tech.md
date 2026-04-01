# Tech Stack

## Language & Runtime
- C# targeting .NET Framework 4.5
- Windows Forms (WinForms) — MDI desktop application

## Database
- SQL Server with T-SQL stored procedures
- ADO.NET for all data access (no ORM)
- Schema managed via numbered migration scripts in `CMS.Data/Scripts/`

## Key Libraries
- **log4net 2.0.15** — logging; falls back to Windows Event Log on failure
- **iTextSharp 5.5.13.3** — PDF generation and security
- **BouncyCastle 1.8.9** — cryptography
- **System.DirectoryServices** — Active Directory integration
- **ClamAV** — virus scanning (via adapter in Infrastructure)

## Build System
- Visual Studio 2016+ solution (`CMS.sln`)
- MSBuild `.csproj` files per project
- NuGet for package management (`packages.config` per project)

## Common Commands

```bash
# Build the solution (from repo root)
msbuild CMS.sln /p:Configuration=Debug

# Build release
msbuild CMS.sln /p:Configuration=Release

# Restore NuGet packages (if using nuget.exe)
nuget restore CMS.sln
```

## Database Migrations
SQL scripts are applied manually in order. Always add new scripts with the next sequential number:
```
CMS.Data/Scripts/NNN_Description.sql
```
