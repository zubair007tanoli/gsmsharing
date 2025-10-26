# 🔍 Community Feature Analysis Report - DiscussionSpot9 vs GSMSharing

## 📊 Analysis Date: October 19, 2025

---

## ✅ **DiscussionSpot9 - Community Features Status**

### **What's Implemented:**

#### **1. Database Schema** ✅ EXCELLENT
```sql
Community Table:
- CommunityId (PK)
- Name, Slug, Title
- Description, ShortDescription
- CategoryId (FK)
- CreatorId (FK to AspNetUsers)
- CommunityType (public/restricted/private)
- IconUrl ✅ 
- BannerUrl ✅ 
- ThemeColor ✅
- MemberCount, PostCount
- IsNSFW, IsDeleted
- CreatedAt, UpdatedAt
```

**✅ Icon and Banner ARE in the database!**

---

#### **2. ViewModel Support** ✅ COMPLETE

**CreateCommunityViewModel:**
```csharp
✅ IFormFile? IconFile        // For file upload
✅ IFormFile? BannerFile       // For file upload
✅ string? IconUrl             // For storing URL
✅ string? BannerUrl           // For storing URL
✅ string ThemeColor = "#0079D3"
```

**CommunityDetailViewModel:**
```csharp
✅ IconUrl
✅ BannerUrl  
✅ ThemeColor
```

---

#### **3. View Implementation** ✅ UI EXISTS

**Create.cshtml:**
- ✅ Icon upload area with preview
- ✅ Banner upload area with preview
- ✅ Theme color picker
- ✅ JavaScript file preview
- ✅ Drag & drop UI
- ✅ Image preview before upload

**Details.cshtml:**
- ✅ Banner display in header
- ✅ Icon display in avatar
- ✅ Theme color applied to header gradient
- ✅ Fallback to placeholder if no icon
- ✅ Beautiful Reddit-style design

**Index.cshtml:**
- ✅ Community cards with icons
- ✅ Modern layout
- ✅ Responsive design
- ✅ Proper theme integration

---

### **❌ CRITICAL PROBLEM FOUND!**

#### **🔴 File Upload is NOT Being Processed!**

**Issue Location:** `CommunityService.cs` - Line 188-236

```csharp
public async Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model)
{
    // ...
    var community = new Community
    {
        Name = model.Name,
        Slug = slug,
        Title = model.Title,
        // ...
        IconUrl = model.IconUrl,      // ❌ PROBLEM: Just copying URL property
        BannerUrl = model.BannerUrl,  // ❌ PROBLEM: Just copying URL property
        // ...
    };
    
    // ❌ MISSING: No code to process IconFile or BannerFile!
    // ❌ MISSING: No file upload service call
    // ❌ MISSING: No image saving logic
}
```

**What's Missing:**
1. No file upload processing
2. IconFile and BannerFile are ignored
3. Files are selected in UI but never saved
4. IconUrl/BannerUrl remain empty unless manually entered

---

## 🔴 **Issues in DiscussionSpot9**

### **Problem 1: File Upload Not Implemented**

**Symptom:**
- User uploads icon/banner in Create.cshtml
- Files are selected, preview shows
- Form submits
- **Files are NOT saved to server**
- IconUrl and BannerUrl remain NULL
- Community created without images

**Root Cause:**
```csharp
// CommunityService.cs - CreateCommunityAsync method
// Missing file processing logic:

if (model.IconFile != null)
{
    // THIS CODE DOESN'T EXIST!
    // Should: Save file, get URL, assign to IconUrl
}

if (model.BannerFile != null)
{
    // THIS CODE DOESN'T EXIST!
    // Should: Save file, get URL, assign to BannerUrl
}
```

---

### **Problem 2: No File Storage Service**

**Missing Infrastructure:**
- ❌ No IFileStorageService interface
- ❌ No file upload helper
- ❌ No image resize/optimization
- ❌ No file validation
- ❌ No storage path configuration

**What's Needed:**
```csharp
public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder);
    Task<bool> DeleteImageAsync(string url);
    Task<string> ResizeImageAsync(string url, int width, int height);
}
```

---

### **Problem 3: Views Don't Match Site Theme**

**Current State:**
- Create.cshtml has **custom CSS** (`/css/CustomStyles/V1/CreateCommunityStyle.css`)
- Details.cshtml has **inline styles** (730+ lines of CSS!)
- Index.cshtml has **inline styles** (190+ lines!)

**Problems:**
- ❌ Custom CSS may not exist or match theme
- ❌ Massive inline styles make maintenance hard
- ❌ Inconsistent styling across views
- ❌ Not using shared theme variables
- ❌ Dark mode support is hit-or-miss

**Site Theme:**
Based on other views, the site uses:
- Bootstrap 5
- modern-style.css
- dark-mode.css
- DiscussionMain.css
- Reddit-style design system

---

## ⚠️ **GSMSharing - Community Status**

### **What Exists:**

**Database:**
```csharp
// Models/Community.cs
Community {
    CommunityID, Name, Slug, Description
    Rules, CoverImage ✅, IconImage ✅
    CreatorId, IsPrivate, MemberCount
    CategoryID, CreatedAt, UpdatedAt
}
```

**✅ GSMSharing HAS Icon and Cover fields!**
- IconImage
- CoverImage

**Controller:**
- Very basic (32 lines)
- Only has CreateCommunity POST method
- No Index, Details, or Edit views
- Uses CommunityHelper for SEO

**ViewModels:**
- CommunityViewModel exists
- Unclear if it has file upload properties

**Views:**
- ❌ NO VIEWS CREATED
- No Create.cshtml
- No Details.cshtml
- No Index.cshtml

---

## 📊 **Feature Comparison Matrix**

| Feature | DiscussionSpot9 | GSMSharing | Winner |
|---------|----------------|------------|--------|
| **Database Schema** |
| Icon/Banner fields | ✅ IconUrl, BannerUrl | ✅ IconImage, CoverImage | Tie |
| Theme color | ✅ | ❌ | DS9 |
| Short description | ✅ | ❌ | DS9 |
| Community type | ✅ | ✅ IsPrivate | DS9 |
| Rules | ✅ | ✅ | Tie |
| Member count | ✅ Auto | ✅ | DS9 |
| Post count | ✅ Auto | ❌ | DS9 |
| **File Upload** |
| ViewModel support | ✅ IFormFile props | ❓ Unknown | ? |
| Upload processing | ❌ NOT IMPLEMENTED | ❌ NOT IMPLEMENTED | Neither |
| File storage service | ❌ Missing | ❓ ImageRepository exists | GSM? |
| **Views & UI** |
| Create view | ✅ Beautiful | ❌ Missing | DS9 |
| Details view | ✅ Reddit-style | ❌ Missing | DS9 |
| Index view | ✅ Modern | ❌ Missing | DS9 |
| Theme consistency | ❌ Mixed styles | N/A | - |
| **Controller** |
| Create POST | ✅ Full | ✅ Basic | DS9 |
| Details GET | ✅ | ❌ | DS9 |
| Index GET | ✅ | ❌ | DS9 |
| Edit/Update | ✅ | ❌ | DS9 |
| Join/Leave | ✅ AJAX | ❌ | DS9 |
| **Service Layer** |
| Complete service | ✅ | ❓ Unclear | DS9 |
| File handling | ❌ Missing | ❓ | ? |

**Winner:** DiscussionSpot9 (far more complete)

---

## 🚨 **Critical Issues Summary**

### **DiscussionSpot9 Issues:**

1. **🔴 CRITICAL: File Upload Not Working**
   - Views have upload UI
   - ViewModel has IFormFile properties
   - Service IGNORES the files
   - Files are not saved to disk
   - URLs remain empty

2. **🟡 MEDIUM: Styling Inconsistency**
   - Each view has own CSS
   - Massive inline styles
   - May not match site theme
   - Hard to maintain

3. **🟢 LOW: Missing Edit Community View**
   - Edit modal exists in Details.cshtml
   - But no dedicated edit page
   - Update method exists in service

---

### **GSMSharing Issues:**

1. **🔴 CRITICAL: No Views Created**
   - Database ready
   - Controller has basic create
   - NO UI at all
   - Complete rebuild needed

2. **🔴 CRITICAL: Unknown File Upload Status**
   - ImageRepository exists
   - Unknown if it works with communities
   - Needs investigation

---

## 🎯 **Recommendations**

### **For DiscussionSpot9:** ⭐ RECOMMENDED

**Priority: Fix File Upload (Critical)**

**Steps:**
1. Create/use IFileStorageService
2. Update CommunityService.CreateCommunityAsync:
   ```csharp
   // Process icon
   if (model.IconFile != null)
   {
       var iconUrl = await _fileService.SaveImageAsync(
           model.IconFile, 
           "communities/icons"
       );
       community.IconUrl = iconUrl;
   }
   
   // Process banner
   if (model.BannerFile != null)
   {
       var bannerUrl = await _fileService.SaveImageAsync(
           model.BannerFile, 
           "communities/banners"
       );
       community.BannerUrl = bannerUrl;
   }
   ```

3. Update UpdateCommunityDetailsAsync similarly

4. Clean up view styles:
   - Extract inline CSS to separate file
   - Use shared theme variables
   - Ensure dark mode works

**Timeline:** 2-3 days
**Impact:** 🔥🔥🔥 HIGH - Completes community creation

---

### **For GSMSharing:**

**Priority: Build Everything**

**Steps:**
1. Create all 3 views (Create, Details, Index)
2. Copy/adapt from DiscussionSpot9
3. Implement file upload
4. Create full controller
5. Build service layer

**Timeline:** 1-2 weeks
**Impact:** 🔥 Creates feature from scratch

---

## 📝 **Detailed Issue Report**

### **Issue #1: DiscussionSpot9 File Upload**

**Location:** `discussionspot9/Services/CommunityService.cs`  
**Method:** `CreateCommunityAsync` (lines 188-236)

**Current Code:**
```csharp
public async Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model)
{
    // ... validation ...
    
    var community = new Community
    {
        // ... other properties ...
        IconUrl = model.IconUrl,      // ❌ Always NULL (file not processed)
        BannerUrl = model.BannerUrl,  // ❌ Always NULL (file not processed)
        // ...
    };
    
    _context.Communities.Add(community);
    await _context.SaveChangesAsync();
    // ...
}
```

**What Should Happen:**
```csharp
public async Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model)
{
    // ... validation ...
    
    string? iconUrl = null;
    string? bannerUrl = null;
    
    // ✅ Process icon upload
    if (model.IconFile != null)
    {
        iconUrl = await SaveCommunityImageAsync(
            model.IconFile, 
            "icons", 
            256, // resize to 256x256
            256
        );
    }
    
    // ✅ Process banner upload
    if (model.BannerFile != null)
    {
        bannerUrl = await SaveCommunityImageAsync(
            model.BannerFile, 
            "banners",
            1920, // resize to 1920x384
            384
        );
    }
    
    var community = new Community
    {
        // ... other properties ...
        IconUrl = iconUrl ?? model.IconUrl,      // ✅ Use uploaded or provided URL
        BannerUrl = bannerUrl ?? model.BannerUrl, // ✅ Use uploaded or provided URL
        ThemeColor = model.ThemeColor,
        // ...
    };
    
    _context.Communities.Add(community);
    await _context.SaveChangesAsync();
    // ...
}

private async Task<string> SaveCommunityImageAsync(
    IFormFile file, 
    string subfolder, 
    int maxWidth, 
    int maxHeight)
{
    // Validate file
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    
    if (!allowedExtensions.Contains(extension))
    {
        throw new Exception("Invalid file type");
    }
    
    if (file.Length > 5 * 1024 * 1024) // 5MB
    {
        throw new Exception("File too large");
    }
    
    // Generate unique filename
    var fileName = $"{Guid.NewGuid()}{extension}";
    var relativePath = $"/uploads/communities/{subfolder}/{fileName}";
    var fullPath = Path.Combine("wwwroot", relativePath.TrimStart('/'));
    
    // Ensure directory exists
    var directory = Path.GetDirectoryName(fullPath);
    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory!);
    }
    
    // Save file
    using (var stream = new FileStream(fullPath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    // TODO: Resize image to maxWidth x maxHeight
    // You could use ImageSharp or similar library
    
    return relativePath;
}
```

---

### **Issue #2: DiscussionSpot9 Styling Inconsistency**

**Problem:**
Each view has different styling approach:

**Create.cshtml:**
```html
<!-- References external CSS (may not exist) -->
<link href="/css/CustomStyles/V1/CreateCommunityStyle.css" rel="stylesheet" />
```

**Details.cshtml:**
```html
<!-- 730+ lines of INLINE CSS in <style> tag! -->
@section Styles {
    <link rel="stylesheet" href="~/css/modern-style.css" />
    <link rel="stylesheet" href="~/css/dark-mode.css" />
    <link rel="stylesheet" href="~/css/DiscussionMain.css" />
    <link rel="stylesheet" href="~/css/Improve.css" />
    <style>
        /* 730 lines of CSS here! */
    </style>
}
```

**Index.cshtml:**
```html
<!-- 190+ lines of INLINE CSS -->
<style>
    /* 190 lines of CSS here! */
</style>
```

**Solution Needed:**
- Extract all CSS to dedicated stylesheet
- Use shared theme variables
- Ensure consistency with site design
- Remove inline styles
- Use CSS classes from main theme

---

### **Issue #3: Missing Features**

Even with file upload fixed, still missing:

❌ **Edit Community Page**
- Modal exists in Details.cshtml
- But should have dedicated page
- Needs file re-upload capability

❌ **Community Settings**
- Moderator management
- Member permissions
- Custom sidebar content
- Featured posts
- Widgets

❌ **Community Analytics**
- Member growth chart
- Post activity graph
- Top contributors
- Traffic stats

---

## 🎯 **GSMSharing Community Status**

### **What Exists:**

**Database Model:**
```csharp
public class Community
{
    int CommunityID
    string Name, Slug, Description, Rules
    string CoverImage  ✅ 
    string IconImage   ✅ 
    string CreatorId
    bool? IsPrivate
    int? MemberCount
    int? CategoryID
    DateTime? CreatedAt, UpdatedAt
}
```

**Controller:**
```csharp
// Very basic - only 32 lines
[HttpPost]
public IActionResult CreateCommunity(CommunityViewModel viewModel)
{
    CommunityHelper.SetDefaultSeoValues(viewModel);
    string UID = userService.GetCurrentUserId()??string.Empty;
    communityRepository.CreateAsync(viewModel, UID);
    return RedirectToAction("Create","Posts");
}
```

**Views:**
```
❌ No Create.cshtml
❌ No Details.cshtml  
❌ No Index.cshtml
❌ NO VIEWS AT ALL!
```

**Repository:**
- ICommunityRepository exists
- CommunityRepository implementation unknown
- Likely has CreateAsync method
- Unknown if file upload works

---

## 🏆 **Winner: DiscussionSpot9**

**Why:**
✅ Complete UI (3 beautiful views)  
✅ Full controller implementation  
✅ Comprehensive service layer  
✅ Database schema is excellent  
✅ 95% complete (just needs file upload!)

**GSMSharing:**
❌ No UI at all  
❌ Basic controller  
❌ Unknown service status  
❌ 5% complete

---

## 🚀 **Action Plan**

### **For DiscussionSpot9** (Recommended)

#### **Phase 1: Fix File Upload** (Day 1-2) 🔴
1. Create `IFileStorageService` interface
2. Implement `FileStorageService` class
3. Update `CommunityService.CreateCommunityAsync`
4. Add image validation & resize
5. Test file upload flow

#### **Phase 2: Clean Up Styles** (Day 3) 🟡
1. Extract inline CSS from Details.cshtml
2. Extract inline CSS from Index.cshtml
3. Create `community-styles.css`
4. Ensure theme consistency
5. Test dark mode

#### **Phase 3: Add Edit Page** (Day 4) 🟡
1. Create Edit.cshtml view
2. Add Edit GET action
3. Handle file re-upload
4. Add file delete option
5. Test update flow

**Total Timeline:** 4 days  
**Result:** Fully functional community system

---

### **For GSMSharing**

#### **Phase 1: Create Views** (Week 1)
1. Copy/adapt Create.cshtml from DS9
2. Copy/adapt Details.cshtml from DS9
3. Copy/adapt Index.cshtml from DS9
4. Adjust for GSMSharing theme
5. Update controller actions

#### **Phase 2: Implement Backend** (Week 2)
1. Build CommunityService
2. Implement file upload
3. Add all CRUD operations
4. Test thoroughly

**Total Timeline:** 2 weeks  
**Result:** Community feature from scratch

---

## 📸 **File Upload Implementation Guide**

### **Step 1: Create File Storage Service**

```csharp
// Interfaces/IFileStorageService.cs
public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder, int? maxWidth = null, int? maxHeight = null);
    Task<bool> DeleteImageAsync(string relativePath);
    string GetPhysicalPath(string relativePath);
}

// Services/FileStorageService.cs
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;
    
    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }
    
    public async Task<string> SaveImageAsync(IFormFile file, string folder, int? maxWidth = null, int? maxHeight = null)
    {
        // Validation
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type. Only images are allowed.");
        
        if (file.Length > 5 * 1024 * 1024) // 5MB
            throw new ArgumentException("File size exceeds 5MB limit.");
        
        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var relativePath = $"/uploads/{folder}/{fileName}";
        var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
        
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);
        
        // Save file
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // TODO: Resize if maxWidth/maxHeight specified
        // Install SixLabors.ImageSharp NuGet package
        // Resize and optimize image
        
        _logger.LogInformation("Saved image: {RelativePath}", relativePath);
        return relativePath;
    }
    
    public async Task<bool> DeleteImageAsync(string relativePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted image: {RelativePath}", relativePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {RelativePath}", relativePath);
            return false;
        }
    }
    
    public string GetPhysicalPath(string relativePath)
    {
        return Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
    }
}
```

### **Step 2: Register Service**

```csharp
// Program.cs
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
```

### **Step 3: Update CommunityService**

```csharp
// Add to constructor
private readonly IFileStorageService _fileService;

public CommunityService(
    ApplicationDbContext context, 
    IMemoryCache cache, 
    ILogger<CommunityService> logger,
    IFileStorageService fileService)  // ✅ Add this
{
    _context = context;
    _cache = cache;
    _logger = logger;
    _fileService = fileService;  // ✅ Add this
}

// Update CreateCommunityAsync
public async Task<CreateCommunityResult> CreateCommunityAsync(CreateCommunityViewModel model)
{
    // Check if name already exists
    if (await _context.Communities.AnyAsync(c => c.Name == model.Name))
    {
        return new CreateCommunityResult
        {
            Success = false,
            ErrorMessage = "A community with this name already exists."
        };
    }

    // ✅ Process file uploads
    string? iconUrl = null;
    string? bannerUrl = null;
    
    if (model.IconFile != null)
    {
        iconUrl = await _fileService.SaveImageAsync(
            model.IconFile, 
            "communities/icons",
            256,  // Max width
            256   // Max height
        );
    }
    
    if (model.BannerFile != null)
    {
        bannerUrl = await _fileService.SaveImageAsync(
            model.BannerFile, 
            "communities/banners",
            1920, // Max width
            384   // Max height
        );
    }

    var slug = model.Name.ToSlug();
    var community = new Community
    {
        Name = model.Name,
        Slug = slug,
        Title = model.Title,
        Description = model.Description,
        ShortDescription = model.ShortDescription,
        CategoryId = model.CategoryId,
        CreatorId = model.CreatorId,
        CommunityType = model.CommunityType,
        IsNSFW = model.IsNSFW,
        Rules = model.Rules,
        ThemeColor = model.ThemeColor,
        IconUrl = iconUrl ?? model.IconUrl,      // ✅ Use uploaded file
        BannerUrl = bannerUrl ?? model.BannerUrl, // ✅ Use uploaded file
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    _context.Communities.Add(community);
    await _context.SaveChangesAsync();

    // Add creator as admin member
    var member = new CommunityMember
    {
        UserId = model.CreatorId!,
        CommunityId = community.CommunityId,
        Role = "admin",
        JoinedAt = DateTime.UtcNow
    };

    _context.CommunityMembers.Add(member);
    community.MemberCount = 1;
    await _context.SaveChangesAsync();

    return new CreateCommunityResult { Success = true, Slug = slug };
}
```

---

## 📋 **Checklist to Fix DiscussionSpot9**

### **Critical Fixes:**
- [ ] Create IFileStorageService interface
- [ ] Implement FileStorageService class
- [ ] Update CommunityService constructor
- [ ] Update CreateCommunityAsync method
- [ ] Update UpdateCommunityDetailsAsync method
- [ ] Test icon upload
- [ ] Test banner upload
- [ ] Test image preview
- [ ] Test file validation
- [ ] Test file size limits

### **Style Improvements:**
- [ ] Check if `/css/CustomStyles/V1/CreateCommunityStyle.css` exists
- [ ] Extract inline CSS from Details.cshtml to separate file
- [ ] Extract inline CSS from Index.cshtml to separate file
- [ ] Create `community-theme.css`
- [ ] Ensure dark mode compatibility
- [ ] Test responsive design

### **Optional Enhancements:**
- [ ] Add image resize (SixLabors.ImageSharp)
- [ ] Add image optimization
- [ ] Add file type validation (check actual file content, not just extension)
- [ ] Add preview before upload
- [ ] Add drag & drop upload
- [ ] Add crop functionality
- [ ] Add delete uploaded image button

---

## 🎉 **Conclusion**

### **DiscussionSpot9:**
- **Status:** 95% complete
- **Main Issue:** File upload not implemented
- **Fix Timeline:** 2-3 days
- **After Fix:** Fully functional community system! ✅

### **GSMSharing:**
- **Status:** 5% complete (database only)
- **Main Issue:** No UI built
- **Build Timeline:** 2 weeks
- **Current State:** Not usable

---

## 💡 **My Recommendation**

**Fix DiscussionSpot9 first!**

**Reasons:**
1. 95% done vs 5% done
2. Beautiful UI already exists
3. Just needs file upload (2-3 days)
4. Production-ready after fix
5. Can copy to GSMSharing later

**Timeline:**
- Day 1-2: Implement file upload
- Day 3: Fix styling
- Day 4: Test & polish
- **Result:** Complete community system! 🚀

**After that, you can:**
- Copy working code to GSMSharing
- Adapt for GSMSharing theme
- Much faster than building from scratch

---

**Ready to fix this?** Just say "start coding" and I'll:
1. Create FileStorageService
2. Update CommunityService  
3. Fix file upload
4. Clean up styles
5. Test everything

**Let's make communities perfect!** 🎯

---

*Analysis Date: October 19, 2025*  
*Projects Analyzed: DiscussionSpot9, GSMSharing*  
*Status: Ready to implement fixes*

