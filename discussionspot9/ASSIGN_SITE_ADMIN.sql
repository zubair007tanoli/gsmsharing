-- =============================================
-- Script: Assign SiteAdmin Role to User
-- Description: Assigns the SiteAdmin role to a user
-- Usage: Replace @UserEmail with the email of the user you want to make admin
-- =============================================

DECLARE @UserEmail NVARCHAR(256) = 'your-email@example.com'; -- CHANGE THIS to your email
DECLARE @UserId NVARCHAR(450);
DECLARE @AdminUserId NVARCHAR(450);

-- Get the UserId from the email
SELECT @UserId = Id FROM AspNetUsers WHERE Email = @UserEmail OR UserName = @UserEmail;

IF @UserId IS NULL
BEGIN
    PRINT 'Error: User not found with email: ' + @UserEmail;
    PRINT 'Available users:';
    SELECT TOP 10 Id, UserName, Email FROM AspNetUsers;
END
ELSE
BEGIN
    -- Use the same user as both the assignee and assigner for first admin
    SET @AdminUserId = @UserId;
    
    -- Check if user already has SiteAdmin role
    IF EXISTS (SELECT 1 FROM SiteRoles WHERE UserId = @UserId AND RoleName = 'SiteAdmin' AND IsActive = 1)
    BEGIN
        PRINT 'User already has SiteAdmin role.';
    END
    ELSE
    BEGIN
        -- Assign SiteAdmin role
        INSERT INTO SiteRoles (UserId, RoleName, AssignedAt, AssignedByUserId, IsActive, Notes)
        VALUES (@UserId, 'SiteAdmin', GETDATE(), @AdminUserId, 1, 'Initial admin assignment');
        
        PRINT 'SiteAdmin role assigned successfully!';
        PRINT 'User ID: ' + @UserId;
        PRINT 'Email: ' + @UserEmail;
    END
    
    -- Also add to AspNetRoles if not exists
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SiteAdmin')
    BEGIN
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (NEWID(), 'SiteAdmin', 'SITEADMIN', NEWID());
        PRINT 'SiteAdmin role created in AspNetRoles.';
    END
    
    -- Add user to AspNetUserRoles
    DECLARE @RoleId NVARCHAR(450);
    SELECT @RoleId = Id FROM AspNetRoles WHERE Name = 'SiteAdmin';
    
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@UserId, @RoleId);
        PRINT 'User added to AspNetUserRoles for SiteAdmin.';
    END
    
    -- Display current admin users
    PRINT '';
    PRINT 'Current Site Admins:';
    SELECT 
        u.UserName,
        u.Email,
        sr.AssignedAt,
        sr.IsActive
    FROM SiteRoles sr
    INNER JOIN AspNetUsers u ON sr.UserId = u.Id
    WHERE sr.RoleName = 'SiteAdmin' AND sr.IsActive = 1;
END
GO

