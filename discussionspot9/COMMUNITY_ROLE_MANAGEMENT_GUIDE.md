# 👑 Community Role Management Guide

## How to Set Roles for Community Members

---

## 🎯 **Quick Answer**

### **As Community Admin:**

1. **Go to your community:**
   ```
   http://localhost:5099/r/{your-community-slug}
   ```

2. **Click "Community Settings" button** (visible only if you're admin/moderator)

3. **You'll see the Settings page with:**
   - List of current moderators
   - "Load Members" button to see all members
   - Dropdown actions to change roles

---

## 📋 **Step-by-Step Guide**

### **Step 1: Access Community Settings**

Navigate to any community you created or admin:
```
http://localhost:5099/r/gsmsharing/settings
```

**Requirements:**
- You must be logged in
- You must be community admin or moderator
- If you created the community, you're automatically the admin

### **Step 2: Load Community Members**

1. Click the **"Load Members"** button
2. You'll see a list of all community members with:
   - User avatars
   - Display names
   - Join dates
   - Current roles (badges)
   - Action buttons

### **Step 3: Change a Member's Role**

**Option A: From Moderators Section**
- Click **"Change Role"** button next to a moderator
- Select new role from dropdown:
  - 👤 **Member** - Regular member (no special permissions)
  - 🛡️ **Moderator** - Can moderate content, ban members
  - 👑 **Admin** - Full community control
- Click "Change Role" to confirm

**Option B: From Members List**
- After clicking "Load Members"
- Click action button next to any member
- Select role from dropdown
- Confirm the change

---

## 🎭 **Community Roles Explained**

### **👑 Admin** (Highest Level)
**Permissions:**
- ✅ Change community settings
- ✅ Assign moderators
- ✅ Promote members to admin
- ✅ Ban/unban members from community
- ✅ Delete posts and comments
- ✅ Change member roles
- ✅ Full community control

**How to Assign:**
- Only community admins can promote to admin
- From settings: Click member → "Promote to Admin"

### **🛡️ Moderator** (Middle Level)
**Permissions:**
- ✅ Ban/unban members from community
- ✅ Delete spam posts and comments
- ✅ Pin/unpin posts
- ✅ Lock/unlock discussions
- ❌ Cannot change community settings
- ❌ Cannot assign other moderators

**How to Assign:**
- Community admins can promote anyone
- From settings: Click member → "Promote to Moderator"

### **👤 Member** (Regular Level)
**Permissions:**
- ✅ Post content
- ✅ Comment on posts
- ✅ Vote on posts/comments
- ✅ Join/leave community
- ❌ No moderation powers

**Default Role:**
- All users who join get "member" role automatically

---

## 🔧 **How to Use the API**

### **Change Member Role (Programmatic)**

**Endpoint:**
```
POST /api/community/change-member-role
```

**Request Body:**
```json
{
  "communityId": 5,
  "userId": "user-id-here",
  "newRole": "moderator"
}
```

**Valid Roles:**
- `"member"`
- `"moderator"`
- `"admin"`

**Example JavaScript:**
```javascript
async function promoteToModerator(communityId, userId) {
    const response = await fetch('/api/community/change-member-role', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            communityId: communityId,
            userId: userId,
            newRole: 'moderator'
        })
    });
    
    const result = await response.json();
    if (result.success) {
        alert('User promoted to moderator!');
    }
}
```

---

## 🎨 **UI Walkthrough**

### **What You'll See:**

```
┌────────────────────────────────────────────────┐
│  ⚙️ COMMUNITY SETTINGS: r/gsmsharing           │
│  Manage your community settings, members, and  │
│  moderation                                    │
└────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│ 🛡️ CURRENT MODERATORS                          │
│                                                │
│ 👤 John Doe                [🛡️ MODERATOR]      │
│    Moderator               [Change Role ▼]    │
│                                                │
│ 👤 Jane Smith              [👑 ADMIN]          │
│    Moderator               [Change Role ▼]    │
└────────────────────────────────────────────────┘

┌────────────────────────────────────────────────┐
│ 👥 MEMBER MANAGEMENT                           │
│                                                │
│ 🔍 [Search members...]     [Load Members]     │
│                                                │
│ Click "Load Members" to view all members      │
└────────────────────────────────────────────────┘
```

**After clicking "Load Members":**

```
┌────────────────────────────────────────────────┐
│ 👤 Alice Brown   [👤 MEMBER]  [Change] [Ban]  │
│ 👤 Bob Wilson    [👤 MEMBER]  [Change] [Ban]  │
│ 👤 Charlie Lee   [🛡️ MOD]     [Change] [Ban]  │
└────────────────────────────────────────────────┘
```

---

## 💡 **Common Tasks**

### **Task 1: Promote Member to Moderator**
1. Go to `/r/{slug}/settings`
2. Click "Load Members"
3. Find the user
4. Click "Change Role" button
5. Select "🛡️ Moderator"
6. Click "Change Role"
7. Done! User is now moderator

### **Task 2: Make Someone Co-Admin**
1. Go to `/r/{slug}/settings`
2. Click "Change Role" on a moderator
3. Select "👑 Admin"
4. Confirm
5. They now have full community control

### **Task 3: Demote a Moderator**
1. Go to `/r/{slug}/settings`
2. Find the moderator in the list
3. Click "Change Role"
4. Select "👤 Member"
5. Confirm
6. They're now a regular member

### **Task 4: Remove Someone's Permissions**
1. Go to `/r/{slug}/settings`
2. Click "Load Members"
3. Find the user
4. Click dropdown → "Ban from Community"
5. Enter reason
6. They're removed and banned

---

## 🔒 **Permissions Required**

### **Who Can Change Roles:**
- ✅ Community **Admins** - Can change anyone's role
- ✅ Community **Moderators** - Can promote members to moderator (if admin allows)
- ❌ Regular members - Cannot change roles

### **Who Can Ban:**
- ✅ Community **Admins** - Can ban anyone
- ✅ Community **Moderators** - Can ban members
- ❌ Regular members - Cannot ban

### **Protection:**
- ❌ Cannot change your own role
- ❌ Moderators cannot demote admins
- ❌ Cannot promote someone higher than yourself

---

## 🎯 **Quick Access Checklist**

To manage community roles, you need:

- [ ] Be logged in
- [ ] Be community admin or moderator
- [ ] Navigate to `/r/{slug}/settings`
- [ ] Click "Load Members" to see everyone
- [ ] Use "Change Role" button to promote/demote
- [ ] All changes are logged in moderation system

---

## 📊 **Role Change Examples**

### **Example 1: Build a Moderation Team**
```
1. Create community "Technology"
2. You're automatically admin
3. Go to /r/technology/settings
4. Load members
5. Promote Alice → Moderator
6. Promote Bob → Moderator
7. Promote Charlie → Admin (co-admin)
```

Now you have a team!
- You: Admin
- Charlie: Admin (co-admin)
- Alice: Moderator
- Bob: Moderator

### **Example 2: Replace a Moderator**
```
1. Go to /r/technology/settings
2. Find old moderator
3. Click "Change Role" → Select "Member"
4. Find new person
5. Click "Change Role" → Select "Moderator"
```

---

## 🚀 **Testing the Feature**

### **Test Right Now:**

1. **Navigate to**:
   ```
   http://localhost:5099/r/gsmsharing/settings
   ```

2. **You should see**:
   - Community info section
   - Moderators section
   - Member management section

3. **Click "Load Members"**:
   - Fetches all community members via AJAX
   - Displays with role badges
   - Shows action buttons

4. **Click "Change Role"**:
   - Modal opens
   - Select new role
   - Confirm
   - Role updated!

---

## 🎊 **Success!**

**How to Access**: `http://localhost:5099/r/{slug}/settings`  
**Who Can Access**: Community admins and moderators  
**What You Can Do**: Change roles, ban members, manage community  

**Your community role management system is fully functional!** 🚀

---

## 📞 **Quick Help**

**Q: I don't see "Community Settings" button**  
**A:** You must be community admin/moderator. If you created the community, you're automatically admin.

**Q: How do I become admin of a community?**  
**A:** Either create the community, or have current admin promote you.

**Q: Can I have multiple admins?**  
**A:** Yes! Admins can promote other members to admin.

**Q: What's the difference between admin and moderator?**  
**A:** Admins can change settings and assign roles. Moderators can only moderate content.

---

**Everything you need to manage community roles is ready!** 🎉

