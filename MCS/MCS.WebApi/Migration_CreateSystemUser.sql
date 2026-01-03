-- ============================================================
-- Helper Script: Create System User
-- ============================================================
-- This script creates a system user that can be used for audit fields
-- Run this BEFORE running the main migration script if Users table doesn't exist
-- OR run this after creating the Users table but before adding foreign key constraints
-- ============================================================

USE [YourDatabaseName]; -- Replace with your actual database name
GO

BEGIN TRANSACTION;
GO

-- Check if Users table exists, if not create it first
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    PRINT 'Creating Users table first...';
    
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
    
    PRINT 'Users table created.';
END
GO

-- Create a temporary Organizations table if it doesn't exist (for the system user)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Organizations')
BEGIN
    PRINT 'Creating Organizations table for system user...';
    
    CREATE TABLE Organizations (
        Id INT IDENTITY PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Address1 NVARCHAR(200) NULL,
        Address2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        ZipCode NVARCHAR(20) NULL,
        PhoneNumber NVARCHAR(20) NULL,
        CreatedBy INT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    
    -- Insert a system organization
    INSERT INTO Organizations (Name, CreatedBy, CreatedAt)
    VALUES ('System Organization', 1, GETDATE());
    
    PRINT 'System organization created.';
END
GO

-- Check if system user (Id=1) already exists
IF EXISTS (SELECT * FROM Users WHERE Id = 1)
BEGIN
    PRINT 'System user (Id=1) already exists.';
END
ELSE
BEGIN
    PRINT 'Creating system user...';
    
    -- Get the first organization ID
    DECLARE @OrgId INT;
    SELECT TOP 1 @OrgId = Id FROM Organizations ORDER BY Id;
    
    IF @OrgId IS NULL
    BEGIN
        -- Create a default organization if none exists
        INSERT INTO Organizations (Name, CreatedBy, CreatedAt)
        VALUES ('Default Organization', 1, GETDATE());
        SET @OrgId = SCOPE_IDENTITY();
    END
    
    -- Temporarily disable the CreatedBy foreign key constraint if it exists
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
        @OrgId,
        'Org',
        '$2a$11$SystemUserPasswordHashPlaceholder', -- Replace with actual hash if needed
        1, -- Self-reference
        GETDATE(),
        0
    );
    
    SET IDENTITY_INSERT Users OFF;
    
    -- Re-enable the constraint
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    BEGIN
        ALTER TABLE Users CHECK CONSTRAINT FK_Users_CreatedBy;
    END
    
    PRINT 'System user created successfully with Id=1.';
END
GO

-- Update Organizations.CreatedBy to reference the system user
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Organizations') AND name = 'CreatedBy')
BEGIN
    UPDATE Organizations SET CreatedBy = 1 WHERE CreatedBy IS NULL OR CreatedBy NOT IN (SELECT Id FROM Users);
    PRINT 'Updated Organizations.CreatedBy to reference system user.';
END
GO

COMMIT TRANSACTION;
GO

PRINT '';
PRINT 'System user setup completed!';
PRINT 'You can now run the main migration script.';
GO

