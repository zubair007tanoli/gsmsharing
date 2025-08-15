// This class manages all SignalR connections and real-time interactions for posts and comments.
class SignalRManager {
    constructor() {
        this.postConnection = null; // Connection to PostHub
        this.notificationConnection = null; // Connection to NotificationHub
        this.currentPostId = null; // Tracks the postId of the currently viewed page
        this.typingTimeout = null; // Timeout for typing indicators
        this.isTyping = false; // Flag to prevent multiple 'started typing' messages
        this.pagePostId = null; // Stores the postId from the page's data attribute
        this.openShareDropdown = null; // Tracks the currently open share dropdown for click-outside-to-close functionality
        this.connectionCheckInterval = null; // Interval for checking connection state

        // Bind 'this' to event handlers to ensure correct context
        this.handleDocumentClick = this.handleDocumentClick.bind(this);
        this.handleVoteButtonClick = this.handleVoteButtonClick.bind(this);
        this.handleReplyButtonClick = this.handleReplyButtonClick.bind(this);
        this.handleSubmitReplyButtonClick = this.handleSubmitReplyButtonClick.bind(this);
        this.handleReplyCancel = this.handleReplyCancel.bind(this);
        this.handleShareButtonClick = this.handleShareButtonClick.bind(this);
        this.handleShareOptionClick = this.handleShareOptionClick.bind(this);
        this.handlePostVoteButtonClick = this.handlePostVoteButtonClick.bind(this);
        this.handlePollVoteButtonClick = this.handlePollVoteButtonClick.bind(this);
        this.handleDeleteCommentClick = this.handleDeleteCommentClick.bind(this);
        this.handleEditCommentClick = this.handleEditCommentClick.bind(this); // Restored
        this.checkConnectionState = this.checkConnectionState.bind(this);
    }

    // Initializes SignalR connections to both PostHub and NotificationHub.
    async initializeConnections() {
        if (typeof signalR === 'undefined' || !signalR.HubConnectionBuilder) {
            console.error("CRITICAL ERROR: SignalR JavaScript client library is not loaded.");
            this.showNotification("Real-time features not available. SignalR library missing.", 'error');
            return;
        }

        try {
            this.postConnection = new signalR.HubConnectionBuilder()
                .withUrl("/postHub")
                .withAutomaticReconnect()
                .build();

            this.notificationConnection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect()
                .build();
        } catch (buildError) {
            console.error("Error building SignalR connections:", buildError);
            this.showNotification("Failed to set up real-time connections.", 'error');
            return;
        }

        this.postConnection.onclose(error => {
            console.error("SignalR PostHub connection closed.", error);
            this.showNotification("Real-time features disconnected. Reconnecting...", 'error');
            clearInterval(this.connectionCheckInterval);
        });
        this.postConnection.onreconnecting(error => {
            console.warn("SignalR PostHub reconnecting...", error);
            this.showNotification("Real-time features reconnecting...", 'info');
        });
        this.postConnection.onreconnected(connectionId => {
            console.log(`SignalR PostHub reconnected. ID: ${connectionId}`);
            this.showNotification("Real-time features reconnected!", 'success');
            if (this.pagePostId) {
                this.postConnection.invoke("JoinPostGroup", this.pagePostId)
                    .catch(err => console.error(`Error re-joining post group on reconnect:`, err));
            }
            this.startConnectionStateCheck();
        });

        await this.setupPostEventHandlers();
        await this.setupNotificationEventHandlers();

        try {
            await this.postConnection.start();
            console.log("PostHub connection started.");
            await this.notificationConnection.start();
            console.log("NotificationHub connection started.");
            this.startConnectionStateCheck();
        } catch (err) {
            console.error("SignalR connection start failed: ", err);
            this.showNotification("Failed to connect to real-time features.", 'error');
        }

        document.addEventListener('click', this.handleDocumentClick);
    }

    startConnectionStateCheck() {
        if (this.connectionCheckInterval) clearInterval(this.connectionCheckInterval);
        if (this.postConnection && this.notificationConnection) {
            this.connectionCheckInterval = setInterval(this.checkConnectionState, 5000);
        }
    }

    checkConnectionState() {
        const stateMap = { 0: 'Disconnected', 1: 'Connecting', 2: 'Connected', 4: 'Reconnecting' };
        console.log(`SignalR Status: PostHub: ${stateMap[this.postConnection?.state] ?? 'Uninitialized'}, NotificationHub: ${stateMap[this.notificationConnection?.state] ?? 'Uninitialized'}`);
    }

    handleDocumentClick(event) {
        if (this.openShareDropdown && !this.openShareDropdown.contains(event.target)) {
            this.openShareDropdown.classList.remove('active');
            this.openShareDropdown = null;
        }
    }

    async setupPostEventHandlers() {
        this.postConnection.on("ReceiveComment", (commentHtml, commentId, parentCommentId) => {
            try {
                if (parentCommentId) {
                    const parentCommentItem = document.querySelector(`.pd-comment-item[data-comment-id="${parentCommentId}"]`);
                    if (parentCommentItem) {
                        let repliesContainer = parentCommentItem.querySelector('.pd-comment-replies');
                        if (!repliesContainer) {
                            repliesContainer = document.createElement('div');
                            repliesContainer.className = 'pd-comment-replies';
                            parentCommentItem.appendChild(repliesContainer);
                        }
                        repliesContainer.insertAdjacentHTML('beforeend', commentHtml);
                    }
                } else {
                    const commentsContainer = document.querySelector('.comment-list');
                    if (commentsContainer) {
                        commentsContainer.insertAdjacentHTML('beforeend', commentHtml);
                    } else {
                        console.error("Could not find the '.comment-list' container to append new comment.");
                    }
                }
                this.rebindCommentEvents();
            } catch (error) {
                console.error("Error processing received comment:", error);
            }
        });

        this.postConnection.on("CommentEdited", (comment) => { // Restored
            const commentTextElement = document.querySelector(`.pd-comment-item[data-comment-id="${comment.commentId}"] .pd-comment-text`);
            const editedTimestampElement = document.querySelector(`.pd-comment-item[data-comment-id="${comment.commentId}"] .pd-comment-meta .text-muted.small`);
            if (commentTextElement) {
                commentTextElement.innerHTML = comment.content;
            }
            if (editedTimestampElement && !editedTimestampElement.textContent.includes('(edited)')) {
                editedTimestampElement.textContent += ' (edited)';
            }
        });

        this.postConnection.on("CommentDeleted", (commentId) => {
            const commentElement = document.querySelector(`.pd-comment-item[data-comment-id="${commentId}"]`);
            if (commentElement) {
                commentElement.remove();
                this.showNotification("Comment deleted.", "info");
            }
        });

        this.postConnection.on("CommentVoteUpdated", (commentId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote);
        });

        this.postConnection.on("UpdatePostVotesUI", (postId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote);
        });

        this.postConnection.on("ReceivePollUpdate", (updatedVoteCounts) => {
            if (updatedVoteCounts && updatedVoteCounts.length > 0) {
                const postId = updatedVoteCounts[0].postId;
                this.updatePollResultsUI(postId, updatedVoteCounts);
            }
        });

        this.postConnection.on("ReceivePollVoteSuccess", (message) => this.showNotification(message, 'success'));
        this.postConnection.on("ReceivePollVoteError", (errorMessage) => this.showNotification(errorMessage, 'error'));
        this.postConnection.on("VoteError", (errorMessage) => this.showNotification(errorMessage, 'error'));
        this.postConnection.on("UserStartedTyping", (userName) => this.showTypingIndicator(userName));
        this.postConnection.on("UserStoppedTyping", (userName) => this.hideTypingIndicator(userName));
    }

    async setupNotificationEventHandlers() {
        this.notificationConnection.on("ReceiveNotification", (notificationData) => {
            this.showNotification(notificationData.Message, notificationData.Type);
            this.updateNotificationBadge();
        });
        this.notificationConnection.on("UnreadNotificationCount", (count) => this.updateNotificationBadge(count));
    }

    async joinPostGroup(postId) {
        if (this.currentPostId && this.currentPostId !== postId) {
            await this.leavePostGroup(this.currentPostId);
        }
        this.currentPostId = postId;
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            await this.postConnection.invoke("JoinPostGroup", postId).catch(err => console.error(`Error joining post group ${postId}:`, err));
        }
    }

    async leavePostGroup(postId) {
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            await this.postConnection.invoke("LeavePostGroup", postId).catch(err => console.error(err));
        }
        if (this.currentPostId === postId) this.currentPostId = null;
    }

    async sendComment(postId, commentContent, parentCommentId = null) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected.", 'error');
            return;
        }
        await this.postConnection.invoke("SendComment", postId, commentContent, parentCommentId).catch(err => {
            console.error("Error sending comment:", err);
            this.showNotification("Failed to post comment.", 'error');
        });
    }

    // Restored editComment method
    async editComment(commentId, newContent) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected.", 'error');
            return;
        }
        await this.postConnection.invoke("EditComment", commentId, newContent).catch(err => {
            console.error("Error editing comment:", err);
            this.showNotification("Failed to edit comment.", 'error');
        });
    }

    async deleteComment(commentId) {
        this.showConfirmationModal("Are you sure you want to delete this comment?", async () => {
            if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
                this.showNotification("Not connected.", 'error');
                return;
            }
            await this.postConnection.invoke("DeleteComment", commentId).catch(err => {
                console.error("Error deleting comment:", err);
                this.showNotification("Failed to delete comment.", 'error');
            });
        });
    }

    async voteComment(commentId, voteType) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected.", 'error');
            return;
        }
        await this.postConnection.invoke("VoteComment", commentId, voteType).catch(err => {
            console.error("Error voting on comment:", err);
            this.showNotification("Failed to vote on comment.", 'error');
        });
    }

    async votePost(postId, voteType) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected.", 'error');
            return;
        }
        await this.postConnection.invoke("VotePost", postId, voteType).catch(err => {
            console.error("Error voting on post:", err);
            this.showNotification("Failed to vote on post.", 'error');
        });
    }

    async castPollVote(postId, optionId) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected.", 'error');
            return;
        }
        await this.postConnection.invoke("CastPollVote", postId, optionId).catch(err => {
            console.error("Error casting poll vote:", err);
            this.showNotification("Failed to cast your vote.", 'error');
        });
    }

    async startTyping(postId) {
        if (!this.isTyping && this.postConnection.state === signalR.HubConnectionState.Connected) {
            this.isTyping = true;
            await this.postConnection.invoke("StartTyping", postId);
        }
        clearTimeout(this.typingTimeout);
        this.typingTimeout = setTimeout(() => this.stopTyping(postId), 1000);
    }

    async stopTyping(postId) {
        if (this.isTyping && this.postConnection.state === signalR.HubConnectionState.Connected) {
            this.isTyping = false;
            await this.postConnection.invoke("StopTyping", postId);
            clearTimeout(this.typingTimeout);
            this.typingTimeout = null;
        }
    }

    rebindCommentEvents() {
        document.querySelectorAll('.pd-comment-vote-btn').forEach(b => {
            b.removeEventListener('click', this.handleVoteButtonClick);
            b.addEventListener('click', this.handleVoteButtonClick);
        });
        document.querySelectorAll('.pd-comment-reply-btn').forEach(b => {
            b.removeEventListener('click', this.handleReplyButtonClick);
            b.addEventListener('click', this.handleReplyButtonClick);
        });
        document.querySelectorAll('.pd-submit-reply').forEach(b => {
            b.removeEventListener('click', this.handleSubmitReplyButtonClick);
            b.addEventListener('click', this.handleSubmitReplyButtonClick);
        });
        document.querySelectorAll('.pd-cancel-reply').forEach(b => {
            b.removeEventListener('click', this.handleReplyCancel);
            b.addEventListener('click', this.handleReplyCancel);
        });
        document.querySelectorAll('.pd-comment-delete-btn').forEach(b => {
            b.removeEventListener('click', this.handleDeleteCommentClick);
            b.addEventListener('click', this.handleDeleteCommentClick);
        });
        // Restored edit button binding
        document.querySelectorAll('.pd-comment-edit-btn').forEach(b => {
            b.removeEventListener('click', this.handleEditCommentClick);
            b.addEventListener('click', this.handleEditCommentClick);
        });
        this.rebindShareButtons();
        this.rebindPostVoteButtons();
        this.rebindPollVoteButtons();
    }

    handleVoteButtonClick(event) {
        event.preventDefault();
        const btn = event.currentTarget;
        const commentId = parseInt(btn.dataset.commentId);
        const voteTypeString = btn.dataset.voteType;
        const voteType = voteTypeString === 'up' ? 1 : -1;
        this.voteComment(commentId, voteType);
    }

    handleReplyButtonClick(event) {
        event.preventDefault();
        this.showReplyForm(event.currentTarget.dataset.commentId);
    }

    // Restored edit handler
    handleEditCommentClick(event) {
        event.preventDefault();
        const commentId = event.currentTarget.dataset.commentId;
        this.showEditForm(commentId);
    }

    handleDeleteCommentClick(event) {
        event.preventDefault();
        const btn = event.currentTarget;
        this.deleteComment(parseInt(btn.dataset.commentId));
    }

    async handleSubmitReplyButtonClick(event) {
        event.preventDefault();
        const form = event.currentTarget.closest('.pd-reply-form');
        const content = form.querySelector('textarea').value.trim();
        if (content && this.pagePostId) {
            const parentId = parseInt(form.dataset.commentId);
            await this.sendComment(this.pagePostId, content, parentId);
            form.querySelector('textarea').value = '';
            form.style.display = 'none';
        }
    }

    handleReplyCancel(event) {
        event.preventDefault();
        const form = event.currentTarget.closest('.pd-reply-form');
        form.style.display = 'none';
        form.querySelector('textarea').value = '';
    }

    async handlePollVoteButtonClick(event) {
        const button = event.currentTarget;
        await this.castPollVote(parseInt(button.dataset.postId), parseInt(button.dataset.optionId));
    }

    rebindPollVoteButtons() {
        document.querySelectorAll('.poll-option-vote-btn').forEach(button => {
            button.removeEventListener('click', this.handlePollVoteButtonClick);
            button.addEventListener('click', this.handlePollVoteButtonClick);
        });
    }

    handlePostVoteButtonClick(event) {
        event.preventDefault();
        const button = event.currentTarget;
        this.votePost(parseInt(button.dataset.postId), parseInt(button.dataset.voteType));
    }

    rebindPostVoteButtons() {
        document.querySelectorAll('.post-actions .vote-btn').forEach(button => {
            button.removeEventListener('click', this.handlePostVoteButtonClick);
            button.addEventListener('click', this.handlePostVoteButtonClick);
        });
    }

    rebindShareButtons() {
        document.querySelectorAll('.share-button, [id^="shareBtn-"]').forEach(button => {
            button.removeEventListener('click', this.handleShareButtonClick);
            button.addEventListener('click', this.handleShareButtonClick);
        });
        document.querySelectorAll('.share-option').forEach(option => {
            option.removeEventListener('click', this.handleShareOptionClick);
            option.addEventListener('click', this.handleShareOptionClick);
        });
    }

    handleShareButtonClick(event) {
        event.preventDefault();
        event.stopPropagation();
        const dropdown = event.currentTarget.closest('.share-dropdown');
        if (this.openShareDropdown && this.openShareDropdown !== dropdown) {
            this.openShareDropdown.classList.remove('active');
        }
        dropdown.classList.toggle('active');
        this.openShareDropdown = dropdown.classList.contains('active') ? dropdown : null;
    }

    async handleShareOptionClick(event) {
        event.preventDefault();
        event.stopPropagation();
        const platform = event.currentTarget.dataset.platform;
        const postId = event.currentTarget.closest('.share-dropdown-menu').dataset.postId;
        const postUrl = `${window.location.origin}/r/communitySlug/posts/${postId}`;
        const postTitle = document.title;
        let shareUrl = '';

        switch (platform) {
            case 'facebook': shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(postUrl)}`; break;
            case 'twitter': shareUrl = `https://twitter.com/intent/tweet?url=${encodeURIComponent(postUrl)}&text=${encodeURIComponent(postTitle)}`; break;
            case 'reddit': shareUrl = `https://www.reddit.com/submit?url=${encodeURIComponent(postUrl)}&title=${encodeURIComponent(postTitle)}`; break;
            case 'linkedin': shareUrl = `https://www.linkedin.com/shareArticle?mini=true&url=${encodeURIComponent(postUrl)}&title=${encodeURIComponent(postTitle)}`; break;
            case 'whatsapp': shareUrl = `https://api.whatsapp.com/send?text=${encodeURIComponent(postTitle + ' ' + postUrl)}`; break;
            case 'copy':
                try {
                    const ta = document.createElement('textarea');
                    ta.value = postUrl;
                    document.body.appendChild(ta);
                    ta.select();
                    document.execCommand('copy');
                    document.body.removeChild(ta);
                    this.showNotification('Link copied!', 'success');
                } catch (err) { this.showNotification("Failed to copy link.", 'error'); }
                if (this.openShareDropdown) this.openShareDropdown.classList.remove('active');
                this.openShareDropdown = null;
                return;
        }
        if (shareUrl) window.open(shareUrl, '_blank');
        if (this.openShareDropdown) this.openShareDropdown.classList.remove('active');
        this.openShareDropdown = null;
    }

    // --- UI Update Helper Methods ---

    showNotification(message, type = 'info') {
        const container = document.getElementById('toast-container') || (() => {
            const c = document.createElement('div');
            c.id = 'toast-container';
            c.style.cssText = 'position:fixed; top:1rem; right:1rem; z-index:1055;';
            document.body.appendChild(c);
            return c;
        })();
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0`;
        toast.innerHTML = `<div class="d-flex"><div class="toast-body">${message}</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>`;
        container.appendChild(toast);
        const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
        bsToast.show();
        toast.addEventListener('hidden.bs.toast', () => toast.remove());
    }

    showConfirmationModal(message, onConfirm) {
        const modalId = 'customConfirmationModal';
        let modal = document.getElementById(modalId);
        if (!modal) {
            modal = document.createElement('div');
            modal.id = modalId;
            modal.className = 'modal fade';
            modal.innerHTML = `<div class="modal-dialog modal-dialog-centered"><div class="modal-content"><div class="modal-header"><h5 class="modal-title">Confirm Action</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div><div class="modal-body">${message}</div><div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button><button type="button" class="btn btn-danger" id="confirmActionBtn">Confirm</button></div></div></div>`;
            document.body.appendChild(modal);
        } else {
            modal.querySelector('.modal-body').textContent = message;
        }
        const bsModal = new bootstrap.Modal(modal);
        const confirmBtn = document.getElementById('confirmActionBtn');
        const confirmHandler = () => {
            onConfirm();
            bsModal.hide();
            confirmBtn.removeEventListener('click', confirmHandler);
        };
        confirmBtn.addEventListener('click', confirmHandler);
        modal.addEventListener('hidden.bs.modal', () => confirmBtn.removeEventListener('click', confirmHandler), { once: true });
        bsModal.show();
    }

    showReplyForm(commentId) {
        document.querySelectorAll('.pd-reply-form').forEach(form => form.style.display = 'none');
        const replyForm = document.getElementById(`reply-form-${commentId}`);
        if (replyForm) {
            replyForm.style.display = 'block';
            replyForm.querySelector('textarea')?.focus();
        }
    }

    // Restored showEditForm logic
    showEditForm(commentId) {
        const commentItem = document.querySelector(`.pd-comment-item[data-comment-id="${commentId}"]`);
        const commentTextElement = commentItem.querySelector('.pd-comment-text');
        const originalContent = commentTextElement.innerHTML;

        // Create textarea for editing
        const editForm = document.createElement('div');
        editForm.className = 'edit-form mt-2';
        editForm.innerHTML = `
            <textarea class="form-control" rows="3">${originalContent.replace(/<br\s*\/?>/gi, "\n")}</textarea>
            <div class="mt-2">
                <button class="btn btn-sm btn-primary save-edit-btn">Save</button>
                <button class="btn btn-sm btn-outline-secondary cancel-edit-btn">Cancel</button>
            </div>
        `;

        commentTextElement.style.display = 'none';
        commentTextElement.after(editForm);

        editForm.querySelector('.save-edit-btn').addEventListener('click', async () => {
            const newContent = editForm.querySelector('textarea').value.trim();
            if (newContent) {
                await this.editComment(commentId, newContent);
                // The 'CommentEdited' SignalR event will handle updating the UI
            }
            editForm.remove();
            commentTextElement.style.display = 'block';
        });

        editForm.querySelector('.cancel-edit-btn').addEventListener('click', () => {
            editForm.remove();
            commentTextElement.style.display = 'block';
        });
    }

    updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote) {
        const el = document.querySelector(`.pd-comment-item[data-comment-id="${commentId}"]`);
        if (!el) return;
        const scoreEl = el.querySelector(`.pd-comment-vote-count`);
        if (scoreEl) scoreEl.textContent = upvoteCount - downvoteCount;
        el.querySelector('.upvote').classList.toggle('active', currentUserVote === 1);
        el.querySelector('.downvote').classList.toggle('active', currentUserVote === -1);
    }

    updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote) {
        document.getElementById(`upvoteCount-${postId}`).textContent = upvoteCount;
        document.getElementById(`downvoteCount-${postId}`).textContent = `-${downvoteCount}`;
        document.getElementById(`totalScore-${postId}`).textContent = `Score ${upvoteCount - downvoteCount}`;
        document.getElementById(`upvoteBtn-${postId}`).classList.toggle('active', currentUserVote === 1);
        document.getElementById(`downvoteBtn-${postId}`).classList.toggle('active', currentUserVote === -1);
    }

    updatePollResultsUI(postId, pollOptionsData) {
        const pollContainer = document.querySelector(`.poll-container[data-post-id="${postId}"]`);
        if (!pollContainer) return;

        const totalVotes = pollOptionsData.reduce((sum, opt) => sum + opt.voteCount, 0);
        pollContainer.querySelector('.poll-total-votes').textContent = `${totalVotes.toLocaleString()} votes`;

        const votedIndicator = pollContainer.querySelector('.text-success');
        if (!votedIndicator) {
            pollContainer.querySelector('.text-muted')?.remove();
            const newIndicator = document.createElement('p');
            newIndicator.className = 'text-success text-center small mb-3';
            newIndicator.innerHTML = `<i class="fas fa-check-circle"></i> You have voted in this poll`;
            pollContainer.querySelector('.poll-question').after(newIndicator);
        }

        pollOptionsData.forEach(optionData => {
            const optionEl = pollContainer.querySelector(`.poll-option[data-option-id="${optionData.pollOptionId}"]`);
            if (optionEl) {
                const percentage = totalVotes > 0 ? (optionData.voteCount / totalVotes) * 100 : 0;
                optionEl.querySelector('.poll-option-bar').style.width = `${percentage.toFixed(0)}%`;
                optionEl.querySelector('.poll-option-count').textContent = optionData.voteCount;
                optionEl.querySelector('.poll-option-percent').textContent = `${percentage.toFixed(0)}%`;

                const button = optionEl.querySelector('.poll-option-vote-btn');
                if (button) {
                    button.disabled = true;
                    button.style.cursor = 'default';
                }
                optionEl.classList.add('voted');
            }
        });
    }

    showTypingIndicator(userName) {
        const container = document.getElementById('typing-indicators');
        if (container && !document.getElementById(`typing-${userName}`)) {
            const indicator = document.createElement('div');
            indicator.id = `typing-${userName}`;
            indicator.className = 'typing-indicator text-muted small';
            indicator.textContent = `${userName} is typing...`;
            container.appendChild(indicator);
        }
    }

    hideTypingIndicator(userName) {
        document.getElementById(`typing-${userName}`)?.remove();
    }

    updateNotificationBadge(count = null) {
        const badge = document.getElementById('notification-badge');
        if (badge) {
            const currentCount = count ?? (parseInt(badge.textContent) || 0) + 1;
            badge.textContent = currentCount > 99 ? '99+' : currentCount;
            badge.style.display = currentCount > 0 ? 'inline-block' : 'none';
        }
    }
}

// Instantiate and initialize
const signalRManager = new SignalRManager();
window.signalRManager = signalRManager;

document.addEventListener('DOMContentLoaded', async () => {
    await signalRManager.initializeConnections();
    signalRManager.rebindCommentEvents();

    const postIdElement = document.getElementById('pagePostId');
    if (postIdElement) {
        signalRManager.pagePostId = parseInt(postIdElement.value);
        if (!isNaN(signalRManager.pagePostId)) {
            await signalRManager.joinPostGroup(signalRManager.pagePostId);
        }
    }

    const commentTextarea = document.getElementById('mainCommentContent');
    if (commentTextarea) {
        commentTextarea.addEventListener('input', () => {
            if (signalRManager.pagePostId) signalRManager.startTyping(signalRManager.pagePostId);
        });
    }

    document.getElementById('submitMainComment')?.addEventListener('click', async () => {
        const content = document.getElementById('mainCommentContent').value;
        if (content.trim() && signalRManager.pagePostId) {
            await signalRManager.sendComment(signalRManager.pagePostId, content.trim());
            document.getElementById('mainCommentContent').value = '';
        }
    });
});
