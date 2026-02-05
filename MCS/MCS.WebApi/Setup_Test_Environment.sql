-- ============================================================
-- Script: Setup Test Environment
-- Database: dinspire_mcs_test
-- Purpose: Create all tables and admin user for test environment
-- ============================================================

USE dinspire_mcs_test;
GO

BEGIN TRANSACTION;
GO

PRINT '========================================';
PRINT 'Setting up Test Environment';
PRINT 'Database: dinspire_mcs_test';
PRINT '========================================';
PRINT '';

-- ============================================================
-- 1. CREATE ORGANIZATIONS TABLE
-- ============================================================
PRINT 'Creating Organizations table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Organizations')
BEGIN
    CREATE TABLE Organizations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Address1 NVARCHAR(200) NULL,
        Address2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        ZipCode NVARCHAR(20) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    PRINT 'Organizations table created successfully.';
END
ELSE
BEGIN
    PRINT 'Organizations table already exists.';
END
GO

-- ============================================================
-- 2. CREATE USERS TABLE
-- ============================================================
PRINT 'Creating Users table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NOT NULL,
        Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Owner', 'BranchAdmin', 'Staff')),
        Email NVARCHAR(200) NOT NULL,
        PhoneNumber NVARCHAR(20) NULL,
        Address1 NVARCHAR(200) NULL,
        Address2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        ZipCode NVARCHAR(20) NULL,
        OrgId INT NOT NULL,
        Level NVARCHAR(20) NOT NULL CHECK (Level IN ('Org', 'Branch')),
        BranchId INT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );

    -- Create unique index on Email (only for non-deleted users)
    CREATE UNIQUE INDEX UQ_Users_Email ON Users(Email) WHERE IsDeleted = 0;

    PRINT 'Users table created successfully.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END
GO

-- ============================================================
-- 3. CREATE BRANCHES TABLE
-- ============================================================
PRINT 'Creating Branches table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Branches')
BEGIN
    CREATE TABLE Branches (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Address1 NVARCHAR(200) NULL,
        Address2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        ZipCode NVARCHAR(20) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        OrgId INT NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    PRINT 'Branches table created successfully.';
END
ELSE
BEGIN
    PRINT 'Branches table already exists.';
END
GO

-- ============================================================
-- 4. CREATE CENTERS TABLE
-- ============================================================
PRINT 'Creating Centers table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Centers')
BEGIN
    CREATE TABLE Centers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        BranchId INT NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    PRINT 'Centers table created successfully.';
END
ELSE
BEGIN
    PRINT 'Centers table already exists.';
END
GO

-- ============================================================
-- 5. CREATE POCs TABLE
-- ============================================================
PRINT 'Creating POCs table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'POCs')
BEGIN
    CREATE TABLE POCs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        AltPhone NVARCHAR(20) NULL,
        Address1 NVARCHAR(200) NULL,
        Address2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        ZipCode NVARCHAR(20) NULL,
        Aadhaar NVARCHAR(20) NULL,
        DOB DATE NULL,
        Age INT NOT NULL,
        CenterId INT NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    PRINT 'POCs table created successfully.';
END
ELSE
BEGIN
    PRINT 'POCs table already exists.';
END
GO

-- ============================================================
-- 6. CREATE MEMBERS TABLE
-- ============================================================
PRINT 'Creating Members table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Members')
BEGIN
    CREATE TABLE Members (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        AltPhone NVARCHAR(20) NULL,
        Address1 NVARCHAR(200) NULL,
        Address2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        ZipCode NVARCHAR(20) NULL,
        Aadhaar NVARCHAR(20) NULL,
        DOB DATE NULL,
        Age INT NOT NULL,
        GuardianFirstName NVARCHAR(100) NOT NULL,
        GuardianMiddleName NVARCHAR(100) NULL,
        GuardianLastName NVARCHAR(100) NOT NULL,
        GuardianPhone NVARCHAR(20) NOT NULL,
        GuardianDOB DATE NULL,
        GuardianAge INT NOT NULL,
        CenterId INT NOT NULL,
        POCId INT NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    PRINT 'Members table created successfully.';
END
ELSE
BEGIN
    PRINT 'Members table already exists.';
END
GO

-- ============================================================
-- 7. CREATE FOREIGN KEY CONSTRAINTS
-- ============================================================
PRINT 'Creating foreign key constraints...';

-- Users table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Org')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT FK_Users_Org FOREIGN KEY (OrgId) REFERENCES Organizations(Id);
    PRINT 'Created FK_Users_Org.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Branch')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT FK_Users_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id);
    PRINT 'Created FK_Users_Branch.';
END

-- Branches table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Branches_Org')
BEGIN
    ALTER TABLE Branches ADD CONSTRAINT FK_Branches_Org FOREIGN KEY (OrgId) REFERENCES Organizations(Id);
    PRINT 'Created FK_Branches_Org.';
END

-- Centers table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Centers_Branch')
BEGIN
    ALTER TABLE Centers ADD CONSTRAINT FK_Centers_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id);
    PRINT 'Created FK_Centers_Branch.';
END

-- POCs table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_POCs_Center')
BEGIN
    ALTER TABLE POCs ADD CONSTRAINT FK_POCs_Center FOREIGN KEY (CenterId) REFERENCES Centers(Id);
    PRINT 'Created FK_POCs_Center.';
END

-- Members table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_Center')
BEGIN
    ALTER TABLE Members ADD CONSTRAINT FK_Members_Center FOREIGN KEY (CenterId) REFERENCES Centers(Id);
    PRINT 'Created FK_Members_Center.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_POC')
BEGIN
    ALTER TABLE Members ADD CONSTRAINT FK_Members_POC FOREIGN KEY (POCId) REFERENCES POCs(Id);
    PRINT 'Created FK_Members_POC.';
END

-- Audit foreign keys (self-referencing for Users)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Users_CreatedBy.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_ModifiedBy')
BEGIN
    ALTER TABLE Users ADD CONSTRAINT FK_Users_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Users_ModifiedBy.';
END

-- Audit foreign keys for Organizations
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_CreatedBy')
BEGIN
    ALTER TABLE Organizations ADD CONSTRAINT FK_Org_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Org_CreatedBy.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_ModifiedBy')
BEGIN
    ALTER TABLE Organizations ADD CONSTRAINT FK_Org_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Org_ModifiedBy.';
END

-- Audit foreign keys for Branches
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Branch_CreatedBy')
BEGIN
    ALTER TABLE Branches ADD CONSTRAINT FK_Branch_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Branch_CreatedBy.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Branch_ModifiedBy')
BEGIN
    ALTER TABLE Branches ADD CONSTRAINT FK_Branch_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Branch_ModifiedBy.';
END

-- Audit foreign keys for Centers
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Centers_CreatedBy')
BEGIN
    ALTER TABLE Centers ADD CONSTRAINT FK_Centers_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Centers_CreatedBy.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Centers_ModifiedBy')
BEGIN
    ALTER TABLE Centers ADD CONSTRAINT FK_Centers_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Centers_ModifiedBy.';
END

-- Audit foreign keys for POCs
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_POCs_CreatedBy')
BEGIN
    ALTER TABLE POCs ADD CONSTRAINT FK_POCs_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    PRINT 'Created FK_POCs_CreatedBy.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_POCs_ModifiedBy')
BEGIN
    ALTER TABLE POCs ADD CONSTRAINT FK_POCs_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    PRINT 'Created FK_POCs_ModifiedBy.';
END

-- Audit foreign keys for Members
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_CreatedBy')
BEGIN
    ALTER TABLE Members ADD CONSTRAINT FK_Members_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Members_CreatedBy.';
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_ModifiedBy')
BEGIN
    ALTER TABLE Members ADD CONSTRAINT FK_Members_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
    PRINT 'Created FK_Members_ModifiedBy.';
END

PRINT 'Foreign key constraints created.';
PRINT '';

-- ============================================================
-- 8. CREATE SYSTEM USER (Id=1)
-- ============================================================
PRINT 'Creating system user...';

IF NOT EXISTS (SELECT * FROM Users WHERE Id = 1)
BEGIN
    -- Temporarily disable foreign key constraints to break circular dependency
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    BEGIN
        ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_CreatedBy;
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Org')
    BEGIN
        ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_Org;
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_CreatedBy')
    BEGIN
        ALTER TABLE Organizations NOCHECK CONSTRAINT FK_Org_CreatedBy;
    END

    -- Create a system organization first
    DECLARE @SystemOrgId INT;
    
    IF NOT EXISTS (SELECT * FROM Organizations WHERE Id = 1)
    BEGIN
        SET IDENTITY_INSERT Organizations ON;
        INSERT INTO Organizations (Id, Name, CreatedBy, CreatedAt, IsDeleted)
        VALUES (1, 'System Organization', 1, GETDATE(), 0);
        SET IDENTITY_INSERT Organizations OFF;
        SET @SystemOrgId = 1;
        PRINT 'Created system organization (Id=1).';
    END
    ELSE
    BEGIN
        SET @SystemOrgId = 1;
        PRINT 'System organization (Id=1) already exists.';
    END

    -- Create system user with self-reference
    SET IDENTITY_INSERT Users ON;
    
    INSERT INTO Users (
        Id,
        FirstName,
        LastName,
        Role,
        Email,
        OrgId,
        Level,
        PasswordHash,
        CreatedBy,
        CreatedAt,
        IsDeleted
    )
    VALUES (
        1,
        'System',
        'User',
        'Owner',
        'system@mcs.local',
        @SystemOrgId,
        'Org',
        '$2a$11$SystemUserPasswordHashPlaceholder', -- Placeholder hash
        1, -- Self-reference
        GETDATE(),
        0
    );
    
    SET IDENTITY_INSERT Users OFF;
    
    -- Re-enable the constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_CreatedBy')
    BEGIN
        ALTER TABLE Organizations CHECK CONSTRAINT FK_Org_CreatedBy;
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Org')
    BEGIN
        ALTER TABLE Users CHECK CONSTRAINT FK_Users_Org;
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    BEGIN
        ALTER TABLE Users CHECK CONSTRAINT FK_Users_CreatedBy;
    END
    
    PRINT 'System user created successfully (Id=1).';
END
ELSE
BEGIN
    PRINT 'System user (Id=1) already exists.';
END
GO

-- ============================================================
-- 9. CREATE ADMIN USER
-- ============================================================
PRINT 'Creating admin user...';

-- Configuration - Update these values as needed
DECLARE @AdminFirstName NVARCHAR(100) = 'Admin';
DECLARE @AdminMiddleName NVARCHAR(100) = NULL;
DECLARE @AdminLastName NVARCHAR(100) = 'User';
DECLARE @AdminEmail NVARCHAR(200) = 'admin@test.com';
DECLARE @AdminPassword NVARCHAR(MAX) = 'Admin123!'; -- Change this password
DECLARE @AdminPhoneNumber NVARCHAR(20) = NULL;
DECLARE @AdminAddress1 NVARCHAR(200) = NULL;
DECLARE @AdminAddress2 NVARCHAR(200) = NULL;
DECLARE @AdminCity NVARCHAR(100) = NULL;
DECLARE @AdminState NVARCHAR(100) = NULL;
DECLARE @AdminZipCode NVARCHAR(20) = NULL;

-- Check if admin user already exists
IF EXISTS (SELECT * FROM Users WHERE Email = @AdminEmail AND IsDeleted = 0)
BEGIN
    PRINT 'Admin user with email ' + @AdminEmail + ' already exists.';
END
ELSE
BEGIN
    -- Create admin organization
    DECLARE @AdminOrgId INT;
    
    INSERT INTO Organizations (
        Name,
        Address1,
        Address2,
        City,
        State,
        ZipCode,
        PhoneNumber,
        CreatedBy,
        CreatedAt,
        IsDeleted
    )
    VALUES (
        'Test Organization',
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        1, -- System user
        GETDATE(),
        0
    );
    
    SET @AdminOrgId = SCOPE_IDENTITY();
    PRINT 'Created admin organization with Id: ' + CAST(@AdminOrgId AS NVARCHAR);
    
    -- Generate BCrypt hash for password
    -- NOTE: You need to generate a BCrypt hash externally and replace the placeholder below
    -- Use: https://bcrypt-generator.com/ or the Generate_BCrypt_Hash.ps1 script
    -- For now, using a placeholder - REPLACE THIS WITH ACTUAL BCRYPT HASH
    DECLARE @PasswordHash NVARCHAR(MAX);
    
    -- Option 1: Use a pre-generated BCrypt hash (replace with actual hash)
    -- Example BCrypt hash for 'Admin123!': $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy
    -- SET @PasswordHash = '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy';
    
    -- Option 2: For testing only - using SHA256 (NOT for production)
    -- Replace with actual BCrypt hash before using in production
    SET @PasswordHash = CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_256', @AdminPassword), 2);
    
    PRINT 'WARNING: Using SHA256 hash for testing. Replace with BCrypt hash for production!';
    PRINT 'To generate BCrypt hash, use: https://bcrypt-generator.com/';
    PRINT 'Or run: .\Generate_BCrypt_Hash.ps1 -Password "' + @AdminPassword + '"';
    PRINT '';
    
    -- Create admin user
    INSERT INTO Users (
        FirstName,
        MiddleName,
        LastName,
        Role,
        Email,
        PhoneNumber,
        Address1,
        Address2,
        City,
        State,
        ZipCode,
        OrgId,
        Level,
        BranchId,
        PasswordHash,
        CreatedBy,
        CreatedAt,
        IsDeleted
    )
    VALUES (
        @AdminFirstName,
        @AdminMiddleName,
        @AdminLastName,
        'Owner',
        @AdminEmail,
        @AdminPhoneNumber,
        @AdminAddress1,
        @AdminAddress2,
        @AdminCity,
        @AdminState,
        @AdminZipCode,
        @AdminOrgId,
        'Org',
        NULL, -- Admin is at Org level, not Branch level
        @PasswordHash,
        1, -- System user
        GETDATE(),
        0
    );
    
    DECLARE @AdminUserId INT = SCOPE_IDENTITY();
    
    PRINT '========================================';
    PRINT 'Admin user created successfully!';
    PRINT '========================================';
    PRINT 'User ID: ' + CAST(@AdminUserId AS NVARCHAR);
    PRINT 'Email: ' + @AdminEmail;
    PRINT 'Password: ' + @AdminPassword;
    PRINT 'Organization ID: ' + CAST(@AdminOrgId AS NVARCHAR);
    PRINT '';
    PRINT 'IMPORTANT:';
    PRINT '1. Change the password after first login';
    PRINT '2. Replace the password hash with a proper BCrypt hash';
    PRINT '   Use: https://bcrypt-generator.com/';
    PRINT '   Or: .\Generate_BCrypt_Hash.ps1 -Password "' + @AdminPassword + '"';
    PRINT '========================================';
END
GO

-- ============================================================
-- 10. SUMMARY
-- ============================================================
PRINT '';
PRINT '========================================';
PRINT 'Test Environment Setup Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Tables created:';
PRINT '  - Organizations';
PRINT '  - Users';
PRINT '  - Branches';
PRINT '  - Centers';
PRINT '  - POCs';
PRINT '  - Members';
PRINT '';
PRINT 'Users created:';
PRINT '  - System User (Id=1, Email: system@mcs.local)';
PRINT '  - Admin User (Email: admin@test.com)';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Generate BCrypt hash for admin password';
PRINT '  2. Update Users.PasswordHash with BCrypt hash';
PRINT '  3. Test login with admin credentials';
PRINT '========================================';
GO

COMMIT TRANSACTION;
GO
