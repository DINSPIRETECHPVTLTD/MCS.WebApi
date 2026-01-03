-- ============================================================
-- Migration Script: Add Missing Columns and Create New Tables
-- ============================================================
-- This script adds missing columns to existing tables and creates new tables
-- based on the updated schema requirements.
-- ============================================================

USE [YourDatabaseName]; -- Replace with your actual database name
GO

BEGIN TRANSACTION;
GO

-- ============================================================
-- 1. ALTER ORGANIZATIONS TABLE
-- ============================================================
PRINT 'Altering Organizations table...';

-- Add new address columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'Address1')
    ALTER TABLE Organizations ADD Address1 NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'Address2')
    ALTER TABLE Organizations ADD Address2 NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'City')
    ALTER TABLE Organizations ADD City NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'State')
    ALTER TABLE Organizations ADD State NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'ZipCode')
    ALTER TABLE Organizations ADD ZipCode NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'PhoneNumber')
    ALTER TABLE Organizations ADD PhoneNumber NVARCHAR(20) NULL;
GO

-- Rename existing columns if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'Phone')
BEGIN
    EXEC sp_rename 'Organizations.Phone', 'PhoneNumber', 'COLUMN';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'Address')
BEGIN
    -- Migrate Address to Address1
    UPDATE Organizations SET Address1 = Address WHERE Address1 IS NULL AND Address IS NOT NULL;
    ALTER TABLE Organizations DROP COLUMN Address;
END
GO

-- Add audit columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'CreatedBy')
    ALTER TABLE Organizations ADD CreatedBy INT NOT NULL DEFAULT 1; -- Default to system user
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Organizations ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
    -- Migrate existing CreatedDate if it exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'CreatedDate')
    BEGIN
        UPDATE Organizations SET CreatedAt = CreatedDate WHERE CreatedAt IS NULL;
        ALTER TABLE Organizations DROP COLUMN CreatedDate;
    END
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'ModifiedBy')
    ALTER TABLE Organizations ADD ModifiedBy INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'ModifiedAt')
    ALTER TABLE Organizations ADD ModifiedAt DATETIME2 NULL;
GO

-- Rename OrganizationId to Id if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'OrganizationId' AND name <> 'Id')
BEGIN
    EXEC sp_rename 'Organizations.OrganizationId', 'Id', 'COLUMN';
END
GO

-- ============================================================
-- 2. ALTER BRANCHES TABLE
-- ============================================================
PRINT 'Altering Branches table...';

-- Rename OrganizationId to OrgId if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'OrganizationId')
BEGIN
    EXEC sp_rename 'Branches.OrganizationId', 'OrgId', 'COLUMN';
END
GO

-- Add new address columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'Address1')
    ALTER TABLE Branches ADD Address1 NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'Address2')
    ALTER TABLE Branches ADD Address2 NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'City')
    ALTER TABLE Branches ADD City NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'State')
    ALTER TABLE Branches ADD State NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'Country')
    ALTER TABLE Branches ADD Country NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'ZipCode')
    ALTER TABLE Branches ADD ZipCode NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'PhoneNumber')
    ALTER TABLE Branches ADD PhoneNumber NVARCHAR(20) NULL;
GO

-- Rename existing columns if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'Phone')
BEGIN
    EXEC sp_rename 'Branches.Phone', 'PhoneNumber', 'COLUMN';
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'Address')
BEGIN
    -- Migrate Address to Address1
    UPDATE Branches SET Address1 = Address WHERE Address1 IS NULL AND Address IS NOT NULL;
    ALTER TABLE Branches DROP COLUMN Address;
END
GO

-- Add audit columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'CreatedBy')
    ALTER TABLE Branches ADD CreatedBy INT NOT NULL DEFAULT 1; -- Default to system user
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Branches ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
    -- Migrate existing CreatedDate if it exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'CreatedDate')
    BEGIN
        UPDATE Branches SET CreatedAt = CreatedDate WHERE CreatedAt IS NULL;
        ALTER TABLE Branches DROP COLUMN CreatedDate;
    END
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'ModifiedBy')
    ALTER TABLE Branches ADD ModifiedBy INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'ModifiedAt')
    ALTER TABLE Branches ADD ModifiedAt DATETIME2 NULL;
GO

-- Rename BranchId to Id if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'BranchId' AND name <> 'Id')
BEGIN
    EXEC sp_rename 'Branches.BranchId', 'Id', 'COLUMN';
END
GO

-- ============================================================
-- 3. CREATE USERS TABLE (New Unified Table)
-- ============================================================
PRINT 'Creating Users table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY PRIMARY KEY,
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

    -- Create unique index on Email
    CREATE UNIQUE INDEX UQ_Users_Email ON Users(Email) WHERE IsDeleted = 0;

    PRINT 'Users table created successfully.';
END
ELSE
BEGIN
    PRINT 'Users table already exists.';
END
GO

-- ============================================================
-- 4. ALTER CENTERS TABLE
-- ============================================================
PRINT 'Altering Centers table...';

-- Remove Description column if it exists (as per new schema)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'Description')
BEGIN
    ALTER TABLE Centers DROP COLUMN Description;
END
GO

-- Add audit columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'CreatedBy')
    ALTER TABLE Centers ADD CreatedBy INT NOT NULL DEFAULT 1; -- Default to system user
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Centers ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
    -- Migrate existing CreatedDate if it exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'CreatedDate')
    BEGIN
        UPDATE Centers SET CreatedAt = CreatedDate WHERE CreatedAt IS NULL;
        ALTER TABLE Centers DROP COLUMN CreatedDate;
    END
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'ModifiedBy')
    ALTER TABLE Centers ADD ModifiedBy INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'ModifiedAt')
    ALTER TABLE Centers ADD ModifiedAt DATETIME2 NULL;
GO

-- Rename CenterId to Id if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'CenterId' AND name <> 'Id')
BEGIN
    EXEC sp_rename 'Centers.CenterId', 'Id', 'COLUMN';
END
GO

-- ============================================================
-- 5. CREATE POCs TABLE (New Table)
-- ============================================================
PRINT 'Creating POCs table...';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'POCs')
BEGIN
    CREATE TABLE POCs (
        Id INT IDENTITY PRIMARY KEY,
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
-- 6. ALTER MEMBERS TABLE
-- ============================================================
PRINT 'Altering Members table...';

-- Add new address columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'Address1')
    ALTER TABLE Members ADD Address1 NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'Address2')
    ALTER TABLE Members ADD Address2 NVARCHAR(200) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'City')
    ALTER TABLE Members ADD City NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'State')
    ALTER TABLE Members ADD State NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'ZipCode')
    ALTER TABLE Members ADD ZipCode NVARCHAR(20) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'PhoneNumber')
    ALTER TABLE Members ADD PhoneNumber NVARCHAR(20) NOT NULL DEFAULT '';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'AltPhone')
    ALTER TABLE Members ADD AltPhone NVARCHAR(20) NULL;
GO

-- Rename existing columns if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'Phone')
BEGIN
    UPDATE Members SET PhoneNumber = Phone WHERE PhoneNumber IS NULL OR PhoneNumber = '';
    ALTER TABLE Members DROP COLUMN Phone;
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'Address')
BEGIN
    -- Migrate Address to Address1
    UPDATE Members SET Address1 = Address WHERE Address1 IS NULL AND Address IS NOT NULL;
    ALTER TABLE Members DROP COLUMN Address;
END
GO

-- Add guardian fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'GuardianFirstName')
    ALTER TABLE Members ADD GuardianFirstName NVARCHAR(100) NOT NULL DEFAULT '';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'GuardianMiddleName')
    ALTER TABLE Members ADD GuardianMiddleName NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'GuardianLastName')
    ALTER TABLE Members ADD GuardianLastName NVARCHAR(100) NOT NULL DEFAULT '';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'GuardianPhone')
    ALTER TABLE Members ADD GuardianPhone NVARCHAR(20) NOT NULL DEFAULT '';
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'GuardianDOB')
    ALTER TABLE Members ADD GuardianDOB DATE NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'GuardianAge')
    ALTER TABLE Members ADD GuardianAge INT NOT NULL DEFAULT 0;
GO

-- Add POCId
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'POCId')
    ALTER TABLE Members ADD POCId INT NULL; -- Will be set to NOT NULL after data migration
GO

-- Make CenterId required (remove nullable if it exists)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'CenterId' AND is_nullable = 1)
BEGIN
    -- Set default CenterId for existing records (you may need to adjust this logic)
    UPDATE Members SET CenterId = (SELECT TOP 1 Id FROM Centers WHERE BranchId = (SELECT TOP 1 BranchId FROM Branches)) 
    WHERE CenterId IS NULL;
    
    ALTER TABLE Members ALTER COLUMN CenterId INT NOT NULL;
END
GO

-- Remove BranchId if it exists (Members now only relate to Centers)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'BranchId')
BEGIN
    ALTER TABLE Members DROP COLUMN BranchId;
END
GO

-- Remove Occupation if it exists (not in new schema)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'Occupation')
BEGIN
    ALTER TABLE Members DROP COLUMN Occupation;
END
GO

-- Change DOB from DateTime to Date if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'DOB' AND system_type_id = 61) -- 61 = datetime
BEGIN
    ALTER TABLE Members ALTER COLUMN DOB DATE NULL;
END
GO

-- Make Age required
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'Age' AND is_nullable = 1)
BEGIN
    UPDATE Members SET Age = 0 WHERE Age IS NULL;
    ALTER TABLE Members ALTER COLUMN Age INT NOT NULL;
END
GO

-- Add audit columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'CreatedBy')
    ALTER TABLE Members ADD CreatedBy INT NOT NULL DEFAULT 1; -- Default to system user
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE Members ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
    -- Migrate existing CreatedDate if it exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'CreatedDate')
    BEGIN
        UPDATE Members SET CreatedAt = CreatedDate WHERE CreatedAt IS NULL;
        ALTER TABLE Members DROP COLUMN CreatedDate;
    END
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'ModifiedBy')
    ALTER TABLE Members ADD ModifiedBy INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'ModifiedAt')
    ALTER TABLE Members ADD ModifiedAt DATETIME2 NULL;
GO

-- Rename MemberId to Id if needed
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'MemberId' AND name <> 'Id')
BEGIN
    EXEC sp_rename 'Members.MemberId', 'Id', 'COLUMN';
END
GO

-- ============================================================
-- 7. CREATE SYSTEM USER (if needed)
-- ============================================================
PRINT 'Checking for system user...';

-- Create system user if Users table exists but no users exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    IF NOT EXISTS (SELECT * FROM Users WHERE Id = 1)
    BEGIN
        PRINT 'Creating system user (Id=1)...';
        
        -- Get the first organization ID
        DECLARE @SystemOrgId INT;
        SELECT TOP 1 @SystemOrgId = Id FROM Organizations ORDER BY Id;
        
        IF @SystemOrgId IS NULL
        BEGIN
            -- Create a default organization if none exists
            INSERT INTO Organizations (Name, CreatedBy, CreatedAt)
            VALUES ('System Organization', 1, GETDATE());
            SET @SystemOrgId = SCOPE_IDENTITY();
        END
        
        -- Temporarily disable self-referencing constraint if it exists
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
        BEGIN
            ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_CreatedBy;
        END
        
        -- Insert system user with self-reference
        SET IDENTITY_INSERT Users ON;
        
        INSERT INTO Users (Id, FirstName, LastName, Role, Email, OrgId, Level, PasswordHash, CreatedBy, CreatedAt, IsDeleted)
        VALUES (
            1,
            'System',
            'User',
            'Owner',
            'system@mcs.local',
            @SystemOrgId,
            'Org',
            '$2a$11$SystemUserPasswordHashPlaceholder', -- Replace with actual hash
            1,
            GETDATE(),
            0
        );
        
        SET IDENTITY_INSERT Users OFF;
        
        -- Re-enable the constraint
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
        BEGIN
            ALTER TABLE Users CHECK CONSTRAINT FK_Users_CreatedBy;
        END
        
        PRINT 'System user created.';
    END
    ELSE
    BEGIN
        PRINT 'System user already exists.';
    END
END
GO

-- ============================================================
-- 8. ADD FOREIGN KEY CONSTRAINTS
-- ============================================================
PRINT 'Adding foreign key constraints...';

-- Users table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Org')
    ALTER TABLE Users ADD CONSTRAINT FK_Users_Org FOREIGN KEY (OrgId) REFERENCES Organizations(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_Branch')
    ALTER TABLE Users ADD CONSTRAINT FK_Users_Branch FOREIGN KEY (BranchId) REFERENCES Branches(Id);
GO

-- POCs table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_POCs_Center')
    ALTER TABLE POCs ADD CONSTRAINT FK_POCs_Center FOREIGN KEY (CenterId) REFERENCES Centers(Id);
GO

-- Members table foreign keys
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_POC')
    ALTER TABLE Members ADD CONSTRAINT FK_Members_POC FOREIGN KEY (POCId) REFERENCES POCs(Id);
GO

-- Audit foreign keys (self-referencing for Users)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    ALTER TABLE Users ADD CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_ModifiedBy')
    ALTER TABLE Users ADD CONSTRAINT FK_Users_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

-- Audit foreign keys for Organizations
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_CreatedBy')
    ALTER TABLE Organizations ADD CONSTRAINT FK_Org_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Org_ModifiedBy')
    ALTER TABLE Organizations ADD CONSTRAINT FK_Org_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

-- Audit foreign keys for Branches
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Branch_CreatedBy')
    ALTER TABLE Branches ADD CONSTRAINT FK_Branch_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Branch_ModifiedBy')
    ALTER TABLE Branches ADD CONSTRAINT FK_Branch_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

-- Audit foreign keys for Centers
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Centers_CreatedBy')
    ALTER TABLE Centers ADD CONSTRAINT FK_Centers_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Centers_ModifiedBy')
    ALTER TABLE Centers ADD CONSTRAINT FK_Centers_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

-- Audit foreign keys for POCs
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_POCs_CreatedBy')
    ALTER TABLE POCs ADD CONSTRAINT FK_POCs_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_POCs_ModifiedBy')
    ALTER TABLE POCs ADD CONSTRAINT FK_POCs_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

-- Audit foreign keys for Members
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_CreatedBy')
    ALTER TABLE Members ADD CONSTRAINT FK_Members_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Members_ModifiedBy')
    ALTER TABLE Members ADD CONSTRAINT FK_Members_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id);
GO

-- Update Branches foreign key if OrganizationId was renamed
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Branches_Organization')
BEGIN
    ALTER TABLE Branches DROP CONSTRAINT FK_Branches_Organization;
    ALTER TABLE Branches ADD CONSTRAINT FK_Branches_Org FOREIGN KEY (OrgId) REFERENCES Organizations(Id);
END
GO

-- ============================================================
-- 9. DATA MIGRATION (Optional - Migrate from old tables)
-- ============================================================
PRINT 'Starting data migration...';

-- Migrate OrganizationUsers to Users (if OrganizationUsers table exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrganizationUsers')
BEGIN
    PRINT 'Migrating OrganizationUsers to Users...';
    
    INSERT INTO Users (FirstName, MiddleName, LastName, Role, Email, PhoneNumber, Address1, 
                       OrgId, Level, PasswordHash, CreatedBy, CreatedAt, IsDeleted)
    SELECT 
        FirstName,
        MiddleName,
        LastName,
        CASE 
            WHEN Role = 1 THEN 'Owner'
            WHEN Role = 2 THEN 'BranchAdmin'
            ELSE 'Staff'
        END,
        Email,
        Phone,
        Address,
        OrganizationId,
        'Org',
        PasswordHash,
        1, -- Default CreatedBy
        CreatedDate,
        IsDeleted
    FROM OrganizationUsers
    WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Users.Email = OrganizationUsers.Email);
    
    PRINT 'OrganizationUsers migration completed.';
END
GO

-- Migrate BranchUsers to Users (if BranchUsers table exists)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BranchUsers')
BEGIN
    PRINT 'Migrating BranchUsers to Users...';
    
    INSERT INTO Users (FirstName, MiddleName, LastName, Role, Email, PhoneNumber, Address1, 
                       OrgId, Level, BranchId, PasswordHash, CreatedBy, CreatedAt, IsDeleted)
    SELECT 
        bu.FirstName,
        bu.MiddleName,
        bu.LastName,
        CASE 
            WHEN bu.Role = 1 THEN 'BranchAdmin'
            WHEN bu.Role = 2 THEN 'Staff'
            ELSE 'Staff'
        END,
        bu.Email,
        bu.Phone,
        bu.Address,
        b.OrgId,
        'Branch',
        bu.BranchId,
        bu.PasswordHash,
        1, -- Default CreatedBy
        bu.CreatedDate,
        bu.IsDeleted
    FROM BranchUsers bu
    INNER JOIN Branches b ON bu.BranchId = b.Id
    WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Users.Email = bu.Email);
    
    PRINT 'BranchUsers migration completed.';
END
GO

-- Update all CreatedBy/ModifiedBy fields to reference system user (Id=1) if they reference invalid users
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users' AND EXISTS (SELECT * FROM Users WHERE Id = 1))
BEGIN
    PRINT 'Updating audit fields to reference system user...';
    
    -- Update Organizations
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'CreatedBy')
    BEGIN
        UPDATE Organizations SET CreatedBy = 1 WHERE CreatedBy NOT IN (SELECT Id FROM Users) OR CreatedBy IS NULL;
    END
    
    -- Update Branches
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Branches') AND name = 'CreatedBy')
    BEGIN
        UPDATE Branches SET CreatedBy = 1 WHERE CreatedBy NOT IN (SELECT Id FROM Users) OR CreatedBy IS NULL;
    END
    
    -- Update Centers
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Centers') AND name = 'CreatedBy')
    BEGIN
        UPDATE Centers SET CreatedBy = 1 WHERE CreatedBy NOT IN (SELECT Id FROM Users) OR CreatedBy IS NULL;
    END
    
    -- Update Members
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'CreatedBy')
    BEGIN
        UPDATE Members SET CreatedBy = 1 WHERE CreatedBy NOT IN (SELECT Id FROM Users) OR CreatedBy IS NULL;
    END
    
    PRINT 'Audit fields updated.';
END
GO

-- Note: Guardians cannot be directly migrated to POCs as they have different relationships
-- POCs are related to Centers, while Guardians were related to Members
-- You'll need to manually create POCs based on your business logic

-- After creating POCs and assigning them to Members, make POCId NOT NULL
-- Uncomment the following when ready:
/*
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Members') AND name = 'POCId' AND is_nullable = 1)
BEGIN
    -- Ensure all Members have a POCId before making it NOT NULL
    IF NOT EXISTS (SELECT * FROM Members WHERE POCId IS NULL)
    BEGIN
        ALTER TABLE Members ALTER COLUMN POCId INT NOT NULL;
        PRINT 'Members.POCId is now NOT NULL.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Some Members still have NULL POCId. Cannot make it NOT NULL.';
    END
END
*/

-- ============================================================
-- 10. CLEANUP - Drop old tables (Uncomment when ready)
-- ============================================================
/*
PRINT 'Dropping old tables...';

-- Drop old foreign keys first
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_OrganizationUsers_Organization')
    ALTER TABLE OrganizationUsers DROP CONSTRAINT FK_OrganizationUsers_Organization;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BranchUsers_Branch')
    ALTER TABLE BranchUsers DROP CONSTRAINT FK_BranchUsers_Branch;
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Guardians_Member')
    ALTER TABLE Guardians DROP CONSTRAINT FK_Guardians_Member;
GO

-- Drop old tables
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Guardians')
    DROP TABLE Guardians;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'OrganizationUsers')
    DROP TABLE OrganizationUsers;
GO

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BranchUsers')
    DROP TABLE BranchUsers;
GO

PRINT 'Old tables dropped.';
*/

-- ============================================================
-- 11. FINAL STEPS
-- ============================================================
PRINT 'Migration script completed successfully!';
PRINT '';
PRINT 'IMPORTANT NOTES:';
PRINT '1. Review the data migration results above.';
PRINT '2. Manually create POCs from Guardians data if needed.';
PRINT '3. Update Members.POCId to reference appropriate POCs.';
PRINT '4. Once verified, uncomment the cleanup section to drop old tables.';
PRINT '5. Update all CreatedBy/ModifiedBy values to reference valid Users (default is 1 for system user).';
PRINT '6. Make Members.POCId NOT NULL after all POCs are created and assigned.';
GO

COMMIT TRANSACTION;
GO

