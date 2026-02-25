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
DECLARE @OrgName        NVARCHAR(200) = N'Navya Micro Credit Services';
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
IF NOT EXISTS (SELECT 1 FROM dinspire_sa.Users WHERE Id = 1)
BEGIN
    PRINT N'Creating System Organization and System User (Id=1)...';

    -- Disable FKs that create cycle: Org.CreatedBy -> Users, Users.OrgId -> Organizations, Users.CreatedBy -> Users
    DECLARE @sql NVARCHAR(MAX) = N'';

    SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                 + N' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + NCHAR(13)
    FROM sys.foreign_keys AS fk
    WHERE (fk.referenced_object_id = OBJECT_ID(N'dinspire_sa.Organizations') OR fk.referenced_object_id = OBJECT_ID(N'dinspire_sa.Users')
           OR fk.parent_object_id = OBJECT_ID(N'dinspire_sa.Organizations') OR fk.parent_object_id = OBJECT_ID(N'dinspire_sa.Users'));

    IF @sql <> N''
        EXEC sp_executesql @sql;

    -- System Organization (Id=1)
    SET IDENTITY_INSERT dinspire_sa.Organizations ON;
    INSERT INTO dinspire_sa.Organizations (Id, Name, Address1, Address2, City, State, ZipCode, PhoneNumber, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (1, N'System Organization', NULL, NULL, NULL, NULL, NULL, NULL, 1, GETUTCDATE(), NULL, NULL, 0);
    SET IDENTITY_INSERT dinspire_sa.Organizations OFF;

    -- System User (Id=1) – self-referencing CreatedBy=1
    SET IDENTITY_INSERT dinspire_sa.Users ON;
    INSERT INTO dinspire_sa.Users (Id, FirstName, MiddleName, LastName, Role, Email, PhoneNumber, Address1, Address2, City, State, ZipCode, OrgId, Level, BranchId, PasswordHash, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
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
    SET IDENTITY_INSERT dinspire_sa.Users OFF;

    -- Re-enable the same FKs
    SET @sql = N'';
    SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id))
                 + N' CHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + NCHAR(13)
    FROM sys.foreign_keys AS fk
    WHERE (fk.referenced_object_id = OBJECT_ID(N'dinspire_sa.Organizations') OR fk.referenced_object_id = OBJECT_ID(N'dinspire_sa.Users')
           OR fk.parent_object_id = OBJECT_ID(N'dinspire_sa.Organizations') OR fk.parent_object_id = OBJECT_ID(N'dinspire_sa.Users'));

    IF @sql <> N''
        EXEC sp_executesql @sql;

    PRINT N'System Organization and System User created.';
END
ELSE
    PRINT N'System User (Id=1) already exists.';

-- Default Organization and Owner user (skip if email already exists)
IF NOT EXISTS (SELECT 1 FROM dinspire_sa.Users WHERE Email = @OwnerEmail AND IsDeleted = 0)
BEGIN
    PRINT N'Creating default Organization and Owner user...';

    DECLARE @PasswordHash NVARCHAR(MAX) = CONVERT(NVARCHAR(MAX), HASHBYTES(N'SHA2_256', @OwnerPassword), 2);
    -- For production, replace with a BCrypt hash (e.g. from https://bcrypt-generator.com/)

    INSERT INTO dinspire_sa.Organizations (Name, Address1, Address2, City, State, ZipCode, PhoneNumber, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (@OrgName, @OrgAddress1, NULL, @OrgCity, @OrgState, @OrgZipCode, @OrgPhone, 1, GETUTCDATE(), NULL, NULL, 0);

    DECLARE @NewOrgId INT = SCOPE_IDENTITY();

    INSERT INTO dinspire_sa.Users (FirstName, MiddleName, LastName, Role, Email, PhoneNumber, Address1, Address2, City, State, ZipCode, OrgId, Level, BranchId, PasswordHash, CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, IsDeleted)
    VALUES (@OwnerFirstName, NULL, @OwnerLastName, N'Owner', @OwnerEmail, @OwnerPhone, NULL, NULL, NULL, NULL, NULL, @NewOrgId, N'Org', NULL, @PasswordHash, 1, GETUTCDATE(), NULL, NULL, 0);

    PRINT N'Organization Id: ' + CAST(@NewOrgId AS NVARCHAR(20)) + N', Owner: ' + @OwnerEmail;
END
ELSE
    PRINT N'Owner with email ' + @OwnerEmail + N' already exists.';

-- ============================================================
-- MasterLookups - Seed all Indian States (and UTs) with CreatedBy = first user (Id = 1)
-- ============================================================

PRINT N'Seeding MasterLookups for Indian states...';

DECLARE @NowUtc DATETIME2 = GETUTCDATE();

-- Helper: insert one state row if it doesn't already exist
IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'AP')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'AP', N'Andhra Pradesh',       NULL, 1, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'AR')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'AR', N'Arunachal Pradesh',    NULL, 2, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'AS')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'AS', N'Assam',               NULL, 3, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'BR')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'BR', N'Bihar',               NULL, 4, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'CG')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'CG', N'Chhattisgarh',        NULL, 5, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'GA')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'GA', N'Goa',                 NULL, 6, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'GJ')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'GJ', N'Gujarat',             NULL, 7, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'HR')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'HR', N'Haryana',             NULL, 8, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'HP')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'HP', N'Himachal Pradesh',    NULL, 9, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'JH')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'JH', N'Jharkhand',           NULL, 10, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'KA')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'KA', N'Karnataka',           NULL, 11, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'KL')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'KL', N'Kerala',              NULL, 12, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'MP')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'MP', N'Madhya Pradesh',      NULL, 13, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'MH')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'MH', N'Maharashtra',         NULL, 14, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'MN')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'MN', N'Manipur',             NULL, 15, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'ML')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'ML', N'Meghalaya',           NULL, 16, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'MZ')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'MZ', N'Mizoram',             NULL, 17, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'NL')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'NL', N'Nagaland',            NULL, 18, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'OD')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'OD', N'Odisha',              NULL, 19, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'PB')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'PB', N'Punjab',              NULL, 20, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'RJ')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'RJ', N'Rajasthan',           NULL, 21, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'SK')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'SK', N'Sikkim',              NULL, 22, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'TN')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'TN', N'Tamil Nadu',          NULL, 23, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'TG')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'TG', N'Telangana',           NULL, 24, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'TR')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'TR', N'Tripura',             NULL, 25, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'UP')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'UP', N'Uttar Pradesh',       NULL, 26, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'UT')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'UT', N'Uttarakhand',         NULL, 27, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'WB')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'WB', N'West Bengal',         NULL, 28, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

-- Union Territories

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'AN')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'AN', N'Andaman and Nicobar Islands', NULL, 29, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'CH')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'CH', N'Chandigarh',          NULL, 30, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'DN')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'DN', N'Dadra and Nagar Haveli and Daman and Diu', NULL, 31, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'DL')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'DL', N'Delhi',              NULL, 32, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'JK')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'JK', N'Jammu and Kashmir',  NULL, 33, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'LA')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'LA', N'Ladakh',             NULL, 34, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'PY')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'PY', N'Puducherry',         NULL, 35, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

IF NOT EXISTS (SELECT 1 FROM dinspire_sa.MasterLookups WHERE LookupKey = 'STATE' AND LookupCode = 'LD')
BEGIN
    INSERT INTO dinspire_sa.MasterLookups (LookupKey, LookupCode, LookupValue, NumericValue, SortOrder, IsActive, Description, CreatedOn, CreatedBy, UpdatedOn, UpdatedBy)
    VALUES ('STATE', 'LD', N'Lakshadweep',        NULL, 36, 1, NULL, @NowUtc, '1', NULL, NULL);
END;

PRINT N'Seeding MasterLookups for Indian states completed.';

PRINT N'';
PRINT N'Done. For production, set Users.PasswordHash to a BCrypt hash for ' + @OwnerEmail + N'.';

COMMIT TRANSACTION;
GO
