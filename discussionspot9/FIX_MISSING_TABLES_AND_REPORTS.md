# 🔧 Fix Missing Tables & Test Report System

## ❌ Current Issues

1. **`Invalid object name 'EmailQueues'`** - EmailQueues table is missing from database
2. **`Invalid object name 'PostReports'`** - PostReports table might be missing
3. **Report system page exists but may not work** - Tables not created yet

---

## ✅ Solution: Create Missing Tables

### Step 1: Stop the Application

```bash
# Press Ctrl+C to stop the running application
```

### Step 2: Run the SQL Script

**Option A - Using SQL Server Management Studio (SSMS):**
1. Open SSMS
2. Connect to server: `167.88.42.56`
3. Login: `sa` / `1nsp1r0N@321`
4. Select database: `DiscussionspotADO`
5. Open file: `E:\Repo\discussionspot9\CREATE_MISSING_TABLES.sql`
6. Click "Execute" or press F5

**Option B - Using Command Line:**
```bash
cd E:\Repo\discussionspot9
sqlcmd -S 167.88.42.56 -d DiscussionspotADO -U sa -P "1nsp1r0N@321" -i CREATE_MISSING_TABLES.sql
```

**Option C - Using Azure Data Studio:**
1. Connect to your server
2. Open `CREATE_MISSING_TABLES.sql`
3. Run the script

---

## 📋 Tables That Will Be Created

### 1. **PostReports** Table
For storing user-reported posts:
- Report ID, Post ID, Reporter ID
- Reason & Details
- Status (pending, reviewed, resolved, dismissed)
- Admin notes & reviewed by info

### 2. **EmailQueues** Table
For reliable email delivery:
- Email details (to, subject, body)
- Status (pending, sending, sent, failed)
- Priority & retry logic
- Queue timestamps

### 3. **NotificationPreferences** Table
Per-type user notification settings:
- User ID & Notification Type
- Web/Email/Push enabled flags
- Email frequency (instant, hourly, daily, weekly)

### 4. **UserNotificationSettings** Table
Global user notification settings:
- Email/Web/Push master switches
- Digest frequency
- Quiet hours
- Group notifications, previews, sounds

### 5. **UserFollows** Table
User follow relationships:
- Follower ID & Followed ID
- Follow date
- Notifications enabled
- Active status

---

## 🧪 Testing the Report System

### Step 1: Verify Tables Exist

Run this query in SSMS:

```sql
USE DiscussionspotADO;

SELECT 
    'PostReports' as TableName,
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'PostReports')) 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END as Status
UNION ALL
SELECT 
    'EmailQueues',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'EmailQueues')) 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END
UNION ALL
SELECT 
    'NotificationPreferences',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'NotificationPreferences')) 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END
UNION ALL
SELECT 
    'UserNotificationSettings',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'UserNotificationSettings')) 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END
UNION ALL
SELECT 
    'UserFollows',
    CASE WHEN EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'UserFollows')) 
        THEN '✅ EXISTS' ELSE '❌ MISSING' END;
```

**Expected Result:** All tables should show ✅ EXISTS

### Step 2: Restart the Application

```bash
cd E:\Repo\discussionspot9
dotnet run
```

### Step 3: Access the Reports Page

Navigate to: **`http://localhost:5099/admin/manage/reports`**

**You should see:**
- ✅ Page loads without errors
- ✅ Filter buttons (All, Pending, Reviewed, Resolved, Dismissed)
- ✅ Empty message: "No reports found" (if no reports exist yet)

### Step 4: Test Report Creation

#### Method 1: Test via Post Detail Page

1. Go to any post: `http://localhost:5099/r/{community-slug}/posts/{post-slug}`
2. Look for "Report" button (usually in post actions)
3. Click "Report"
4. Select a reason:
   - Spam
   - Harassment
   - Misinformation
   - Adult Content
   - Copyright Violation
   - Other
5. Add details (optional)
6. Submit

#### Method 2: Test via API (Postman/curl)

```bash
curl -X POST http://localhost:5099/api/report/post \
  -H "Content-Type: application/json" \
  -d '{
    "postId": 1,
    "reason": "spam",
    "details": "This is a test report"
  }'
```

### Step 5: Verify Report Appears

1. Go back to: `http://localhost:5099/admin/manage/reports`
2. Click "Pending" filter
3. You should see your test report:
   - Report ID
   - Post title with link
   - Reporter name
   - Reason badge
   - Details preview
   - "Resolve" and "Dismiss" buttons

### Step 6: Test Report Actions

**Resolve a Report:**
1. Click "Resolve" button
2. Enter action taken (optional): "Removed post"
3. Confirm
4. Report should move to "Resolved" status

**Dismiss a Report:**
1. Click "Dismiss" button
2. Confirm
3. Report should move to "Dismissed" status

---

## 🔍 Troubleshooting

### Issue: "You don't have permission to access this page"

**Solution:** Make sure you're logged in as admin

```sql
-- Check if you're an admin
SELECT * FROM AspNetUserRoles ur
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE ur.UserId = 'your-user-id' AND r.Name = 'Admin';

-- OR check if your email is in the hardcoded list
-- Your email: zubair007tanoli@gmail.com should have admin access
```

### Issue: "Report button not showing on posts"

**Check these files:**

1. **Post Detail View:** `discussionspot9/Views/DiscussionPost/Detail.cshtml`
   - Should have a report button
   
2. **Report Controller:** Check if `ReportController.cs` or API endpoint exists

3. **JavaScript:** Check if report functionality is in:
   - `/wwwroot/js/post-actions.js`
   - Or inline scripts in Detail.cshtml

### Issue: EmailWorkerService Errors Still Showing

This is **NORMAL** until you create the tables. The errors will stop after you run the `CREATE_MISSING_TABLES.sql` script and restart the application.

---

## 📊 Verify Email System is Working

### Test Email Queue

```sql
-- Check email queue is working
SELECT TOP 10 
    EmailQueueId,
    ToEmail,
    Subject,
    Status,
    Priority,
    CreatedAt,
    ErrorMessage
FROM EmailQueues
ORDER BY CreatedAt DESC;
```

### Test Notification Preferences

```sql
-- Check user notification preferences
SELECT 
    np.NotificationType,
    np.WebEnabled,
    np.EmailEnabled,
    np.EmailFrequency
FROM NotificationPreferences np
WHERE np.UserId = 'your-user-id';
```

---

## 📝 Report System Features

### For Users:
- ✅ Report posts with reason & details
- ✅ Track report status
- ✅ Receive notifications when report is reviewed

### For Admins:
- ✅ View all reports (filtered by status)
- ✅ See pending reports count
- ✅ Resolve reports with action notes
- ✅ Dismiss invalid reports
- ✅ Auto-refresh pending count every 30 seconds
- ✅ Direct link to reported post
- ✅ Reporter information
- ✅ Timestamps & reviewer info

---

## 🎯 Quick Test Checklist

- [ ] Run `CREATE_MISSING_TABLES.sql`
- [ ] Verify all 5 tables created (PostReports, EmailQueues, etc.)
- [ ] Restart application
- [ ] Navigate to `/admin/manage/reports`
- [ ] Page loads without errors
- [ ] Create a test report on any post
- [ ] Report appears in admin panel
- [ ] Test "Resolve" action
- [ ] Test "Dismiss" action
- [ ] Check email queue for notification emails

---

## 📧 Expected Behavior After Fix

### Email System:
- ✅ No more "Invalid object name 'EmailQueues'" errors
- ✅ Emails queue properly
- ✅ Background worker processes queue
- ✅ Users receive email notifications

### Report System:
- ✅ Users can report posts
- ✅ Reports stored in database
- ✅ Admin panel shows all reports
- ✅ Filter by status works
- ✅ Resolve/Dismiss actions work
- ✅ Real-time pending count updates

---

## 🆘 Need Help?

If you still have issues after running the SQL script:

1. **Check Application Logs**
   ```bash
   # Look for errors in terminal output
   ```

2. **Check Database Connection**
   ```sql
   SELECT @@VERSION; -- Verify connection
   ```

3. **Verify Table Structure**
   ```sql
   EXEC sp_help 'PostReports';
   EXEC sp_help 'EmailQueues';
   ```

4. **Check Service Registration** in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IReportService, ReportService>();
   builder.Services.AddScoped<IEmailService, EmailService>();
   builder.Services.AddHostedService<EmailWorkerService>();
   ```

---

## ✅ Success Indicators

You'll know everything is working when:

1. ✅ Application starts without EmailQueues/PostReports errors
2. ✅ `/admin/manage/reports` page loads successfully  
3. ✅ You can create and view reports
4. ✅ Email notifications are being queued
5. ✅ Background email worker is processing queue
6. ✅ All report actions (resolve/dismiss) work

---

**Last Updated:** October 28, 2025  
**Priority:** HIGH - Required for admin functionality  
**Status:** 🔧 Fix Required - Run SQL script to resolve

