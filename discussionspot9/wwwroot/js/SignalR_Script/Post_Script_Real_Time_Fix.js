class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.pagePostId = null;

        // Bind 'this' context for the main event handler
        this.handleDelegatedClick = this.handleDelegatedClick.bind(this);
    }

    async initializeConnections() {
        this.postConnection = new signalR.HubConnectionBuilder()
            .withUrl("/postHub")
            .withAutomaticReconnect()
            .build();

        this.notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        this.setupPostEventHandlers();
        this.setupNotificationEventHandlers();

        try {
            await this.postConnection.start();
            console.log("✅ PostHub connection started successfully.");
            await this.notificationConnection.start();
            console.log("✅ NotificationHub connection started successfully.");
        } catch (err) {
            console.error("❌ SignalR connection start failed: ", err);
            this.showNotification("Failed to connect to real-time features.", 'error');
        }
    }

    setupPostEventHandlers() {
        // This handler for receiving comments remains the same and is working correctly.
        this.postConnection.on("ReceiveComment", (htmlContent, commentId, parentCommentId) => {
            const optimisticComment = document.getElementById('optimistic-comment-placeholder');
            if (optimisticComment) {
                optimisticComment.remove();
            }

            if (!htmlContent) {
                console.error("Received null comment HTML from server.");
                return;
            }

            const targetContainer = parentCommentId
                ? document.getElementById(`commentReplies-${parentCommentId}`)
                : document.querySelector('.comment-list');

            if (targetContainer) {
                targetContainer.insertAdjacentHTML('beforeend', htmlContent);
            } else {
                console.error(`Could not find container for comment. ParentID: ${parentCommentId || 'none'}`);
            }

            document.querySelectorAll('.reply-form').forEach(form => {
                form.classList.add('d-none');
                const textarea = form.querySelector('textarea');
                if (textarea) textarea.value = '';
            });
        });

        // Other SignalR event handlers...
        this.postConnection.on("CommentDeleted", (commentId) => this.deleteCommentUI(commentId));
        this.postConnection.on("CommentVoteUpdated", (commentId, up, down, score) => this.updateCommentVotes(commentId, up, down, score));
        this.postConnection.on("UpdatePostVotesUI", (postId, up, down, vote) => this.updatePostVotesUI(postId, up, down, vote));
        this.postConnection.on("ReceivePollUpdate", (pollData) => this.updatePollResultsUI(pollData));
        this.postConnection.on("ReceivePollVoteError", (msg) => this.showNotification(msg, 'error'));
        this.postConnection.on("ReceivePollVoteSuccess", (msg) => this.showNotification(msg, 'success'));
        this.postConnection.on("VoteError", (msg) => this.showNotification(msg, 'error'));
        this.postConnection.on("CommentError", (msg) => {
            document.getElementById('optimistic-comment-placeholder')?.remove();
            this.showNotification(msg, 'error');
        });
    }

    setupNotificationEventHandlers() {
        this.notificationConnection.on("ReceiveNotification", (notification) => {
            this.showNotification(notification.message || notification.Message, 'info');
        });
    }

    // This single, delegated event listener handles all clicks robustly.
    initializeDelegatedListener() {
        document.body.addEventListener('click', this.handleDelegatedClick);
    }

    // The master click handler
    async handleDelegatedClick(event) {
        const target = event.target;

        // --- Poll Vote ---
        const pollOption = target.closest('.poll-option');
        if (pollOption) {
            const pollContainer = pollOption.closest('.poll-container');
            const postId = parseInt(pollContainer?.dataset.postId, 10);
            const optionId = parseInt(pollOption.dataset.optionId, 10);
            if (!isNaN(postId) && !isNaN(optionId)) {
                await this.castPollVote(postId, optionId);
            }
            return;
        }

        // --- Post Vote ---
        const postVoteBtn = target.closest('.post-container .vote-btn');
        if (postVoteBtn) {
            event.preventDefault();
            const postId = parseInt(postVoteBtn.dataset.postId, 10);
            const voteType = parseInt(postVoteBtn.dataset.voteType, 10);
            if (!isNaN(postId) && !isNaN(voteType)) this.votePost(postId, voteType);
            return;
        }

        // --- Comment Vote ---
        const commentVoteBtn = target.closest('.comment-vote-btn');
        if (commentVoteBtn) {
            event.preventDefault();
            const commentId = parseInt(commentVoteBtn.dataset.commentId, 10);
            const voteType = parseInt(commentVoteBtn.dataset.voteType, 10);
            if (!isNaN(commentId) && !isNaN(voteType)) this.voteComment(commentId, voteType);
            return;
        }

        // --- Show Reply Form ---
        const replyBtn = target.closest('.comment-reply-btn');
        if (replyBtn) {
            event.preventDefault();
            this.showReplyForm(replyBtn.dataset.commentId);
            return;
        }

        // --- Submit Reply ---
        const submitReplyBtn = target.closest('.reply-submit-btn');
        if (submitReplyBtn) {
            event.preventDefault();
            const form = submitReplyBtn.closest('.reply-form');
            const textarea = form?.querySelector('textarea');
            const content = textarea?.value.trim();
            const parentId = parseInt(form?.dataset.commentId, 10);
            if (content && this.pagePostId && !isNaN(parentId)) {
                await this.sendComment(this.pagePostId, content, parentId);
                textarea.value = '';
                form.classList.add('d-none');
            }
            return;
        }

        // --- Cancel Reply ---
        const cancelReplyBtn = target.closest('.reply-cancel-btn');
        if (cancelReplyBtn) {
            event.preventDefault();
            const form = cancelReplyBtn.closest('.reply-form');
            if (form) {
                form.classList.add('d-none');
                form.querySelector('textarea').value = '';
            }
            return;
        }

        // --- Delete Comment ---
        const deleteCommentBtn = target.closest('.comment-delete-btn');
        if (deleteCommentBtn) {
            event.preventDefault();
            const commentId = parseInt(deleteCommentBtn.dataset.commentId, 10);
            if (!isNaN(commentId)) this.deleteComment(commentId);
            return;
        }
    }


    // HUB INVOCATION METHODS (Calling the server)
    async joinPostGroup(postId) {
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            try {
                await this.postConnection.invoke("JoinPostGroup", postId);
            } catch (err) { console.error(`Failed to join group ${postId}:`, err); }
        }
    }

    async sendComment(postId, content, parentId = null) {
        const placeholderHtml = `<div id="optimistic-comment-placeholder" style="opacity: 0.6;"><p><strong>You</strong> <small>sending...</small></p><p>${content.replace(/</g, "&lt;").replace(/>/g, "&gt;")}</p><hr/></div>`;
        const targetContainer = parentId ? document.getElementById(`commentReplies-${parentId}`) : document.querySelector('.comment-list');
        targetContainer?.insertAdjacentHTML('beforeend', placeholderHtml);

        try {
            await this.postConnection.invoke("SendComment", postId, content, parentId);
        } catch (err) {
            console.error("Failed to send comment:", err);
            document.getElementById('optimistic-comment-placeholder')?.remove();
        }
    }

    async deleteComment(commentId) {
        if (confirm("Are you sure?")) {
            try { await this.postConnection.invoke("DeleteComment", commentId); }
            catch (err) { console.error("Failed to delete comment:", err); }
        }
    }

    async voteComment(commentId, voteType) {
        try { await this.postConnection.invoke("VoteComment", commentId, voteType); }
        catch (err) { console.error("Failed to vote on comment:", err); }
    }

    async votePost(postId, voteType) {
        try { await this.postConnection.invoke("VotePost", postId, voteType); }
        catch (err) { console.error("Failed to vote on post:", err); }
    }

    async castPollVote(postId, optionId) {
        try { await this.postConnection.invoke("CastPollVote", postId, optionId); }
        catch (err) { console.error("Failed to cast poll vote:", err); }
    }

    // UI UPDATE METHODS
    showReplyForm(commentId) {
        const targetForm = document.getElementById(`replyForm${commentId}`);
        if (!targetForm) return;

        document.querySelectorAll('.reply-form').forEach(form => {
            if (form.id !== targetForm.id) {
                form.classList.add('d-none');
                form.querySelector('textarea').value = '';
            }
        });

        targetForm.classList.toggle('d-none');
        if (!targetForm.classList.contains('d-none')) {
            targetForm.querySelector('textarea').focus();
        }
    }

    deleteCommentUI(commentId) {
        const el = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (el) {
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 300);
        }
    }

    updateCommentVotes(commentId, upvoteCount, downvoteCount, score) {
        const voteCountEl = document.querySelector(`#commentVote${commentId}`);
        if (voteCountEl) voteCountEl.textContent = score;
    }

    updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote) {
        document.getElementById(`upvoteCount-${postId}`).textContent = upvoteCount;
        document.getElementById(`downvoteCount-${postId}`).textContent = `-${downvoteCount}`;
        document.getElementById(`totalScore-${postId}`).textContent = `Score ${upvoteCount - downvoteCount}`;
        document.getElementById(`upvoteBtn-${postId}`)?.classList.toggle('active', currentUserVote === 1);
        document.getElementById(`downvoteBtn-${postId}`)?.classList.toggle('active', currentUserVote === -1);
    }

    updatePollResultsUI(pollData) { /* Your existing poll UI update logic */ }
    showNotification(message, type = 'info') { /* Your existing notification logic */ }
}


// INITIALIZATION
document.addEventListener('DOMContentLoaded', async () => {
    try {
        const signalRManager = new SignalRManager();
        window.signalRManager = signalRManager;

        await signalRManager.initializeConnections();

        const postIdElement = document.getElementById('pagePostId');
        if (postIdElement) {
            signalRManager.pagePostId = parseInt(postIdElement.value, 10);
            if (!isNaN(signalRManager.pagePostId)) {
                await signalRManager.joinPostGroup(signalRManager.pagePostId);
            }
        }

        // This is the only event listener we need to set up now.
        signalRManager.initializeDelegatedListener();

        // Main comment form submission
        const submitButton = document.getElementById('submitMainComment');
        if (submitButton) {
            submitButton.addEventListener('click', async (e) => {
                e.preventDefault();
                const contentElement = document.getElementById('mainCommentContent');
                const content = contentElement?.value.trim();
                if (content && signalRManager.pagePostId) {
                    await signalRManager.sendComment(signalRManager.pagePostId, content);
                    contentElement.value = '';
                }
            });
        }

        // Handle 'Enter' key submission for main comment
        document.getElementById('mainCommentContent')?.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                submitButton?.click();
            }
        });

    } catch (error) {
        console.error('Fatal error during initialization:', error);
    }
});

