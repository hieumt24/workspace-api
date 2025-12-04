-- Migration: Add CustomerId to CustomerChatSession
-- Date: 2025-12-03

-- Check if column already exists before adding
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('CustomerChatSessions') 
    AND name = 'CustomerId'
)
BEGIN
    -- Add CustomerId column
    ALTER TABLE CustomerChatSessions 
    ADD CustomerId int NOT NULL DEFAULT 0;

    PRINT 'CustomerId column added successfully';
END
ELSE
BEGIN
    PRINT 'CustomerId column already exists';
END
GO

-- Check if index already exists before creating
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_CustomerChatSessions_CustomerId' 
    AND object_id = OBJECT_ID('CustomerChatSessions')
)
BEGIN
    -- Create index for CustomerId
    CREATE INDEX IX_CustomerChatSessions_CustomerId 
    ON CustomerChatSessions (CustomerId);

    PRINT 'Index IX_CustomerChatSessions_CustomerId created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_CustomerChatSessions_CustomerId already exists';
END
GO

-- Check if foreign key already exists before adding
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_CustomerChatSessions_Users_CustomerId'
)
BEGIN
    -- Add foreign key constraint
    ALTER TABLE CustomerChatSessions 
    ADD CONSTRAINT FK_CustomerChatSessions_Users_CustomerId 
    FOREIGN KEY (CustomerId) REFERENCES Users(Id);

    PRINT 'Foreign key FK_CustomerChatSessions_Users_CustomerId added successfully';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_CustomerChatSessions_Users_CustomerId already exists';
END
GO

-- Insert migration history record
IF NOT EXISTS (
    SELECT 1 FROM __EFMigrationsHistory 
    WHERE MigrationId = '20251203180000_AddCustomerIdToCustomerChatSession'
)
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251203180000_AddCustomerIdToCustomerChatSession', '8.0.20');

    PRINT 'Migration record added to __EFMigrationsHistory';
END
ELSE
BEGIN
    PRINT 'Migration record already exists in __EFMigrationsHistory';
END
GO

PRINT 'Migration completed successfully!';

