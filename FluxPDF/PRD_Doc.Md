Product Requirements Document (PRD): Next-Gen Hybrid PDF Platform
Document Metadata
Details
Project Name
Hybrid PDF Manipulation Platform (Codename: FluxDoc)
Version
1.0
Status
Approved for Development
Date
December 28, 2025
Primary Architecture
Hybrid: ASP.NET Core 10 (Web/API) + Python (Processing)

1. Executive Summary
The goal is to build a high-performance, enterprise-grade document intelligence solution that bridges the gap between static archival formats (PDF) and dynamic data structures (Excel, Word). Unlike standard converters, this platform leverages a hybrid architecture—combining the high throughput of ASP.NET Core 10 with the scientific data processing capabilities of Python—to deliver superior accuracy in table extraction, OCR, and image enhancement.
2. User Personas
To ensure the product meets real-world needs, features are prioritized based on four key personas:
"Finance Fiona" (Corporate Analyst): Needs precision in "PDF to Excel". Values accuracy over speed; requires handling of multi-page tables and merged cells without data loss.
"Student Sam" (Academic): Needs "Image to PDF" with "Magic Color" enhancement for lecture notes. Price-sensitive, mobile-first user.
"Archivist Alan" (Record Keeper): Needs bulk OCR (Searchable PDFs) for digitizing paper archives. Prioritizes data privacy and handling of large files.
"Dev Dave" (Integrator): Needs a reliable API to automate document merging from third-party systems.
3. Functional Requirements
3.1 Core Manipulation Features (Web & Mobile)
Fast, I/O-bound operations handled primarily by the orchestration layer.
ID
Feature
Description
Acceptance Criteria
FR-01
Intelligent PDF Merge
Combine multiple PDFs into one with drag-and-drop reordering.
• Processing time <2s for files <50MB.

• Preserves bookmarks/orientation.

• Supports page range selection (e.g., File A pgs 1-5 + File B).
FR-02
Image to PDF
Convert raster images (JPG, PNG, HEIC) to PDF.
• Auto-rotation based on EXIF.

• User-selectable page sizes (A4, Letter).

• Smart compression to prevent bloat.
FR-03
PDF to Images
Extract PDF pages as individual image files.
• Output: JPG/PNG.

• Selectable DPI (72 for screen, 300 for print).

3.2 Advanced Processing Features (Python Worker Cluster)
CPU-intensive tasks processed asynchronously via Redis queues.
ID
Feature
Description
Technical Implementation
FR-04
High-Fidelity PDF to Word
Convert PDF to editable .docx preserving layout.
Hybrid Engine: Uses pdf2docx for simple text streams and a headless LibreOffice instance for complex layouts.
FR-05
PDF to Excel (Smart Extract)
Extract tabular data to .xlsx.
Camelot Library:

• Lattice Mode: For tables with gridlines.

• Stream Mode: For whitespace-separated tables.

• Heuristic Router: Auto-detects best mode.
FR-06
Searchable PDF (OCR)
Convert scanned images to PDF/A text layers.
OCRmyPDF + Tesseract:

• Deskewing and noise cleaning.

• Skip-text optimization (ignores pages with existing text).

• Multi-language support.
FR-07
"Magic Color" Enhancement
Enhance photos of documents (remove shadows, whiten background).
OpenCV Pipeline:

• Edge detection (Canny) & Perspective Transform.

• Adaptive Gaussian Thresholding to handle uneven lighting.

3.3 Platform Services
ID
Feature
Details
FR-08
Authentication
OAuth2 (Google/Microsoft) + Email/Password via ASP.NET Identity.
FR-09
Storage Lifecycle
• Guest: 1-hour retention.

• Registered: 24-hour retention.

• Pro: Permanent "Vault" storage.
FR-10
Developer API
RESTful endpoints protected by API Keys. Rate limiting via token-bucket algorithm.

4. Non-Functional Requirements (NFR)
Performance: Web TTFB <200ms. Long-running job feedback (queue position) within 1s.
Scalability: Horizontal scaling of Python workers via Kubernetes KEDA based on Redis queue length.
Security: AES-256 encryption at rest (S3). TLS 1.3 for transit. Logical isolation of user data.
Availability: 99.9% uptime target. Graceful degradation (e.g., if OCR fails, Merge must still work).
5. Technical Architecture
5.1 Stack Overview
Frontend: ASP.NET Core MVC (Razor + HTMX) for Web; .NET MAUI for Mobile (Android/iOS).
API Gateway: ASP.NET Core 10.
Message Broker: Redis (Job Queue & Pub/Sub for progress updates).
Worker Nodes: Python 3.12+ containers (Dramatiq task queue).
Database: PostgreSQL (User data), Redis (State), S3/Blob Storage (Files).
5.2 Key Workflows
Direct-to-Storage Uploads: Client requests Presigned URL -> Uploads to S3 -> Notifies API. Prevents server memory exhaustion.
Async Processing: API accepts job -> Pushes to Redis -> Returns "202 Accepted" -> Python Worker picks up job -> Worker updates Redis -> SignalR pushes progress to Client.
6. Development Roadmap
Phase 1: Foundation (Months 1-2)
Focus: Infrastructure setup and Core MVP (Merge).
Infrastructure: Set up Kubernetes (AKS/EKS), Redis, and PostgreSQL.
Backend: Initialize ASP.NET Core 10 solution. Implement Auth (Identity) and S3 Presigned URL logic.
Worker: Create Python worker container with PyMuPDF for merging.
Frontend: Basic MVC interface with drag-and-drop file zone.
Milestone: Users can log in, upload 2 PDFs via S3, merge them, and download the result.
Phase 2: Intelligence (Months 3-4)
Focus: The "Heavy Lifting" features (OCR, Excel, Word).
OCR Engine: Integrate Tesseract and OCRmyPDF into the worker. Implement deskew/clean pipeline.
Table Extraction: Implement Camelot with Lattice/Stream routing logic.
Word Conversion: Deploy Headless LibreOffice sidecar container or service.
UX: Implement SignalR real-time progress bars ("Processing Page 3 of 10...").
Milestone: All extraction and conversion features fully functional on the web.
Phase 3: Mobile & Expansion (Months 5-6)
Focus: Mobile App and Image Enhancement.
Mobile App: Develop .NET MAUI application structure.
Native Scanning: Bind Google ML Kit (Android) and VisionKit (iOS) for edge detection and scanning.
Magic Color: Implement OpenCV adaptive thresholding pipeline in Python worker (for web uploads).
Sync: Build "My Documents" synchronization logic.
Milestone: Beta release of Mobile App with camera scanning and cloud sync.
Phase 4: Monetization & API (Month 7+)
Focus: Revenue generation and Third-party access.
Billing: Integrate Stripe for "Pro" subscriptions ($9/mo).
API Management: Implement API Key generation, usage tracking, and rate limiting middleware.
Documentation: Generate OpenAPI (Swagger) docs for external developers.
Milestone: Public launch of SaaS tiers and Developer API.
7. Monetization Strategy
Guest: No login, 2 tasks/day, 50MB limit.
Free Registered: 5 tasks/day, 100MB limit, Basic OCR (Low Priority).
Pro ($9/mo): Unlimited tasks, 2GB limit, High Priority OCR, Secure Vault.
API: Pay-per-use (per page processed).
