-- Migration: Add IsPinned and EditedAt columns to Comments table
-- Purpose: Support comment pinning and edit tracking features
-- Date: 2025-10-25

-- Add new columns to Comments table
ALTER TABLE Comments
ADD IsPinned BIT NOT NULL DEFAULT 0;

ALTER TABLE Comments
ADD EditedAt DATETIME NULL;

-- Note: IsEdited column already exists in the model, verify if it exists in database
-- If not, uncomment the following line:
-- ALTER TABLE Comments ADD IsEdited BIT NOT NULL DEFAULT 0;

-- Create index for pinned comments (performance optimization)
CREATE INDEX IX_Comments_IsPinned_PostId_CreatedAt 
ON Comments(PostId, IsPinned, CreatedAt DESC)
WHERE IsPinned = 1;

-- Add comment to track changes
PRINT 'Added IsPinned and EditedAt columns to Comments table';
PRINT 'Created index IX_Comments_IsPinned_PostId_CreatedAt';

-- Optional: Add constraint to ensure only one pinned comment per post
-- This is better handled in application logic, but you can add it here if needed
-- ALTER TABLE Comments
-- ADD CONSTRAINT CK_Comments_OnePinnedPerPost 
-- CHECK (IsPinned = 0 OR NOT EXISTS (
--     SELECT 1 FROM Comments c2 
--     WHERE c2.PostId = Comments.PostId 
--     AND c2.CommentId != Comments.CommentId 
--     AND c2.IsPinned = 1
-- ));

GO

