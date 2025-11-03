# 🗄️ Database Scripts Index

This folder contains SQL scripts and database-related files.

## 📁 File Categories

### Setup & Schema

#### Full Setup
- `FULL_DATABASE_SETUP.sql` - **Complete database setup** ⭐
  - Creates all tables
  - Sets up constraints
  - Adds indexes
  - Inserts initial data

#### Schema Updates
- `DATABASE_UPDATE.sql` - Database schema updates
- `SIMPLE_DB_UPDATE.sql` - Simple schema updates
- `UPDATE_LOCAL_SCHEMA_FOR_IMPORT.sql` - Prepare schema for import
- `UPDATE_SCHEMA_FOR_SPS.sql` - Update schema for stored procedures
- `MAKE_LOCAL_COMPATIBLE.sql` - Make local DB compatible

### Data Import/Export

#### Sample Data
- `INSERT_SAMPLE_DATA.sql` - **Insert sample data** ⭐
  - 19 students
  - 4 teachers
  - 5 classes
  - 10 courses
  - Sample grades

#### Import Scripts
- `QUICK_IMPORT.sql` - Quick data import
- `CLEAN_IMPORT.sql` - Clean import (removes existing data)
- `FINAL_IMPORT_FROM_REMOTE.sql` - Import from remote DB

#### Export Scripts
- `REMOTE_DATA_EXPORT.sql` - Export remote data
- `REMOTE_DATA_EXPORT_CORRECT.sql` - Corrected export
- `export_query.sql` - Custom export queries

### Stored Procedures

#### Main Procedures
- `STORED_PROCEDURES.sql` - **All stored procedures** ⭐
  - Students CRUD
  - Teachers CRUD
  - Classes CRUD
  - Courses CRUD
  - Grades CRUD

#### Modular Procedures
- `STORED_PROCEDURES_CLASSES.sql` - Classes procedures
- `STORED_PROCEDURES_COURSES.sql` - Courses procedures
- `STORED_PROCEDURES_GRADES.sql` - Grades procedures
- `STORED_PROCEDURES_TEACHERS.sql` - Teachers procedures

### Bug Fixes & Patches

#### Constraint Fixes
- `FIX_UNIQUE_CONSTRAINTS.sql` - Fix unique constraint issues
- `QUICK_FIX_UNIQUE.sql` - Quick unique constraint fix
- `FIX_GRADE_DELETION_POLICY.sql` - Fix grade deletion constraints

#### Authentication Fixes
- `FIX_ADMIN_LOGIN.sql` - Fix admin login
- `UPDATE_CORRECT_HASHES.sql` - Update password hashes

#### Feature Additions
- `ADD_EMAIL_VERIFICATION.sql` - Add email verification fields

### Testing & Utilities

#### Connection Testing
- `TEST_CONNECTION.sql` - Test database connection

#### Data Cleanup
- `clear_local.sql` - Clear local database

### Result Files

#### Import Results
- `import_result.txt` - Sample data import results
- `db_setup_result.txt` - Database setup results

#### Schema Documentation
- `REMOTE_SCHEMA.txt` - Remote database schema

#### Stored Procedure Results
- `sp_import_result.txt` - SP import results
- `sp_result.txt` - SP execution results
- `sp_result2.txt` - Additional SP results

## 🚀 Quick Start

### 1. Full Database Setup
```sql
-- Run in SSMS or Azure Data Studio
-- Execute: FULL_DATABASE_SETUP.sql
-- Then: INSERT_SAMPLE_DATA.sql
```

### 2. PowerShell Import (Recommended)
```powershell
cd ..\Scripts
.\ImportSampleData.ps1
```

### 3. Stored Procedures Only
```sql
-- Execute: STORED_PROCEDURES.sql
-- Or use modular approach:
STORED_PROCEDURES_CLASSES.sql
STORED_PROCEDURES_COURSES.sql
STORED_PROCEDURES_GRADES.sql
STORED_PROCEDURES_TEACHERS.sql
```

## 📊 Database Schema

### Core Tables
- **Users** - Authentication
- **Students** - Student information
- **Teachers** - Teacher information
- **Classes** - Class information
- **Courses** - Course catalog
- **Grades** - Student grades
- **Departments** - Academic departments

### Authentication Tables
- **OTPVerifications** - Email verification OTPs

## ⚠️ Important Notes

### Connection String
Update in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Execution Order
1. `FULL_DATABASE_SETUP.sql` - Creates schema
2. `INSERT_SAMPLE_DATA.sql` - Adds sample data
3. `STORED_PROCEDURES.sql` - Adds procedures
4. `ADD_EMAIL_VERIFICATION.sql` - Optional email feature

### Sample Accounts
After running `INSERT_SAMPLE_DATA.sql`:
- **Admin:** admin / admin123
- **Teacher:** gv001 / gv001
- **Student:** sv001 / sv001

### Password Hashing
All passwords use **SHA256** hashing:
```sql
-- Example
CONVERT(VARCHAR(64), 
  HASHBYTES('SHA2_256', 'password123'), 2)
```

## 🔧 Common Tasks

### Reset Database
```sql
-- 1. Drop database
DROP DATABASE IF EXISTS StudentManagementDB;

-- 2. Run FULL_DATABASE_SETUP.sql
-- 3. Run INSERT_SAMPLE_DATA.sql
```

### Add New User
```sql
-- See INSERT_SAMPLE_DATA.sql for examples
INSERT INTO Users (Username, PasswordHash, Role, EntityId)
VALUES ('newuser', 
  CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'password'), 2),
  'Student', 'SV999');
```

### Update Stored Procedures
```sql
-- Run STORED_PROCEDURES.sql
-- Or specific procedure file
```

## 📝 File Descriptions

### FULL_DATABASE_SETUP.sql
Complete database initialization:
- Creates StudentManagementDB
- All tables with constraints
- Indexes for performance
- Initial departments data
- Utility functions

### STORED_PROCEDURES.sql
All CRUD stored procedures:
- `sp_GetStudents` - Paginated student list
- `sp_CreateStudent` - Create new student
- `sp_UpdateStudent` - Update student
- `sp_DeleteStudent` - Delete student
- Similar procedures for Teachers, Classes, Courses, Grades

### INSERT_SAMPLE_DATA.sql
Sample data for testing:
- 4 Departments (CNTT, Kinh tế, Cơ khí, KT)
- 5 Classes
- 4 Teachers
- 19 Students
- 10 Courses
- Sample grades with classifications

---

**Total SQL Files:** 28 files
**Database:** StudentManagementDB
**Last Updated:** October 27, 2025
