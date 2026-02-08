-- =============================================
-- Script: Add IsDeleted column to LoanPayments table
-- Description: Adds IsDeleted BIT column with default value of 0
-- Created: 2026-02-07
-- =============================================

-- Add IsDeleted column to LoanPayments table
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dinspire_mfdev'
    AND TABLE_NAME = 'LoanPayments' 
    AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE dinspire_mfdev.LoanPayments
    ADD IsDeleted BIT NOT NULL DEFAULT 0;
    
    PRINT 'Column IsDeleted added to LoanPayments table successfully.';
END
ELSE
BEGIN
    PRINT 'Column IsDeleted already exists in LoanPayments table.';
END
GO

-- Verification
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dinspire_mfdev'
    AND TABLE_NAME = 'LoanPayments' 
    AND COLUMN_NAME = 'IsDeleted'
)
    PRINT '✓ LoanPayments.IsDeleted column exists';
ELSE
    PRINT '✗ LoanPayments.IsDeleted column NOT found';
GO
