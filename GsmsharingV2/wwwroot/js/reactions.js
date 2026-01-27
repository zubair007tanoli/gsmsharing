// Reactions System JavaScript
(function() {
    'use strict';

    const emojiMap = {
        'like': '👍',
        'love': '❤️',
        'laugh': '😂',
        'wow': '😮',
        'sad': '😢',
        'angry': '😡'
    };

    function initializeReactions() {
        // Initialize reaction triggers
        document.querySelectorAll('.reaction-trigger').forEach(trigger => {
            trigger.addEventListener('click', function(e) {
                e.stopPropagation();
                const postId = this.id.replace('reactionTrigger-', '');
                toggleReactionPicker(postId);
            });
        });

        // Initialize emoji buttons
        document.querySelectorAll('.reaction-emoji').forEach(emoji => {
            emoji.addEventListener('click', handleReactionClick);
        });

        // Close pickers on outside click
        document.addEventListener('click', function(e) {
            if (!e.target.closest('.reaction-section')) {
                document.querySelectorAll('.reaction-picker').forEach(picker => {
                    picker.style.display = 'none';
                });
            }
        });
    }

    function toggleReactionPicker(postId) {
        const picker = document.getElementById(`reactionPicker-${postId}`);
        if (picker) {
            picker.style.display = picker.style.display === 'none' ? 'flex' : 'none';
        }
    }

    async function handleReactionClick(e) {
        e.stopPropagation();
        const btn = e.currentTarget;
        const reaction = btn.dataset.reaction;
        const postId = parseInt(btn.dataset.postId);
        const commentId = btn.dataset.commentId ? parseInt(btn.dataset.commentId) : null;
        
        if (!postId && !commentId) {
            console.error('No post or comment ID provided');
            return;
        }
        
        btn.disabled = true;
        
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const response = await fetch('/api/Reactions/Add', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token || ''
                },
                body: JSON.stringify({
                    postID: postId || null,
                    commentID: commentId || null,
                    reactionType: reaction
                })
            });
            
            if (!response.ok) {
                throw new Error('Reaction failed');
            }
            
            const data = await response.json();
            
            if (data.success) {
                // Reload reactions display
                if (postId) {
                    loadReactions(postId, null);
                } else if (commentId) {
                    loadReactions(null, commentId);
                }
                
                // Close picker
                if (postId) {
                    document.getElementById(`reactionPicker-${postId}`).style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Reaction error:', error);
        } finally {
            btn.disabled = false;
        }
    }

    async function loadReactions(postId, commentId) {
        try {
            const url = postId 
                ? `/api/Reactions/Get?postID=${postId}`
                : `/api/Reactions/Get?commentID=${commentId}`;
            
            const response = await fetch(url);
            if (!response.ok) return;
            
            const reactions = await response.json();
            
            const displayId = postId 
                ? `reactionsDisplay-${postId}`
                : `reactionsDisplay-comment-${commentId}`;
            
            const display = document.getElementById(displayId);
            if (!display) return;
            
            if (reactions && reactions.length > 0) {
                display.innerHTML = reactions.map(r => 
                    `<span class="reaction-badge" title="${r.reactionType}">
                        ${emojiMap[r.reactionType] || '👍'} ${r.count}
                    </span>`
                ).join('');
            } else {
                display.innerHTML = '';
            }
        } catch (error) {
            console.error('Error loading reactions:', error);
        }
    }

    // Export functions
    window.initializeReactions = initializeReactions;
    window.loadReactions = loadReactions;
    
    // Auto-initialize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeReactions);
    } else {
        initializeReactions();
    }
})();
