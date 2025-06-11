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
        // Receive new comments - expects rendered HTML from the server
        this.postConnection.on("ReceiveComment", (commentHtml, commentId, parentCommentId) => {
            try {
                if (parentCommentId) {
                    // This is a reply - add it to the parent comment's replies section
                    this.addReplyToParent(commentHtml, parentCommentId);
                } else {
                    // This is a top-level comment - add to the main comment list
                    const commentList = document.querySelector(".comment-list");
                    if (commentList) {
                        // Add new comment at the top of the list
                        commentList.insertAdjacentHTML('afterbegin', commentHtml);
                    } else {
                        console.error("Comment list container (.comment-list) not found");
                        return;
                    }
                }

                // Update comment count
                this.updateCommentCount();

                // Show notification
                this.showNotification('New comment added!', 'info');

                // Rebind events for newly added comment
                this.rebindCommentEvents();

            } catch (error) {
                console.error('Error handling received comment:', error);
            }
        });

        // Comment updated - expects rendered HTML and the commentId from the server
        this.postConnection.on("CommentUpdated", (commentHtml, commentId) => {
            this.updateCommentInUI(commentHtml, commentId);
            this.showNotification('Comment updated!', 'info');
            this.rebindCommentEvents(); // Rebind events for the updated comment
        });

        // Comment deleted
        this.postConnection.on("CommentDeleted", (commentId) => {
            this.removeCommentFromUI(commentId);
            this.showNotification(`Comment deleted.`, 'info');
            this.updateCommentCount(); // Recalculate count after deletion
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
        // IMPORTANT: Never use window.confirm() in production as it blocks UI.
        // Replace with a custom modal/dialog for user confirmation.
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

    // --- UI Update Methods (now receive pre-rendered HTML) ---

    // Helper method to add a reply to a parent comment's children container
    addReplyToParent(replyHtml, parentCommentId) {
        const parentComment = document.querySelector(`.comment-item[data-comment-id="${parentCommentId}"]`);
        if (!parentComment) {
            console.error(`Parent comment with ID ${parentCommentId} not found`);
            return;
        }

        // Look for existing replies container (from your partial view: .comment-children)
        let repliesContainer = parentComment.querySelector('.comment-children');
        if (!repliesContainer) {
            // Create replies container if it doesn't exist
            repliesContainer = document.createElement('div');
            repliesContainer.className = 'comment-children mt-3'; // Class from your partial view
            parentComment.appendChild(repliesContainer);
        }

        // Add the reply at the end of existing replies
        repliesContainer.insertAdjacentHTML('beforeend', replyHtml);
    }

    // Helper method to update the displayed comment count
    updateCommentCount() {
        const commentCountElement = document.querySelector('.comment-count'); // Assuming you have a span/div with this class for total comment count
        if (commentCountElement) {
            // Count all comment items based on the data-comment-id attribute
            const totalComments = document.querySelectorAll('.comment-item').length;
            commentCountElement.textContent = `${totalComments} Comments`;
        }
    }

    // Updates an existing comment element with new HTML from the server
    updateCommentInUI(commentHtml, commentId) {
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`); // Select by data-comment-id
        if (commentElement) {
            // Replace the entire comment item HTML with the new HTML from the server
            commentElement.outerHTML = commentHtml;
        }
    }

    removeCommentFromUI(commentId) {
        // Select the comment element using its data-comment-id
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (commentElement) {
            commentElement.remove();
        }
    }

    updateCommentVotes(voteData) {
        // Select the comment element using its data-comment-id
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${voteData.CommentId}"]`);
        if (commentElement) {
            // Your partial view uses a single .vote-count span for score
            const scoreElement = commentElement.querySelector('.vote-count');
            if (scoreElement) scoreElement.textContent = voteData.Score; // Update to voteData.Score

            // Update button active states based on CurrentUserVote
            const upvoteBtn = commentElement.querySelector('.comment-upvote'); // Select by class name
            const downvoteBtn = commentElement.querySelector('.comment-downvote'); // Select by class name

            // Toggle 'active' class based on the CurrentUserVote from voteData
            upvoteBtn?.classList.toggle('active', voteData.CurrentUserVote === 1);
            downvoteBtn?.classList.toggle('active', voteData.CurrentUserVote === -1);
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
            // Ensure only one indicator per user
            let indicator = document.getElementById(`typing-${userName}`);
            if (!indicator) {
                indicator = document.createElement('div');
                indicator.id = `typing-${userName}`;
                indicator.className = 'typing-indicator text-muted small';
                indicator.innerHTML = `${userName} is typing...`;
                typingContainer.appendChild(indicator);
            }
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
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

        const toastContainer = document.getElementById('toast-container');
        // If toast container doesn't exist, create it (e.g., at the top right of the viewport)
        if (!toastContainer) {
            const newToastContainer = document.createElement('div');
            newToastContainer.id = 'toast-container';
            newToastContainer.style.position = 'fixed';
            newToastContainer.style.top = '1rem';
            newToastContainer.style.right = '1rem';
            newToastContainer.style.zIndex = '1050'; // Ensure it's above other elements
            document.body.appendChild(newToastContainer);
            newToastContainer.appendChild(toast);
        } else {
            toastContainer.appendChild(toast);
        }


        const bsToast = new bootstrap.Toast(toast, { delay: 3000 }); // Auto-hide after 3 seconds
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    // New method to re-bind events for newly added/updated comments
    rebindCommentEvents() {
        // Re-bind vote buttons
        document.querySelectorAll('.vote-btn').forEach(button => {
            // Remove any existing click listeners to prevent duplicates
            button.removeEventListener('click', this.handleVoteButtonClick);
            button.addEventListener('click', this.handleVoteButtonClick);
        });

        // Re-bind reply buttons
        document.querySelectorAll('.reply-btn').forEach(button => {
            button.removeEventListener('click', this.handleReplyButtonClick);
            button.addEventListener('click', this.handleReplyButtonClick);
        });

        // Re-bind edit buttons
        document.querySelectorAll('.edit-btn').forEach(button => {
            button.removeEventListener('click', this.handleEditButtonClick);
            button.addEventListener('click', this.handleEditButtonClick);
        });

        // Re-bind delete buttons
        document.querySelectorAll('.delete-btn').forEach(button => {
            button.removeEventListener('click', this.handleDeleteButtonClick);
            button.addEventListener('click', this.handleDeleteButtonClick);
        });

        // Re-bind reply form submission
        document.querySelectorAll('.reply-comment-form').forEach(form => {
            form.removeEventListener('submit', this.handleReplyFormSubmit);
            form.addEventListener('submit', this.handleReplyFormSubmit);
        });

        // Re-bind cancel reply buttons
        document.querySelectorAll('.cancel-reply').forEach(button => {
            button.removeEventListener('click', this.handleCancelReplyClick);
            button.addEventListener('click', this.handleCancelReplyClick);
        });
    }

    // Event handlers for rebindCommentEvents (defined as arrow functions to preserve 'this')
    handleVoteButtonClick = (e) => {
        e.preventDefault(); // Prevent default button action
        const commentId = e.currentTarget.dataset.commentId;
        const voteType = parseInt(e.currentTarget.dataset.voteType); // 1 for upvote, -1 for downvote
        this.voteComment(commentId, voteType === 1);
    }

    handleReplyButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        showReplyForm(commentId); // Assuming showReplyForm is a global function
    }

    handleEditButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        // You'll need a showEditForm(commentId) function or similar
        // that populates an edit form with the comment's content
        // and then calls signalRManager.editComment when submitted.
        this.showNotification(`Edit functionality not yet implemented for comment: ${commentId}`, 'info');
        console.log(`Edit button clicked for comment: ${commentId}`);
    }

    handleDeleteButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        this.deleteComment(commentId);
    }

    handleReplyFormSubmit = async (event) => {
        event.preventDefault();
        const postId = event.target.querySelector('input[name="PostId"]').value;
        const parentCommentId = event.target.querySelector('input[name="ParentCommentId"]').value;
        const content = event.target.querySelector('textarea[name="Content"]').value;

        if (!content.trim()) {
            this.showNotification("Please enter a reply.", "error");
            return;
        }

        await this.sendComment(parseInt(postId), content.trim(), parentCommentId); // Ensure postId is int if your hub expects int
        event.target.reset(); // Clear the form
        // Hide the reply form after submission
        event.target.closest('.reply-form').classList.add('d-none');
    }

    handleCancelReplyClick = (e) => {
        e.preventDefault();
        e.target.closest('.reply-form').classList.add('d-none');
    }

    // IMPORTANT: Remove the old createCommentHTML function entirely as it's replaced by server-side rendering.
    // The previous addCommentToUI function has also been removed.
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

        // Initial binding of events for comments already rendered on page load
        signalRManager.rebindCommentEvents();

        // Setup typing indicators for comment textarea
        const commentTextarea = document.getElementById('mainCommentContent');
        if (commentTextarea) {
            let typingTimer;
            commentTextarea.addEventListener('input', () => {
                signalRManager.startTyping(parseInt(postId));

                clearTimeout(typingTimer);
                typingTimer = setTimeout(async () => {
                    await signalRManager.stopTyping(parseInt(postId));
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

    // Send via SignalR
    await signalRManager.sendComment(postId, commentContent.trim());
    document.getElementById('mainCommentContent').value = '';
});

// Make sure showReplyForm is defined globally or within SignalRManager if it handles DOM directly
function showReplyForm(commentId) {
    // Hide any currently open reply forms first, if desired
    document.querySelectorAll('.reply-form').forEach(form => form.classList.add('d-none'));

    const replyForm = document.getElementById(`reply-form-${commentId}`);
    if (replyForm) {
        replyForm.classList.remove('d-none');
        replyForm.querySelector('textarea').focus();
    }
}
