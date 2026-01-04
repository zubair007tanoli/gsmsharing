# Fluxdox

Fluxdox is the web frontend for the FluxDoc hybrid PDF manipulation platform.

## PRD Summary (Core MVP)

- Feature: Intelligent PDF Merge (FR-01)
- Acceptance: Drag-and-drop UI, reordering, page range selection, merge via presigned upload and background worker
- Architecture: ASP.NET Core 10 frontend + Redis job queue + Python worker (merge)

## Local Development

- Configure S3-compatible storage (MinIO recommended), Redis, and PostgreSQL for full functionality.
- App configuration keys: `Storage:Bucket`, `AWS:AccessKey`, `AWS:Secret`, `Redis:Connection`, `Database:ConnectionString`.

## Roadmap

1. Implement core merge UI and presigned upload endpoint
2. Add Redis job queue and Python worker container
3. Add authentication and quotas
