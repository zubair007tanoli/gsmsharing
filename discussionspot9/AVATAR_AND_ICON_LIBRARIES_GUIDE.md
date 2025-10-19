# 🎨 Avatar & Icon Libraries Guide for DiscussionSpot9

## 📚 **Best Libraries for Your Needs**

---

## 1️⃣ **DiceBear Avatars** ⭐ HIGHLY RECOMMENDED

### **Perfect For:**
- ✅ User profile avatars (auto-generated)
- ✅ Community placeholder icons
- ✅ Category icons
- ✅ Unique, colorful, professional

### **What It Is:**
DiceBear creates beautiful, unique avatars from usernames/IDs. No storage needed - generates via URL!

### **Example URLs:**
```
User Avatar (initials):
https://api.dicebear.com/7.x/initials/svg?seed=JohnDoe

User Avatar (fun styles):
https://api.dicebear.com/7.x/avataaars/svg?seed=user123
https://api.dicebear.com/7.x/bottts/svg?seed=user456
https://api.dicebear.com/7.x/personas/svg?seed=user789

Community Icons:
https://api.dicebear.com/7.x/shapes/svg?seed=Technology
https://api.dicebear.com/7.x/identicon/svg?seed=Gaming
```

### **Available Styles:**
- `initials` - Letter-based (like "JD" for John Doe)
- `avataaars` - Cartoon faces (customizable)
- `bottts` - Robot avatars
- `personas` - Modern illustrations
- `shapes` - Geometric patterns
- `identicon` - GitHub-style
- `pixel-art` - Retro gaming style
- `lorelei` - Neutral avatars
- `micah` - Diverse avatars
- `big-ears` - Playful characters

### **Pricing:**
- ✅ **FREE** for up to 100,000 requests/day
- ✅ No API key needed
- ✅ Fast CDN delivery
- ✅ SVG format (scalable, small)

### **Implementation:**
```csharp
// Helper method to generate avatar URL
public static string GetUserAvatarUrl(string userId, string? displayName = null, string style = "initials")
{
    var seed = displayName ?? userId;
    return $"https://api.dicebear.com/7.x/{style}/svg?seed={Uri.EscapeDataString(seed)}";
}

// Usage:
var avatarUrl = GetUserAvatarUrl("user123", "JohnDoe");
// Returns: https://api.dicebear.com/7.x/initials/svg?seed=JohnDoe
```

**Live Examples:**
- https://api.dicebear.com/7.x/initials/svg?seed=TechHub
- https://api.dicebear.com/7.x/shapes/svg?seed=Gaming&backgroundColor=0079d3
- https://api.dicebear.com/7.x/avataaars/svg?seed=User123

---

## 2️⃣ **UI Avatars** ⭐ SIMPLE & FREE

### **Perfect For:**
- ✅ Quick initials-based avatars
- ✅ User placeholders
- ✅ Simple, clean design

### **Example URL:**
```
https://ui-avatars.com/api/?name=John+Doe&background=0079d3&color=fff&size=128
```

### **Customization:**
```
Parameters:
- name: User's name
- background: Hex color (without #)
- color: Text color
- size: Image size in pixels
- font-size: Font size (0.1 to 1)
- rounded: true/false
- bold: true/false
```

### **Pricing:**
- ✅ **100% FREE**
- ✅ No limits
- ✅ No API key

### **Implementation:**
```csharp
public static string GetUIAvatar(string displayName, string backgroundColor = "0079d3", int size = 128)
{
    var name = Uri.EscapeDataString(displayName);
    return $"https://ui-avatars.com/api/?name={name}&background={backgroundColor}&color=fff&size={size}&rounded=true&bold=true";
}
```

---

## 3️⃣ **Boring Avatars** ⭐ MODERN & MINIMAL

### **Perfect For:**
- ✅ Modern geometric avatars
- ✅ Unique patterns
- ✅ Professional look

### **Example:**
```
https://source.boringavatars.com/beam/120/user123?colors=264653,2a9d8f,e9c46a,f4a261,e76f51
```

### **Styles:**
- `beam` - Geometric beams
- `bauhaus` - Bauhaus-inspired
- `ring` - Concentric rings
- `pixel` - Pixel art
- `sunset` - Gradient patterns
- `marble` - Marbled patterns

### **Pricing:**
- ✅ **FREE**
- ✅ Open source
- ✅ Self-hostable

---

## 4️⃣ **Gravatar** 📧 EMAIL-BASED

### **Perfect For:**
- ✅ Users who have Gravatar accounts
- ✅ WordPress/blog users
- ✅ Fallback avatars

### **How It Works:**
```csharp
public static string GetGravatarUrl(string email, int size = 128)
{
    var hash = CreateMD5Hash(email.Trim().ToLower());
    return $"https://www.gravatar.com/avatar/{hash}?s={size}&d=identicon";
}

private static string CreateMD5Hash(string input)
{
    using var md5 = System.Security.Cryptography.MD5.Create();
    var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
    var hashBytes = md5.ComputeHash(inputBytes);
    return Convert.ToHexString(hashBytes).ToLower();
}
```

### **Fallback Options (d parameter):**
- `404` - Return 404 if no avatar
- `mp` - Mystery person (default)
- `identicon` - Geometric pattern
- `monsterid` - Monster avatar
- `wavatar` - Generated faces
- `retro` - 8-bit style
- `robohash` - Robot avatars
- `blank` - Transparent

---

## 5️⃣ **Font Awesome Icons** 🎨 COMPREHENSIVE

### **Perfect For:**
- ✅ Category icons
- ✅ UI elements
- ✅ Action buttons
- ✅ Status indicators

### **You Already Have This!**
Check your layout - you're likely using Font Awesome already.

### **Category Icon Examples:**
```html
<i class="fas fa-laptop-code"></i> Technology
<i class="fas fa-gamepad"></i> Gaming
<i class="fas fa-flask"></i> Science
<i class="fas fa-palette"></i> Arts
<i class="fas fa-football-ball"></i> Sports
<i class="fas fa-music"></i> Music
<i class="fas fa-film"></i> Movies
<i class="fas fa-book"></i> Books
<i class="fas fa-utensils"></i> Food
<i class="fas fa-plane"></i> Travel
```

### **Pricing:**
- ✅ **FREE** (6,000+ icons)
- 💰 Pro ($99/year for 30,000+ icons)

---

## 6️⃣ **Heroicons** 🎯 MODERN SVG

### **Perfect For:**
- ✅ Clean, modern UI icons
- ✅ Tailwind CSS projects
- ✅ Minimalist design

### **Installation:**
```bash
npm install @heroicons/react
```

### **Usage:**
```jsx
import { UserCircleIcon } from '@heroicons/react/24/outline'
<UserCircleIcon className="h-6 w-6" />
```

### **CDN (No Install):**
```html
<!-- Use SVG directly -->
<svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
</svg>
```

---

## 7️⃣ **Multiavatar** 🎭 UNIQUE AVATARS

### **Perfect For:**
- ✅ Unique user avatars
- ✅ Deterministic (same input = same avatar)
- ✅ 12 billion+ unique combinations

### **Example:**
```
https://api.multiavatar.com/user123.svg
https://api.multiavatar.com/JohnDoe.png?size=128
```

### **Pricing:**
- ✅ **FREE**
- ✅ No API key
- ✅ SVG or PNG

---

## 8️⃣ **RoboHash** 🤖 FUN ROBOTS

### **Perfect For:**
- ✅ Fun, playful avatars
- ✅ Gaming communities
- ✅ Tech communities

### **Example:**
```
https://robohash.org/user123
https://robohash.org/user456?set=set2
https://robohash.org/user789?set=set4
```

### **Sets Available:**
- set1: Robots
- set2: Monsters
- set3: Robot heads
- set4: Cats
- set5: Humans

### **Pricing:**
- ✅ **100% FREE**
- ✅ Unlimited

---

## 📊 **Comparison Table**

| Library | Best For | Free? | Customizable | Style |
|---------|----------|-------|--------------|-------|
| **DiceBear** ⭐ | Everything! | ✅ Yes | ✅✅✅ High | Modern, varied |
| **UI Avatars** | Initials | ✅ Yes | ✅✅ Medium | Simple, clean |
| **Boring Avatars** | Geometric | ✅ Yes | ✅ Low | Abstract, modern |
| **Gravatar** | Email users | ✅ Yes | ✅ Low | Various fallbacks |
| **Font Awesome** | UI Icons | ✅ Yes | ✅✅ Medium | Professional |
| **Heroicons** | Modern UI | ✅ Yes | ✅ Low | Minimal, clean |
| **Multiavatar** | Unique faces | ✅ Yes | ✅ Low | Cartoon, diverse |
| **RoboHash** | Fun/Gaming | ✅ Yes | ✅ Medium | Playful, robots |

---

## 🎯 **My Recommendation for DiscussionSpot9**

### **Best Combination:**

1. **For User Avatars:**
   - **Primary:** DiceBear (initials or avataaars style)
   - **Fallback:** UI Avatars
   - **Why:** Automatic, unique, professional

2. **For Community Icons:**
   - **Option A:** DiceBear shapes/identicon
   - **Option B:** Let users upload (you have this now!)
   - **Fallback:** First letter in colored circle

3. **For Category Icons:**
   - **Font Awesome** icons
   - **Why:** You already have it, comprehensive

4. **For UI Elements:**
   - **Font Awesome** (main icons)
   - **Heroicons** (optional, if you want cleaner look)

---

## 💻 **Implementation for DiscussionSpot9**

### **Create Avatar Helper Class:**

```csharp
// Helpers/AvatarHelper.cs
using System.Security.Cryptography;
using System.Text;

namespace discussionspot9.Helpers
{
    public static class AvatarHelper
    {
        /// <summary>
        /// Gets avatar URL for a user. Returns uploaded avatar or generates one.
        /// </summary>
        public static string GetUserAvatarUrl(string? uploadedAvatarUrl, string userId, string? displayName, string style = "initials")
        {
            // If user has uploaded avatar, use it
            if (!string.IsNullOrEmpty(uploadedAvatarUrl))
                return uploadedAvatarUrl;

            // Otherwise generate one from DiceBear
            var seed = displayName ?? userId;
            return $"https://api.dicebear.com/7.x/{style}/svg?seed={Uri.EscapeDataString(seed)}";
        }

        /// <summary>
        /// Gets community icon URL. Returns uploaded icon or generates one.
        /// </summary>
        public static string GetCommunityIconUrl(string? uploadedIconUrl, string communityName, string? themeColor = null)
        {
            if (!string.IsNullOrEmpty(uploadedIconUrl))
                return uploadedIconUrl;

            var backgroundColor = themeColor?.TrimStart('#') ?? "0079d3";
            return $"https://api.dicebear.com/7.x/shapes/svg?seed={Uri.EscapeDataString(communityName)}&backgroundColor={backgroundColor}";
        }

        /// <summary>
        /// Gets UI Avatar (simple initials-based)
        /// </summary>
        public static string GetUIAvatar(string name, string? backgroundColor = null, int size = 128)
        {
            var bg = backgroundColor?.TrimStart('#') ?? "0079d3";
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background={bg}&color=fff&size={size}&rounded=true&bold=true";
        }

        /// <summary>
        /// Gets Gravatar URL from email
        /// </summary>
        public static string GetGravatarUrl(string email, int size = 128, string defaultStyle = "identicon")
        {
            var hash = CreateMD5Hash(email.Trim().ToLower());
            return $"https://www.gravatar.com/avatar/{hash}?s={size}&d={defaultStyle}";
        }

        /// <summary>
        /// Gets category icon class from Font Awesome
        /// </summary>
        public static string GetCategoryIconClass(string categoryName)
        {
            return categoryName.ToLower() switch
            {
                "technology" or "tech" => "fas fa-laptop-code",
                "gaming" or "games" => "fas fa-gamepad",
                "science" => "fas fa-flask",
                "arts" or "art" or "creative" => "fas fa-palette",
                "sports" => "fas fa-football-ball",
                "music" => "fas fa-music",
                "movies" or "tv" or "film" => "fas fa-film",
                "books" or "reading" => "fas fa-book",
                "food" or "cooking" => "fas fa-utensils",
                "travel" => "fas fa-plane",
                "business" => "fas fa-briefcase",
                "education" or "learning" => "fas fa-graduation-cap",
                "health" or "fitness" => "fas fa-heartbeat",
                "photography" => "fas fa-camera",
                "programming" or "coding" => "fas fa-code",
                "design" => "fas fa-pencil-ruler",
                "nature" or "outdoors" => "fas fa-tree",
                "pets" or "animals" => "fas fa-paw",
                "cars" or "automotive" => "fas fa-car",
                "fashion" or "style" => "fas fa-tshirt",
                _ => "fas fa-comments"  // Default
            };
        }

        private static string CreateMD5Hash(string input)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
```

---

## 📝 **Usage in Views**

### **User Profile Avatar:**

```html
@{
    var avatarUrl = AvatarHelper.GetUserAvatarUrl(
        Model.ProfilePhotoUrl,  // Uploaded photo (or null)
        Model.UserId,
        Model.DisplayName,
        "initials"  // or "avataaars" for fun style
    );
}

<img src="@avatarUrl" alt="@Model.DisplayName" class="user-avatar" />
```

### **Community Icon:**

```html
@{
    var iconUrl = AvatarHelper.GetCommunityIconUrl(
        Model.IconUrl,  // Uploaded icon (or null)
        Model.CommunityName,
        Model.ThemeColor
    );
}

<img src="@iconUrl" alt="@Model.CommunityName" class="community-icon" />
```

### **Category Icon:**

```html
<i class="@AvatarHelper.GetCategoryIconClass(category.Name)"></i>
<span>@category.Name</span>
```

---

## 🎨 **DiceBear Style Examples**

### **Initials (Best for Professional Look):**
```
John Doe → "JD" in colored circle
https://api.dicebear.com/7.x/initials/svg?seed=JohnDoe&backgroundColor=0079d3
```

### **Avataaars (Fun Cartoon Faces):**
```
Unique cartoon face based on name
https://api.dicebear.com/7.x/avataaars/svg?seed=user123
```

### **Shapes (Good for Communities):**
```
Abstract geometric patterns
https://api.dicebear.com/7.x/shapes/svg?seed=Technology&backgroundColor=ff4500
```

### **Bottts (Robot Avatars):**
```
Unique robot designs
https://api.dicebear.com/7.x/bottts/svg?seed=user456
```

### **Personas (Modern Minimal):**
```
Clean, modern faces
https://api.dicebear.com/7.x/personas/svg?seed=user789
```

---

## 🔧 **Implementation Steps for DiscussionSpot9**

### **Step 1: Create Helper Class**

Create file: `Helpers/AvatarHelper.cs` (see code above)

### **Step 2: Update ViewModels**

Add helper method to your view models or create extension:

```csharp
// Models/ViewModels/Extensions/AvatarExtensions.cs
public static class AvatarExtensions
{
    public static string GetAvatarUrl(this UserProfile profile)
    {
        return AvatarHelper.GetUserAvatarUrl(
            profile.ProfilePhotoUrl,
            profile.UserId,
            profile.DisplayName
        );
    }

    public static string GetIconUrl(this CommunityDetailViewModel community)
    {
        return AvatarHelper.GetCommunityIconUrl(
            community.IconUrl,
            community.Name,
            community.ThemeColor
        );
    }
}
```

### **Step 3: Update Views**

**Before:**
```html
<img src="@Model.ProfilePhotoUrl" alt="Avatar" />
```

**After:**
```html
<img src="@AvatarHelper.GetUserAvatarUrl(Model.ProfilePhotoUrl, Model.UserId, Model.DisplayName)" 
     alt="@Model.DisplayName" />
```

Or with extension:
```html
<img src="@Model.GetAvatarUrl()" alt="@Model.DisplayName" />
```

---

## 🎨 **Category Icon Mapping**

### **Pre-defined Icons for Categories:**

```csharp
public static Dictionary<string, (string Icon, string Color)> CategoryIcons = new()
{
    ["Technology"] = ("fas fa-laptop-code", "#0079d3"),
    ["Gaming"] = ("fas fa-gamepad", "#7c3aed"),
    ["Science"] = ("fas fa-flask", "#10b981"),
    ["Arts"] = ("fas fa-palette", "#f59e0b"),
    ["Sports"] = ("fas fa-football-ball", "#ef4444"),
    ["Music"] = ("fas fa-music", "#ec4899"),
    ["Movies"] = ("fas fa-film", "#8b5cf6"),
    ["Books"] = ("fas fa-book", "#14b8a6"),
    ["Food"] = ("fas fa-utensils", "#f97316"),
    ["Travel"] = ("fas fa-plane", "#06b6d4"),
    ["Business"] = ("fas fa-briefcase", "#64748b"),
    ["Education"] = ("fas fa-graduation-cap", "#3b82f6"),
    ["Health"] = ("fas fa-heartbeat", "#ef4444"),
    ["Photography"] = ("fas fa-camera", "#8b5cf6"),
    ["Programming"] = ("fas fa-code", "#22c55e"),
    ["Design"] = ("fas fa-pencil-ruler", "#f59e0b"),
    ["Nature"] = ("fas fa-tree", "#10b981"),
    ["Pets"] = ("fas fa-paw", "#f59e0b"),
    ["Automotive"] = ("fas fa-car", "#64748b"),
    ["Fashion"] = ("fas fa-tshirt", "#ec4899")
};

// Usage:
var (icon, color) = CategoryIcons.GetValueOrDefault(categoryName, ("fas fa-comments", "#0079d3"));
```

---

## 🚀 **Quick Implementation**

### **Minimal Setup (5 minutes):**

Just add this helper and use generated avatars:

```csharp
// In your existing Helpers folder
public static class QuickAvatarHelper
{
    public static string GetAvatar(string? uploadedUrl, string seedText, string style = "initials")
    {
        return string.IsNullOrEmpty(uploadedUrl) 
            ? $"https://api.dicebear.com/7.x/{style}/svg?seed={Uri.EscapeDataString(seedText)}"
            : uploadedUrl;
    }
}
```

**Then in views:**
```html
<img src="@QuickAvatarHelper.GetAvatar(user.AvatarUrl, user.DisplayName)" />
```

**Done!** No storage needed, automatic fallbacks!

---

## 💰 **Cost Comparison**

| Solution | Cost | Storage Needed | Bandwidth | Maintenance |
|----------|------|----------------|-----------|-------------|
| **Upload + DiceBear Fallback** | FREE | Your server | Your server | Low |
| **DiceBear Only** | FREE | None | None | None |
| **UI Avatars** | FREE | None | None | None |
| **Gravatar** | FREE | None | None | None |
| **Font Awesome Free** | FREE | None | CDN | None |
| **Font Awesome Pro** | $99/yr | None | CDN | None |

---

## 🎯 **Recommended Setup for DiscussionSpot9**

### **Best Approach:**

```
User Avatars:
├── User uploaded photo (if exists)
└── DiceBear initials (auto-generated fallback)

Community Icons:
├── Uploaded icon (if exists)
└── DiceBear shapes (auto-generated fallback)

Category Icons:
└── Font Awesome icons (static mapping)

UI Icons:
└── Font Awesome (buttons, actions, etc.)
```

### **Benefits:**
- ✅ Users can upload custom avatars
- ✅ Automatic beautiful fallbacks
- ✅ No storage for defaults
- ✅ Unique per user/community
- ✅ Professional appearance
- ✅ Zero maintenance

---

## 📦 **Complete Implementation Package**

### **Option 1: Just Use Generated Avatars (Easiest)**

**Pros:**
- ✅ No storage needed
- ✅ Instant setup (5 minutes)
- ✅ Unique avatars automatically
- ✅ Zero maintenance

**Cons:**
- ❌ Users can't upload custom photos

**Best for:** MVP, quick launch

---

### **Option 2: Upload + Generated Fallback** ⭐ RECOMMENDED

**Pros:**
- ✅ Users can upload custom avatars
- ✅ Beautiful fallbacks for those who don't
- ✅ Best of both worlds
- ✅ Professional appearance

**Cons:**
- Need storage (you have this now!)
- Slightly more complex

**Best for:** Production, user satisfaction

---

### **Option 3: Upload Only**

**Pros:**
- ✅ Full user control
- ✅ Custom branding

**Cons:**
- ❌ Users without avatars look bad
- ❌ Need default placeholder image
- ❌ More storage needed

**Best for:** Professional platforms, brands

---

## 📝 **Sample Code for Your Project**

### **Create This File:**

`discussionspot9/Helpers/AvatarHelper.cs` - Full implementation ready to use!

### **Update Your Views:**

**In _Layout.cshtml or Header component:**
```html
<!-- User avatar in navbar -->
@if (User.Identity.IsAuthenticated)
{
    var avatarUrl = AvatarHelper.GetUserAvatarUrl(
        userProfile?.ProfilePhotoUrl,
        userId,
        userProfile?.DisplayName ?? User.Identity.Name
    );
    
    <img src="@avatarUrl" 
         alt="@userProfile?.DisplayName" 
         class="rounded-circle"
         style="width: 32px; height: 32px; object-fit: cover;" />
}
```

**In Community cards:**
```html
@{
    var iconUrl = AvatarHelper.GetCommunityIconUrl(
        community.IconUrl,
        community.Name,
        community.ThemeColor
    );
}

<img src="@iconUrl" alt="@community.Name" class="community-icon" />
```

---

## 🎁 **Bonus: Avatar Upload Feature**

### **For User Profiles (Future Enhancement):**

```csharp
// In UserService or ProfileService
public async Task<string> UpdateUserAvatarAsync(string userId, IFormFile avatarFile)
{
    // Validate image
    var validation = _fileStorageService.ValidateFile(avatarFile, new[] { ".jpg", ".png" }, 5);
    if (!validation.IsValid)
        throw new Exception(validation.ErrorMessage);

    // Save avatar (resize to 256x256)
    var avatarUrl = await _fileStorageService.SaveImageAsync(
        avatarFile, 
        "users/avatars",
        256,  // max width
        256   // max height
    );

    // Update user profile
    var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    if (profile != null)
    {
        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(profile.ProfilePhotoUrl))
        {
            await _fileStorageService.DeleteFileAsync(profile.ProfilePhotoUrl);
        }

        profile.ProfilePhotoUrl = avatarUrl;
        await _context.SaveChangesAsync();
    }

    return avatarUrl;
}
```

---

## 🌟 **Live Examples You Can Test**

### **Try These URLs in Browser:**

**DiceBear Initials:**
```
https://api.dicebear.com/7.x/initials/svg?seed=John+Doe&backgroundColor=0079d3
```

**DiceBear Avatar (Cartoon):**
```
https://api.dicebear.com/7.x/avataaars/svg?seed=happyuser
```

**DiceBear Shapes (Community Icons):**
```
https://api.dicebear.com/7.x/shapes/svg?seed=Technology&backgroundColor=1a73e8
```

**UI Avatars:**
```
https://ui-avatars.com/api/?name=Discussion+Spot&background=6366f1&color=fff&size=128&rounded=true&bold=true
```

**RoboHash (Fun):**
```
https://robohash.org/discussionspot?set=set1&size=200x200
```

**Multiavatar:**
```
https://api.multiavatar.com/DiscussionSpot.svg
```

---

## ✅ **Action Plan**

### **Immediate (15 minutes):**

1. ✅ Create `Helpers/AvatarHelper.cs`
2. ✅ Add to your project
3. ✅ Use in navbar for user avatars
4. ✅ Use in community cards
5. ✅ Test in browser

### **Optional (Later):**

- Add avatar upload to user profile page
- Add avatar cropping tool
- Cache avatar URLs
- Add avatar customization options
- Let users choose avatar style

---

## 🎯 **My Recommendation**

**Use DiceBear + Your Upload System:**

1. **When user uploads:** Use their photo
2. **When no upload:** Generate with DiceBear
3. **Result:** Everyone has beautiful avatar!

**Code:**
```csharp
var avatarUrl = uploadedPhoto ?? DiceBearUrl(userName);
```

**Simple, effective, professional!** ✅

---

**Want me to implement AvatarHelper.cs right now?** Just say "implement avatar helper" and I'll add it to your project! 🚀

---

*Guide Created: October 19, 2025*  
*Recommendation: DiceBear + FileStorageService*  
*Best for: Professional avatars with zero maintenance*

