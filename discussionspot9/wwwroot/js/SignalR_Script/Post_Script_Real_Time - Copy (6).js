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
        // Track the currently open share dropdown for click-outside-to-close functionality
        this.openShareDropdown = null;

        // Bind the click-outside handler to 'this'
        this.handleDocumentClick = this.handleDocumentClick.bind(this);
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

        // Add a global click listener to close dropdowns when clicking outside
        document.addEventListener('click', this.handleDocumentClick);
    }

    // Handle clicks anywhere on the document to close dropdowns
    handleDocumentClick(event) {
        if (this.openShareDropdown && !this.openShareDropdown.contains(event.target)) {
            // Clicked outside the currently open dropdown, close it
            console.log("Document click: Closing share dropdown.");
            this.openShareDropdown.classList.remove('active'); // Remove 'active' from the parent dropdown
            this.openShareDropdown = null;
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
        this.postConnection.on("CommentVoteUpdated", (commentId, upvoteCount, downvoteCount) => {
            this.updateCommentVotes({
                CommentId: commentId,
                Score: upvoteCount - downvoteCount, // Assuming score is upvotes - downvotes
                CurrentUserVote: 0 // Placeholder, as CurrentUserVote is not passed here directly
            });
        });

        // Post vote count updated - This is the method for post voting
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

        // New: PostVoteError for handling voting errors specifically
        this.postConnection.on("VoteError", (errorMessage) => {
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
            console.log(`SignalR: Attempting to send comment to Hub. PostId: ${postId}, Content: "${content}", ParentCommentId: ${parentCommentId}`);
            await this.postConnection.invoke("SendComment", postId, content, parentCommentId);
            console.log("SignalR: SendComment invoked successfully.");
        } catch (err) {
            console.error("SignalR: Error sending comment via invoke:", err);
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
            // SignalR Hub's VoteComment takes (commentId, voteType)
            await this.postConnection.invoke("VoteComment", commentId, isUpvote ? 1 : -1);
        } catch (err) {
            console.error("Error voting comment:", err);
            this.showNotification("Failed to vote on comment.", 'error'); // More specific notification
        }
    }

    // New method: Vote on a Post
    async votePost(postId, voteType) {
        try {
            console.log(`SignalR: Attempting to vote on post. PostId: ${postId}, VoteType: ${voteType}`);
            await this.postConnection.invoke("VotePost", postId, voteType);
            console.log("SignalR: VotePost invoked successfully.");
        } catch (err) {
            console.error("SignalR: Error voting on post via invoke:", err);
            this.showNotification("Failed to vote on post.", 'error'); // More specific notification
        }
    }

    async startTyping(postId) {
        if (!this.isTyping) {
            this.isTyping = true;
            await this.postConnection.invoke("StartTyping", postId);
        }

        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

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

    addReplyToParent(replyHtml, parentCommentId) {
        const parentComment = document.querySelector(`.comment-item[data-comment-id="${parentCommentId}"]`);
        if (!parentComment) {
            console.error(`Parent comment with ID ${parentCommentId} not found`);
            return;
        }

        let repliesContainer = parentComment.querySelector('.comment-children');
        if (!repliesContainer) {
            repliesContainer = document.createElement('div');
            repliesContainer.className = 'comment-children mt-3';
            parentComment.appendChild(repliesContainer);
        }

        repliesContainer.insertAdjacentHTML('beforeend', replyHtml);
    }

    updateCommentCount() {
        const commentCountElement = document.querySelector('.comment-count');
        if (commentCountElement) {
            const totalComments = document.querySelectorAll('.comment-item').length;
            commentCountElement.textContent = `${totalComments} Comments`;
        }
    }

    updateCommentInUI(commentHtml, commentId) {
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (commentElement) {
            commentElement.outerHTML = commentHtml;
        }
    }

    removeCommentFromUI(commentId) {
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (commentElement) {
            commentElement.remove();
        }
    }

    updateCommentVotes(voteData) {
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${voteData.CommentId}"]`);
        if (commentElement) {
            const scoreElement = commentElement.querySelector('.comment-vote-count'); // Corrected selector for comment votes
            if (scoreElement) scoreElement.textContent = voteData.Score;

            const upvoteBtn = commentElement.querySelector('.comment-vote-btn.upvote'); // Use more specific class
            const downvoteBtn = commentElement.querySelector('.comment-vote-btn.downvote'); // Use more specific class

            // Assuming voteData.CurrentUserVote is 1 for upvote, -1 for downvote, 0 for no vote
            upvoteBtn?.classList.toggle('active', voteData.CurrentUserVote === 1);
            downvoteBtn?.classList.toggle('active', voteData.CurrentUserVote === -1);
        }
    }

    updatePostVoteCount(newCount) {
        console.log(`updatePostVoteCount: Updating post vote count to: ${newCount}`); // Debugging
        const voteCountElement = document.getElementById('voteCount-' + this.pagePostId); // Ensure correct ID
        if (voteCountElement) {
            voteCountElement.textContent = newCount;
        }

        // You may also want to update the active state of the upvote/downvote buttons for the post itself
        // This requires getting the CurrentUserVote for the post, which is not currently passed in UpdateVoteCount.
        // If your server sends the current user's vote status for the post, you would update it here similarly to updateCommentVotes.
        // For now, only the score is updated.
    }

    showTypingIndicator(userName) {
        const typingContainer = document.getElementById('typing-indicators');
        if (typingContainer) {
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
                const currentCount = parseInt(badge.textContent) || 0;
                badge.textContent = currentCount + 1;
                badge.style.display = 'inline';
            }
        }
    }

    showNotification(message, type = 'info') {
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
        if (!toastContainer) {
            const newToastContainer = document.createElement('div');
            newToastContainer.id = 'toast-container';
            newToastContainer.style.position = 'fixed';
            newToastContainer.style.top = '1rem';
            newToastContainer.style.right = '1rem';
            newToastContainer.style.zIndex = '1050';
            document.body.appendChild(newToastContainer);
            newToastContainer.appendChild(toast);
        } else {
            toastContainer.appendChild(toast);
        }

        const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    // Method to re-bind all events for dynamically added/updated elements
    rebindCommentEvents() {
        // Re-bind comment vote buttons
        document.querySelectorAll('.comment-vote-btn').forEach(button => {
            button.removeEventListener('click', this.handleVoteButtonClick);
            button.addEventListener('click', this.handleVoteButtonClick);
        });

        // Re-bind reply buttons
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
            button.removeEventListener('click', this.handleSubmitReplyButtonClick);
            button.addEventListener('click', this.handleSubmitReplyButtonClick);
        });

        // Re-bind cancel reply buttons
        document.querySelectorAll('.cancel-reply, .reply-cancel-btn').forEach(button => {
            button.removeEventListener('click', this.handleCancelReplyClick);
            button.addEventListener('click', this.handleCancelReplyClick);
        });

        // Rebind share buttons
        this.rebindShareButtons();

        // New: Rebind Post Vote Buttons
        this.rebindPostVoteButtons();
    }

    // Event handlers for rebindCommentEvents (defined as arrow functions to preserve 'this')
    handleVoteButtonClick = (e) => {
        e.preventDefault();
        const commentId = e.currentTarget.dataset.commentId;
        const voteType = parseInt(e.currentTarget.dataset.voteType); // 1 for upvote, -1 for downvote
        this.voteComment(commentId, voteType === 1); // Pass true for upvote, false for downvote
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

    handleSubmitReplyButtonClick = async (event) => {
        console.log("handleSubmitReplyButtonClick triggered.");
        event.preventDefault();
        const replyFormDiv = event.currentTarget.closest('.reply-form');

        if (!replyFormDiv) {
            console.error("Could not find parent .reply-form div for reply submit button.");
            this.showNotification("Error: Could not submit reply.", "error");
            return;
        }

        const parentCommentId = replyFormDiv.dataset.commentId;
        const contentTextarea = replyFormDiv.querySelector('textarea');
        const content = contentTextarea ? contentTextarea.value : '';

        console.log(`Reply content: "${content}"`);
        console.log(`Parent Comment ID (from data-comment-id): ${parentCommentId}`);

        if (!content.trim()) {
            this.showNotification("Please enter a reply.", "error");
            return;
        }

        if (this.pagePostId === null) {
            console.error("Post ID not available for reply submission. this.pagePostId is null.");
            this.showNotification("Error: Post ID missing for reply.", "error");
            return;
        }
        console.log(`Page Post ID: ${this.pagePostId}`);

        await this.sendComment(
            parseInt(this.pagePostId),
            content.trim(),
            parentCommentId ? parseInt(parentCommentId) : null
        );

        if (contentTextarea) contentTextarea.value = '';
        replyFormDiv.classList.remove('active');
        replyFormDiv.classList.add('d-none');
    }

    handleCancelReplyClick = (e) => {
        e.preventDefault();
        const form = e.target.closest('.reply-form');
        if (form) {
            form.classList.remove('active');
            form.classList.add('d-none');
            const textarea = form.querySelector('textarea');
            if (textarea) textarea.value = '';
        }
    }

    showReplyForm(commentId) {
        console.log(`Attempting to show reply form for commentId: ${commentId}`);

        document.querySelectorAll('.reply-form').forEach(form => {
            const formCommentId = form.dataset.commentId;
            if (formCommentId != commentId && form.classList.contains('active')) {
                console.log(`Hiding form: `, form);
                form.classList.remove('active');
                form.classList.add('d-none');
            }
        });

        const replyForm = document.getElementById(`replyForm${commentId}`);
        if (replyForm) {
            const isCurrentlyHidden = replyForm.classList.contains('d-none');
            console.log(`Found reply form: `, replyForm);
            console.log(`Classes before explicit toggle: `, replyForm.classList.value);

            if (isCurrentlyHidden) {
                replyForm.classList.remove('d-none');
                replyForm.classList.add('active');
            } else {
                replyForm.classList.remove('active');
                replyForm.classList.add('d-none');
            }

            console.log(`Classes after explicit toggle: `, replyForm.classList.value);

            if (replyForm.classList.contains('active')) {
                replyForm.querySelector('textarea').focus();
            }
        } else {
            console.log(`Reply form with ID "replyForm${commentId}" not found.`);
            const fallbackReplyForm = document.querySelector(`.reply-form[data-comment-id="${commentId}"]`);
            if (fallbackReplyForm) {
                console.log(`Found reply form using fallback data-comment-id: `, fallbackReplyForm);
                console.log(`Fallback Classes before explicit toggle: `, fallbackReplyForm.classList.value);

                const isFallbackCurrentlyHidden = fallbackReplyForm.classList.contains('d-none');
                if (isFallbackCurrentlyHidden) {
                    fallbackReplyForm.classList.remove('d-none');
                    fallbackReplyForm.classList.add('active');
                } else {
                    fallbackReplyForm.classList.remove('active');
                    fallbackReplyForm.classList.add('d-none');
                }

                console.log(`Fallback Classes after explicit toggle: `, fallbackReplyForm.classList.value);
                if (fallbackReplyForm.classList.contains('active')) {
                    fallbackReplyForm.querySelector('textarea').focus();
                }
            } else {
                console.log(`Fallback reply form with data-comment-id="${commentId}" also not found.`);
            }
        }
    }

    rebindShareButtons() {
        console.log("rebindShareButtons: Attempting to bind share buttons.");
        document.querySelectorAll('.share-dropdown .action-btn').forEach((button, index) => {
            if (button.querySelector('.fas.fa-share')) {
                button.removeEventListener('click', this.handleShareButtonClick);
                button.addEventListener('click', this.handleShareButtonClick);
                console.log(`rebindShareButtons: Bound click handler to Share button with id: ${button.id}`);
            } else {
                console.log(`rebindShareButtons: Skipping action-btn, not the primary share toggle:`, button);
            }
        });

        document.querySelectorAll('.share-option').forEach(option => {
            option.removeEventListener('click', this.handleShareOptionClick);
            option.addEventListener('click', this.handleShareOptionClick);
            console.log(`rebindShareButtons: Bound click handler to share option:`, option);
        });
    }

    handleShareButtonClick = (event) => {
        console.log("handleShareButtonClick: Share button clicked.");
        event.preventDefault();
        event.stopPropagation();

        const shareDropdown = event.currentTarget.closest('.share-dropdown');
        if (!shareDropdown) {
            console.error("handleShareButtonClick: Share dropdown parent not found.");
            return;
        }

        if (this.openShareDropdown && this.openShareDropdown !== shareDropdown) {
            console.log("handleShareButtonClick: Closing previously open dropdown.");
            this.openShareDropdown.classList.remove('active');
        }

        console.log(`handleShareButtonClick: Parent dropdown classes before toggle: ${shareDropdown.classList.value}`);
        shareDropdown.classList.toggle('active');
        console.log(`handleShareButtonClick: Parent dropdown classes after toggle: ${shareDropdown.classList.value}`);

        this.openShareDropdown = shareDropdown.classList.contains('active') ? shareDropdown : null;
        console.log(`handleShareButtonClick: openShareDropdown is now:`, this.openShareDropdown);
    }

    handleShareOptionClick = (event) => {
        console.log("handleShareOptionClick: Share option clicked.");
        event.preventDefault();
        event.stopPropagation();

        const platform = event.currentTarget.dataset.platform;
        const postLink = window.location.href;

        console.log(`handleShareOptionClick: Platform: ${platform}, Link: ${postLink}`);

        switch (platform) {
            case 'facebook':
                window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(postLink)}`, '_blank');
                break;
            case 'twitter':
                window.open(`https://twitter.com/intent/tweet?url=${encodeURIComponent(postLink)}&text=${encodeURIComponent(document.title)}`, '_blank');
                break;
            case 'reddit':
                window.open(`https://www.reddit.com/submit?url=${encodeURIComponent(postLink)}&title=${encodeURIComponent(document.title)}`, '_blank');
                break;
            case 'linkedin':
                window.open(`https://www.linkedin.com/shareArticle?mini=true&url=${encodeURIComponent(postLink)}&title=${encodeURIComponent(document.title)}`, '_blank');
                break;
            case 'whatsapp':
                window.open(`https://api.whatsapp.com/send?text=${encodeURIComponent(document.title + ' ' + postLink)}`, '_blank');
                break;
            case 'copy':
                this.copyToClipboard(postLink);
                this.showNotification('Link copied to clipboard!', 'success');
                break;
            default:
                console.warn('Unknown share platform:', platform);
        }

        const shareDropdown = event.currentTarget.closest('.share-dropdown');
        if (shareDropdown) {
            console.log("handleShareOptionClick: Closing dropdown after selection.");
            shareDropdown.classList.remove('active');
            this.openShareDropdown = null;
        }
    }

    copyToClipboard(text) {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();
        try {
            document.execCommand('copy');
        } catch (err) {
            console.error('Failed to copy text: ', err);
            this.showNotification('Failed to copy link. Please copy manually.', 'error');
        }
        document.body.removeChild(textarea);
    }

    // New: Method to bind Post Vote Buttons
    rebindPostVoteButtons() {
        console.log("rebindPostVoteButtons: Attempting to bind post vote buttons.");
        document.querySelectorAll('.post-actions .vote-btn').forEach(button => {
            button.removeEventListener('click', this.handlePostVoteButtonClick);
            button.addEventListener('click', this.handlePostVoteButtonClick);
            console.log(`rebindPostVoteButtons: Bound click handler to post vote button with id: ${button.id}`);
        });
    }

    // New: Handler for Post Vote Button Click
    handlePostVoteButtonClick = (event) => {
        console.log("handlePostVoteButtonClick: Post vote button clicked.");
        event.preventDefault();

        const postId = event.currentTarget.dataset.postId;
        const voteType = parseInt(event.currentTarget.dataset.voteType); // 1 for upvote, -1 for downvote

        if (!postId) {
            console.error("handlePostVoteButtonClick: Post ID not found for voting.");
            this.showNotification("Error: Could not vote on post. Post ID missing.", "error");
            return;
        }

        this.votePost(parseInt(postId), voteType);
    }
}

// Initialize SignalR Manager
const signalRManager = new SignalRManager();

// Initialize when page loads
document.addEventListener('DOMContentLoaded', async () => {
    await signalRManager.initializeConnections();

    const postIdElement = document.querySelector('[data-post-id]');
    if (postIdElement) {
        signalRManager.pagePostId = parseInt(postIdElement.getAttribute('data-post-id'));
        console.log(`DOMContentLoaded: Page Post ID set to: ${signalRManager.pagePostId}`);
        await signalRManager.joinPostGroup(signalRManager.pagePostId);

        signalRManager.rebindCommentEvents(); // This now includes rebindShareButtons() and rebindPostVoteButtons()

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
        console.error("Post ID element not found on the page. Cannot set signalRManager.pagePostId.");
    }
});

document.getElementById('submitMainComment')?.addEventListener('click', async function () {
    const postId = signalRManager.pagePostId;
    const commentContent = document.getElementById('mainCommentContent').value;

    if (!commentContent.trim()) {
        signalRManager.showNotification("Please enter a comment.", "error");
        return;
    }

    if (postId === null) {
        console.error("Main comment submission: Post ID is null.");
        signalRManager.showNotification("Error: Post ID missing for main comment.", "error");
        return;
    }

    await signalRManager.sendComment(postId, commentContent.trim());
    document.getElementById('mainCommentContent').value = '';
});
