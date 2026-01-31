-- =============================================
-- Script: Add Loan Management Tables and Update Member Table
-- Description: 
--   1. Adds JoiningFee column to Member table
--   2. Creates Loan table
--   3. Creates LoanPayment table
-- Created: 2026-01-31
-- =============================================

-- =============================================
-- 1. ALTER MEMBER TABLE - Add JoiningFee Column
-- =============================================
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Members' 
    AND COLUMN_NAME = 'JoiningFee'
)
BEGIN
    ALTER TABLE Members
    ADD JoiningFee DECIMAL(18, 2) NULL;
    
    PRINT 'Column JoiningFee added to Member table successfully.';
END
ELSE
BEGIN
    PRINT 'Column JoiningFee already exists in Member table.';
END
GO

-- =============================================
-- 2. CREATE LOAN TABLE
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Loans')
BEGIN
    CREATE TABLE Loans (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LoanCode NVARCHAR(50) NOT NULL UNIQUE,
        MemberId INT NOT NULL,
        LoanAmount DECIMAL(18, 2) NOT NULL,
        InterestAmount DECIMAL(18, 2) NOT NULL,
        ProcessingFee DECIMAL(18, 2) NULL,
        InsuranceFee DECIMAL(18, 2) NULL,
        IsSavingEnabled BIT NOT NULL DEFAULT 0,
        SavingAmount DECIMAL(18, 2) NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        
        -- Foreign Key Constraints
        CONSTRAINT FK_Loans_Members FOREIGN KEY (MemberId) 
            REFERENCES Members(Id)
            ON DELETE CASCADE,
        CONSTRAINT FK_Loan_CreatedBy FOREIGN KEY (CreatedBy) 
            REFERENCES [Users](Id)
            ON DELETE NO ACTION,
        CONSTRAINT FK_Loan_ModifiedBy FOREIGN KEY (ModifiedBy) 
            REFERENCES [Users](Id)
            ON DELETE NO ACTION
    );
    
    -- Create Index on MemberId for better query performance
    CREATE NONCLUSTERED INDEX IX_Loans_MemberId 
        ON Loans(MemberId);
    
    -- Create Index on LoanCode for lookups
    CREATE NONCLUSTERED INDEX IX_Loans_LoanCode 
        ON Loans(LoanCode);
    
    PRINT 'Loans table created successfully.';
END
ELSE
BEGIN
    PRINT 'Loan table already exists.';
END
GO

-- =============================================
-- 3. CREATE LOANPAYMENT TABLE
-- =============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LoanPayments')
BEGIN
    CREATE TABLE LoanPayments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LoanId INT NOT NULL,
        SavingAmount DECIMAL(18, 2) NULL,
        PrincipalAmount DECIMAL(18, 2) NOT NULL,
        InterestAmount DECIMAL(18, 2) NOT NULL,
        PenaltyAmount DECIMAL(18, 2) NULL DEFAULT 0,
        ActualPaymentDate DATETIME NULL,
        PaymentDate DATETIME NOT NULL,
        InstallmentNo INT NOT NULL,
        Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Paid', 'Partial', 'Not Paid')),
        PaymentMode NVARCHAR(50) NULL CHECK (PaymentMode IN ('Cash', 'Branch Bank Account', 'UPI', 'Other')),
        ReceivedBy NVARCHAR(100) NULL,
        Comments NVARCHAR(500) NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy INT NULL,
        ModifiedAt DATETIME NULL,
        
        -- Foreign Key Constraints
        CONSTRAINT FK_LoanPayments_Loans FOREIGN KEY (LoanId) 
            REFERENCES Loans(Id)
            ON DELETE CASCADE,
        CONSTRAINT FK_LoanPayment_CreatedBy FOREIGN KEY (CreatedBy) 
            REFERENCES [Users](Id)
            ON DELETE NO ACTION,
        CONSTRAINT FK_LoanPayment_ModifiedBy FOREIGN KEY (ModifiedBy) 
            REFERENCES [Users](Id)
            ON DELETE NO ACTION
    );
    
    -- Create Index on LoanId for better query performance
    CREATE NONCLUSTERED INDEX IX_LoanPayment_LoanId 
        ON LoanPayments(LoanId);
    
    -- Create Index on PaymentDate for date-based queries
    CREATE NONCLUSTERED INDEX IX_LoanPayment_PaymentDate 
        ON LoanPayments(PaymentDate);
    
    -- Create Index on Status for filtering
    CREATE NONCLUSTERED INDEX IX_LoanPayment_Status 
        ON LoanPayments(Status);
    
    -- Create Index on InstallmentNo for sorting
    CREATE NONCLUSTERED INDEX IX_LoanPayment_InstallmentNo 
        ON LoanPayments(LoanId, InstallmentNo);
    
    PRINT 'LoanPayment table created successfully.';
END
ELSE
BEGIN
    PRINT 'LoanPayment table already exists.';
END
GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================
PRINT '==============================================';
PRINT 'VERIFICATION: Checking created objects...';
PRINT '==============================================';

-- Check Member table JoiningFee column
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Members' 
    AND COLUMN_NAME = 'JoiningFee'
)
    PRINT '✓ Member.JoiningFee column exists';
ELSE
    PRINT '✗ Member.JoiningFee column NOT found';

-- Check Loan table
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Loans')
    PRINT '✓ Loan table exists';
ELSE
    PRINT '✗ Loan table NOT found';

-- Check LoanPayment table
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LoanPayments')
    PRINT '✓ LoanPayment table exists';
ELSE
    PRINT '✗ LoanPayment table NOT found';

PRINT '==============================================';
PRINT 'Script execution completed.';
PRINT '==============================================';
GO
