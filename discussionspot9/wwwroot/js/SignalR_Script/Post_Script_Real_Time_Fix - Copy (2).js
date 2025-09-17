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
        this.handlePollVote = this.handlePollVote.bind(this);
        this.handleDeleteCommentClick = this.handleDeleteCommentClick.bind(this);
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

        // Setup lifecycle events
        this.postConnection.onclose(error => {
            console.error("SignalR PostHub connection closed.", error);
            this.showNotification("Real-time features disconnected.", 'error');
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
                this.joinPostGroup(this.pagePostId).catch(err => console.error(`Error re-joining post group on reconnect:`, err));
            }
            this.startConnectionStateCheck();
        });

        // Setup message handlers
        await this.setupPostEventHandlers();
        await this.setupNotificationEventHandlers();

        try {
            await this.postConnection.start();
            console.log("PostHub connection started successfully.");
            await this.notificationConnection.start();
            console.log("NotificationHub connection started successfully.");
            this.startConnectionStateCheck();
        } catch (err) {
            console.error("SignalR connection start failed: ", err);
            this.showNotification("Failed to connect to real-time features. Please refresh the page.", 'error');
        }

        document.addEventListener('click', this.handleDocumentClick);
    }

    startConnectionStateCheck() {
        if (this.connectionCheckInterval) clearInterval(this.connectionCheckInterval);
        if (this.postConnection && this.notificationConnection) {
            this.connectionCheckInterval = setInterval(this.checkConnectionState, 10000); // Check every 10 seconds
        }
    }

    checkConnectionState() {
        const stateMap = { 0: 'Disconnected', 1: 'Connecting', 2: 'Connected', 4: 'Reconnecting' };
        const postState = this.postConnection ? stateMap[this.postConnection.state] || 'Unknown' : 'Uninitialized';
        const notifState = this.notificationConnection ? stateMap[this.notificationConnection.state] || 'Unknown' : 'Uninitialized';
        console.log(`SignalR Status: PostHub: ${postState}, NotificationHub: ${notifState}`);
    }

    handleDocumentClick(event) {
        if (this.openShareDropdown && !this.openShareDropdown.contains(event.target)) {
            this.openShareDropdown.classList.remove('active');
            this.openShareDropdown = null;
        }
    }

    async setupPostEventHandlers() {
        // --- COMMENT HANDLERS ---
        this.postConnection.on("ReceiveComment", (commentHtml, commentId, parentCommentId) => {
            try {
                if (parentCommentId) {
                    const parentCommentItem = document.querySelector(`.comment-item[data-comment-id="${parentCommentId}"]`);
                    if (parentCommentItem) {
                        let repliesContainer = parentCommentItem.querySelector('.comment-replies');
                        if (!repliesContainer) {
                            repliesContainer = document.createElement('div');
                            repliesContainer.className = 'comment-replies';
                            parentCommentItem.querySelector('.comment-content').after(repliesContainer);
                        }
                        repliesContainer.insertAdjacentHTML('beforeend', commentHtml);
                    }
                } else {
                    document.querySelector('.comment-list')?.insertAdjacentHTML('beforeend', commentHtml);
                }
                this.rebindAllEvents(); // Rebind for new comment
            } catch (error) {
                console.error("Error processing received comment:", error);
            }
        });

        this.postConnection.on("CommentDeleted", (commentId) => {
            document.querySelector(`.comment-item[data-comment-id="${commentId}"]`)?.remove();
            this.showNotification("Comment deleted.", "info");
        });

        // --- VOTE HANDLERS ---
        this.postConnection.on("CommentVoteUpdated", (commentId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote);
        });

        this.postConnection.on("UpdatePostVotesUI", (postId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote);
        });

        // --- POLL HANDLER ---
        this.postConnection.on("ReceivePollUpdate", (pollData) => {
            this.updatePollResultsUI(pollData);
        });

        // --- ERROR & NOTIFICATION HANDLERS ---
        this.postConnection.on("VoteError", (errorMessage) => this.showNotification(errorMessage, 'error'));
        this.postConnection.on("CommentError", (errorMessage) => this.showNotification(errorMessage, 'error'));
        this.postConnection.on("ReceivePollVoteSuccess", (message) => this.showNotification(message, 'success'));
        this.postConnection.on("ReceivePollVoteError", (errorMessage) => this.showNotification(errorMessage, 'error'));

        // --- TYPING INDICATOR HANDLERS ---
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

    // --- HUB INVOCATION METHODS ---

    async joinPostGroup(postId) {
        if (this.currentPostId && this.currentPostId !== postId) {
            await this.leavePostGroup(this.currentPostId);
        }
        this.currentPostId = postId;
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            try {
                await this.postConnection.invoke("JoinPostGroup", postId);
                console.log(`Successfully joined post group ${postId}`);
            } catch (err) {
                console.error(`Error invoking JoinPostGroup for post-${postId}:`, err);
            }
        } else {
            console.warn(`Cannot join group post-${postId}. Connection not established.`);
        }
    }

    async leavePostGroup(postId) {
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            await this.postConnection.invoke("LeavePostGroup", postId).catch(err => console.error(err));
        }
        if (this.currentPostId === postId) this.currentPostId = null;
    }

    async sendComment(postId, commentContent, parentCommentId = null) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) return this.showNotification("Not connected.", 'error');
        await this.postConnection.invoke("SendComment", postId, commentContent, parentCommentId).catch(err => this.showNotification("Failed to post comment.", 'error'));
    }

    async deleteComment(commentId) {
        this.showConfirmationModal("Are you sure you want to delete this comment?", async () => {
            if (this.postConnection.state !== signalR.HubConnectionState.Connected) return this.showNotification("Not connected.", 'error');
            await this.postConnection.invoke("DeleteComment", commentId).catch(err => this.showNotification("Failed to delete comment.", 'error'));
        });
    }

    async voteComment(commentId, voteType) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) return this.showNotification("Not connected.", 'error');
        await this.postConnection.invoke("VoteComment", commentId, voteType).catch(err => this.showNotification("Failed to vote on comment.", 'error'));
    }

    async votePost(postId, voteType) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) return this.showNotification("Not connected.", 'error');
        await this.postConnection.invoke("VotePost", postId, voteType).catch(err => this.showNotification("Failed to vote on post.", 'error'));
    }

    async castPollVote(postId, optionId) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) return this.showNotification("Not connected.", 'error');
        await this.postConnection.invoke("CastPollVote", postId, optionId).catch(err => this.showNotification("Failed to cast your vote.", 'error'));
    }

    async startTyping(postId) {
        if (!this.isTyping && this.postConnection.state === signalR.HubConnectionState.Connected) {
            this.isTyping = true;
            await this.postConnection.invoke("StartTyping", postId);
        }
        clearTimeout(this.typingTimeout);
        this.typingTimeout = setTimeout(() => this.stopTyping(postId), 1500); // 1.5 second timeout
    }

    async stopTyping(postId) {
        if (this.isTyping && this.postConnection.state === signalR.HubConnectionState.Connected) {
            this.isTyping = false;
            await this.postConnection.invoke("StopTyping", postId);
            clearTimeout(this.typingTimeout);
            this.typingTimeout = null;
        }
    }

    // --- EVENT BINDING & HANDLING ---

    rebindAllEvents() {
        // Comment-related buttons
        document.querySelectorAll('.comment-vote-btn').forEach(b => {
            b.removeEventListener('click', this.handleVoteButtonClick);
            b.addEventListener('click', this.handleVoteButtonClick);
        });
        document.querySelectorAll('.comment-reply-btn').forEach(b => {
            b.removeEventListener('click', this.handleReplyButtonClick);
            b.addEventListener('click', this.handleReplyButtonClick);
        });
        document.querySelectorAll('.reply-submit-btn').forEach(b => {
            b.removeEventListener('click', this.handleSubmitReplyButtonClick);
            b.addEventListener('click', this.handleSubmitReplyButtonClick);
        });
        document.querySelectorAll('.reply-cancel-btn').forEach(b => {
            b.removeEventListener('click', this.handleReplyCancel);
            b.addEventListener('click', this.handleReplyCancel);
        });
        document.querySelectorAll('.comment-delete-btn').forEach(b => {
            b.removeEventListener('click', this.handleDeleteCommentClick);
            b.addEventListener('click', this.handleDeleteCommentClick);
        });

        // Post-level buttons
        this.rebindShareButtons();
        this.rebindPostVoteButtons();
        this.rebindPollVoteButtons();
    }

    handleVoteButtonClick(event) {
        event.preventDefault();
        const btn = event.currentTarget;
        const commentId = parseInt(btn.dataset.commentId);
        const voteType = parseInt(btn.dataset.voteType);
        this.voteComment(commentId, voteType);
    }

    handleReplyButtonClick(event) {
        event.preventDefault();
        this.showReplyForm(event.currentTarget.dataset.commentId);
    }

    handleDeleteCommentClick(event) {
        event.preventDefault();
        this.deleteComment(parseInt(event.currentTarget.dataset.commentId));
    }

    async handleSubmitReplyButtonClick(event) {
        event.preventDefault();
        const form = event.currentTarget.closest('.reply-form');
        const content = form.querySelector('textarea').value.trim();
        if (content && this.pagePostId) {
            const parentId = parseInt(form.dataset.commentId);
            await this.sendComment(this.pagePostId, content, parentId);
            form.querySelector('textarea').value = '';
            form.classList.add('d-none');
            form.classList.remove('d-flex');
        }
    }

    handleReplyCancel(event) {
        event.preventDefault();
        const form = event.currentTarget.closest('.reply-form');
        form.classList.add('d-none');
        form.classList.remove('d-flex');
        form.querySelector('textarea').value = '';
    }

    // **FIX:** This handler is now more robust.
    async handlePollVote(event) {
        // It finds the closest '.poll-option' container from the click target.
        const optionContainer = event.target.closest('.poll-option');
        if (!optionContainer) return; // Click was outside any option, do nothing.

        // It then finds the main '.poll-container' to reliably get the Post ID.
        const pollContainer = optionContainer.closest('.poll-container');
        if (!pollContainer) return; // Should not happen if HTML is correct.

        // Get IDs from the reliable parent containers, not the click target itself.
        const postId = parseInt(pollContainer.dataset.postId, 10);
        const optionId = parseInt(optionContainer.dataset.optionId, 10);

        // The validation remains the same.
        if (isNaN(postId) || isNaN(optionId) || postId <= 0 || optionId <= 0) {
            console.error("Invalid Post ID or Option ID from data attributes.", {
                postId: pollContainer.dataset.postId,
                optionId: optionContainer.dataset.optionId
            });
            this.showNotification("Could not cast vote due to an internal error.", 'error');
            return;
        }

        await this.castPollVote(postId, optionId);
    }

    rebindPollVoteButtons() {
        // The event delegation setup remains correct.
        const pollOptionsContainer = document.getElementById('pollOptions');
        if (pollOptionsContainer) {
            pollOptionsContainer.removeEventListener('click', this.handlePollVote);
            pollOptionsContainer.addEventListener('click', this.handlePollVote);
        }
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
        document.querySelectorAll('.share-dropdown .action-btn').forEach(button => {
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
                    await navigator.clipboard.writeText(postUrl);
                    this.showNotification('Link copied!', 'success');
                } catch (err) { this.showNotification("Failed to copy link.", 'error'); }
                if (this.openShareDropdown) this.openShareDropdown.classList.remove('active');
                return;
        }
        if (shareUrl) window.open(shareUrl, '_blank');
        if (this.openShareDropdown) this.openShareDropdown.classList.remove('active');
        this.openShareDropdown = null;
    }

    // --- UI UPDATE & HELPER METHODS ---

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
        const bsToast = new bootstrap.Toast(toast, { delay: 4000 });
        bsToast.show();
        toast.addEventListener('hidden.bs.toast', () => toast.remove());
    }

    showConfirmationModal(message, onConfirm) {
        let modal = document.getElementById('customConfirmationModal');
        if (!modal) {
            modal = document.createElement('div');
            modal.id = 'customConfirmationModal';
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
        };
        confirmBtn.addEventListener('click', confirmHandler, { once: true });
        modal.addEventListener('hidden.bs.modal', () => confirmBtn.removeEventListener('click', confirmHandler), { once: true });
        bsModal.show();
    }

    showReplyForm(commentId) {
        document.querySelectorAll('.reply-form').forEach(form => {
            form.classList.add('d-none');
            form.classList.remove('d-flex');
        });

        const replyForm = document.getElementById(`replyForm${commentId}`);
        if (replyForm) {
            replyForm.classList.remove('d-none');
            replyForm.classList.add('d-flex');
            replyForm.querySelector('textarea')?.focus();
        }
    }

    updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote) {
        const el = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (!el) return;

        const scoreEl = el.querySelector(`.comment-vote-count`);
        if (scoreEl) scoreEl.textContent = upvoteCount - downvoteCount;

        el.querySelector('.comment-vote-btn.upvote')?.classList.toggle('active', currentUserVote === 1);
        el.querySelector('.comment-vote-btn.downvote')?.classList.toggle('active', currentUserVote === -1);
    }

    updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote) {
        const upvoteCountElement = document.getElementById(`upvoteCount-${postId}`);
        const downvoteCountElement = document.getElementById(`downvoteCount-${postId}`);
        const totalScoreElement = document.getElementById(`totalScore-${postId}`);
        const upvoteBtn = document.getElementById(`upvoteBtn-${postId}`);
        const downvoteBtn = document.getElementById(`downvoteBtn-${postId}`);

        if (upvoteCountElement) upvoteCountElement.textContent = upvoteCount;
        if (downvoteCountElement) downvoteCountElement.textContent = `-${downvoteCount}`;
        if (totalScoreElement) totalScoreElement.textContent = `Score ${upvoteCount - downvoteCount}`;

        if (upvoteBtn) upvoteBtn.classList.toggle('active', currentUserVote === 1);
        if (downvoteBtn) downvoteBtn.classList.toggle('active', currentUserVote === -1);
    }


    // Replace this entire function in your file with the corrected version below.
    updatePollResultsUI(pollData) {
        const { postId, options, totalVotes, hasUserVoted } = pollData;
        const pollContainer = document.querySelector(`.poll-container[data-post-id="${postId}"]`);
        if (!pollContainer) return;

        // Update total votes display
        const totalVotesEl = pollContainer.querySelector('.poll-total-votes');
        if (totalVotesEl) {
            totalVotesEl.textContent = `${totalVotes.toLocaleString()} votes`;
        }

        // If the user has voted, ensure the "You have voted" message is visible
        if (hasUserVoted) {
            // Remove the initial "Click to cast..." message if it exists
            const initialMessage = pollContainer.querySelector('.text-muted.text-center');
            if (initialMessage) initialMessage.remove();

            // Add the "You have voted" message if it doesn't exist
            if (!pollContainer.querySelector('.text-success.text-center')) {
                const votedText = document.createElement('p');
                votedText.className = 'text-success text-center small mb-3';
                votedText.innerHTML = `<i class="fas fa-check-circle"></i> You have voted in this poll`;
                pollContainer.querySelector('.poll-question').after(votedText);
            }
        }

        // Loop through each poll option to update its display
        options.forEach(optionData => {
            const optionEl = pollContainer.querySelector(`.poll-option[data-option-id="${optionData.pollOptionId}"]`);
            if (optionEl) {
                const percentage = totalVotes > 0 ? (optionData.voteCount / totalVotes) * 100 : 0;

                // FIX 1: Changed selector from '.poll-option-bar' to '.poll-progress'
                const bar = optionEl.querySelector('.poll-progress');
                if (bar) bar.style.width = `${percentage.toFixed(0)}%`;

                // FIX 2: Updated selectors and content for vote counts and percentages.
                // Your new HTML combines this into one element, so we'll target that.
                const statsEl = optionEl.querySelector('.poll-percentage, .poll-stats-inline'); // Target both old and new for robustness
                if (statsEl) {
                    // The view now shows different text based on voted state. We need to handle this.
                    // This will update the stats for everyone viewing the poll.
                    const statsContainer = statsEl.parentElement;
                    statsContainer.innerHTML = `
                    <span class="poll-option-text">${optionData.optionText}</span>
                    <div class="poll-stats-inline">
                        <span class="poll-option-count">${optionData.voteCount}</span>
                        <span class="poll-option-percent">${percentage.toFixed(0)}%</span>
                    </div>
                 `;
                }

                // Remove the vote button and replace it with a static div if the user has voted
                const button = optionEl.querySelector('.poll-option-vote-btn');
                if (button && hasUserVoted) {
                    const staticContent = button.innerHTML; // Grab the inner content
                    const staticDiv = document.createElement('div');
                    staticDiv.className = 'poll-option-inner';
                    staticDiv.innerHTML = staticContent;
                    button.replaceWith(staticDiv);
                }

                // Add 'selected' and 'voted' classes to reflect the vote state
                optionEl.classList.add('voted');
                if (optionData.hasUserVoted) {
                    optionEl.classList.add('selected');
                    const radio = optionEl.querySelector('.radio-circle');
                    if (radio) {
                        radio.classList.add('selected');
                        radio.innerHTML = '<i class="fas fa-check"></i>';
                    }
                }
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

// --- INITIALIZATION ---
const signalRManager = new SignalRManager();
window.signalRManager = signalRManager; // For debugging

document.addEventListener('DOMContentLoaded', async () => {
    // 1. Initialize SignalR Connections
    await signalRManager.initializeConnections();

    // 2. Wait for the connection to be fully established before proceeding
    const waitForConnection = new Promise((resolve, reject) => {
        const timeout = setTimeout(() => reject("SignalR connection timed out."), 10000); // 10-second timeout
        const interval = setInterval(() => {
            if (signalRManager.postConnection?.state === signalR.HubConnectionState.Connected) {
                clearTimeout(timeout);
                clearInterval(interval);
                resolve();
            }
        }, 100);
    });

    try {
        await waitForConnection;
        console.log("SignalR connection confirmed. Proceeding with page setup.");

        // 3. Find the Post ID
        const postIdElement = document.getElementById('pagePostId');
        if (postIdElement) {
            signalRManager.pagePostId = parseInt(postIdElement.value);
            if (!isNaN(signalRManager.pagePostId)) {
                // 4. Join the SignalR group for this specific post
                await signalRManager.joinPostGroup(signalRManager.pagePostId);
            } else {
                console.error("Could not parse Post ID from element:", postIdElement);
            }
        } else {
            console.log("No Post ID element found on this page. Some real-time features may be limited.");
        }

        // 5. Bind all initial event handlers
        signalRManager.rebindAllEvents();

        // 6. Set up the main comment form
        const commentTextarea = document.getElementById('mainCommentContent');
        if (commentTextarea) {
            commentTextarea.addEventListener('input', () => {
                if (signalRManager.pagePostId) signalRManager.startTyping(signalRManager.pagePostId);
            });
        }

        document.getElementById('submitMainComment')?.addEventListener('click', async () => {
            if (!signalRManager.pagePostId) return signalRManager.showNotification("Error: Post ID is missing.", "error");

            const contentElement = document.getElementById('mainCommentContent');
            const content = contentElement.value;

            if (!content.trim()) return signalRManager.showNotification("Please enter a comment.", "error");

            await signalRManager.sendComment(signalRManager.pagePostId, content.trim());
            contentElement.value = '';
        });

    } catch (error) {
        console.error("Initialization failed:", error);
        signalRManager.showNotification("Could not establish a real-time connection.", "error");
    }
});
