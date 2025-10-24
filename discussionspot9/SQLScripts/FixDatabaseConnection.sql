-- ============================================
-- SQL Server Connection Fix Script
-- ============================================
-- Problem: Error 17892 - Logon failed for login 'sa' due to trigger execution
-- Solution: Check and fix login triggers
-- ============================================

USE master;
GO

-- ============================================
-- STEP 1: Check Login Triggers
-- ============================================
-- View all server-level login triggers
SELECT 
    name AS TriggerName,
    is_disabled AS IsDisabled,
    create_date AS CreateDate,
    modify_date AS ModifyDate
FROM sys.server_triggers
WHERE type = 'TR'  -- TR = Trigger
ORDER BY name;

-- ============================================
-- STEP 2: View Trigger Definition
-- ============================================
-- Check what the trigger does (replace 'TriggerName' with actual name)
SELECT OBJECT_DEFINITION(OBJECT_ID('YourTriggerNameHere')) AS TriggerDefinition;

-- OR view all trigger definitions
SELECT 
    name AS TriggerName,
    OBJECT_DEFINITION(OBJECT_ID(name)) AS TriggerDefinition
FROM sys.server_triggers
WHERE type = 'TR';

-- ============================================
-- STEP 3: Fix Option A - Disable Trigger Temporarily
-- ============================================
-- WARNING: Only do this if you understand the security implications
-- Replace 'YourTriggerNameHere' with actual trigger name from Step 1
/*
DISABLE TRIGGER YourTriggerNameHere ON ALL SERVER;
GO
*/

-- ============================================
-- STEP 4: Fix Option B - Enable SA Account
-- ============================================
-- Check if SA account is enabled
SELECT 
    name AS LoginName,
    is_disabled AS IsDisabled,
    create_date AS CreateDate
FROM sys.sql_logins
WHERE name = 'sa';

-- Enable SA account if disabled
ALTER LOGIN sa ENABLE;
GO

-- Reset SA password (optional)
-- ALTER LOGIN sa WITH PASSWORD = '1nsp1r0N@321';
-- GO

-- ============================================
-- STEP 5: Fix Option C - Create Application User
-- ============================================
-- Create a dedicated application user (RECOMMENDED)

-- Create login
CREATE LOGIN ds_app WITH PASSWORD = '1nsp1r0N@321';
GO

-- Grant necessary permissions
USE DiscussionspotADO;
GO

-- Create user and map to login
CREATE USER ds_app FOR LOGIN ds_app;
GO

-- Grant database roles
ALTER ROLE db_datareader ADD MEMBER ds_app;
ALTER ROLE db_datawriter ADD MEMBER ds_app;
ALTER ROLE db_ddladmin ADD MEMBER ds_app;
ALTER ROLE db_securityadmin ADD MEMBER ds_app;

-- Grant execute permission on stored procedures
GRANT EXECUTE TO ds_app;
GO

-- ============================================
-- STEP 6: Alternative - Grant SA Permissions
-- ============================================
-- If SA account should have full access
USE DiscussionspotADO;
GO

-- Grant db_owner role to SA
ALTER ROLE db_owner ADD MEMBER sa;
GO

-- ============================================
-- STEP 7: Check Connection
-- ============================================
-- Test connection with the new user
-- Connect using: 
-- Server: 167.88.42.56
-- Database: DiscussionspotADO
-- User: ds_app
-- Password: 1nsp1r0N@321

-- ============================================
-- Troubleshooting Commands
-- ============================================

-- Check all logins
SELECT name, type_desc, is_disabled, create_date
FROM sys.server_principals
WHERE type IN ('S', 'U', 'G')  -- SQL login, Windows login, Windows group
ORDER BY name;

-- Check failed login attempts
SELECT * FROM sys.event_log
WHERE event_type IN ('connection_ring_buffer_recorded', 'errorlog_message')
ORDER BY event_time DESC;

-- Check login trigger status
SELECT * FROM sys.server_triggers WHERE type = 'TR';


