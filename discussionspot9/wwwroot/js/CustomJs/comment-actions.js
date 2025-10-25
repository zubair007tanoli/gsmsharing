/**
 * Comment Actions Handler
 * Handles edit, delete, pin, and other comment actions
 */

// Edit Comment
async function editComment(commentId) {
    console.log('📝 Editing comment:', commentId);
    
    const commentText = document.querySelector(`#commentsContainer${commentId} .comment-text`);
    const editForm = document.getElementById(`editForm${commentId}`);
    const commentActions = document.querySelector(`#commentsContainer${commentId} .comment-actions`);
    
    if (!commentText || !editForm) {
        console.error('Comment elements not found');
        return;
    }
    
    // Hide comment text and actions
    commentText.style.display = 'none';
    if (commentActions) commentActions.style.display = 'none';
    
    // Show edit form
    editForm.classList.remove('d-none');
    
    // Initialize Quill editor for editing
    initializeEditEditor(commentId, commentText.innerHTML);
}

// Initialize Edit Editor (Quill)
function initializeEditEditor(commentId, currentContent) {
    const editorContainer = `#editEditor${commentId}`;
    
    // Check if already initialized
    if (window[`editQuill${commentId}`]) {
        window[`editQuill${commentId}`].root.innerHTML = currentContent;
        return;
    }
    
    // Quill toolbar options
    const toolbarOptions = [
        ['bold', 'italic', 'underline', 'strike'],
        ['blockquote', 'code-block', 'link'],
        [{ 'list': 'ordered'}, { 'list': 'bullet' }],
        ['clean']
    ];
    
    // Initialize Quill
    window[`editQuill${commentId}`] = new Quill(editorContainer, {
        theme: 'snow',
        modules: {
            toolbar: toolbarOptions
        },
        placeholder: 'Edit your comment...'
    });
    
    // Set current content
    window[`editQuill${commentId}`].root.innerHTML = currentContent;
    
    // Focus the editor
    window[`editQuill${commentId}`].focus();
    
    // Handle cancel button
    const cancelBtn = document.querySelector(`#editForm${commentId} .edit-cancel-btn`);
    if (cancelBtn) {
        cancelBtn.addEventListener('click', () => cancelEditComment(commentId));
    }
    
    // Handle submit button
    const submitBtn = document.querySelector(`#editForm${commentId} .edit-submit-btn`);
    if (submitBtn) {
        submitBtn.addEventListener('click', () => submitEditComment(commentId));
    }
}

// Cancel Edit
function cancelEditComment(commentId) {
    const commentText = document.querySelector(`#commentsContainer${commentId} .comment-text`);
    const editForm = document.getElementById(`editForm${commentId}`);
    const commentActions = document.querySelector(`#commentsContainer${commentId} .comment-actions`);
    
    // Show comment text and actions
    if (commentText) commentText.style.display = 'block';
    if (commentActions) commentActions.style.display = 'flex';
    
    // Hide edit form
    if (editForm) editForm.classList.add('d-none');
}

// Submit Edit
async function submitEditComment(commentId) {
    const quill = window[`editQuill${commentId}`];
    
    if (!quill) {
        console.error('Editor not found');
        return;
    }
    
    const content = quill.root.innerHTML.trim();
    const text = quill.getText().trim();
    
    if (!text || text.length === 0) {
        showCommentToast('error', 'Comment cannot be empty');
        return;
    }
    
    console.log('💾 Saving edited comment:', commentId);
    
    try {
        const response = await fetch('/Comment/Edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                commentId: commentId,
                content: content
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Update comment text
            const commentText = document.querySelector(`#commentsContainer${commentId} .comment-text`);
            if (commentText) {
                commentText.innerHTML = content;
            }
            
            // Add (edited) marker
            const commentTime = document.querySelector(`#commentsContainer${commentId} .comment-time`);
            if (commentTime && !commentTime.textContent.includes('(edited)')) {
                commentTime.textContent += ' (edited)';
            }
            
            // Hide edit form and show comment
            cancelEditComment(commentId);
            
            showCommentToast('success', 'Comment updated successfully!');
        } else {
            showCommentToast('error', result.message || 'Failed to update comment');
        }
    } catch (error) {
        console.error('Error updating comment:', error);
        showCommentToast('error', 'An error occurred while updating the comment');
    }
}

// Delete Comment
async function deleteComment(commentId) {
    // Show confirmation dialog
    const confirmed = confirm('Are you sure you want to delete this comment? This action cannot be undone.');
    
    if (!confirmed) {
        return;
    }
    
    console.log('🗑️ Deleting comment:', commentId);
    
    try {
        const response = await fetch('/Comment/Delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                commentId: commentId
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            // Remove comment from DOM
            const commentContainer = document.getElementById(`commentsContainer${commentId}`);
            if (commentContainer) {
                commentContainer.style.transition = 'opacity 0.3s ease';
                commentContainer.style.opacity = '0';
                
                setTimeout(() => {
                    commentContainer.remove();
                    
                    // Update comment count via SignalR manager if available
                    if (window.signalRManager && typeof window.signalRManager.updateCommentCount === 'function') {
                        window.signalRManager.updateCommentCount(-1);
                    } else {
                        // Fallback to local function
                        updateCommentCount(-1);
                    }
                }, 300);
            }
            
            showCommentToast('success', 'Comment deleted successfully!');
        } else {
            showCommentToast('error', result.message || 'Failed to delete comment');
        }
    } catch (error) {
        console.error('Error deleting comment:', error);
        showCommentToast('error', 'An error occurred while deleting the comment');
    }
}

// Toggle Pin Comment
async function togglePinComment(commentId, isPinned) {
    console.log(isPinned ? '📌 Pinning comment:' : '📍 Unpinning comment:', commentId);
    
    try {
        const response = await fetch('/Comment/TogglePin', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({
                commentId: commentId
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            const newPinStatus = result.isPinned;
            
            // Update pin button text and data attribute
            const pinBtn = document.querySelector(`.pin-comment-btn[data-comment-id="${commentId}"]`);
            if (pinBtn) {
                pinBtn.setAttribute('data-is-pinned', newPinStatus.toString());
                const icon = pinBtn.querySelector('i');
                const text = pinBtn.childNodes[2]; // Text node after icon
                if (text) {
                    text.textContent = newPinStatus ? ' Unpin Comment' : ' Pin Comment';
                }
            }
            
            // Add/remove pinned badge
            const dropdownMenu = document.querySelector(`#commentsContainer${commentId} .dropdown-menu`);
            if (dropdownMenu) {
                let pinnedBadge = dropdownMenu.querySelector('.dropdown-item-text');
                
                if (newPinStatus) {
                    // Add pinned badge if not exists
                    if (!pinnedBadge) {
                        const badgeHTML = `
                            <li>
                                <span class="dropdown-item-text text-success">
                                    <i class="fas fa-thumbtack me-2"></i> Pinned Comment
                                </span>
                            </li>
                            <li><hr class="dropdown-divider"></li>
                        `;
                        dropdownMenu.insertAdjacentHTML('afterbegin', badgeHTML);
                    }
                    
                    // Move comment to top of list
                    const commentContainer = document.getElementById(`commentsContainer${commentId}`);
                    const commentList = document.querySelector('.comment-list');
                    if (commentContainer && commentList) {
                        commentList.insertBefore(commentContainer, commentList.firstChild);
                    }
                } else {
                    // Remove pinned badge
                    if (pinnedBadge) {
                        pinnedBadge.closest('li').remove();
                        // Also remove the divider
                        const divider = dropdownMenu.querySelector('.dropdown-divider');
                        if (divider) divider.closest('li').remove();
                    }
                }
            }
            
            showCommentToast('success', newPinStatus ? 'Comment pinned!' : 'Comment unpinned!');
        } else {
            showCommentToast('error', result.message || 'Failed to pin comment');
        }
    } catch (error) {
        console.error('Error toggling pin:', error);
        showCommentToast('error', 'An error occurred while pinning the comment');
    }
}

// Copy Comment Link
function copyCommentLink(commentId) {
    const commentUrl = `${window.location.origin}${window.location.pathname}#comment-${commentId}`;
    
    navigator.clipboard.writeText(commentUrl).then(() => {
        showCommentToast('success', 'Comment link copied to clipboard!');
    }).catch(err => {
        console.error('Failed to copy:', err);
        showCommentToast('error', 'Failed to copy link');
    });
}

// Update Comment Count
function updateCommentCount(change) {
    const commentCountEl = document.querySelector('.comment-count');
    if (commentCountEl) {
        const currentText = commentCountEl.textContent;
        const currentCount = parseInt(currentText.match(/\d+/)[0]);
        const newCount = Math.max(0, currentCount + change);
        commentCountEl.textContent = `${newCount} Comment${newCount !== 1 ? 's' : ''}`;
    }
}

// Show Toast Notification
function showCommentToast(type, message) {
    // Remove existing toast if any
    const existingToast = document.getElementById('commentActionToast');
    if (existingToast) {
        existingToast.remove();
    }
    
    const toastContainer = document.createElement('div');
    toastContainer.className = 'position-fixed top-0 end-0 p-3';
    toastContainer.style.zIndex = '9999';
    toastContainer.id = 'commentActionToast';
    
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'warning' ? 'warning' : 'danger'} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : 'exclamation-circle'} me-2"></i>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    document.body.appendChild(toastContainer);
    
    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();
    
    // Remove toast container after it's hidden
    toast.addEventListener('hidden.bs.toast', () => {
        toastContainer.remove();
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log('✅ Comment actions initialized');
    
    // Scroll to comment if URL contains comment anchor
    const hash = window.location.hash;
    if (hash && hash.startsWith('#comment-')) {
        const commentId = hash.replace('#comment-', '');
        const commentEl = document.getElementById(`commentsContainer${commentId}`);
        if (commentEl) {
            setTimeout(() => {
                commentEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
                commentEl.style.backgroundColor = 'rgba(255, 193, 7, 0.1)';
                setTimeout(() => {
                    commentEl.style.transition = 'background-color 1s ease';
                    commentEl.style.backgroundColor = '';
                }, 1000);
            }, 500);
        }
    }
});

