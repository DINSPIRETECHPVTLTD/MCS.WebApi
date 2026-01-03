-- ============================================================
-- Script: Insert Organization and Owner User
-- ============================================================
-- This script creates a new Organization and an Owner user
-- Run this after the migration script has been executed
-- ============================================================

USE dinspire_mf_dev; 
GO

BEGIN TRANSACTION;
GO

-- ============================================================
-- CONFIGURATION - Update these values as needed
-- ============================================================
DECLARE @OrgName NVARCHAR(200) = 'Navya Micro Credit Services'; -- Change this
DECLARE @OrgAddress1 NVARCHAR(200) = '123 Main Street';
DECLARE @OrgAddress2 NVARCHAR(200) = NULL;
DECLARE @OrgCity NVARCHAR(100) = 'City';
DECLARE @OrgState NVARCHAR(100) = 'State';
DECLARE @OrgZipCode NVARCHAR(20) = '12345';
DECLARE @OrgPhoneNumber NVARCHAR(20) = '123-456-7890';

DECLARE @OwnerFirstName NVARCHAR(100) = 'John';
DECLARE @OwnerMiddleName NVARCHAR(100) = NULL;
DECLARE @OwnerLastName NVARCHAR(100) = 'Doe';
DECLARE @OwnerEmail NVARCHAR(200) = 'owner@demo.com'; 
DECLARE @OwnerPassword NVARCHAR(MAX) = 'Admin123!'; -- Change this - will be hashed
DECLARE @OwnerPhoneNumber NVARCHAR(20) = '123-456-7890';
DECLARE @OwnerAddress1 NVARCHAR(200) = NULL;
DECLARE @OwnerAddress2 NVARCHAR(200) = NULL;
DECLARE @OwnerCity NVARCHAR(100) = NULL;
DECLARE @OwnerState NVARCHAR(100) = NULL;
DECLARE @OwnerZipCode NVARCHAR(20) = NULL;

-- System user ID (should be 1 if system user was created)
DECLARE @SystemUserId INT = 1;
-- ============================================================

PRINT 'Starting Organization and Owner creation...';
PRINT '';

-- ============================================================
-- 0. CREATE SYSTEM USER IF IT DOESN'T EXIST
-- ============================================================
IF NOT EXISTS (SELECT * FROM Users WHERE Id = @SystemUserId)
BEGIN
    PRINT 'System user (Id=' + CAST(@SystemUserId AS NVARCHAR) + ') does not exist. Creating system user...';
    
    -- Check if Organizations table exists
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Organizations')
    BEGIN
        PRINT 'ERROR: Organizations table does not exist!';
        PRINT 'Please run Migration_AddMissingColumns.sql first.';
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    DECLARE @SystemOrgId INT;
    
    -- Temporarily disable foreign key constraints to break circular dependency
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_CreatedBy')
    BEGIN
        ALTER TABLE Organizations NOCHECK CONSTRAINT FK_Org_CreatedBy;
        PRINT 'Temporarily disabled FK_Org_CreatedBy constraint.';
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    BEGIN
        ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_CreatedBy;
        PRINT 'Temporarily disabled FK_Users_CreatedBy constraint.';
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Org')
    BEGIN
        ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_Org;
        PRINT 'Temporarily disabled FK_Users_Org constraint.';
    END
    
    -- Check if organization exists AFTER disabling constraints
    SELECT TOP 1 @SystemOrgId = Id FROM Organizations ORDER BY Id;
    
    -- Create a system organization if none exists
    IF @SystemOrgId IS NULL
    BEGIN
        PRINT 'Creating system organization...';
        INSERT INTO Organizations (Name, CreatedBy, CreatedAt, IsDeleted)
        VALUES ('System Organization', @SystemUserId, GETDATE(), 0);
        SET @SystemOrgId = SCOPE_IDENTITY();
        PRINT 'Created system organization with Id: ' + CAST(@SystemOrgId AS NVARCHAR);
    END
    ELSE
    BEGIN
        PRINT 'Using existing organization with Id: ' + CAST(@SystemOrgId AS NVARCHAR);
    END
    
    -- Verify organization ID is set
    IF @SystemOrgId IS NULL
    BEGIN
        PRINT 'ERROR: Failed to get or create system organization!';
        ROLLBACK TRANSACTION;
        RETURN;
    END
    
    -- Create system user with self-reference
    PRINT 'Creating system user...';
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
        @SystemUserId,
        'System',
        'User',
        'Owner',
        'system@mcs.local',
        @SystemOrgId,
        'Org',
        CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_256', 'SystemPassword123!'), 2), -- Temporary hash
        @SystemUserId, -- Self-reference
        GETDATE(),
        0
    );
    
    SET IDENTITY_INSERT Users OFF;
    
    -- Re-enable the constraints
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Org')
    BEGIN
        ALTER TABLE Users CHECK CONSTRAINT FK_Users_Org;
        PRINT 'Re-enabled FK_Users_Org constraint.';
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    BEGIN
        ALTER TABLE Users CHECK CONSTRAINT FK_Users_CreatedBy;
        PRINT 'Re-enabled FK_Users_CreatedBy constraint.';
    END
    
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_CreatedBy')
    BEGIN
        ALTER TABLE Organizations CHECK CONSTRAINT FK_Org_CreatedBy;
        PRINT 'Re-enabled FK_Org_CreatedBy constraint.';
    END
    
    PRINT 'System user created successfully with Id: ' + CAST(@SystemUserId AS NVARCHAR);
    PRINT '';
END
ELSE
BEGIN
    PRINT 'System user (Id=' + CAST(@SystemUserId AS NVARCHAR) + ') already exists.';
    PRINT '';
END

-- Check if email already exists
IF EXISTS (SELECT * FROM Users WHERE Email = @OwnerEmail AND IsDeleted = 0)
BEGIN
    PRINT 'ERROR: User with email ' + @OwnerEmail + ' already exists!';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- ============================================================
-- 1. INSERT ORGANIZATION
-- ============================================================
PRINT 'Creating Organization: ' + @OrgName;

DECLARE @NewOrgId INT;

-- Verify system user exists before creating organization
IF NOT EXISTS (SELECT * FROM Users WHERE Id = @SystemUserId)
BEGIN
    PRINT 'ERROR: System user (Id=' + CAST(@SystemUserId AS NVARCHAR) + ') does not exist!';
    PRINT 'Cannot create organization without system user.';
    ROLLBACK TRANSACTION;
    RETURN;
END

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
    @OrgName,
    @OrgAddress1,
    @OrgAddress2,
    @OrgCity,
    @OrgState,
    @OrgZipCode,
    @OrgPhoneNumber,
    @SystemUserId,
    GETDATE(),
    0
);

SET @NewOrgId = SCOPE_IDENTITY();

-- Verify organization was created
IF @NewOrgId IS NULL
BEGIN
    PRINT 'ERROR: Failed to create organization!';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT 'Organization created with Id: ' + CAST(@NewOrgId AS NVARCHAR);
PRINT '';

-- ============================================================
-- 2. HASH PASSWORD (BCrypt format)
-- ============================================================
-- Note: In a real scenario, you would hash the password using BCrypt
-- For SQL Server, you can use a simple hash or generate BCrypt hash externally
-- This example uses a placeholder - you should replace with actual BCrypt hash
-- 
-- To generate BCrypt hash, you can:
-- 1. Use online BCrypt generator: https://bcrypt-generator.com/
-- 2. Use .NET code: BCrypt.Net.BCrypt.HashPassword("yourpassword")
-- 3. Use PowerShell or other tools
--
-- Example BCrypt hash format: $2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy

DECLARE @PasswordHash NVARCHAR(MAX);

-- Option 1: Use a pre-generated BCrypt hash (replace with actual hash)
-- SET @PasswordHash = '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy';

-- Option 2: Use SQL Server HASHBYTES (less secure, but works for initial setup)
-- This is NOT BCrypt, but can be used temporarily. Replace with BCrypt hash later.
SET @PasswordHash = CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_256', @OwnerPassword), 2);

-- Option 3: For production, generate BCrypt hash externally and use:
-- SET @PasswordHash = 'YOUR_BCRYPT_HASH_HERE';

PRINT 'Password hash generated (using SHA2_256 - replace with BCrypt hash for production)';
PRINT '';

-- ============================================================
-- 3. INSERT OWNER USER
-- ============================================================
PRINT 'Creating Owner user: ' + @OwnerEmail;

-- Verify organization was created
IF @NewOrgId IS NULL
BEGIN
    PRINT 'ERROR: Organization was not created! Cannot create owner user.';
    ROLLBACK TRANSACTION;
    RETURN;
END

DECLARE @NewOwnerId INT;

-- Temporarily disable self-referencing constraint if needed
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
BEGIN
    ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_CreatedBy;
END

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
    @OwnerFirstName,
    @OwnerMiddleName,
    @OwnerLastName,
    'Owner',
    @OwnerEmail,
    @OwnerPhoneNumber,
    @OwnerAddress1,
    @OwnerAddress2,
    @OwnerCity,
    @OwnerState,
    @OwnerZipCode,
    @NewOrgId,
    'Org',
    NULL, -- Owner is at Org level, not Branch level
    @PasswordHash,
    @SystemUserId,
    GETDATE(),
    0
);

SET @NewOwnerId = SCOPE_IDENTITY();

-- Re-enable the constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
BEGIN
    ALTER TABLE Users CHECK CONSTRAINT FK_Users_CreatedBy;
END

PRINT 'Owner user created with Id: ' + CAST(@NewOwnerId AS NVARCHAR);
PRINT '';

-- ============================================================
-- 4. UPDATE ORGANIZATION CREATEDBY TO REFERENCE OWNER (Optional)
-- ============================================================
-- Uncomment if you want the Organization to reference the Owner as CreatedBy
/*
UPDATE Organizations 
SET CreatedBy = @NewOwnerId, ModifiedBy = NULL, ModifiedAt = NULL
WHERE Id = @NewOrgId;

PRINT 'Updated Organization.CreatedBy to reference Owner user.';
PRINT '';
*/

-- ============================================================
-- 5. SUMMARY
-- ============================================================
PRINT '========================================';
PRINT 'SUCCESS! Organization and Owner created:';
PRINT '========================================';
PRINT 'Organization ID: ' + CAST(@NewOrgId AS NVARCHAR);
PRINT 'Organization Name: ' + @OrgName;
PRINT '';
PRINT 'Owner User ID: ' + CAST(@NewOwnerId AS NVARCHAR);
PRINT 'Owner Email: ' + @OwnerEmail;
PRINT 'Owner Password: ' + @OwnerPassword;
PRINT '';
PRINT 'IMPORTANT:';
PRINT '1. Change the password after first login';
PRINT '2. Replace the password hash with a proper BCrypt hash for production';
PRINT '3. Update the Organization.CreatedBy if you want it to reference the Owner';
PRINT '========================================';
GO

COMMIT TRANSACTION;
GO

