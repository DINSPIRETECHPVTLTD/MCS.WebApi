-- Add missing columns to Loans table (Simplified version)
-- Run this against the dinspire_mf_dev database

-- Add TotalAmount if it doesn't exist
IF COL_LENGTH('dinspire_mfdev.Loans', 'TotalAmount') IS NULL
BEGIN
    ALTER TABLE dinspire_mfdev.Loans ADD TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added TotalAmount column';
END
ELSE
BEGIN
    PRINT 'TotalAmount column already exists';
END

-- Add OutstandingAmount if it doesn't exist
IF COL_LENGTH('dinspire_mfdev.Loans', 'OutstandingAmount') IS NULL
BEGIN
    ALTER TABLE dinspire_mfdev.Loans ADD OutstandingAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added OutstandingAmount column';
END
ELSE
BEGIN
    PRINT 'OutstandingAmount column already exists';
END

-- Add Status if it doesn't exist
IF COL_LENGTH('dinspire_mfdev.Loans', 'Status') IS NULL
BEGIN
    ALTER TABLE dinspire_mfdev.Loans ADD Status NVARCHAR(20) NOT NULL DEFAULT 'Active';
    PRINT 'Added Status column';
END
ELSE
BEGIN
    PRINT 'Status column already exists';
END

-- Add DisbursementDate if it doesn't exist
IF COL_LENGTH('dinspire_mfdev.Loans', 'DisbursementDate') IS NULL
BEGIN
    ALTER TABLE dinspire_mfdev.Loans ADD DisbursementDate DATETIME2 NULL;
    PRINT 'Added DisbursementDate column';
END
ELSE
BEGIN
    PRINT 'DisbursementDate column already exists';
END

-- Add ClosureDate if it doesn't exist
IF COL_LENGTH('dinspire_mfdev.Loans', 'ClosureDate') IS NULL
BEGIN
    ALTER TABLE dinspire_mfdev.Loans ADD ClosureDate DATETIME2 NULL;
    PRINT 'Added ClosureDate column';
END
ELSE
BEGIN
    PRINT 'ClosureDate column already exists';
END

-- Add IsDeleted if it doesn't exist
IF COL_LENGTH('dinspire_mfdev.Loans', 'IsDeleted') IS NULL
BEGIN
    ALTER TABLE dinspire_mfdev.Loans ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists';
END

PRINT 'Migration completed successfully!';
