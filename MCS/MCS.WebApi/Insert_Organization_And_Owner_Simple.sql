-- ============================================================
-- Simple Script: Insert Organization and Owner User
-- ============================================================
-- Quick setup script - Update the values below and run
-- ============================================================

USE [YourDatabaseName]; -- ⚠️ CHANGE THIS to your database name
GO

BEGIN TRANSACTION;
GO

-- ============================================================
-- ⚠️ UPDATE THESE VALUES ⚠️
-- ============================================================
DECLARE @OrgName NVARCHAR(200) = 'My Organization';
DECLARE @OwnerEmail NVARCHAR(200) = 'owner@myorganization.com';
DECLARE @OwnerPassword NVARCHAR(MAX) = 'TempPassword123!';
DECLARE @OwnerFirstName NVARCHAR(100) = 'John';
DECLARE @OwnerLastName NVARCHAR(100) = 'Doe';
-- ============================================================

-- System user must exist (created by migration script)
DECLARE @SystemUserId INT = 1;

-- Check if system user exists
IF NOT EXISTS (SELECT * FROM Users WHERE Id = @SystemUserId)
BEGIN
    RAISERROR('System user (Id=1) does not exist! Run Migration_CreateSystemUser.sql first.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Check if email already exists
IF EXISTS (SELECT * FROM Users WHERE Email = @OwnerEmail AND IsDeleted = 0)
BEGIN
    RAISERROR('User with email %s already exists!', 16, 1, @OwnerEmail);
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Insert Organization
DECLARE @NewOrgId INT;

INSERT INTO Organizations (Name, CreatedBy, CreatedAt, IsDeleted)
VALUES (@OrgName, @SystemUserId, GETDATE(), 0);

SET @NewOrgId = SCOPE_IDENTITY();

PRINT 'Organization created: ' + CAST(@NewOrgId AS NVARCHAR) + ' - ' + @OrgName;

-- Generate password hash (SHA2_256 - replace with BCrypt hash for production)
-- To get BCrypt hash, use: https://bcrypt-generator.com/ or .NET BCrypt.Net.BCrypt.HashPassword()
DECLARE @PasswordHash NVARCHAR(MAX) = CONVERT(NVARCHAR(MAX), HASHBYTES('SHA2_256', @OwnerPassword), 2);

-- Insert Owner User
DECLARE @NewOwnerId INT;

-- Temporarily disable self-referencing constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    ALTER TABLE Users NOCHECK CONSTRAINT FK_Users_CreatedBy;

INSERT INTO Users (
    FirstName, LastName, Role, Email, OrgId, Level, 
    PasswordHash, CreatedBy, CreatedAt, IsDeleted
)
VALUES (
    @OwnerFirstName, 
    @OwnerLastName, 
    'Owner', 
    @OwnerEmail, 
    @NewOrgId, 
    'Org',
    @PasswordHash,
    @SystemUserId,
    GETDATE(),
    0
);

SET @NewOwnerId = SCOPE_IDENTITY();

-- Re-enable constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Users_CreatedBy')
    ALTER TABLE Users CHECK CONSTRAINT FK_Users_CreatedBy;

PRINT 'Owner user created: ' + CAST(@NewOwnerId AS NVARCHAR) + ' - ' + @OwnerEmail;
PRINT '';
PRINT '========================================';
PRINT 'SUCCESS!';
PRINT 'Organization ID: ' + CAST(@NewOrgId AS NVARCHAR);
PRINT 'Owner User ID: ' + CAST(@NewOwnerId AS NVARCHAR);
PRINT 'Email: ' + @OwnerEmail;
PRINT 'Password: ' + @OwnerPassword;
PRINT '========================================';
PRINT '';
PRINT '⚠️ IMPORTANT:';
PRINT '1. Change password after first login';
PRINT '2. Replace password hash with BCrypt hash for production';
PRINT '   Use: https://bcrypt-generator.com/';
PRINT '   Or: BCrypt.Net.BCrypt.HashPassword("' + @OwnerPassword + '")';
PRINT '========================================';

COMMIT TRANSACTION;
GO

