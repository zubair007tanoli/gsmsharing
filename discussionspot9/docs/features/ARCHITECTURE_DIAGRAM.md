# Comment Link Preview - Architecture Diagram

## System Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER POSTS COMMENT                       │
│              "Check out https://github.com"                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    COMMENT RENDERED IN HTML                      │
│         <div class="comment-text">...</div>                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│              JAVASCRIPT: comment-link-preview.js                 │
│                                                                   │
│  1. MutationObserver detects new comment                         │
│  2. Scan comment text for URLs                                   │
│  3. Regex: /(https?:\/\/[^\s<]+)|(www\.[^\s<]+)/gi              │
│  4. Find: ["https://github.com"]                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                   REPLACE URL WITH ICON                          │
│                                                                   │
│  Before: "Check out https://github.com"                          │
│  After:  "Check out 🔗"                                          │
│                                                                   │
│  HTML: <a class="comment-inline-link-icon">                      │
│          <i class="fas fa-link"></i>                             │
│        </a>                                                       │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                   FETCH METADATA FROM API                        │
│                                                                   │
│  POST /api/LinkMetadata/GetMetadata                              │
│  {                                                                │
│    "url": "https://github.com"                                   │
│  }                                                                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│           BACKEND: LinkMetadataController.cs                     │
│                                                                   │
│  1. Validate URL (HTTP/HTTPS only)                               │
│  2. Check rate limits (max 10 URLs)                              │
│  3. Call LinkMetadataService                                     │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│           BACKEND: LinkMetadataService.cs                        │
│                                                                   │
│  1. HTTP GET to https://github.com                               │
│  2. Parse HTML with HtmlAgilityPack                              │
│  3. Extract Open Graph metadata:                                 │
│     - og:title → "GitHub"                                        │
│     - og:description → "Where developers..."                     │
│     - og:image → "https://github.com/logo.png"                   │
│     - favicon → "https://github.com/favicon.ico"                 │
│  4. Return LinkPreviewViewModel                                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                   API RESPONSE (JSON)                            │
│                                                                   │
│  {                                                                │
│    "title": "GitHub",                                            │
│    "description": "Where developers shape the future...",        │
│    "url": "https://github.com",                                  │
│    "domain": "github.com",                                       │
│    "thumbnailUrl": "https://github.com/logo.png",                │
│    "faviconUrl": "https://github.com/favicon.ico"                │
│  }                                                                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│              JAVASCRIPT: Create Preview Card                     │
│                                                                   │
│  createLinkPreviewCard(metadata)                                 │
│  - Build HTML structure                                          │
│  - Add thumbnail image                                           │
│  - Add favicon and domain                                        │
│  - Add title and description                                     │
│  - Add "Visit link" footer                                       │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                   INSERT INTO DOM                                │
│                                                                   │
│  <div class="comment-link-previews">                             │
│    <a href="..." class="comment-link-preview-card">              │
│      <div class="comment-link-preview-container">                │
│        <div class="comment-link-preview-thumbnail">              │
│          <img src="logo.png" />                                  │
│        </div>                                                     │
│        <div class="comment-link-preview-content">                │
│          <div class="comment-link-preview-header">               │
│            <img src="favicon.ico" />                             │
│            <span>github.com</span>                               │
│          </div>                                                   │
│          <h4>GitHub</h4>                                         │
│          <p>Where developers...</p>                              │
│          <div class="comment-link-preview-footer">               │
│            <i class="fas fa-external-link-alt"></i>              │
│            <span>Visit link</span>                               │
│          </div>                                                   │
│        </div>                                                     │
│      </div>                                                       │
│    </a>                                                           │
│  </div>                                                           │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                   FINAL RENDERED RESULT                          │
│                                                                   │
│  User sees:                                                       │
│  "Check out 🔗"                                                  │
│                                                                   │
│  ┌─────────────────────────────────────┐                        │
│  │ [GitHub Logo] │ github.com          │                        │
│  │               │ GitHub              │                        │
│  │               │ Where developers... │                        │
│  │               │ → Visit link        │                        │
│  └─────────────────────────────────────┘                        │
└─────────────────────────────────────────────────────────────────┘
```

## Component Interaction Diagram

```
┌──────────────┐
│   Browser    │
│              │
│  ┌────────┐  │
│  │Comment │  │
│  │  Text  │  │
│  └───┬────┘  │
│      │       │
└──────┼───────┘
       │
       │ 1. Detect URL
       ▼
┌──────────────────────────┐
│  comment-link-preview.js │
│                          │
│  • URL Detection         │
│  • Text Replacement      │
│  • API Calls             │
│  • Card Generation       │
└──────────┬───────────────┘
           │
           │ 2. POST /api/LinkMetadata/GetMetadata
           ▼
┌──────────────────────────┐
│ LinkMetadataController   │
│                          │
│  • Validate URL          │
│  • Rate Limiting         │
│  • Call Service          │
└──────────┬───────────────┘
           │
           │ 3. GetMetadataAsync(url)
           ▼
┌──────────────────────────┐
│  LinkMetadataService     │
│                          │
│  • HTTP GET              │
│  • Parse HTML            │
│  • Extract Metadata      │
└──────────┬───────────────┘
           │
           │ 4. Return Metadata
           ▼
┌──────────────────────────┐
│  LinkPreviewViewModel    │
│                          │
│  • Title                 │
│  • Description           │
│  • ThumbnailUrl          │
│  • FaviconUrl            │
│  • Domain                │
└──────────┬───────────────┘
           │
           │ 5. Create Card HTML
           ▼
┌──────────────────────────┐
│  Preview Card Element    │
│                          │
│  • Styled with CSS       │
│  • Clickable Link        │
│  • Responsive Layout     │
└──────────┬───────────────┘
           │
           │ 6. Insert into DOM
           ▼
┌──────────────────────────┐
│  User Sees Preview       │
└──────────────────────────┘
```

## File Structure Diagram

```
discussionspot9/
│
├── Controllers/
│   └── Api/
│       └── LinkMetadataController.cs ⭐ NEW
│           ├── GetMetadata (POST)
│           └── GetMetadataBatch (POST)
│
├── Services/
│   └── LinkMetadataService.cs (existing)
│       └── GetMetadataAsync(url)
│
├── Models/
│   └── ViewModels/
│       └── CreativeViewModels/
│           └── LinkPreviewViewModel.cs (existing)
│
├── Views/
│   └── Post/
│       └── DetailTestPage.cshtml ⭐ MODIFIED
│           ├── Added: comment-link-preview.css
│           └── Added: comment-link-preview.js
│
├── wwwroot/
│   ├── css/
│   │   └── comment-link-preview.css ⭐ NEW
│   │       ├── .comment-link-preview-card
│   │       ├── .comment-inline-link-icon
│   │       └── Dark theme styles
│   │
│   └── js/
│       ├── CustomJs/
│       │   └── comment-link-preview.js ⭐ NEW
│       │       ├── processCommentLinks()
│       │       ├── fetchLinkMetadata()
│       │       ├── createLinkPreviewCard()
│       │       └── MutationObserver
│       │
│       └── SignalR_Script/
│           └── Post_Script_Real_Time_Fix.js ⭐ MODIFIED
│               └── Added: Link preview processing
│
└── docs/
    └── features/ ⭐ NEW
        ├── README.md
        ├── IMPLEMENTATION_SUMMARY.md
        ├── COMMENT_LINK_PREVIEW_FEATURE.md
        ├── COMMENT_LINK_PREVIEW_QUICK_START.md
        ├── COMMENT_LINK_PREVIEW_VISUAL_EXAMPLE.html
        ├── TESTING_SCRIPT.js
        └── ARCHITECTURE_DIAGRAM.md (this file)
```

## Data Flow Diagram

```
┌─────────┐
│  User   │
│ Input   │
└────┬────┘
     │
     │ "Check out https://github.com"
     ▼
┌─────────────────┐
│  Comment HTML   │
│  (Server-side)  │
└────┬────────────┘
     │
     │ <div class="comment-text">Check out https://github.com</div>
     ▼
┌─────────────────┐
│  JavaScript     │
│  Detection      │
└────┬────────────┘
     │
     │ URLs: ["https://github.com"]
     ▼
┌─────────────────┐
│  Text           │
│  Replacement    │
└────┬────────────┘
     │
     │ <a class="comment-inline-link-icon">🔗</a>
     ▼
┌─────────────────┐
│  API Request    │
│  (Frontend)     │
└────┬────────────┘
     │
     │ POST /api/LinkMetadata/GetMetadata
     ▼
┌─────────────────┐
│  Controller     │
│  (Backend)      │
└────┬────────────┘
     │
     │ Validate & Route
     ▼
┌─────────────────┐
│  Service        │
│  (Backend)      │
└────┬────────────┘
     │
     │ HTTP GET to external URL
     ▼
┌─────────────────┐
│  External Site  │
│  (github.com)   │
└────┬────────────┘
     │
     │ HTML Response
     ▼
┌─────────────────┐
│  Parse HTML     │
│  (Backend)      │
└────┬────────────┘
     │
     │ Extract Open Graph metadata
     ▼
┌─────────────────┐
│  ViewModel      │
│  (Backend)      │
└────┬────────────┘
     │
     │ JSON Response
     ▼
┌─────────────────┐
│  Create Card    │
│  (Frontend)     │
└────┬────────────┘
     │
     │ HTML Element
     ▼
┌─────────────────┐
│  Insert DOM     │
│  (Frontend)     │
└────┬────────────┘
     │
     │ Visible Preview Card
     ▼
┌─────────────────┐
│  User Sees      │
│  Result         │
└─────────────────┘
```

## Security Flow Diagram

```
┌─────────────┐
│ User Input  │
│   (URL)     │
└──────┬──────┘
       │
       ▼
┌──────────────────────┐
│  Frontend Validation │
│                      │
│  ✓ Regex Match       │
│  ✓ Basic Format      │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  API Request         │
│                      │
│  POST to Backend     │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  Backend Validation  │
│                      │
│  ✓ HTTP/HTTPS only   │
│  ✓ Well-formed URI   │
│  ✓ Rate limiting     │
│  ✓ Max 10 URLs       │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  Service Layer       │
│                      │
│  ✓ Timeout (10s)     │
│  ✓ SSRF Protection   │
│  ✓ HTML Sanitization │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  HTTP Request        │
│                      │
│  ✓ User-Agent Set    │
│  ✓ Timeout Applied   │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  Response Processing │
│                      │
│  ✓ Content-Type Check│
│  ✓ HTML Parsing      │
│  ✓ XSS Prevention    │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────┐
│  Return to Frontend  │
│                      │
│  ✓ Sanitized Data    │
│  ✓ Safe URLs         │
└──────────────────────┘
```

## Performance Optimization Diagram

```
┌─────────────────────┐
│  Multiple URLs      │
│  Detected           │
│  [URL1, URL2, URL3] │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  Batch Decision     │
│                     │
│  2-10 URLs?         │
│  ✓ Yes → Batch      │
│  ✗ No → Individual  │
└──────┬──────────────┘
       │
       ▼ (Batch)
┌─────────────────────┐
│  Single API Call    │
│                     │
│  POST /GetMetadata  │
│  Batch              │
│                     │
│  Saves: 2 requests  │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  Parallel Fetch     │
│                     │
│  Fetch all URLs     │
│  simultaneously     │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  Browser Cache      │
│                     │
│  Cache responses    │
│  for future use     │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  Lazy Load Images   │
│                     │
│  Load only when     │
│  visible            │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  Fast Display       │
│                     │
│  < 2s average       │
└─────────────────────┘
```

---

## Legend

- ⭐ NEW - Newly created file
- ⭐ MODIFIED - Modified existing file
- ✓ - Validation/Check passed
- ✗ - Validation/Check failed
- → - Data flow direction
- │ - Vertical connection
- ┌─┐ - Box/Component
- └─┘ - Box/Component bottom

---

**Date:** December 5, 2024
**Version:** 1.0
**Status:** ✅ Complete

