class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.pagePostId = null;

        // Bind methods to ensure correct context
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
        // FIX: Receive HTML string directly from server and insert it
        this.postConnection.on("ReceiveComment", (htmlContent, commentId, parentCommentId) => {
            if (!htmlContent) {
                console.error("Received null comment HTML from server.");
                return;
            }

            if (parentCommentId) {
                // Find the replies container for the parent comment
                const repliesContainer = document.getElementById(`commentReplies-${parentCommentId}`);
                if (repliesContainer) {
                    repliesContainer.insertAdjacentHTML('beforeend', htmlContent);
                } else {
                    console.error(`Could not find replies container for parent comment ${parentCommentId}`);
                }
            } else {
                // Add to main comment list
                const commentList = document.querySelector('.comment-list');
                if (commentList) {
                    commentList.insertAdjacentHTML('beforeend', htmlContent);
                } else {
                    console.error("Could not find comment list container");
                }
            }

            // Hide any open reply forms after successful comment submission
            const replyForms = document.querySelectorAll('.reply-form');
            replyForms.forEach(form => {
                form.classList.add('d-none');
                const textarea = form.querySelector('textarea');
                if (textarea) textarea.value = '';
            });
        });

        this.postConnection.on("CommentDeleted", (commentId) => {
            const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
            if (commentElement) {
                // Fade out effect before removing
                commentElement.style.opacity = '0';
                commentElement.style.transition = 'opacity 0.3s';
                setTimeout(() => commentElement.remove(), 300);
            }
        });

        this.postConnection.on("CommentVoteUpdated", (commentId, upvoteCount, downvoteCount, score) => {
            this.updateCommentVotes(commentId, upvoteCount, downvoteCount, score);
        });

        this.postConnection.on("UpdatePostVotesUI", (postId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote);
        });

        this.postConnection.on("ReceivePollUpdate", (pollData) => {
            this.updatePollResultsUI(pollData);
        });

        this.postConnection.on("ReceivePollVoteError", (errorMessage) => {
            this.showNotification(errorMessage, 'error');
        });

        this.postConnection.on("ReceivePollVoteSuccess", (message) => {
            this.showNotification(message, 'success');
        });

        this.postConnection.on("CommentError", (errorMessage) => {
            this.showNotification(errorMessage, 'error');
        });

        this.postConnection.on("VoteError", (errorMessage) => {
            this.showNotification(errorMessage, 'error');
        });
    }

    setupNotificationEventHandlers() {
        this.notificationConnection.on("ReceiveNotification", (notification) => {
            console.log("Notification Received: ", notification);
            this.showNotification(notification.message || notification.Message, 'info');
        });
    }

    async joinPostGroup(postId) {
        if (this.postConnection?.state === signalR.HubConnectionState.Connected) {
            try {
                await this.postConnection.invoke("JoinPostGroup", postId);
                console.log(`Successfully joined post group ${postId}`);
            } catch (err) {
                console.error(`Failed to join post group ${postId}:`, err);
            }
        }
    }

    // Hub invocation methods
    async sendComment(postId, commentContent, parentCommentId = null) {
        try {
            await this.postConnection.invoke("SendComment", postId, commentContent, parentCommentId);
        } catch (err) {
            console.error("Failed to send comment:", err);
            this.showNotification("Failed to send comment. Please try again.", 'error');
        }
    }

    async deleteComment(commentId) {
        try {
            if (confirm("Are you sure you want to delete this comment?")) {
                await this.postConnection.invoke("DeleteComment", commentId);
            }
        } catch (err) {
            console.error("Failed to delete comment:", err);
            this.showNotification("Failed to delete comment.", 'error');
        }
    }

    async voteComment(commentId, voteType) {
        try {
            await this.postConnection.invoke("VoteComment", commentId, voteType);
        } catch (err) {
            console.error("Failed to vote on comment:", err);
            this.showNotification("Failed to vote. Please try again.", 'error');
        }
    }

    async votePost(postId, voteType) {
        try {
            await this.postConnection.invoke("VotePost", postId, voteType);
        } catch (err) {
            console.error("Failed to vote on post:", err);
            this.showNotification("Failed to vote. Please try again.", 'error');
        }
    }

    async castPollVote(postId, optionId) {
        try {
            await this.postConnection.invoke("CastPollVote", postId, optionId);
        } catch (err) {
            console.error("Failed to cast poll vote:", err);
            this.showNotification("Failed to vote in poll.", 'error');
        }
    }

    // Event delegation handlers
    initializeDelegatedEvents() {
        // Use document body for event delegation to catch all dynamically added elements
        document.body.addEventListener('click', this.handleDelegatedCommentClick);

        const pollOptionsContainer = document.getElementById('pollOptions');
        if (pollOptionsContainer) {
            pollOptionsContainer.addEventListener('click', this.handlePollVote);
        }

        const postActions = document.querySelector('.post-container .post-actions');
        if (postActions) {
            postActions.addEventListener('click', this.handlePostVoteButtonClick);
        }
    }

    handleDelegatedCommentClick(event) {
        const target = event.target;

        // Check if clicked element or its parent matches our selectors
        const voteButton = target.closest('.comment-vote-btn');
        if (voteButton) {
            event.preventDefault();
            event.stopPropagation();
            const commentId = parseInt(voteButton.dataset.commentId, 10);
            const voteType = parseInt(voteButton.dataset.voteType, 10);
            if (!isNaN(commentId) && !isNaN(voteType)) {
                this.voteComment(commentId, voteType);
            }
            return;
        }

        const replyButton = target.closest('.comment-reply-btn');
        if (replyButton) {
            event.preventDefault();
            event.stopPropagation();
            const commentId = replyButton.dataset.commentId;
            if (commentId) {
                this.showReplyForm(commentId);
            }
            return;
        }

        const submitReplyButton = target.closest('.reply-submit-btn');
        if (submitReplyButton) {
            event.preventDefault();
            event.stopPropagation();
            const form = submitReplyButton.closest('.reply-form');
            const textarea = form.querySelector('textarea');
            const content = textarea ? textarea.value.trim() : '';

            if (content && this.pagePostId) {
                const parentId = parseInt(form.dataset.commentId, 10);
                if (!isNaN(parentId)) {
                    this.sendComment(this.pagePostId, content, parentId);
                    textarea.value = '';
                    form.classList.add('d-none');
                }
            } else if (!content) {
                this.showNotification("Please enter a reply before submitting.", 'warning');
            }
            return;
        }

        const cancelReplyButton = target.closest('.reply-cancel-btn');
        if (cancelReplyButton) {
            event.preventDefault();
            event.stopPropagation();
            const form = cancelReplyButton.closest('.reply-form');
            if (form) {
                form.classList.add('d-none');
                const textarea = form.querySelector('textarea');
                if (textarea) textarea.value = '';
            }
            return;
        }

        const deleteButton = target.closest('.comment-delete-btn');
        if (deleteButton) {
            event.preventDefault();
            event.stopPropagation();
            const commentId = parseInt(deleteButton.dataset.commentId, 10);
            if (!isNaN(commentId)) {
                this.deleteComment(commentId);
            }
        }
    }

    handlePostVoteButtonClick(event) {
        const button = event.target.closest('.vote-btn');
        if (!button) return;

        event.preventDefault();
        const postId = parseInt(button.dataset.postId, 10);
        const voteType = parseInt(button.dataset.voteType, 10);

        if (!isNaN(postId) && !isNaN(voteType)) {
            this.votePost(postId, voteType);
        }
    }

    async handlePollVote(event) {
        const optionContainer = event.target.closest('.poll-option');
        if (!optionContainer) return;

        const pollContainer = optionContainer.closest('.poll-container');
        if (!pollContainer) return;

        const postId = parseInt(pollContainer.dataset.postId, 10);
        const optionId = parseInt(optionContainer.dataset.optionId, 10);

        if (!isNaN(postId) && !isNaN(optionId)) {
            await this.castPollVote(postId, optionId);
        }
    }

    // FIX: Updated showReplyForm to handle the flex-column class properly
    showReplyForm(commentId) {
        const targetForm = document.getElementById(`replyForm${commentId}`);
        if (!targetForm) {
            console.error(`Reply form with ID replyForm${commentId} not found.`);
            return;
        }

        // Close all other reply forms first
        document.querySelectorAll('.reply-form').forEach(form => {
            if (form.id !== `replyForm${commentId}`) {
                form.classList.add('d-none');
                const textarea = form.querySelector('textarea');
                if (textarea) textarea.value = '';
            }
        });

        // Toggle the current form
        const isCurrentlyHidden = targetForm.classList.contains('d-none');

        if (isCurrentlyHidden) {
            targetForm.classList.remove('d-none');
            // Focus on textarea for better UX
            const textarea = targetForm.querySelector('textarea');
            if (textarea) {
                textarea.focus();
            }
        } else {
            targetForm.classList.add('d-none');
            const textarea = targetForm.querySelector('textarea');
            if (textarea) textarea.value = '';
        }
    }

    updateCommentVotes(commentId, upvoteCount, downvoteCount, score) {
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (!commentElement) return;

        const voteCountElement = commentElement.querySelector('.comment-vote-count');
        if (voteCountElement) {
            voteCountElement.textContent = upvoteCount - downvoteCount;
        }

        // Update vote button states based on current user's vote
        // Note: This requires the server to send user-specific vote state
        const upvoteBtn = commentElement.querySelector('.comment-vote-btn.upvote');
        const downvoteBtn = commentElement.querySelector('.comment-vote-btn.downvote');

        // The server should send currentUserVote as well for accurate state updates
        // For now, we'll just update the count
    }

    updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote) {
        const upvoteCountEl = document.getElementById(`upvoteCount-${postId}`);
        const downvoteCountEl = document.getElementById(`downvoteCount-${postId}`);
        const totalScoreEl = document.getElementById(`totalScore-${postId}`);
        const upvoteBtn = document.getElementById(`upvoteBtn-${postId}`);
        const downvoteBtn = document.getElementById(`downvoteBtn-${postId}`);

        if (upvoteCountEl) upvoteCountEl.textContent = upvoteCount;
        if (downvoteCountEl) downvoteCountEl.textContent = `-${downvoteCount}`;
        if (totalScoreEl) totalScoreEl.textContent = `Score ${upvoteCount - downvoteCount}`;

        if (upvoteBtn) {
            upvoteBtn.classList.toggle('active', currentUserVote === 1);
        }
        if (downvoteBtn) {
            downvoteBtn.classList.toggle('active', currentUserVote === -1);
        }
    }

    updatePollResultsUI(pollData) {
        const { postId, options, totalVotes, hasUserVoted } = pollData;
        const pollContainer = document.querySelector(`.poll-container[data-post-id="${postId}"]`);
        if (!pollContainer) return;

        const totalVotesEl = pollContainer.querySelector('.poll-total-votes');
        if (totalVotesEl) {
            totalVotesEl.textContent = `${totalVotes.toLocaleString()} votes`;
        }

        // Clear previous selections
        pollContainer.querySelectorAll('.poll-option').forEach(opt => {
            opt.classList.remove('selected');
        });

        pollContainer.querySelectorAll('.radio-circle.selected').forEach(circle => {
            circle.classList.remove('selected');
            circle.innerHTML = '';
        });

        if (hasUserVoted) {
            // Remove "not voted" text and add "voted" text if needed
            const notVotedText = pollContainer.querySelector('.text-muted.text-center');
            if (notVotedText) notVotedText.remove();

            if (!pollContainer.querySelector('.text-success.text-center')) {
                const votedText = document.createElement('p');
                votedText.className = 'text-success text-center small mb-3';
                votedText.innerHTML = '<i class="fas fa-check-circle"></i> You have voted in this poll';
                const pollQuestion = pollContainer.querySelector('.poll-question');
                if (pollQuestion) {
                    pollQuestion.after(votedText);
                }
            }
        }

        // Update each option
        options.forEach(optionData => {
            const optionEl = pollContainer.querySelector(`.poll-option[data-option-id="${optionData.pollOptionId}"]`);
            if (!optionEl) return;

            const percentage = totalVotes > 0 ? (optionData.voteCount / totalVotes) * 100 : 0;

            const progressBar = optionEl.querySelector('.poll-progress');
            if (progressBar) {
                progressBar.style.width = `${percentage.toFixed(0)}%`;
            }

            const percentEl = optionEl.querySelector('.poll-percentage');
            if (percentEl) {
                percentEl.textContent = `${percentage.toFixed(0)}%`;
            }

            // Convert button to static display if user has voted
            if (hasUserVoted) {
                const button = optionEl.querySelector('.poll-option-vote-btn');
                if (button) {
                    const staticDiv = document.createElement('div');
                    staticDiv.className = 'poll-option-inner';
                    staticDiv.innerHTML = button.innerHTML;
                    button.replaceWith(staticDiv);
                }
                optionEl.classList.add('voted');
            }

            // Mark user's selection
            if (optionData.hasUserVoted) {
                optionEl.classList.add('selected');
                const radio = optionEl.querySelector('.radio-circle');
                if (radio) {
                    radio.classList.add('selected');
                    radio.innerHTML = '<i class="fas fa-check"></i>';
                }
            }
        });
    }

    showNotification(message, type = 'info') {
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999;';
            document.body.appendChild(toastContainer);
        }

        const toast = document.createElement('div');
        const bgColors = {
            'error': '#dc3545',
            'success': '#198754',
            'info': '#0d6efd',
            'warning': '#ffc107'
        };

        const bgColor = bgColors[type] || bgColors.info;
        const textColor = type === 'warning' ? '#000' : '#fff';

        toast.style.cssText = `
            background-color: ${bgColor}; 
            color: ${textColor}; 
            padding: 15px 20px; 
            margin-bottom: 10px; 
            border-radius: 5px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.2);
            max-width: 350px;
            animation: slideIn 0.3s ease-out;
        `;
        toast.textContent = message;
        toastContainer.appendChild(toast);

        // Auto-remove after 4 seconds
        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease-in';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
}

// Add CSS animations for notifications
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
`;
document.head.appendChild(style);

// Initialization
document.addEventListener('DOMContentLoaded', async () => {
    const signalRManager = new SignalRManager();
    window.signalRManager = signalRManager;

    try {
        await signalRManager.initializeConnections();

        const postIdElement = document.getElementById('pagePostId');
        if (postIdElement && signalRManager.postConnection?.state === signalR.HubConnectionState.Connected) {
            signalRManager.pagePostId = parseInt(postIdElement.value);
            if (!isNaN(signalRManager.pagePostId)) {
                await signalRManager.joinPostGroup(signalRManager.pagePostId);
            }
        }

        signalRManager.initializeDelegatedEvents();

        // Setup main comment submission
        const submitButton = document.getElementById('submitMainComment');
        if (submitButton) {
            submitButton.addEventListener('click', async (e) => {
                e.preventDefault();
                const contentElement = document.getElementById('mainCommentContent');
                if (contentElement) {
                    const content = contentElement.value.trim();
                    if (content && signalRManager.pagePostId) {
                        await signalRManager.sendComment(signalRManager.pagePostId, content);
                        contentElement.value = '';
                    } else if (!content) {
                        signalRManager.showNotification('Please enter a comment before submitting.', 'warning');
                    }
                }
            });
        }

        // Allow Enter key to submit main comment (Shift+Enter for new line)
        const mainCommentContent = document.getElementById('mainCommentContent');
        if (mainCommentContent) {
            mainCommentContent.addEventListener('keydown', async (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    const submitButton = document.getElementById('submitMainComment');
                    if (submitButton) submitButton.click();
                }
            });
        }

    } catch (error) {
        console.error('Failed to initialize SignalR:', error);
    }
});