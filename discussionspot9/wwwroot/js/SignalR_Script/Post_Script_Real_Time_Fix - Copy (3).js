class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.pagePostId = null;

        // BIND 'THIS'
        this.handlePollVote = this.handlePollVote.bind(this);
        this.handlePostVoteButtonClick = this.handlePostVoteButtonClick.bind(this);
        this.handleDelegatedCommentClick = this.handleDelegatedCommentClick.bind(this);
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
            console.log("PostHub connection started successfully.");
            await this.notificationConnection.start();
            console.log("NotificationHub connection started successfully.");
        } catch (err) {
            console.error("SignalR connection start failed: ", err);
            this.showNotification("Failed to connect to real-time features.", 'error');
        }
    }

    setupPostEventHandlers() {
        // PERFORMANCE FIX: RECEIVE JSON, NOT HTML
        // This handler now expects a JSON object with the new comment's data.
        this.postConnection.on("ReceiveComment", (commentData, parentCommentId) => {
            if (!commentData) {
                console.error("Received null comment data from server.");
                return;
            }
            // A new function builds the HTML on the client side.
            const commentHtml = this.createCommentElement(commentData);

            if (parentCommentId) {
                const parentCommentItem = document.querySelector(`.comment-item[data-comment-id="${parentCommentId}"]`);
                if (parentCommentItem) {
                    let repliesContainer = parentCommentItem.querySelector('.comment-replies');
                    if (!repliesContainer) {
                        repliesContainer = document.createElement('div');
                        repliesContainer.className = 'comment-replies';
                        // Adjust where it's inserted based on your HTML structure
                        parentCommentItem.append(repliesContainer);
                    }
                    repliesContainer.insertAdjacentHTML('beforeend', commentHtml);
                }
            } else {
                document.querySelector('.comment-list')?.insertAdjacentHTML('beforeend', commentHtml);
            }
            // We no longer need rebindAllEvents() because of event delegation.
        });

        this.postConnection.on("CommentDeleted", (commentId) => {
            document.querySelector(`.comment-item[data-comment-id="${commentId}"]`)?.remove();
        });

        this.postConnection.on("CommentVoteUpdated", (commentId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote);
        });

        this.postConnection.on("UpdatePostVotesUI", (postId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote);
        });

        this.postConnection.on("ReceivePollUpdate", (pollData) => {
            this.updatePollResultsUI(pollData);
        });

        this.postConnection.on("ReceivePollVoteError", (errorMessage) => this.showNotification(errorMessage, 'error'));
    }

    setupNotificationEventHandlers() {
        // Notification handlers remain the same
    }

    async joinPostGroup(postId) {
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            await this.postConnection.invoke("JoinPostGroup", postId);
        }
    }

    // --- HUB INVOCATION METHODS ---
    async sendComment(postId, commentContent, parentCommentId = null) {
        await this.postConnection.invoke("SendComment", postId, commentContent, parentCommentId);
    }
    async deleteComment(commentId) {
        await this.postConnection.invoke("DeleteComment", commentId);
    }
    async voteComment(commentId, voteType) {
        await this.postConnection.invoke("VoteComment", commentId, voteType);
    }
    async votePost(postId, voteType) {
        await this.postConnection.invoke("VotePost", postId, voteType);
    }
    async castPollVote(postId, optionId) {
        await this.postConnection.invoke("CastPollVote", postId, optionId);
    }

    // --- EVENT BINDING & HANDLING ---

    // PERFORMANCE FIX: EVENT DELEGATION
    // Instead of binding to every button, we bind ONE listener to the comment list.
    // This is much faster and automatically works for new comments.
    initializeDelegatedEvents() {
        // For comments
        const commentList = document.querySelector('.comment-list');
        if (commentList) {
            commentList.addEventListener('click', this.handleDelegatedCommentClick);
        }

        // For Polls (already uses a good pattern)
        const pollOptionsContainer = document.getElementById('pollOptions');
        if (pollOptionsContainer) {
            pollOptionsContainer.addEventListener('click', this.handlePollVote);
        }

        // For Post votes
        const postActions = document.querySelector('.post-container .post-actions');
        if (postActions) {
            postActions.addEventListener('click', this.handlePostVoteButtonClick);
        }
    }

    handleDelegatedCommentClick(event) {
        const target = event.target;

        // Handle comment voting
        const voteButton = target.closest('.comment-vote-btn');
        if (voteButton) {
            event.preventDefault();
            const commentId = parseInt(voteButton.dataset.commentId, 10);
            const voteType = parseInt(voteButton.dataset.voteType, 10);
            this.voteComment(commentId, voteType);
            return;
        }

        // Handle showing the reply form
        const replyButton = target.closest('.comment-reply-btn');
        if (replyButton) {
            event.preventDefault();
            this.showReplyForm(replyButton.dataset.commentId);
            return;
        }

        // Handle submitting a reply
        const submitReplyButton = target.closest('.reply-submit-btn');
        if (submitReplyButton) {
            event.preventDefault();
            const form = submitReplyButton.closest('.reply-form');
            const content = form.querySelector('textarea').value.trim();
            if (content && this.pagePostId) {
                const parentId = parseInt(form.dataset.commentId, 10);
                this.sendComment(this.pagePostId, content, parentId);
                form.querySelector('textarea').value = '';
                form.style.display = 'none';
            }
            return;
        }

        // Handle canceling a reply
        const cancelReplyButton = target.closest('.reply-cancel-btn');
        if (cancelReplyButton) {
            event.preventDefault();
            const form = cancelReplyButton.closest('.reply-form');
            form.style.display = 'none';
            form.querySelector('textarea').value = '';
            return;
        }

        // Handle deleting a comment
        const deleteButton = target.closest('.comment-delete-btn');
        if (deleteButton) {
            event.preventDefault();
            this.deleteComment(parseInt(deleteButton.dataset.commentId, 10));
        }
    }

    handlePostVoteButtonClick(event) {
        const button = event.target.closest('.vote-btn');
        if (!button) return;

        event.preventDefault();
        const postId = parseInt(button.dataset.postId, 10);
        const voteType = parseInt(button.dataset.voteType, 10);
        this.votePost(postId, voteType);
    }

    async handlePollVote(event) {
        const optionContainer = event.target.closest('.poll-option');
        if (!optionContainer) return;

        const pollContainer = optionContainer.closest('.poll-container');
        if (!pollContainer) return;

        const postId = parseInt(pollContainer.dataset.postId, 10);
        const optionId = parseInt(optionContainer.dataset.optionId, 10);

        if (isNaN(postId) || isNaN(optionId)) return;
        await this.castPollVote(postId, optionId);
    }

    // --- UI UPDATE & HELPER METHODS ---

    // BUG FIX & PERFORMANCE: CLIENT-SIDE HTML CREATION
    // This function builds the HTML for a new comment using the JSON data from the hub.
    createCommentElement(commentData) {
        const {
            commentId,
            content,
            authorDisplayName,
            authorAvatarColor,
            authorInitials,
            timeAgo,
            upvoteCount,
            downvoteCount,
            currentUserVote
        } = commentData;

        const upvoteActive = currentUserVote === 1 ? 'active' : '';
        const downvoteActive = currentUserVote === -1 ? 'active' : '';

        // This is a template literal. It builds the HTML string.
        // It should match the structure of your _CommentItem.cshtml partial view.
        return `
            <div class="comment-item" data-comment-id="${commentId}">
                <div class="comment-content">
                    <div class="comment-meta">
                        <div class="user-avatar small" style="background-color:${authorAvatarColor};">${authorInitials}</div>
                        <span class="comment-author">${authorDisplayName}</span>
                        <span class="comment-time">${timeAgo}</span>
                    </div>
                    <div class="comment-text">${content}</div>
                    <div class="comment-actions">
                        <div class="comment-vote">
                             <button class="comment-vote-btn upvote ${upvoteActive}" data-comment-id="${commentId}" data-vote-type="1"><i class="fas fa-arrow-up"></i></button>
                             <span class="comment-vote-count">${upvoteCount - downvoteCount}</span>
                             <button class="comment-vote-btn downvote ${downvoteActive}" data-comment-id="${commentId}" data-vote-type="-1"><i class="fas fa-arrow-down"></i></button>
                        </div>
                        <button class="comment-reply-btn" data-comment-id="${commentId}">
                            <i class="fas fa-reply"></i> Reply
                        </button>
                         <button class="comment-delete-btn" data-comment-id="${commentId}">
                            <i class="fas fa-trash"></i> Delete
                        </button>
                    </div>
                </div>
                <div class="reply-form" id="replyForm${commentId}" data-comment-id="${commentId}" style="display: none;">
                    <textarea class="form-control" placeholder="Write a reply..."></textarea>
                    <div class="reply-form-actions">
                        <button class="reply-cancel-btn">Cancel</button>
                        <button class="reply-submit-btn">Reply</button>
                    </div>
                </div>
                <div class="comment-replies"></div>
            </div>`;
    }

    showReplyForm(commentId) {
        document.querySelectorAll('.reply-form').forEach(form => form.style.display = 'none');
        const replyForm = document.getElementById(`replyForm${commentId}`);
        if (replyForm) {
            replyForm.style.display = 'block';
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
        document.getElementById(`upvoteCount-${postId}`).textContent = upvoteCount;
        document.getElementById(`downvoteCount-${postId}`).textContent = `-${downvoteCount}`;
        document.getElementById(`totalScore-${postId}`).textContent = `Score ${upvoteCount - downvoteCount}`;
        document.getElementById(`upvoteBtn-${postId}`)?.classList.toggle('active', currentUserVote === 1);
        document.getElementById(`downvoteBtn-${postId}`)?.classList.toggle('active', currentUserVote === -1);
    }

    // BUG FIX: THIS FUNCTION NOW USES THE CORRECT CSS SELECTORS
    updatePollResultsUI(pollData) {
        const { postId, options, totalVotes, hasUserVoted } = pollData;
        const pollContainer = document.querySelector(`.poll-container[data-post-id="${postId}"]`);
        if (!pollContainer) return;

        pollContainer.querySelector('.poll-total-votes').textContent = `${totalVotes.toLocaleString()} votes`;

        // Hide "Cast Your Vote" button if it exists
        const castVoteButton = document.getElementById('submitVote');
        if (castVoteButton && hasUserVoted) {
            castVoteButton.parentElement.remove();
        }

        if (hasUserVoted) {
            pollContainer.querySelector('.text-muted.text-center')?.remove();
            if (!pollContainer.querySelector('.text-success.text-center')) {
                const votedText = document.createElement('p');
                votedText.className = 'text-success text-center small mb-3';
                votedText.innerHTML = `<i class="fas fa-check-circle"></i> You have voted in this poll`;
                pollContainer.querySelector('.poll-question').after(votedText);
            }
        }

        options.forEach(optionData => {
            const optionEl = pollContainer.querySelector(`.poll-option[data-option-id="${optionData.pollOptionId}"]`);
            if (optionEl) {
                const percentage = totalVotes > 0 ? (optionData.voteCount / totalVotes) * 100 : 0;

                // FIX 1: Changed selector from '.poll-option-bar' to '.poll-progress'
                const bar = optionEl.querySelector('.poll-progress');
                if (bar) bar.style.width = `${percentage.toFixed(0)}%`;

                // FIX 2: Updated selector to '.poll-percentage' and update its content.
                const percentEl = optionEl.querySelector('.poll-percentage');
                if (percentEl) percentEl.textContent = `${percentage.toFixed(0)}%`;

                // Logic to convert clickable button to static div after voting
                const button = optionEl.querySelector('.poll-option-vote-btn');
                if (button && hasUserVoted) {
                    const staticDiv = document.createElement('div');
                    staticDiv.className = 'poll-option-inner';
                    staticDiv.innerHTML = button.innerHTML; // copy content
                    button.replaceWith(staticDiv);
                }

                // Add classes to reflect vote state
                optionEl.classList.add('voted');
                if (optionData.hasUserVoted) {
                    optionEl.classList.add('selected');
                    const radio = optionEl.querySelector('.radio-circle');
                    if (radio && !radio.querySelector('i')) {
                        radio.classList.add('selected');
                        radio.innerHTML = '<i class="fas fa-check"></i>';
                    }
                }
            }
        });
    }

    showNotification(message, type = 'info') { /* Omitted for brevity */ }
}

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', async () => {
    const signalRManager = new SignalRManager();
    window.signalRManager = signalRManager;

    await signalRManager.initializeConnections();

    const postIdElement = document.getElementById('pagePostId');
    if (postIdElement && signalRManager.postConnection?.state === signalR.HubConnectionState.Connected) {
        signalRManager.pagePostId = parseInt(postIdElement.value);
        if (!isNaN(signalRManager.pagePostId)) {
            await signalRManager.joinPostGroup(signalRManager.pagePostId);
        }
    }

    // This single call now handles all future comment interactions efficiently.
    signalRManager.initializeDelegatedEvents();

    document.getElementById('submitMainComment')?.addEventListener('click', async () => {
        const contentElement = document.getElementById('mainCommentContent');
        const content = contentElement.value;
        if (content.trim() && signalRManager.pagePostId) {
            await signalRManager.sendComment(signalRManager.pagePostId, content.trim());
            contentElement.value = '';
        }
    });
});
