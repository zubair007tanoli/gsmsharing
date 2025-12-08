// Reactions Management JavaScript

const reactionEmojis = {
    'like': '👍',
    'love': '❤️',
    'laugh': '😂',
    'wow': '😮',
    'sad': '😢',
    'angry': '😠'
};

function loadReactions(postId, commentId) {
    const container = document.getElementById('reactions-container') || 
                     document.querySelector(`[data-comment-id="${commentId}"] .reactions-container`);
    
    if (!container) return;
    
    fetch(`/Reactions/GetSummary?postId=${postId || ''}&commentId=${commentId || ''}`)
        .then(response => response.json())
        .then(summaries => {
            if (summaries.length === 0) {
                container.innerHTML = '<div class="reactions-container"><button class="reaction-btn" data-reaction="like"><span class="reaction-emoji">👍</span> Like</button></div>';
            } else {
                container.innerHTML = summaries.map(summary => renderReactionButton(summary, postId, commentId)).join('');
            }
            
            attachReactionListeners(postId, commentId);
        })
        .catch(error => {
            console.error('Error loading reactions:', error);
        });
}

function renderReactionButton(summary, postId, commentId) {
    const emoji = reactionEmojis[summary.reactionType] || '👍';
    const activeClass = summary.userHasReacted ? 'active' : '';
    
    return `
        <button class="reaction-btn ${activeClass}" 
                data-reaction="${summary.reactionType}"
                data-post-id="${postId || ''}"
                data-comment-id="${commentId || ''}">
            <span class="reaction-emoji">${emoji}</span>
            <span class="reaction-count">${summary.count}</span>
        </button>
    `;
}

function attachReactionListeners(postId, commentId) {
    document.querySelectorAll('.reaction-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const reactionType = this.dataset.reaction;
            const postId = this.dataset.postId || null;
            const commentId = this.dataset.commentId || null;
            
            toggleReaction(reactionType, postId ? parseInt(postId) : null, commentId ? parseInt(commentId) : null);
        });
    });
}

function toggleReaction(reactionType, postId, commentId) {
    const reactionData = {
        ReactionType: reactionType,
        PostID: postId,
        CommentID: commentId
    };
    
    fetch('/Reactions/Toggle', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify(reactionData)
    })
    .then(response => response.json())
    .then(result => {
        if (result.success) {
            loadReactions(postId, commentId);
        } else {
            alert('Error toggling reaction');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Error toggling reaction');
    });
}

