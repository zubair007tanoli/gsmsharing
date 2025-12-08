// Comments Management JavaScript

function loadComments(postId) {
    fetch(`/Comments/GetByPost?postId=${postId}`)
        .then(response => response.json())
        .then(comments => {
            const commentsList = document.getElementById('comments-list');
            const commentCount = document.getElementById('comment-count');
            
            if (comments.length === 0) {
                commentsList.innerHTML = '<p class="text-muted text-center py-4">No comments yet. Be the first to comment!</p>';
                commentCount.textContent = '0';
                return;
            }
            
            commentCount.textContent = comments.length;
            commentsList.innerHTML = comments.map(comment => renderComment(comment)).join('');
            
            // Attach event listeners
            attachCommentListeners();
        })
        .catch(error => {
            console.error('Error loading comments:', error);
        });
}

function renderComment(comment) {
    const repliesHtml = comment.replies && comment.replies.length > 0
        ? `<div class="comment-nested">${comment.replies.map(reply => renderComment(reply)).join('')}</div>`
        : '';
    
    return `
        <div class="comment-card" data-comment-id="${comment.commentID}">
            <div class="comment-header">
                <div class="comment-avatar">${comment.userName ? comment.userName.substring(0, 1).toUpperCase() : 'U'}</div>
                <div>
                    <div class="comment-author">${comment.userName || 'Anonymous'}</div>
                    <div class="comment-date">${formatDate(comment.createdAt)}</div>
                </div>
            </div>
            <div class="comment-content">${escapeHtml(comment.content)}</div>
            <div class="comment-actions">
                <button class="comment-action-btn reply-btn" data-comment-id="${comment.commentID}">
                    <i class="fas fa-reply me-1"></i>Reply
                </button>
                <div class="reactions-container" data-comment-id="${comment.commentID}"></div>
            </div>
            ${repliesHtml}
        </div>
    `;
}

function attachCommentListeners() {
    // Reply buttons
    document.querySelectorAll('.reply-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const commentId = this.dataset.commentId;
            showReplyForm(commentId);
        });
    });
}

function showReplyForm(parentCommentId) {
    const formHtml = `
        <div class="comment-card mt-3" id="reply-form-${parentCommentId}">
            <form class="reply-form" data-parent-id="${parentCommentId}">
                <input type="hidden" name="PostID" value="${document.querySelector('[name="PostID"]').value}">
                <input type="hidden" name="ParentCommentID" value="${parentCommentId}">
                <div class="mb-3">
                    <textarea name="Content" class="form-control" rows="3" placeholder="Write a reply..." required></textarea>
                </div>
                <div class="d-flex gap-2">
                    <button type="submit" class="btn btn-primary btn-sm">Post Reply</button>
                    <button type="button" class="btn btn-secondary btn-sm cancel-reply">Cancel</button>
                </div>
            </form>
        </div>
    `;
    
    const parentComment = document.querySelector(`[data-comment-id="${parentCommentId}"]`);
    if (!parentComment.querySelector('.comment-nested')) {
        parentComment.insertAdjacentHTML('beforeend', '<div class="comment-nested"></div>');
    }
    parentComment.querySelector('.comment-nested').insertAdjacentHTML('beforeend', formHtml);
    
    // Attach form submit handler
    const form = document.querySelector(`#reply-form-${parentCommentId} form`);
    form.addEventListener('submit', handleReplySubmit);
    
    // Cancel button
    document.querySelector(`#reply-form-${parentCommentId} .cancel-reply`).addEventListener('click', function() {
        document.getElementById(`reply-form-${parentCommentId}`).remove();
    });
}

// Comment form submission
document.getElementById('comment-form')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const formData = {
        PostID: parseInt(this.querySelector('[name="PostID"]').value),
        Content: this.querySelector('[name="Content"]').value
    };
    
    try {
        const response = await fetch('/Comments/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(formData)
        });
        
        const result = await response.json();
        if (result.success) {
            this.reset();
            loadComments(formData.PostID);
        } else {
            alert('Error posting comment');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Error posting comment');
    }
});

// Reply form submission
function handleReplySubmit(e) {
    e.preventDefault();
    
    const formData = {
        PostID: parseInt(this.querySelector('[name="PostID"]').value),
        ParentCommentID: parseInt(this.querySelector('[name="ParentCommentID"]').value),
        Content: this.querySelector('[name="Content"]').value
    };
    
    fetch('/Comments/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify(formData)
    })
    .then(response => response.json())
    .then(result => {
        if (result.success) {
            const postId = document.querySelector('[name="PostID"]').value;
            loadComments(parseInt(postId));
        } else {
            alert('Error posting reply');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Error posting reply');
    });
}

function formatDate(dateString) {
    if (!dateString) return 'just now';
    const date = new Date(dateString);
    const now = new Date();
    const diff = Math.floor((now - date) / 1000);
    
    if (diff < 60) return `${diff}s ago`;
    if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
    if (diff < 2592000) return `${Math.floor(diff / 86400)}d ago`;
    return date.toLocaleDateString();
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

