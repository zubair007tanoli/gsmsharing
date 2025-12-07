# Ban Appeal System Implementation Guide

## Overview
This document describes the implementation of a ban appeal system that allows banned users to submit appeals and administrators to review and manage them.

## Database Changes

### 1. ApplicationUser Model Updates
Added the following properties to `ApplicationUser`:
- `IsBanned` (bool): Indicates if the user is banned
- `BannedAt` (DateTime?): Timestamp when the user was banned
- `BanReason` (string?): Reason for the ban
- `BannedByUserId` (string?): ID of the admin who banned the user
- `BanAppeals` (ICollection<BanAppeal>): Navigation property for appeals

### 2. New BanAppeal Model
Created a new `BanAppeal` model with:
- `BanAppealId` (int): Primary key
- `UserId` (string): Foreign key to ApplicationUser
- `AppealMessage` (string): User's appeal message
- `SubmittedAt` (DateTime): When the appeal was submitted
- `ReviewedAt` (DateTime?): When the appeal was reviewed
- `ReviewedByUserId` (string?): ID of the admin who reviewed
- `Status` (AppealStatus enum): Pending, Approved, or Rejected
- `AdminResponse` (string?): Admin's response to the appeal

### 3. Database Migration
To apply these changes, create and run a migration:

```bash
# In Package Manager Console or terminal
dotnet ef migrations add AddBanAppealSystem
dotnet ef database update
```

Or manually add the migration file with the following SQL:

```sql
-- Add columns to AspNetUsers table
ALTER TABLE AspNetUsers ADD IsBanned BIT NOT NULL DEFAULT 0;
ALTER TABLE AspNetUsers ADD BannedAt DATETIME2 NULL;
ALTER TABLE AspNetUsers ADD BanReason NVARCHAR(MAX) NULL;
ALTER TABLE AspNetUsers ADD BannedByUserId NVARCHAR(450) NULL;

-- Create BanAppeals table
CREATE TABLE BanAppeals (
    BanAppealId INT IDENTITY(1,1) PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    AppealMessage NVARCHAR(2000) NOT NULL,
    SubmittedAt DATETIME2 NOT NULL,
    ReviewedAt DATETIME2 NULL,
    ReviewedByUserId NVARCHAR(450) NULL,
    Status INT NOT NULL DEFAULT 0,
    AdminResponse NVARCHAR(500) NULL,
    CONSTRAINT FK_BanAppeals_Users FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BanAppeals_ReviewedBy FOREIGN KEY (ReviewedByUserId) REFERENCES AspNetUsers(Id) ON DELETE NO ACTION
);

-- Create indexes for better performance
CREATE INDEX IX_BanAppeals_UserId ON BanAppeals(UserId);
CREATE INDEX IX_BanAppeals_Status ON BanAppeals(Status);
CREATE INDEX IX_BanAppeals_SubmittedAt ON BanAppeals(SubmittedAt);
```

## Controllers

### 1. BanAppealController
**Location:** `gsmsharing/Controllers/BanAppealController.cs`

**Actions:**
- `Create()` (GET): Display form for submitting an appeal
- `Create()` (POST): Process appeal submission
- `MyAppeals()` (GET): Display user's appeal history

**Features:**
- Only banned users can submit appeals
- Prevents multiple pending appeals
- Validates appeal message length

### 2. AdminController Updates
**Location:** `gsmsharing/Controllers/AdminController.cs`

**New Actions:**
- `BanAppeals()` (GET): List all appeals
- `ReviewAppeal(int id)` (GET): View appeal details
- `ApproveAppeal(int id, string adminResponse)` (POST): Approve appeal and unban user
- `RejectAppeal(int id, string adminResponse)` (POST): Reject appeal
- `BanUser(string userId, string banReason)` (POST): Ban a user
- `UnbanUser(string userId)` (POST): Unban a user

**Updated Actions:**
- `Index()`: Now includes pending appeals count

## Views

### User Views
1. **Create.cshtml** (`Views/BanAppeal/Create.cshtml`)
   - Form for submitting ban appeals
   - Validation and guidelines

2. **MyAppeals.cshtml** (`Views/BanAppeal/MyAppeals.cshtml`)
   - Display user's appeal history
   - Shows status and admin responses

### Admin Views
1. **BanAppeals.cshtml** (`Views/Admin/BanAppeals.cshtml`)
   - List all appeals with filtering
   - Quick approve/reject actions
   - Status indicators

2. **ReviewAppeal.cshtml** (`Views/Admin/ReviewAppeal.cshtml`)
   - Detailed view of an appeal
   - Approve/reject forms
   - User and ban information

3. **Index.cshtml** (Updated)
   - Alert for pending appeals
   - Link to appeals management

4. **Users.cshtml** (Updated)
   - Added link to Ban Appeals page

## Routes

The following routes are available:

- `/BanAppeal/Create` - Submit new appeal (requires authentication, user must be banned)
- `/BanAppeal/MyAppeals` - View user's appeals (requires authentication)
- `/Admin/BanAppeals` - Manage all appeals (requires Admin role)
- `/Admin/ReviewAppeal/{id}` - Review specific appeal (requires Admin role)
- `/Admin/ApproveAppeal` - Approve appeal (POST, requires Admin role)
- `/Admin/RejectAppeal` - Reject appeal (POST, requires Admin role)
- `/Admin/BanUser` - Ban a user (POST, requires Admin role)
- `/Admin/UnbanUser` - Unban a user (POST, requires Admin role)

## Usage

### For Banned Users
1. Navigate to `/BanAppeal/Create` (or add a link in your banned user message)
2. Fill out the appeal form with a detailed message
3. Submit the appeal
4. View status at `/BanAppeal/MyAppeals`

### For Administrators
1. View pending appeals count on dashboard
2. Navigate to `/Admin/BanAppeals` to see all appeals
3. Review each appeal and approve or reject
4. When approving, user is automatically unbanned
5. Admin can add optional response message

## Security Considerations

1. **Authorization:**
   - BanAppealController requires authentication
   - AdminController requires Admin role
   - Users can only view their own appeals

2. **Validation:**
   - Appeal messages are required and validated
   - Only one pending appeal per user
   - Only banned users can submit appeals

3. **Data Integrity:**
   - Foreign key constraints ensure data consistency
   - Cascade delete for user appeals when user is deleted

## Future Enhancements

Consider adding:
- Email notifications for appeal status changes
- Appeal history tracking
- Appeal statistics and reporting
- Time limits for resubmitting appeals
- Appeal categories/types
- File attachments to appeals
- Admin notes/comments on appeals

## Testing Checklist

- [ ] Banned user can submit appeal
- [ ] Non-banned user cannot submit appeal
- [ ] User cannot submit multiple pending appeals
- [ ] Admin can view all appeals
- [ ] Admin can approve appeal (unbans user)
- [ ] Admin can reject appeal (user remains banned)
- [ ] Appeal status updates correctly
- [ ] Dashboard shows pending appeals count
- [ ] User can view their appeal history
- [ ] Admin response is displayed to user

## Notes

- All timestamps use UTC
- Appeal messages are limited to 2000 characters
- Admin responses are limited to 500 characters
- The system prevents multiple pending appeals per user
- When an appeal is approved, the user is automatically unbanned


