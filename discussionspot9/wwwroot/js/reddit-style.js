// Reddit-Style Page JavaScript
// Handles interactions for the new better-than-Reddit page

document.addEventListener('DOMContentLoaded', function() {
    console.log('🎨 Reddit-style page initialized');
    
    // Initialize Quill editors
    initializeQuillEditors();
    
    // Initialize share dropdown positioning
    initializeShareDropdown();
    
    // Initialize comment reply buttons
    initializeCommentReplies();
    
    // Clean up post content
    cleanupPostContent();
    
    // Initialize poll interactions
    initializePollInteractions();
});

// Initialize Quill Editor
function initializeQuillEditors() {
    // Main comment editor
    if (document.getElementById('mainCommentEditor')) {
        const mainEditor = new Quill('#mainCommentEditor', {
            theme: 'snow',
            placeholder: 'What are your thoughts?',
            modules: {
                toolbar: [
                    ['bold', 'italic', 'underline', 'strike'],
                    ['blockquote', 'code-block', 'link'],
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }]
                ]
            }
        });
        
        // Store editor instance
        window.mainCommentEditor = mainEditor;
    }
}

// Share Dropdown Positioning
function initializeShareDropdown() {
    const shareDropdowns = document.querySelectorAll('.share-dropdown-footer');
    
    shareDropdowns.forEach(dropdown => {
        dropdown.addEventListener('mouseenter', function() {
            const shareBtn = this.querySelector('.action-btn-footer');
            const shareMenu = this.querySelector('.share-menu-footer');
            
            if (shareBtn && shareMenu) {
                const btnRect = shareBtn.getBoundingClientRect();
                shareMenu.style.left = '0';
            }
        });
    });
}

// Comment Reply Functionality
function initializeCommentReplies() {
    const replyBtns = document.querySelectorAll('.comment-reply-btn-reddit');
    
    replyBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const commentId = this.getAttribute('data-comment-id');
            const replyForm = document.getElementById(`replyForm-${commentId}`);
            
            if (replyForm) {
                // Toggle reply form
                if (replyForm.style.display === 'none') {
                    replyForm.style.display = 'block';
                    
                    // Initialize Quill editor for this reply if not already initialized
                    const editorId = `replyEditor-${commentId}`;
                    if (document.getElementById(editorId) && !window[`replyEditor${commentId}`]) {
                        window[`replyEditor${commentId}`] = new Quill(`#${editorId}`, {
                            theme: 'snow',
                            placeholder: 'Write a reply...',
                            modules: {
                                toolbar: [
                                    ['bold', 'italic', 'underline'],
                                    ['blockquote', 'link']
                                ]
                            }
                        });
                    }
                } else {
                    replyForm.style.display = 'none';
                }
            }
        });
    });
    
    // Cancel reply buttons
    const cancelBtns = document.querySelectorAll('.btn-cancel-reply');
    cancelBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const commentId = this.getAttribute('data-comment-id');
            const replyForm = document.getElementById(`replyForm-${commentId}`);
            if (replyForm) {
                replyForm.style.display = 'none';
            }
        });
    });
}

// Clean up post content
function cleanupPostContent() {
    const postContent = document.getElementById('postContent');
    if (postContent) {
        // Remove empty paragraphs
        const paragraphs = postContent.querySelectorAll('p');
        paragraphs.forEach(p => {
            if (!p.textContent.trim()) {
                p.remove();
            }
        });
        
        // Remove excessive line breaks
        const content = postContent.innerHTML;
        const cleanedContent = content
            .replace(/<br\s*\/?>\s*<br\s*\/?>/gi, '<br>')
            .replace(/<p>\s*<\/p>/gi, '')
            .replace(/<p>\s*<br\s*\/?>\s*<\/p>/gi, '')
            .trim();
        
        postContent.innerHTML = cleanedContent;
    }
}

// Poll Interactions
function initializePollInteractions() {
    const pollOptions = document.querySelectorAll('.poll-option-btn');
    
    pollOptions.forEach(option => {
        option.addEventListener('click', function() {
            const postId = this.getAttribute('data-post-id');
            const optionId = this.getAttribute('data-option-id');
            
            console.log('🗳️ Poll vote:', { postId, optionId });
            
            // Optimistic UI update
            const allOptions = this.closest('.poll-options-modern').querySelectorAll('.poll-option-modern');
            allOptions.forEach(opt => opt.classList.remove('voted'));
            
            const selectedOption = this.closest('.poll-option-modern');
            selectedOption.classList.add('voted');
            
            const radioDot = this.querySelector('.radio-dot');
            if (radioDot) {
                document.querySelectorAll('.radio-dot').forEach(dot => dot.classList.remove('selected'));
                radioDot.classList.add('selected');
            }
            
            // Here you would call your poll voting API
            // votePoll(postId, optionId);
        });
    });
}

// Comment Sorting
document.querySelectorAll('.sort-btn').forEach(btn => {
    btn.addEventListener('click', function() {
        document.querySelectorAll('.sort-btn').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
        
        const sortType = this.getAttribute('data-sort');
        console.log('🔄 Sorting comments by:', sortType);
        
        // Here you would implement comment sorting logic
        // sortComments(sortType);
    });
});

// Scroll to comments on comment button click
document.querySelectorAll('.action-btn-footer').forEach(btn => {
    if (btn.querySelector('.fa-comment')) {
        btn.addEventListener('click', function() {
            const commentsSection = document.querySelector('.comments-section');
            if (commentsSection) {
                commentsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    }
});

// Share functionality
document.querySelectorAll('.share-item').forEach(item => {
    item.addEventListener('click', async function(e) {
        e.preventDefault();
        const platform = this.getAttribute('data-platform');
        const postUrl = window.location.href;
        
        if (platform === 'copy') {
            try {
                await navigator.clipboard.writeText(postUrl);
                showNotification('Link copied to clipboard!', 'success');
            } catch (err) {
                console.error('Failed to copy:', err);
            }
        } else {
            // Implement social sharing
            console.log('Share on:', platform);
        }
    });
});

// Notification helper
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        padding: 12px 24px;
        background: ${type === 'success' ? '#46D160' : '#0079D3'};
        color: white;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 10000;
        animation: slideInUp 0.3s ease;
    `;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.animation = 'slideOutDown 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Keyboard shortcuts
document.addEventListener('keydown', function(e) {
    // Alt + C = Focus comment box
    if (e.altKey && e.key === 'c') {
        e.preventDefault();
        const commentEditor = document.querySelector('#mainCommentEditor .ql-editor');
        if (commentEditor) {
            commentEditor.focus();
        }
    }
    
    // Alt + S = Sort comments
    if (e.altKey && e.key === 's') {
        e.preventDefault();
        document.querySelector('.sort-btn')?.click();
    }
});

console.log('✅ Reddit-style page ready!');

