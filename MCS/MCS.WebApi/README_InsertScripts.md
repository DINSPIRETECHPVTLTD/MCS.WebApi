# Organization and Owner Insert Scripts

This folder contains scripts to create an Organization and Owner user in the database.

## Files

1. **`Insert_Organization_And_Owner.sql`** - Full-featured script with all options
2. **`Insert_Organization_And_Owner_Simple.sql`** - Simplified version for quick setup
3. **`Generate_BCrypt_Hash.ps1`** - PowerShell helper to generate BCrypt password hashes

## Quick Start

### Option 1: Simple Script (Recommended for Testing)

1. Open `Insert_Organization_And_Owner_Simple.sql`
2. Update the values at the top:
   ```sql
   DECLARE @OrgName NVARCHAR(200) = 'My Organization';
   DECLARE @OwnerEmail NVARCHAR(200) = 'owner@myorganization.com';
   DECLARE @OwnerPassword NVARCHAR(MAX) = 'TempPassword123!';
   DECLARE @OwnerFirstName NVARCHAR(100) = 'John';
   DECLARE @OwnerLastName NVARCHAR(100) = 'Doe';
   ```
3. Change the database name: `USE [YourDatabaseName];`
4. Run the script in SQL Server Management Studio

### Option 2: Full-Featured Script

1. Open `Insert_Organization_And_Owner.sql`
2. Update all configuration values in the CONFIGURATION section
3. Change the database name
4. Run the script

## Password Hashing

### For Testing (Quick Setup)
The scripts use SHA2_256 hashing by default, which works for initial setup but is **NOT secure for production**.

### For Production (Recommended)
You must use **BCrypt** hashing. Here are your options:

#### Option 1: Online Tool (Easiest)
1. Visit: https://bcrypt-generator.com/
2. Enter your password
3. Copy the generated hash
4. Replace the `@PasswordHash` line in the SQL script:
   ```sql
   SET @PasswordHash = '$2a$11$YourGeneratedBCryptHashHere';
   ```

#### Option 2: .NET Code
```csharp
using BCrypt.Net;

string password = "YourPassword123!";
string hash = BCrypt.HashPassword(password);
Console.WriteLine(hash);
```

#### Option 3: PowerShell Script
Run the provided PowerShell script:
```powershell
.\Generate_BCrypt_Hash.ps1 -Password "YourPassword123!"
```

#### Option 4: C# Console Application
```csharp
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string password = "YourPassword123!";
        string hash = BCrypt.HashPassword(password);
        Console.WriteLine(hash);
    }
}
```

## Prerequisites

1. **Migration scripts must be run first**
   - Run `Migration_AddMissingColumns.sql` or
   - Run `Migration_CreateSystemUser.sql` first

2. **System User must exist**
   - The scripts require a system user with Id = 1
   - This is created automatically by the migration scripts

## What the Scripts Do

1. ✅ Creates a new Organization
2. ✅ Creates an Owner user for that Organization
3. ✅ Links the Owner to the Organization
4. ✅ Sets up proper audit fields (CreatedBy, CreatedAt)
5. ✅ Handles self-referencing foreign key constraints

## After Running

1. **Change the password** after first login
2. **Replace SHA2_256 hash with BCrypt hash** for production
3. **Verify** the Organization and Owner were created correctly:
   ```sql
   SELECT * FROM Organizations WHERE Name = 'My Organization';
   SELECT * FROM Users WHERE Email = 'owner@myorganization.com';
   ```

## Troubleshooting

### Error: "System user does not exist"
- Run `Migration_CreateSystemUser.sql` first
- Or ensure a user with Id = 1 exists in the Users table

### Error: "User with email already exists"
- The email address is already in use
- Change the `@OwnerEmail` value to a unique email

### Error: "Foreign key constraint violation"
- Ensure Organizations table exists
- Ensure Users table exists
- Check that foreign key constraints are properly set up

## Security Notes

⚠️ **IMPORTANT:**
- Never commit passwords to version control
- Use BCrypt hashing for production
- Change default passwords immediately
- Use strong passwords (min 12 characters, mixed case, numbers, symbols)
- Consider using environment variables or secure configuration for passwords

## Example Usage

```sql
-- Quick setup for development
USE [MCS_Database];
GO

DECLARE @OrgName NVARCHAR(200) = 'Acme Corporation';
DECLARE @OwnerEmail NVARCHAR(200) = 'admin@acme.com';
DECLARE @OwnerPassword NVARCHAR(MAX) = 'SecurePass123!';
DECLARE @OwnerFirstName NVARCHAR(100) = 'Jane';
DECLARE @OwnerLastName NVARCHAR(100) = 'Smith';

-- ... rest of script ...
```

