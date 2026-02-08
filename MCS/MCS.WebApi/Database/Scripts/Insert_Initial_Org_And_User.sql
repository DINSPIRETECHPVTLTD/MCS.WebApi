-- ============================================================
-- Insert Initial Organization and User
-- ============================================================
-- Run after Code First migrations have created the tables.
-- Creates: System Organization (Id=1), System User (Id=1),
--          optional default Organization and Owner user.
-- ============================================================

-- **** CHANGE THIS TO YOUR DATABASE NAME ****
USE dinspire_mcs_dev;
GO

SET NOCOUNT ON;

BEGIN TRANSACTION;
GO

-- ============================================================
-- CONFIGURATION – adjust as needed
-- ============================================================
DECLARE @OrgName        NVARCHAR(200) = N'MCS Demo Organization';
DECLARE @OrgAddress1    NVARCHAR(200) = NULL;
DECLARE @OrgCity        NVARCHAR(100) = NULL;
DECLARE @OrgState       NVARCHAR(100) = NULL;
DECLARE @OrgZipCode     NVARCHAR(20)  = NULL;
DECLARE @OrgPhone       NVARCHAR(20)  = NULL;

DECLARE @OwnerFirstName  NVARCHAR(100) = N'Admin';
DECLARE @OwnerLastName   NVARCHAR(100) = N'User';
DECLARE @OwnerEmail      NVARCHAR(200) = N'admin@demo.com';
DECLARE @OwnerPassword   NVARCHAR(MAX) = N'Admin123!';  -- change after first login
DECLARE @OwnerPhone     NVARCHAR(20)  = NULL;
-- ============================================================

-- Check if System User (Id=1) already exists
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = 1)
BEGIN
    PRINT N'Creating System Organization and System User (Id=1)...';

    -- Disable FKs that create cycle: Org.CreatedBy -> Users, Users.OrgId -> Organizations, Users.CreatedBy -> Users
    DECLARE @sql NVARCHAR(MAX) = N'';

    SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                 + N' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + NCHAR(13)
    FROM sys.foreign_keys AS fk
    WHERE (fk.referenced_object_id = OBJECT_ID(N'dbo.Organizations') OR fk.referenced_object_id = OBJECT_ID(N'dbo.Users')
           OR fk.parent_object_id = OBJECT_ID(N'dbo.Organizations') OR fk.parent_object_id = OBJECT_ID(N'dbo.Users'));

    IF @sql <> N''
        EXEC sp_executesql @sql;

    -- System Organization (Id=1)
    SET IDENTITY_INSERT dbo.Organizations ON;
    INSERT INTO dbo.Organizations (Id, Name, Address1, Address2, City, State, ZipCode, PhoneNumber, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (1, N'System Organization', NULL, NULL, NULL, NULL, NULL, NULL, 1, GETUTCDATE(), NULL, NULL, 0);
    SET IDENTITY_INSERT dbo.Organizations OFF;

    -- System User (Id=1) – self-referencing CreatedBy=1
    SET IDENTITY_INSERT dbo.Users ON;
    INSERT INTO dbo.Users (Id, FirstName, MiddleName, LastName, Role, Email, PhoneNumber, Address1, Address2, City, State, ZipCode, OrgId, Level, BranchId, PasswordHash, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (
        1,
        N'System',
        NULL,
        N'User',
        N'Owner',
        N'system@mcs.local',
        NULL,
        NULL, NULL, NULL, NULL, NULL,
        1,   -- OrgId
        N'Org',
        NULL,
        CONVERT(NVARCHAR(MAX), HASHBYTES(N'SHA2_256', N'SystemPassword'), 2),  -- placeholder hash
        1,   -- CreatedBy (self)
        GETUTCDATE(),
        NULL, NULL,
        0
    );
    SET IDENTITY_INSERT dbo.Users OFF;

    -- Re-enable the same FKs
    SET @sql = N'';
    SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                 + N' CHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + NCHAR(13)
    FROM sys.foreign_keys AS fk
    WHERE (fk.referenced_object_id = OBJECT_ID(N'dbo.Organizations') OR fk.referenced_object_id = OBJECT_ID(N'dbo.Users')
           OR fk.parent_object_id = OBJECT_ID(N'dbo.Organizations') OR fk.parent_object_id = OBJECT_ID(N'dbo.Users'));

    IF @sql <> N''
        EXEC sp_executesql @sql;

    PRINT N'System Organization and System User created.';
END
ELSE
    PRINT N'System User (Id=1) already exists.';

-- Default Organization and Owner user (skip if email already exists)
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @OwnerEmail AND IsDeleted = 0)
BEGIN
    PRINT N'Creating default Organization and Owner user...';

    DECLARE @PasswordHash NVARCHAR(MAX) = CONVERT(NVARCHAR(MAX), HASHBYTES(N'SHA2_256', @OwnerPassword), 2);
    -- For production, replace with a BCrypt hash (e.g. from https://bcrypt-generator.com/)

    INSERT INTO dbo.Organizations (Name, Address1, Address2, City, State, ZipCode, PhoneNumber, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (@OrgName, @OrgAddress1, NULL, @OrgCity, @OrgState, @OrgZipCode, @OrgPhone, 1, GETUTCDATE(), NULL, NULL, 0);

    DECLARE @NewOrgId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Users (FirstName, MiddleName, LastName, Role, Email, PhoneNumber, Address1, Address2, City, State, ZipCode, OrgId, Level, BranchId, PasswordHash, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (@OwnerFirstName, NULL, @OwnerLastName, N'Owner', @OwnerEmail, @OwnerPhone, NULL, NULL, NULL, NULL, NULL, @NewOrgId, N'Org', NULL, @PasswordHash, 1, GETUTCDATE(), NULL, NULL, 0);

    PRINT N'Organization Id: ' + CAST(@NewOrgId AS NVARCHAR(20)) + N', Owner: ' + @OwnerEmail;
END
ELSE
    PRINT N'Owner with email ' + @OwnerEmail + N' already exists.';

PRINT N'';
PRINT N'Done. For production, set Users.PasswordHash to a BCrypt hash for ' + @OwnerEmail + N'.';

COMMIT TRANSACTION;
GO
