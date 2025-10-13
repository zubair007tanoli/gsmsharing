# 🔑 Google APIs Setup Guide
**Complete Guide for AdSense & Keyword Planner Integration**

---

## 📋 **Prerequisites**

Before you begin, ensure you have:
- ✅ Active Google AdSense account
- ✅ Access to both websites (gsmsharing.com & discussionspot.com) in AdSense
- ✅ Google Cloud Platform account
- ✅ Google Ads account (for Keyword Planner)
- ✅ Basic understanding of API credentials

---

## 🎯 **Part 1: Google AdSense API Setup**

### Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click "**Select a Project**" → "**New Project**"
3. Name it: `DiscussionSpot-Revenue-Tracker`
4. Click "**Create**"

### Step 2: Enable AdSense Management API

1. In your project, navigate to "**APIs & Services**" → "**Library**"
2. Search for "**AdSense Management API**"
3. Click on it and press "**Enable**"

### Step 3: Create Service Account (Recommended)

1. Go to "**APIs & Services**" → "**Credentials**"
2. Click "**Create Credentials**" → "**Service Account**"
3. Fill in details:
   - **Name:** `adsense-revenue-sync`
   - **Description:** `Automated AdSense revenue syncing service`
4. Click "**Create and Continue**"
5. Grant role: "**Viewer**" (or "**AdSense Viewer**" if available)
6. Click "**Done**"

### Step 4: Generate Service Account Key

1. Find your new service account in the list
2. Click on it → "**Keys**" tab
3. Click "**Add Key**" → "**Create New Key**"
4. Choose "**JSON**" format
5. Click "**Create**"
6. **Save the downloaded JSON file** as:
   ```
   discussionspot9/Config/google-adsense-credentials.json
   ```

### Step 5: Grant AdSense Access

1. Copy the **service account email** (looks like: `adsense-revenue-sync@project-id.iam.gserviceaccount.com`)
2. Go to your [Google AdSense Account](https://www.google.com/adsense/)
3. Navigate to "**Settings**" → "**Account**" → "**Access and authorization**"
4. Click "**Manage**" under "**Account access**"
5. Click "**Invite user**"
6. Paste the service account email
7. Set permission: "**Read-only access**"
8. Click "**Send Invitation**"

### Step 6: Get Your AdSense Publisher ID

1. In your AdSense dashboard, look at the URL or top-right corner
2. Your Publisher ID looks like: `pub-1234567890123456`
3. You'll need this for configuration

### Step 7: Update Configuration

Edit `discussionspot9/appsettings.json`:

```json
"GoogleAdSense": {
  "Sites": [
    {
      "Domain": "gsmsharing.com",
      "AdClientId": "ca-pub-YOUR_PUBLISHER_ID",
      "AccountId": "pub-YOUR_PUBLISHER_ID",
      "IsActive": true,
      "BaseUrl": "https://gsmsharing.com",
      "UrlPatterns": [
        "gsmsharing.com/r/",
        "gsmsharing.com/post/"
      ]
    },
    {
      "Domain": "discussionspot.com",
      "AdClientId": "ca-pub-YOUR_PUBLISHER_ID",
      "AccountId": "pub-YOUR_PUBLISHER_ID",
      "IsActive": true,
      "BaseUrl": "https://discussionspot.com",
      "UrlPatterns": [
        "discussionspot.com/r/",
        "discussionspot.com/post/"
      ]
    }
  ],
  "ServiceAccountEmail": "adsense-revenue-sync@project-id.iam.gserviceaccount.com",
  "ServiceAccountKeyPath": "Config/google-adsense-credentials.json",
  "UseServiceAccount": true,
  "SyncIntervalHours": 24,
  "HistoricalDataDays": 30
}
```

**Replace:**
- `YOUR_PUBLISHER_ID` with your actual AdSense Publisher ID (numbers only, no `pub-` prefix for AccountId)
- `project-id` with your Google Cloud project ID

---

## 🔍 **Part 2: Google Ads API (Keyword Planner) Setup**

### Step 1: Create Google Ads Account

1. Go to [Google Ads](https://ads.google.com/)
2. Create an account if you don't have one
3. **Important:** You DON'T need to run actual ads, but the account must be verified
4. Complete the verification process

### Step 2: Apply for Google Ads API Access

1. Go to [Google Ads API Center](https://developers.google.com/google-ads/api/docs/get-started/dev-token)
2. Sign in with your Google Ads account
3. Navigate to "**Tools & Settings**" → "**Setup**" → "**API Center**"
4. Click "**Apply for Access**"
5. Fill out the application form
6. **Wait for approval** (usually 24-48 hours)

### Step 3: Get Your Developer Token

1. Once approved, go back to API Center
2. Copy your "**Developer Token**"
3. Save it securely - you'll need it later

### Step 4: Create OAuth 2.0 Credentials

1. Return to your [Google Cloud Console](https://console.cloud.google.com/)
2. Same project: `DiscussionSpot-Revenue-Tracker`
3. Navigate to "**APIs & Services**" → "**Library**"
4. Search for "**Google Ads API**"
5. Click "**Enable**"

6. Go to "**Credentials**" → "**Create Credentials**" → "**OAuth client ID**"
7. Configure consent screen (if asked):
   - User Type: **External**
   - App name: `DiscussionSpot Keyword Research`
   - User support email: your email
   - Scopes: `https://www.googleapis.com/auth/adwords`
   - Test users: Add your email
8. Choose application type: "**Desktop app**"
9. Name it: `keyword-research-client`
10. Click "**Create**"
11. **Download the JSON file** and save as:
    ```
    discussionspot9/Config/google-ads-credentials.json
    ```

### Step 5: Generate Refresh Token

**Run this PowerShell script** (save as `get-refresh-token.ps1`):

```powershell
# Replace these with your values from the credentials file
$clientId = "YOUR_CLIENT_ID"
$clientSecret = "YOUR_CLIENT_SECRET"

# Step 1: Get authorization code
$authUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=$clientId&redirect_uri=urn:ietf:wg:oauth:2.0:oob&scope=https://www.googleapis.com/auth/adwords&response_type=code&access_type=offline&prompt=consent"

Write-Host "Open this URL in your browser:`n$authUrl`n"
Write-Host "After authorizing, you'll get a code. Paste it here:"
$authCode = Read-Host

# Step 2: Exchange code for refresh token
$tokenUrl = "https://oauth2.googleapis.com/token"
$body = @{
    code = $authCode
    client_id = $clientId
    client_secret = $clientSecret
    redirect_uri = "urn:ietf:wg:oauth:2.0:oob"
    grant_type = "authorization_code"
}

$response = Invoke-RestMethod -Uri $tokenUrl -Method POST -Body $body
Write-Host "`nYour Refresh Token:`n$($response.refresh_token)" -ForegroundColor Green
```

**Execute:**
```powershell
.\get-refresh-token.ps1
```

Copy the refresh token output.

### Step 6: Get Your Customer ID

1. Log in to [Google Ads](https://ads.google.com/)
2. Look at the top-right corner
3. You'll see a 10-digit number like: `123-456-7890`
4. Remove dashes: `1234567890` (this is your Customer ID)

### Step 7: Update Configuration

Edit `discussionspot9/appsettings.json`:

```json
"GoogleAds": {
  "DeveloperToken": "YOUR_DEVELOPER_TOKEN",
  "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "RefreshToken": "YOUR_REFRESH_TOKEN",
  "CustomerId": "1234567890",
  "IsConfigured": true,
  "MaxKeywordSuggestions": 50,
  "MaxKeywordsPerPost": 15
}
```

---

## 📦 **Part 3: Install Required Packages**

Open terminal in `discussionspot9/` directory:

```bash
# AdSense API
dotnet add package Google.Apis.Adsense.v2
dotnet add package Google.Apis.Auth.AspNetCore3

# Google Ads API (Keyword Planner)
dotnet add package Google.Ads.GoogleAds --version 19.0.0

# Restore
dotnet restore
```

---

## 🧪 **Part 4: Testing**

### Test AdSense Connection

1. Start your application
2. Navigate to: `/admin/seo/dashboard`
3. Click "**Sync AdSense**" button
4. Check logs for:
   ```
   ✅ Google AdSense API initialized successfully
   💰 Syncing revenue for discussionspot.com on YYYY-MM-DD
   ✅ Synced discussionspot.com: $X.XX
   ```

### Test Keyword Planner

1. Go to post creation page: `/post/create`
2. Enter a post title
3. Check logs for:
   ```
   🎯 Generating enhanced SEO for: [Your Title]
   ✅ Enhanced SEO generated. Score: XX, Keywords: XX, Search Volume: XXXXX
   ```

---

## 🔧 **Part 5: Database Migration**

Run these commands to create new tables:

```bash
cd D:\LAPTOP BACKUP\CodingProject2025\Repos\gsmsharing\discussionspot9

# Create migration
dotnet ef migrations add EnhancedSeoAndMultiSiteRevenue

# Apply migration
dotnet ef database update
```

**Verify tables created:**
- `PostKeywords`
- `EnhancedSeoMetadata`
- `MultiSiteRevenues`

---

## 📊 **Part 6: Enable Automatic Syncing**

The application will automatically:
- ✅ Sync AdSense revenue **daily at 2 AM UTC**
- ✅ Sync Search Console data **daily at 3 AM UTC**
- ✅ Run weekly SEO optimization **Sundays at 1 AM UTC**

**To manually trigger:**
1. Go to dashboard: `/admin/seo/dashboard`
2. Use the "**Sync**" buttons in the sidebar

---

## ⚠️ **Troubleshooting**

### Issue: "AdSense API not configured"

**Solution:**
1. Check `Config/google-adsense-credentials.json` exists
2. Verify service account has AdSense access
3. Ensure Publisher ID is correct in `appsettings.json`

### Issue: "Google Ads API authentication failed"

**Solution:**
1. Verify developer token is approved (not "pending")
2. Check OAuth credentials are correct
3. Regenerate refresh token if expired
4. Ensure Customer ID has no dashes

### Issue: "No revenue data showing"

**Solution:**
1. Wait 24-48 hours after setup (AdSense has delay)
2. Check if your sites actually have AdSense code installed
3. Verify URL patterns match your actual post URLs
4. Check application logs for sync errors

### Issue: "Keyword suggestions empty"

**Solution:**
1. Confirm Google Ads API is enabled in Cloud Console
2. Check if developer token is approved
3. Verify billing is set up in Google Ads (required even for $0 spend)
4. The app will fall back to intelligent keyword generation if API fails

---

## 💡 **Best Practices**

1. **Security:**
   - Never commit `Config/*.json` files to git
   - Add to `.gitignore`:
     ```
     Config/google-*.json
     ```

2. **Monitoring:**
   - Check logs daily for first week
   - Set up email alerts for sync failures
   - Monitor API quota usage in Google Cloud Console

3. **Cost Management:**
   - AdSense Management API: **FREE**
   - Google Ads API: **FREE** (no ads needed)
   - Cloud Project: **FREE** (stays within free tier)

4. **Data Accuracy:**
   - AdSense data has 24-48 hour delay
   - First sync may show $0 - wait for data to populate
   - Revenue attribution to posts is based on URL matching

---

## ✅ **Verification Checklist**

- [ ] Google Cloud project created
- [ ] AdSense Management API enabled
- [ ] Service account created with JSON key
- [ ] Service account added to AdSense with read-only access
- [ ] Publisher ID added to `appsettings.json`
- [ ] Google Ads API enabled
- [ ] Developer token obtained and approved
- [ ] OAuth credentials created
- [ ] Refresh token generated
- [ ] Customer ID added to `appsettings.json`
- [ ] NuGet packages installed
- [ ] Database migration applied
- [ ] AdSense sync tested successfully
- [ ] Keyword research tested successfully
- [ ] Credentials files added to `.gitignore`

---

## 📞 **Support**

If you encounter issues:

1. **Check Logs:**
   ```bash
   # View application logs
   tail -f logs/app.log
   ```

2. **Google API Status:**
   - [AdSense API Status](https://status.cloud.google.com/)
   - [Google Ads API Status](https://ads-developers.googleblog.com/)

3. **Common Error Codes:**
   - `401 Unauthorized`: Check credentials/tokens
   - `403 Forbidden`: Verify API access granted
   - `404 Not Found`: Check Publisher/Customer IDs
   - `429 Too Many Requests`: API quota exceeded (rare)

---

## 🎉 **Success!**

Once completed, you'll see:
- ✅ **Real revenue data** from both sites in dashboard
- ✅ **$69.65+ balance** accurately displayed
- ✅ **Revenue per post** tracking
- ✅ **3-tier keyword research** (Primary, Secondary, LSI)
- ✅ **Click-worthy meta descriptions** with power words
- ✅ **Automated daily syncs**
- ✅ **Smart SEO optimization** based on revenue potential

**Your DiscussionSpot is now a true Revenue Machine!** 🚀💰

