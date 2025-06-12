// signalr-connection.js
class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.currentPostId = null;
        this.typingTimeout = null;
        this.isTyping = false;
        // Store global postId once it's available
        this.pagePostId = null;
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
        document.querySelectorAll('.comment-vote-btn').forEach(button => {
            button.removeEventListener('click', this.handleVoteButtonClick);
            button.addEventListener('click', this.handleVoteButtonClick);
        });

        // Re-bind reply buttons (both .reply-btn and .comment-reply-btn)
        document.querySelectorAll('.reply-btn, .comment-reply-btn').forEach(button => {
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

        // Re-bind reply submit buttons
        document.querySelectorAll('.reply-submit-btn').forEach(button => {
            button.removeEventListener('click', this.handleSubmitReplyButtonClick); // Use new handler
            button.addEventListener('click', this.handleSubmitReplyButtonClick); // Use new handler
        });

        // Re-bind cancel reply buttons
        document.querySelectorAll('.cancel-reply, .reply-cancel-btn').forEach(button => {
            button.removeEventListener('click', this.handleCancelReplyClick);
            button.addEventListener('click', this.handleCancelReplyClick);
        });
    }

    // Event handlers for rebindCommentEvents (defined as arrow functions to preserve 'this')
    handleVoteButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        const voteType = parseInt(e.currentTarget.dataset.voteType);
        this.voteComment(commentId, voteType === 1);
    }

    handleReplyButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        this.showReplyForm(commentId);
    }

    handleEditButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        this.showNotification(`Edit functionality not yet implemented for comment: ${commentId}`, 'info');
        console.log(`Edit button clicked for comment: ${commentId}`);
    }

    handleDeleteButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        this.deleteComment(commentId);
    }

    // New handler for reply submit button click
    handleSubmitReplyButtonClick = async (event) => {
        event.preventDefault(); // Prevent default button action (no form submit)
        const replyFormDiv = event.currentTarget.closest('.reply-form'); // Get the containing reply-form div

        if (!replyFormDiv) {
            console.error("Could not find parent .reply-form div for reply submit button.");
            this.showNotification("Error: Could not submit reply.", "error");
            return;
        }

        // Get ParentCommentId from the data-comment-id of the reply-form div
        const parentCommentId = replyFormDiv.dataset.commentId;

        // Get content from the textarea within this specific reply form
        const contentTextarea = replyFormDiv.querySelector('textarea');
        const content = contentTextarea ? contentTextarea.value : '';

        if (!content.trim()) {
            this.showNotification("Please enter a reply.", "error");
            return;
        }

        // Use the global postId variable from the page's context
        // Ensure this.pagePostId is set when the page loads, as done in DOMContentLoaded
        if (!this.pagePostId) {
            console.error("Post ID not available for reply submission.");
            this.showNotification("Error: Post ID missing for reply.", "error");
            return;
        }

        await this.sendComment(parseInt(this.pagePostId), content.trim(), parentCommentId ? parseInt(parentCommentId) : null);

        // Reset the textarea and hide the reply form
        if (contentTextarea) contentTextarea.value = ''; // Clear textarea
        replyFormDiv.classList.remove('active'); // Hide the reply form
        replyFormDiv.classList.add('d-none'); // Ensure it's hidden with Bootstrap class
    }

    handleCancelReplyClick = (e) => {
        e.preventDefault();
        const form = e.target.closest('.reply-form');
        if (form) {
            form.classList.remove('active'); // Hide the reply form
            form.classList.add('d-none'); // Ensure it's hidden with Bootstrap class
            // Find and clear textarea
            const textarea = form.querySelector('textarea');
            if (textarea) textarea.value = '';
        }
    }

    // New class method for showing reply form
    showReplyForm(commentId) {
        console.log(`Attempting to show reply form for commentId: ${commentId}`);

        // Hide all other reply forms by removing 'active' class
        document.querySelectorAll('.reply-form').forEach(form => {
            const formCommentId = form.dataset.commentId;
            // Only hide if active AND it's not the target form
            if (formCommentId != commentId && form.classList.contains('active')) {
                console.log(`Hiding form: `, form);
                form.classList.remove('active');
                form.classList.add('d-none'); // Add d-none back when hiding other forms
            }
        });

        // Show this reply form by selecting based on its UNIQUE ID
        const replyForm = document.getElementById(`replyForm${commentId}`);
        if (replyForm) {
            console.log(`Found reply form: `, replyForm);
            console.log(`Classes before toggle: `, replyForm.classList.value);

            // If it's currently hidden (has d-none), remove d-none and add active.
            // If it's currently visible (has active), remove active and add d-none.
            if (replyForm.classList.contains('d-none')) {
                replyForm.classList.remove('d-none');
                replyForm.classList.add('active');
            } else {
                replyForm.classList.remove('active');
                replyForm.classList.add('d-none');
            }

            console.log(`Classes after toggle: `, replyForm.classList.value);

            if (replyForm.classList.contains('active')) { // If it's now visible
                replyForm.querySelector('textarea').focus();
            }
        } else {
            console.log(`Reply form with ID "replyForm${commentId}" not found.`);
            // Fallback: If for some reason the ID is missing but data-comment-id is present
            const fallbackReplyForm = document.querySelector(`.reply-form[data-comment-id="${commentId}"]`);
            if (fallbackReplyForm) {
                console.log(`Found reply form using fallback data-comment-id: `, fallbackReplyForm);
                console.log(`Fallback Classes before toggle: `, fallbackReplyForm.classList.value);

                if (fallbackReplyForm.classList.contains('d-none')) {
                    fallbackReplyForm.classList.remove('d-none');
                    fallbackReplyForm.classList.add('active');
                } else {
                    fallbackReplyForm.classList.remove('active');
                    fallbackReplyForm.classList.add('d-none');
                }

                console.log(`Fallback Classes after toggle: `, fallbackReplyForm.classList.value);
                if (fallbackReplyForm.classList.contains('active')) {
                    fallbackReplyForm.querySelector('textarea').focus();
                }
            } else {
                console.log(`Fallback reply form with data-comment-id="${commentId}" also not found.`);
            }
        }
    }
}

// Initialize SignalR Manager
const signalRManager = new SignalRManager();

// Initialize when page loads
document.addEventListener('DOMContentLoaded', async () => {
    await signalRManager.initializeConnections();

    // Join post group if on a post page
    // Store postId in the SignalRManager instance for easy access in reply submissions
    const postIdElement = document.querySelector('[data-post-id]');
    if (postIdElement) {
        signalRManager.pagePostId = parseInt(postIdElement.getAttribute('data-post-id'));
        await signalRManager.joinPostGroup(signalRManager.pagePostId);

        // Initial binding of events for comments already rendered on page load
        signalRManager.rebindCommentEvents();

        // Setup typing indicators for main comment textarea
        const commentTextarea = document.getElementById('mainCommentContent');
        if (commentTextarea) {
            let typingTimer;
            commentTextarea.addEventListener('input', () => {
                signalRManager.startTyping(signalRManager.pagePostId);

                clearTimeout(typingTimer);
                typingTimer = setTimeout(async () => {
                    await signalRManager.stopTyping(signalRManager.pagePostId);
                }, 1000);
            });
        }
    } else {
        console.error("Post ID element not found on the page.");
    }
});

// Update your existing main comment submission
document.getElementById('submitMainComment')?.addEventListener('click', async function () {
    const postId = parseInt(this.getAttribute('data-post-id')); // This should also use signalRManager.pagePostId for consistency
    const commentContent = document.getElementById('mainCommentContent').value;

    if (!commentContent.trim()) {
        signalRManager.showNotification("Please enter a comment.", "error");
        return;
    }

    // Send via SignalR
    await signalRManager.sendComment(postId, commentContent.trim());
    document.getElementById('mainCommentContent').value = '';
});
