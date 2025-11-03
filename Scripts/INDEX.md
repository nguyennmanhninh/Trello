# 🛠️ Scripts Index

This folder contains automation scripts for various tasks.

## 📁 Script Categories

### PowerShell Scripts (.ps1)

#### Database & Data Management
- `ImportSampleData.ps1` - Import sample data to database
- `ImportStoredProcedures.ps1` - Import stored procedures
- `SyncData.ps1` - Sync data between environments
- `SyncDataCorrect.ps1` - Corrected data sync
- `SyncRemoteToLocal.ps1` - Sync from remote to local DB

#### Deployment & Production
- `DeployToProduction.ps1` - Deploy to production server
- `DeployToProduction_Fixed.ps1` - Fixed deployment script
- `ApplyImprovements.ps1` - Apply system improvements

#### Testing & Debugging
- `test-ai-chat.ps1` - Test AI chatbot
- `TestApiEndpoints.ps1` - Test API endpoints
- `test_endpoints.ps1` - Endpoint testing
- `test_gemini.ps1` - Test Gemini API integration
- `verify_sp_usage.ps1` - Verify stored procedure usage

#### Authentication & Security
- `FixAdminLogin.ps1` - Fix admin login issues
- `GetHashes.ps1` - Get password hashes for debugging

### Python Scripts (.py)

#### AI & Knowledge Base
- `index_codebase.py` - Index codebase for AI RAG
- `generate_knowledge_base.py` - Generate knowledge base from code

#### Data Processing
- `fix_teachers_template.py` - Fix teacher data template

### Batch Files (.bat)

#### Quick Start
- `run.bat` - Quick start application (backend + frontend)
- `debug.bat` - Start in debug mode

## 🚀 Common Usage

### Start Application
```powershell
.\run.bat
# or
.\debug.bat
```

### Setup Database
```powershell
.\ImportSampleData.ps1
.\ImportStoredProcedures.ps1
```

### Test System
```powershell
.\TestApiEndpoints.ps1
.\test-ai-chat.ps1
.\test_gemini.ps1
```

### Deploy
```powershell
.\DeployToProduction_Fixed.ps1
```

### Sync Data
```powershell
.\SyncRemoteToLocal.ps1
# or
.\SyncDataCorrect.ps1
```

## ⚠️ Important Notes

### Prerequisites
- **PowerShell 5.1+** for .ps1 scripts
- **Python 3.8+** for .py scripts
- **SQL Server** running for database scripts
- **Admin privileges** may be required for some operations

### Execution Policy
If you get "cannot be loaded because running scripts is disabled":
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Environment Variables
Some scripts require:
- `ConnectionStrings:DefaultConnection` in appsettings.json
- `GEMINI_API_KEY` for AI scripts
- `SMTP` credentials for email scripts

## 📝 Script Descriptions

### ImportSampleData.ps1
Imports sample data including:
- Students (19 records)
- Teachers (4 records)
- Classes (5 records)
- Courses (10 records)
- Grades (sample grades)
- Departments

**Usage:**
```powershell
.\ImportSampleData.ps1
```

### test-ai-chat.ps1
Tests AI chatbot functionality:
- Backend health check
- Gemini API connection
- Sample question testing
- Response validation

**Usage:**
```powershell
.\test-ai-chat.ps1
```

### run.bat
Starts both backend and frontend:
1. ASP.NET Core backend (port 5298)
2. Angular frontend (port 4200)

**Usage:**
```cmd
run.bat
```

### index_codebase.py
Indexes codebase for RAG system:
- Scans all C#, TypeScript, HTML files
- Extracts code snippets
- Builds vector embeddings
- Stores in knowledge base

**Usage:**
```bash
python index_codebase.py
```

---

**Total Scripts:** 21 files
**Last Updated:** October 27, 2025
