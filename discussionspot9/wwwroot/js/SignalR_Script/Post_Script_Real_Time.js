// signalr-connection.js
class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.currentPostId = null;
        this.typingTimeout = null;
        this.isTyping = false;
    }

    async initializeConnections() {
        // Initialize Post Hub Connection
        this.postConnection = new signalR.HubConnectionBuilder()
            .withUrl("/postHub")
            .withAutomaticReconnect()
            .build();

        // Initialize Notification Hub Connection
        this.notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        await this.setupPostEventHandlers();
        await this.setupNotificationEventHandlers();

        try {
            await this.postConnection.start();
            await this.notificationConnection.start();
            console.log("SignalR connections established");
        } catch (err) {
            console.error("SignalR connection failed: ", err);
        }
    }

    async setupPostEventHandlers() {
        // Receive new comments
        this.postConnection.on("ReceiveComment", (comment) => {
            this.addCommentToUI(comment);
            this.showNotification(`New comment by ${comment.UserName}`, 'info');
        });

        // Comment updated
        this.postConnection.on("CommentUpdated", (comment) => {
            this.updateCommentInUI(comment);
        });

        // Comment deleted
        this.postConnection.on("CommentDeleted", (commentId) => {
            this.removeCommentFromUI(commentId);
        });

        // Comment vote updated
        this.postConnection.on("CommentVoteUpdated", (voteData) => {
            this.updateCommentVotes(voteData);
        });

        // Post vote count updated
        this.postConnection.on("UpdateVoteCount", (newCount) => {
            this.updatePostVoteCount(newCount);
        });

        // Typing indicators
        this.postConnection.on("UserStartedTyping", (userName) => {
            this.showTypingIndicator(userName);
        });

        this.postConnection.on("UserStoppedTyping", (userName) => {
            this.hideTypingIndicator(userName);
        });

        // Error handling
        this.postConnection.on("CommentError", (errorMessage) => {
            this.showNotification(errorMessage, 'error');
        });
    }

    async setupNotificationEventHandlers() {
        // Receive notifications
        this.notificationConnection.on("ReceiveNotification", (notification) => {
            this.showNotification(notification.Message, 'info');
            this.updateNotificationBadge();
        });

        // Update unread count
        this.notificationConnection.on("UnreadNotificationCount", (count) => {
            this.updateNotificationBadge(count);
        });

        // Notification marked as read
        this.notificationConnection.on("NotificationMarkedAsRead", (notificationId) => {
            this.markNotificationAsRead(notificationId);
        });
    }

    async joinPostGroup(postId) {
        if (this.currentPostId) {
            await this.leavePostGroup(this.currentPostId);
        }

        this.currentPostId = postId;
        await this.postConnection.invoke("JoinPostGroup", postId);
    }

    async leavePostGroup(postId) {
        await this.postConnection.invoke("LeavePostGroup", postId);
        this.currentPostId = null;
    }

    async sendComment(postId, content, parentCommentId = null) {
        try {
            await this.postConnection.invoke("SendComment", postId, content, parentCommentId);
        } catch (err) {
            console.error("Error sending comment:", err);
            this.showNotification("Failed to send comment", 'error');
        }
    }

    async editComment(commentId, newContent) {
        try {
            await this.postConnection.invoke("EditComment", commentId, newContent);
        } catch (err) {
            console.error("Error editing comment:", err);
            this.showNotification("Failed to edit comment", 'error');
        }
    }

    async deleteComment(commentId) {
        if (confirm("Are you sure you want to delete this comment?")) {
            try {
                await this.postConnection.invoke("DeleteComment", commentId);
            } catch (err) {
                console.error("Error deleting comment:", err);
                this.showNotification("Failed to delete comment", 'error');
            }
        }
    }

    async voteComment(commentId, isUpvote) {
        try {
            await this.postConnection.invoke("VoteComment", commentId, isUpvote);
        } catch (err) {
            console.error("Error voting comment:", err);
        }
    }

    async startTyping(postId) {
        if (!this.isTyping) {
            this.isTyping = true;
            await this.postConnection.invoke("StartTyping", postId);
        }

        // Clear existing timeout
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

        // Set timeout to stop typing after 3 seconds
        this.typingTimeout = setTimeout(async () => {
            await this.stopTyping(postId);
        }, 3000);
    }

    async stopTyping(postId) {
        if (this.isTyping) {
            this.isTyping = false;
            await this.postConnection.invoke("StopTyping", postId);

            if (this.typingTimeout) {
                clearTimeout(this.typingTimeout);
                this.typingTimeout = null;
            }
        }
    }

    // UI Update Methods
    addCommentToUI(comment) {
        const commentsContainer = document.getElementById('comments-container');
        const commentHtml = this.createCommentHTML(comment);

        if (comment.ParentCommentId) {
            // Add as reply
            const parentComment = document.getElementById(`comment-${comment.ParentCommentId}`);
            if (parentComment) {
                let repliesContainer = parentComment.querySelector('.replies-container');
                if (!repliesContainer) {
                    repliesContainer = document.createElement('div');
                    repliesContainer.className = 'replies-container ms-4 mt-2';
                    parentComment.appendChild(repliesContainer);
                }
                repliesContainer.insertAdjacentHTML('beforeend', commentHtml);
            }
        } else {
            // Add as main comment
            commentsContainer.insertAdjacentHTML('afterbegin', commentHtml);
        }
    }

    updateCommentInUI(comment) {
        const commentElement = document.getElementById(`comment-${comment.Id}`);
        if (commentElement) {
            const contentElement = commentElement.querySelector('.comment-content');
            if (contentElement) {
                contentElement.textContent = comment.Content;

                // Add "edited" indicator
                const editedIndicator = commentElement.querySelector('.edited-indicator');
                if (!editedIndicator) {
                    const indicator = document.createElement('small');
                    indicator.className = 'edited-indicator text-muted ms-2';
                    indicator.textContent = '(edited)';
                    contentElement.appendChild(indicator);
                }
            }
        }
    }

    removeCommentFromUI(commentId) {
        const commentElement = document.getElementById(`comment-${commentId}`);
        if (commentElement) {
            commentElement.remove();
        }
    }

    updateCommentVotes(voteData) {
        const commentElement = document.getElementById(`comment-${voteData.CommentId}`);
        if (commentElement) {
            const upvoteElement = commentElement.querySelector('.upvote-count');
            const downvoteElement = commentElement.querySelector('.downvote-count');

            if (upvoteElement) upvoteElement.textContent = voteData.UpVotes;
            if (downvoteElement) downvoteElement.textContent = voteData.DownVotes;

            // Update button states based on user vote
            const upvoteBtn = commentElement.querySelector('.upvote-btn');
            const downvoteBtn = commentElement.querySelector('.downvote-btn');

            upvoteBtn?.classList.toggle('active', voteData.UserVote === 1);
            downvoteBtn?.classList.toggle('active', voteData.UserVote === -1);
        }
    }

    updatePostVoteCount(newCount) {
        const voteCountElement = document.getElementById('post-vote-count');
        if (voteCountElement) {
            voteCountElement.textContent = newCount;
        }
    }

    showTypingIndicator(userName) {
        const typingContainer = document.getElementById('typing-indicators');
        if (typingContainer) {
            const indicator = document.createElement('div');
            indicator.id = `typing-${userName}`;
            indicator.className = 'typing-indicator text-muted small';
            indicator.innerHTML = `${userName} is typing...`;
            typingContainer.appendChild(indicator);
        }
    }

    hideTypingIndicator(userName) {
        const indicator = document.getElementById(`typing-${userName}`);
        if (indicator) {
            indicator.remove();
        }
    }

    updateNotificationBadge(count = null) {
        const badge = document.getElementById('notification-badge');
        if (badge) {
            if (count !== null) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = count > 0 ? 'inline' : 'none';
            } else {
                // Increment existing count
                const currentCount = parseInt(badge.textContent) || 0;
                badge.textContent = currentCount + 1;
                badge.style.display = 'inline';
            }
        }
    }

    showNotification(message, type = 'info') {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info'} border-0`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        const toastContainer = document.getElementById('toast-container') || document.body;
        toastContainer.appendChild(toast);

        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    createCommentHTML(comment) {
        return `
            <div class="comment mb-3" id="comment-${comment.Id}">
                <div class="d-flex gap-3">
                    <div class="user-avatar" style="background-color: ${comment.UserName?.toAvatarColor() || 'gray'}">
                        ${comment.UserName?.toInitials() || 'U'}
                    </div>
                    <div class="flex-grow-1">
                        <div class="comment-header d-flex align-items-center gap-2 mb-1">
                            <strong>${comment.UserName}</strong>
                            <small class="text-muted">${comment.CreatedAt}</small>
                        </div>
                        <div class="comment-content">${comment.Content}</div>
                        <div class="comment-actions mt-2">
                            <button class="btn btn-sm btn-outline-primary upvote-btn" onclick="signalRManager.voteComment(${comment.Id}, true)">
                                ↑ <span class="upvote-count">${comment.UpVotes || 0}</span>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary downvote-btn" onclick="signalRManager.voteComment(${comment.Id}, false)">
                                ↓ <span class="downvote-count">${comment.DownVotes || 0}</span>
                            </button>
                            <button class="btn btn-sm btn-link" onclick="showReplyForm(${comment.Id})">Reply</button>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }
}

// Initialize SignalR Manager
const signalRManager = new SignalRManager();

// Initialize when page loads
document.addEventListener('DOMContentLoaded', async () => {
    await signalRManager.initializeConnections();

    // Join post group if on a post page
    const postId = document.querySelector('[data-post-id]')?.getAttribute('data-post-id');
    if (postId) {
        await signalRManager.joinPostGroup(parseInt(postId));

        // Setup typing indicators for comment textarea
        const commentTextarea = document.getElementById('mainCommentContent');
        if (commentTextarea) {
            let typingTimer;
            commentTextarea.addEventListener('input', () => {
                signalRManager.startTyping(parseInt(postId));

                clearTimeout(typingTimer);
                typingTimer = setTimeout(() => {
                    signalRManager.stopTyping(parseInt(postId));
                }, 1000);
            });
        }
    }
});

// Update your existing comment submission
document.getElementById('submitMainComment')?.addEventListener('click', async function () {
    const postId = parseInt(this.getAttribute('data-post-id'));
    const commentContent = document.getElementById('mainCommentContent').value;

    if (!commentContent.trim()) {
        signalRManager.showNotification("Please enter a comment.", "error");
        return;
    }

    // Send via SignalR instead of fetch
    await signalRManager.sendComment(postId, commentContent.trim());
    document.getElementById('mainCommentContent').value = '';
});