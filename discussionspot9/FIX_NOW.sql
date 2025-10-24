-- ============================================
-- QUICK FIX: Run these commands in SSMS
-- ============================================
-- Connect to: 167.88.42.56
-- Use Windows Authentication or admin account
-- ============================================

USE master;
GO

-- STEP 1: Check if SA account is disabled
SELECT 
    name AS LoginName,
    is_disabled AS IsDisabled,
    create_date AS CreateDate
FROM sys.sql_logins
WHERE name = 'sa';
GO

-- STEP 2: Enable SA account
ALTER LOGIN sa ENABLE;
GO

-- STEP 3: Check for login triggers that might be blocking
SELECT 
    name AS TriggerName,
    is_disabled AS IsDisabled,
    create_date AS CreateDate,
    modify_date AS ModifyDate
FROM sys.server_triggers
WHERE type = 'TR'  -- TR = Trigger
ORDER BY name;
GO

-- STEP 4: View trigger definition (replace TriggerName with actual name from Step 3)
-- SELECT OBJECT_DEFINITION(OBJECT_ID('YourTriggerNameHere')) AS TriggerDefinition;
-- GO

-- STEP 5: If you find a problematic trigger, disable it (replace TriggerName)
-- DISABLE TRIGGER YourTriggerNameHere ON ALL SERVER;
-- GO

-- STEP 6: Test SA connection
-- Try connecting with:
-- Server: 167.88.42.56
-- Database: DiscussionspotADO
-- User: sa
-- Password: 1nsp1r0N@321

-- STEP 7: Grant necessary permissions
USE DiscussionspotADO;
GO

-- Ensure SA has proper permissions
ALTER ROLE db_owner ADD MEMBER sa;
GO

-- Verify SA can access the database
SELECT 
    DB_NAME() AS CurrentDatabase,
    USER_NAME() AS CurrentUser,
    SUSER_SNAME() AS LoginName;
GO

-- ============================================
-- ALTERNATIVE: Create application user
-- ============================================
-- If SA still doesn't work, create a dedicated user:

-- USE master;
-- GO
-- CREATE LOGIN ds_app WITH PASSWORD = '1nsp1r0N@321';
-- GO
-- USE DiscussionspotADO;
-- GO
-- CREATE USER ds_app FOR LOGIN ds_app;
-- GO
-- ALTER ROLE db_datareader ADD MEMBER ds_app;
-- ALTER ROLE db_datawriter ADD MEMBER ds_app;
-- ALTER ROLE db_ddladmin ADD MEMBER ds_app;
-- GO

-- Then update connection string to use ds_app instead of sa


