// signalr-connection.js
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
    }

    // Initializes SignalR connections to both PostHub and NotificationHub.
    async initializeConnections() {
        // Build connection to PostHub
        this.postConnection = new signalR.HubConnectionBuilder()
            .withUrl("/postHub")
            .withAutomaticReconnect() // Enable automatic reconnection
            .build();

        // Build connection to NotificationHub
        this.notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        // Set up event handlers for incoming messages from both hubs
        await this.setupPostEventHandlers();
        await this.setupNotificationEventHandlers();

        try {
            // Start both connections
            await this.postConnection.start();
            await this.notificationConnection.start();
            console.log("SignalR connections established");
        } catch (err) {
            console.error("SignalR connection failed: ", err);
            this.showNotification("Failed to connect to real-time features. Please refresh the page.", 'error');
        }

        // Add a global click listener to handle closing dropdowns when clicking outside
        document.addEventListener('click', this.handleDocumentClick);
    }

    // Handles clicks anywhere on the document to close active dropdowns.
    handleDocumentClick(event) {
        if (this.openShareDropdown && !this.openShareDropdown.contains(event.target)) {
            console.log("Document click: Closing share dropdown.");
            this.openShareDropdown.classList.remove('active');
            this.openShareDropdown = null;
        }
    }

    // Sets up event listeners for messages coming from the PostHub.
    async setupPostEventHandlers() {
        // Handles receiving a new comment (or reply) from the server.
        this.postConnection.on("ReceiveComment", (commentHtml, commentId, parentCommentId) => {
            try {
                if (parentCommentId) {
                    // If it's a reply, append to the parent comment's replies section
                    const parentCommentReplies = document.querySelector(`.comment-item[data-comment-id="${parentCommentId}"] .comment-replies`);
                    if (parentCommentReplies) {
                        parentCommentReplies.insertAdjacentHTML('beforeend', commentHtml);
                    } else {
                        console.warn(`Parent comment replies section not found for parentCommentId: ${parentCommentId}`);
                    }
                } else {
                    // If it's a top-level comment, append to the main comments list
                    const commentsList = document.getElementById('commentsList'); // Assumes a main container with this ID
                    if (commentsList) {
                        commentsList.insertAdjacentHTML('beforeend', commentHtml);
                    } else {
                        console.warn("Main comments list element not found (ID 'commentsList').");
                    }
                }
                this.rebindCommentEvents(); // Rebind events for newly added comment elements
                this.showNotification("New comment received!", "info");
            } catch (error) {
                console.error("Error processing received comment:", error);
                this.showNotification("Error displaying new comment.", "error");
            }
        });

        // Handles updates to an existing comment (e.g., content edited).
        this.postConnection.on("CommentEdited", (comment) => {
            const commentElement = document.querySelector(`.comment-item[data-comment-id="${comment.commentId}"] .comment-text`);
            const editedTimestampElement = document.querySelector(`.comment-item[data-comment-id="${comment.commentId}"] .comment-time`);
            if (commentElement) {
                commentElement.innerHTML = comment.content; // Update content
            }
            if (editedTimestampElement) {
                editedTimestampElement.textContent = `${comment.timeAgo} (edited)`; // Update timestamp
            }
            this.showNotification("Comment updated!", "info");
        });

        // Handles deletion of a comment.
        this.postConnection.on("CommentDeleted", (commentId) => {
            const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
            if (commentElement) {
                commentElement.remove();
                this.showNotification("Comment deleted.", "info");
            } else {
                console.warn(`Comment element with ID ${commentId} not found for deletion.`);
            }
        });

        // Handles typing indicator from other users.
        this.postConnection.on("UserStartedTyping", (userName) => {
            this.showTypingIndicator(userName);
        });

        // Handles hiding typing indicator from other users.
        this.postConnection.on("UserStoppedTyping", (userName) => {
            this.hideTypingIndicator(userName);
        });

        // Handles updates to comment vote counts.
        this.postConnection.on("CommentVoteUpdated", (commentId, upvoteCount, downvoteCount, currentUserVote) => {
            // --- Crucial Debugging Log ---
            console.log(`--- RECEIVED CommentVoteUpdated --- CommentId: ${commentId}, Up: ${upvoteCount}, Down: ${downvoteCount}, UserVote: ${currentUserVote}`);
            this.updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote);
        });

        // Handles updates to post vote counts.
        this.postConnection.on("UpdatePostVotesUI", (postId, upvoteCount, downvoteCount, currentUserVote) => {
            this.updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote);
        });

        // Handles general errors specifically from comment/vote operations.
        this.postConnection.on("CommentError", (errorMessage) => {
            this.showNotification(errorMessage, 'error');
        });
        this.postConnection.on("VoteError", (errorMessage) => {
            this.showNotification(errorMessage, 'error');
        });
    }

    // Sets up event listeners for messages coming from the NotificationHub.
    async setupNotificationEventHandlers() {
        this.notificationConnection.on("ReceiveNotification", (notificationData) => {
            const { Message, Type, PostId } = notificationData;
            this.showNotification(Message, Type);
            this.updateNotificationBadge(); // Update badge for new notifications
            if (PostId) {
                console.log(`Notification for Post ID: ${PostId}`);
            }
        });

        // Handles updates to unread notification count.
        this.notificationConnection.on("UnreadNotificationCount", (count) => {
            this.updateNotificationBadge(count);
        });

        // Handles when a notification is marked as read.
        this.notificationConnection.on("NotificationMarkedAsRead", (notificationId) => {
            // Optional: update UI to reflect notification being read
            console.log(`Notification ${notificationId} marked as read.`);
        });
    }

    // Invokes the server to join a specific post group.
    async joinPostGroup(postId) {
        if (this.currentPostId) {
            await this.leavePostGroup(this.currentPostId); // Leave previous group if any
        }
        this.currentPostId = postId;
        try {
            await this.postConnection.invoke("JoinPostGroup", postId);
            console.log(`Joined post group: ${postId}`);
        } catch (err) {
            console.error("Error joining post group:", err);
        }
    }

    // Invokes the server to leave a specific post group.
    async leavePostGroup(postId) {
        try {
            await this.postConnection.invoke("LeavePostGroup", postId);
            console.log(`Left post group: ${postId}`);
        } catch (err) {
            console.error("Error leaving post group:", err);
        }
        this.currentPostId = null;
    }

    // Sends a comment to the server.
    async sendComment(postId, commentContent, parentCommentId = null) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected to the server. Please check your internet connection.", 'error');
            return;
        }
        try {
            await this.postConnection.invoke("SendComment", postId, commentContent, parentCommentId);
            this.showNotification("Comment posted successfully!", 'success');
        } catch (err) {
            console.error("Error sending comment:", err);
            this.showNotification("Failed to post comment. Please try again.", 'error');
        }
    }

    // Edits an existing comment on the server.
    async editComment(commentId, newContent) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected to the server. Please check your internet connection.", 'error');
            return;
        }
        try {
            await this.postConnection.invoke("EditComment", commentId, newContent);
            this.showNotification("Comment updated successfully!", 'success');
        } catch (err) {
            console.error("Error editing comment:", err);
            this.showNotification("Failed to edit comment.", 'error');
        }
    }

    // Deletes a comment on the server.
    async deleteComment(commentId) {
        if (confirm("Are you sure you want to delete this comment?")) {
            if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
                this.showNotification("Not connected to the server. Please check your internet connection.", 'error');
                return;
            }
            try {
                await this.postConnection.invoke("DeleteComment", commentId);
            } catch (err) {
                console.error("Error deleting comment:", err);
                this.showNotification("Failed to delete comment.", 'error');
            }
        }
    }

    // Sends a vote for a comment to the server.
    async voteComment(commentId, isUpvote) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected to the server. Please check your internet connection.", 'error');
            console.error("SignalR: Not connected when attempting to vote on comment.");
            return;
        }

        try {
            const voteType = isUpvote ? 1 : -1;
            // console.log(`Invoking VoteComment on Hub for CommentId: ${commentId}, VoteType: ${voteType}`);
            await this.postConnection.invoke("VoteComment", commentId, voteType);
            // console.log(`VoteComment invoked successfully for CommentId: ${commentId}`);
        } catch (err) {
            console.error("Error voting comment (after invoke attempt):", err);
            this.showNotification("Failed to vote on comment. Please ensure you are logged in or try again.", 'error');
        }
    }

    // Sends a vote for a post to the server.
    async votePost(postId, voteType) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected to the server. Please check your internet connection.", 'error');
            return;
        }
        try {
            await this.postConnection.invoke("VotePost", postId, voteType);
            this.showNotification("Post vote cast successfully!", 'success');
        } catch (err) {
            console.error("Error voting post:", err);
            this.showNotification("Failed to vote on post. Please ensure you are logged in.", 'error');
        }
    }

    // Notifies the server that the user has started typing.
    async startTyping(postId) {
        if (!this.isTyping && this.postConnection.state === signalR.HubConnectionState.Connected) {
            this.isTyping = true;
            await this.postConnection.invoke("StartTyping", postId);
        }

        // Reset timeout to keep typing status active while user types
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }
        this.typingTimeout = setTimeout(async () => {
            await this.stopTyping(postId);
        }, 1000); // Stop typing after 1 second of inactivity
    }

    // Notifies the server that the user has stopped typing.
    async stopTyping(postId) {
        if (this.isTyping && this.postConnection.state === signalR.HubConnectionState.Connected) {
            this.isTyping = false;
            await this.postConnection.invoke("StopTyping", postId);
            if (this.typingTimeout) {
                clearTimeout(this.typingTimeout);
                this.typingTimeout = null;
            }
        }
    }

    // Rebinds event listeners for all comment-related interactive elements.
    rebindCommentEvents() {
        console.log("rebindCommentEvents: Starting re-binding of comment related events.");
        // Vote buttons
        document.querySelectorAll('.comment-vote-btn').forEach(button => {
            button.removeEventListener('click', this.handleVoteButtonClick); // Remove old listener to prevent duplicates
            button.addEventListener('click', this.handleVoteButtonClick); // Add new listener
            console.log("rebindCommentEvents: Bound vote button to comment ID:", button.dataset.commentId); // Added log
        });

        // Reply buttons
        document.querySelectorAll('.comment-reply-btn').forEach(button => {
            button.removeEventListener('click', this.handleReplyButtonClick);
            button.addEventListener('click', this.handleReplyButtonClick);
            console.log("rebindCommentEvents: Bound reply button to comment ID:", button.dataset.commentId); // Added log
        });

        // Reply form submit buttons
        document.querySelectorAll('.reply-submit-btn').forEach(button => {
            button.removeEventListener('click', this.handleSubmitReplyButtonClick);
            button.addEventListener('click', this.handleSubmitReplyButtonClick);
            console.log("rebindCommentEvents: Bound reply submit button for comment ID:", button.dataset.commentId); // Added log
        });

        // Reply form cancel buttons
        document.querySelectorAll('.reply-cancel-btn').forEach(button => {
            button.removeEventListener('click', this.handleReplyCancel);
            button.addEventListener('click', this.handleReplyCancel);
            console.log("rebindCommentEvents: Bound reply cancel button for comment ID:", button.dataset.commentId); // Added log
        });

        // Rebind share and post vote buttons (as they might be on the same page)
        this.rebindShareButtons();
        this.rebindPostVoteButtons();
        this.rebindPollVoteButtons(); // Also rebind poll buttons if present
        console.log("rebindCommentEvents: Finished re-binding of comment related events.");
    }

    // Event handler for comment vote buttons.
    handleVoteButtonClick(event) {
        event.preventDefault(); // Prevent default button behavior
        const button = event.currentTarget;
        const commentId = parseInt(button.getAttribute('data-comment-id'));
        const voteType = parseInt(button.getAttribute('data-vote-type')); // 1 for upvote, -1 for downvote
        const isUpvote = (voteType === 1);
        console.log(`handleVoteButtonClick: CommentId: ${commentId}, VoteType: ${voteType}, isUpvote: ${isUpvote}`); // Added log
        this.voteComment(commentId, isUpvote);
    }

    // Event handler for comment reply buttons.
    handleReplyButtonClick(event) {
        event.preventDefault();
        const commentId = event.currentTarget.getAttribute('data-comment-id');
        console.log(`handleReplyButtonClick: Reply button clicked for Comment ID: ${commentId}`); // Added log
        this.showReplyForm(commentId);
    }

    // Event handler for submitting a reply comment.
    async handleSubmitReplyButtonClick(event) {
        event.preventDefault();
        const replyFormDiv = event.currentTarget.closest('.reply-form');

        if (!replyFormDiv) {
            console.error("handleSubmitReplyButtonClick: Could not find parent .reply-form div for reply submit button.");
            this.showNotification("Error: Could not submit reply.", "error");
            return;
        }

        const parentCommentId = parseInt(replyFormDiv.dataset.commentId);
        const contentTextarea = replyFormDiv.querySelector('textarea');
        const content = contentTextarea ? contentTextarea.value : '';

        console.log(`handleSubmitReplyButtonClick: Attempting to submit reply. Parent Comment ID: ${parentCommentId}, Content: "${content.substring(0, 50)}..."`); // Added log

        if (!content.trim()) {
            this.showNotification("Please enter a reply.", "error");
            return;
        }

        if (this.pagePostId === null) {
            console.error("handleSubmitReplyButtonClick: Post ID not available for reply submission. this.pagePostId is null.");
            this.showNotification("Error: Post ID missing for reply.", "error");
            return;
        }

        await this.sendComment(
            parseInt(this.pagePostId),
            content.trim(),
            parentCommentId
        );

        // Clear textarea and hide form after successful submission
        if (contentTextarea) contentTextarea.value = '';
        replyFormDiv.classList.add('d-none');
        replyFormDiv.classList.remove('d-flex'); // Ensure d-flex is removed too
        console.log(`handleSubmitReplyButtonClick: Reply form hidden for Comment ID: ${parentCommentId}`); // Added log
    }

    // Event handler for canceling a reply.
    handleReplyCancel(event) {
        event.preventDefault();
        const form = event.currentTarget.closest('.reply-form');
        const commentId = form ? form.dataset.commentId : 'unknown'; // Added log context
        console.log(`handleReplyCancel: Cancel button clicked for Comment ID: ${commentId}`); // Added log
        if (form) {
            form.classList.add('d-none');
            form.classList.remove('d-flex'); // Ensure d-flex is removed too
            const textarea = form.querySelector('textarea');
            if (textarea) textarea.value = ''; // Clear content on cancel
            console.log(`handleReplyCancel: Reply form hidden and cleared for Comment ID: ${commentId}`); // Added log
        }
    }

    // Shows the reply form for a specific comment.
    showReplyForm(commentId) {
        console.log(`showReplyForm: Attempting to show reply form for Comment ID: ${commentId}`); // Added log
        // Hide all other reply forms first to ensure only one is open at a time
        document.querySelectorAll('.reply-form').forEach(form => {
            if (!form.classList.contains('d-none')) { // Only log if actually hiding
                console.log(`showReplyForm: Hiding existing reply form for Comment ID: ${form.dataset.commentId}`);
            }
            form.classList.add('d-none');
            form.classList.remove('d-flex'); // Ensure d-flex is removed when hiding others
        });

        const replyForm = document.getElementById(`replyForm${commentId}`);
        if (replyForm) {
            replyForm.classList.remove('d-none'); // Show the target reply form
            replyForm.classList.add('d-flex'); // ADD d-flex to make it visible with flex display
            replyForm.querySelector('textarea')?.focus(); // Focus the textarea
            console.log(`showReplyForm: Reply form shown for Comment ID: ${commentId}`); // Added log
        } else {
            console.warn(`showReplyForm: Reply form with ID 'replyForm${commentId}' not found.`); // Added log
        }
    }

    // Rebinds event listeners for share buttons.
    rebindShareButtons() {
        // console.log("rebindShareButtons: Attempting to bind share buttons.");
        document.querySelectorAll('.share-button').forEach(button => {
            button.removeEventListener('click', this.handleShareButtonClick);
            button.addEventListener('click', this.handleShareButtonClick);
            // console.log("rebindShareButtons: Bound click handler to Share button with id:", button.id);
        });

        document.querySelectorAll('.share-option').forEach(option => {
            option.removeEventListener('click', this.handleShareOptionClick);
            option.addEventListener('click', this.handleShareOptionClick);
            // console.log("rebindShareButtons: Bound click handler to share option:", option.outerHTML.slice(0, 50));
        });
    }

    // Event handler for clicking the main share button.
    handleShareButtonClick(event) {
        event.preventDefault();
        event.stopPropagation(); // Prevent immediate closing by document click

        const postId = event.currentTarget.getAttribute('data-post-id');
        const dropdown = document.getElementById(`shareDropdown-${postId}`);

        // Close any previously open dropdown if it's not the current one
        if (this.openShareDropdown && this.openShareDropdown !== dropdown) {
            this.openShareDropdown.classList.add('d-none');
        }

        if (dropdown) {
            dropdown.classList.toggle('d-none'); // Toggle visibility
            this.openShareDropdown = dropdown.classList.contains('d-none') ? null : dropdown; // Update tracking
        }
    }

    // Event handler for clicking a specific share option (e.g., Facebook, Twitter).
    async handleShareOptionClick(event) {
        event.preventDefault();
        event.stopPropagation(); // Prevent immediate closing by document click

        const platform = event.currentTarget.getAttribute('data-platform');
        const postId = event.currentTarget.closest('.share-dropdown').getAttribute('data-post-id');
        // Construct the full URL for the post
        const postUrl = window.location.origin + `/r/communitySlug/posts/${postId}`; // Adjust this if your post URL structure is different, e.g., using post slug
        const postTitle = document.querySelector(`[data-post-id="${postId}"] .post-title`)?.textContent || "Check out this discussion!";

        let shareUrl = '';

        switch (platform) {
            case 'facebook':
                shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(postUrl)}`;
                break;
            case 'twitter':
                shareUrl = `https://twitter.com/intent/tweet?url=${encodeURIComponent(postUrl)}&text=${encodeURIComponent(postTitle)}`;
                break;
            case 'reddit':
                shareUrl = `https://www.reddit.com/submit?url=${encodeURIComponent(postUrl)}&title=${encodeURIComponent(postTitle)}`;
                break;
            case 'linkedin':
                shareUrl = `https://www.linkedin.com/shareArticle?mini=true&url=${encodeURIComponent(postUrl)}&title=${encodeURIComponent(postTitle)}`;
                break;
            case 'whatsapp':
                shareUrl = `https://api.whatsapp.com/send?text=${encodeURIComponent(postTitle + ' ' + postUrl)}`;
                break;
            case 'copy':
                try {
                    // Use document.execCommand('copy') for better iframe compatibility
                    const textarea = document.createElement('textarea');
                    textarea.value = postUrl;
                    textarea.style.position = 'fixed'; // Keep it off-screen
                    textarea.style.opacity = '0';
                    document.body.appendChild(textarea);
                    textarea.focus();
                    textarea.select();
                    document.execCommand('copy');
                    document.body.removeChild(textarea);
                    this.showNotification('Link copied to clipboard!', 'success');
                } catch (err) {
                    console.error("Failed to copy link:", err);
                    this.showNotification("Failed to copy link. Please copy manually.", 'error');
                }
                // Don't open a new window for copy, but close dropdown
                if (this.openShareDropdown) {
                    this.openShareDropdown.classList.add('d-none');
                    this.openShareDropdown = null;
                }
                return; // Exit without opening a new window
            default:
                console.warn('Unknown share platform:', platform);
                return;
        }

        window.open(shareUrl, '_blank'); // Open share link in a new tab

        // Close the dropdown after sharing (unless it's 'copy')
        if (this.openShareDropdown) {
            this.openShareDropdown.classList.add('d-none');
            this.openShareDropdown = null;
        }

        // Increment share count on the server (optional)
        if (this.postConnection.state === signalR.HubConnectionState.Connected) {
            try {
                await this.postConnection.invoke("IncrementShareCount", parseInt(postId));
            } catch (err) {
                console.error("Error incrementing share count:", err);
            }
        }
    }

    // Rebinds event listeners for post vote buttons.
    rebindPostVoteButtons() {
        // console.log("rebindPostVoteButtons: Attempting to bind post vote buttons.");
        document.querySelectorAll('.post-actions .vote-btn').forEach(button => {
            button.removeEventListener('click', this.handlePostVoteButtonClick);
            button.addEventListener('click', this.handlePostVoteButtonClick);
            // console.log("rebindPostVoteButtons: Bound click handler to post vote button with id:", button.id);
        });
    }

    // Event handler for post vote buttons.
    handlePostVoteButtonClick(event) {
        event.preventDefault();
        const button = event.currentTarget;
        const postId = parseInt(button.getAttribute('data-post-id'));
        const voteType = parseInt(button.getAttribute('data-vote-type')); // 1 for upvote, -1 for downvote
        this.votePost(postId, voteType);
    }

    // Rebinds event listeners for poll vote buttons.
    rebindPollVoteButtons() {
        document.querySelectorAll('.poll-option-vote-btn').forEach(button => {
            button.removeEventListener('click', this.handlePollVoteButtonClick);
            button.addEventListener('click', this.handlePollVoteButtonClick);
        });
    }

    // Event handler for poll vote buttons.
    async handlePollVoteButtonClick(event) {
        const button = event.currentTarget;
        const postId = parseInt(button.getAttribute('data-post-id'));
        const optionId = parseInt(button.getAttribute('data-option-id'));

        // For simplicity, assuming single choice poll. Adjust this logic for multiple choice if needed.
        await this.votePoll(postId, [optionId]); // Pass selected option(s) in an array
    }

    // Sends a poll vote to the server.
    async votePoll(postId, optionIds) {
        if (this.postConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected to the server. Please check your internet connection.", 'error');
            return;
        }
        try {
            await this.postConnection.invoke("VotePoll", postId, optionIds);
            this.showNotification("Your vote has been cast!", 'success');
        } catch (err) {
            console.error("Error voting on poll:", err);
            this.showNotification("Failed to vote on poll. Please try again.", 'error');
        }
    }

    // --- UI Update Helper Methods ---

    // Displays a temporary notification message to the user.
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

        // Ensure a toast container exists, or create one
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            const newToastContainer = document.createElement('div');
            newToastContainer.id = 'toast-container';
            newToastContainer.style.position = 'fixed';
            newToastContainer.style.top = '1rem';
            newToastContainer.style.right = '1rem';
            newToastContainer.style.zIndex = '1050'; // Ensure it's above other content
            document.body.appendChild(newToastContainer);
            newToastContainer.appendChild(toast);
        } else {
            toastContainer.appendChild(toast);
        }

        // Initialize and show the Bootstrap toast
        const bsToast = new bootstrap.Toast(toast, { delay: 3000 }); // Show for 3 seconds
        bsToast.show();

        // Remove toast from DOM after it's hidden
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    // Updates the displayed vote counts and active state for comment buttons.
    //updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote) {
    //    console.log(`updateCommentVotes: Updating UI for commentId: ${commentId}. Received Up: ${upvoteCount}, Down: ${downvoteCount}, UserVote: ${currentUserVote}`); // Added log
    //    const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
    //    if (commentElement) {
    //        // --- FIX HERE: Target by ID directly for robustness ---
    //        const scoreElement = document.getElementById(`commentVote-${commentId}`);
    //        if (scoreElement) {
    //            const newScore = upvoteCount - downvoteCount;
    //            scoreElement.textContent = newScore; // Calculate net score
    //            console.log(`updateCommentVotes: Set scoreElement textContent to: ${newScore}`); // Added log
    //        } else {
    //            console.warn(`updateCommentVotes: Score element with ID 'commentVote-${commentId}' not found.`); // Added log
    //        }

    //        const upvoteBtn = commentElement.querySelector('.comment-vote-btn.upvote');
    //        const downvoteBtn = commentElement.querySelector('.comment-vote-btn.downvote');

    //        // Set 'active' class based on the current user's vote status
    //        upvoteBtn?.classList.toggle('active', currentUserVote === 1);
    //        downvoteBtn?.classList.toggle('active', currentUserVote === -1);
    //        console.log(`updateCommentVotes: Toggled active classes. Upvote active: ${upvoteBtn?.classList.contains('active')}, Downvote active: ${downvoteBtn?.classList.contains('active')}`); // Added log
    //    } else {
    //        console.warn(`updateCommentVotes: Comment element with data-comment-id="${commentId}" not found.`); // Added log
    //    }
    //}

    // In updateCommentVotes method:
    updateCommentVotes(commentId, upvoteCount, downvoteCount, currentUserVote) {
        console.log(`updateCommentVotes: Updating UI for commentId: ${commentId}. Received Up: ${upvoteCount}, Down: ${downvoteCount}, UserVote: ${currentUserVote}`);
        const commentElement = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);

        if (commentElement) {
            // Calculate net score
            const newScore = upvoteCount - downvoteCount;

            // Update score display
            const scoreElement = commentElement.querySelector('.comment-vote-count');
            if (scoreElement) {
                scoreElement.textContent = newScore;
            }

            // Update button active states
            const upvoteBtn = commentElement.querySelector('.comment-vote-btn.upvote');
            const downvoteBtn = commentElement.querySelector('.comment-vote-btn.downvote');

            if (upvoteBtn) {
                upvoteBtn.classList.toggle('active', currentUserVote === 1);
            }
            if (downvoteBtn) {
                downvoteBtn.classList.toggle('active', currentUserVote === -1);
            }
        }
    }

    // Updates the displayed vote counts and active state for post buttons.
    updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote) {
        // console.log(`updatePostVotesUI: PostId: ${postId}, Up: ${upvoteCount}, Down: ${downvoteCount}, UserVote: ${currentUserVote}`);

        const upvoteCountElement = document.getElementById(`upvoteCount-${postId}`);
        const downvoteCountElement = document.getElementById(`downvoteCount-${postId}`);
        const totalScoreElement = document.getElementById(`totalScore-${postId}`); // If you have a combined score element

        const upvoteBtn = document.getElementById(`upvoteBtn-${postId}`);
        const downvoteBtn = document.getElementById(`downvoteBtn-${postId}`);

        if (upvoteCountElement) upvoteCountElement.textContent = upvoteCount;
        if (downvoteCountElement) downvoteCountElement.textContent = downvoteCount;
        if (totalScoreElement) totalScoreElement.textContent = upvoteCount - downvoteCount;

        if (upvoteBtn) {
            upvoteBtn.classList.toggle('active', currentUserVote === 1);
        }
        if (downvoteBtn) {
            downvoteBtn.classList.toggle('active', currentUserVote === -1);
        }
    }

    // Shows a typing indicator for a specific user.
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

    // Hides the typing indicator for a specific user.
    hideTypingIndicator(userName) {
        const indicator = document.getElementById(`typing-${userName}`);
        if (indicator) {
            indicator.remove();
        }
    }

    // Updates the notification badge count (e.g., in a navbar).
    updateNotificationBadge(count = null) {
        const badge = document.getElementById('notification-badge');
        if (badge) {
            if (count !== null) {
                // If a specific count is provided
                badge.textContent = count > 99 ? '99+' : count; // Cap at 99+
                badge.style.display = count > 0 ? 'inline-block' : 'none';
            } else {
                // If no count, increment current count
                const currentCount = parseInt(badge.textContent) || 0;
                badge.textContent = currentCount + 1;
                badge.style.display = 'inline-block';
            }
        }
    }
}

// Instantiate the SignalRManager to make it globally accessible
const signalRManager = new SignalRManager();
window.signalRManager = signalRManager; // Useful for console debugging

// Initialize SignalR connections and event binding when the DOM is fully loaded
document.addEventListener('DOMContentLoaded', async () => {
    // Establish SignalR connections
    await signalRManager.initializeConnections();

    // Get the post ID from the page (assuming a data attribute or hidden input)
    const postIdElement = document.getElementById('pagePostId') || document.querySelector('[data-post-id]');
    if (postIdElement) {
        signalRManager.pagePostId = parseInt(postIdElement.dataset.postId || postIdElement.value);
        console.log("DOMContentLoaded: Page Post ID set to:", signalRManager.pagePostId);

        // Join the post-specific SignalR group for real-time updates
        await signalRManager.postConnection.invoke("JoinPostGroup", signalRManager.pagePostId);

        // Rebind all dynamic event listeners (comments, shares, votes)
        signalRManager.rebindCommentEvents();

        // Set up typing indicator logic for the main comment textarea
        const commentTextarea = document.getElementById('mainCommentContent');
        if (commentTextarea) {
            let typingTimer; // Local timer for this specific textarea
            commentTextarea.addEventListener('input', () => {
                signalRManager.startTyping(signalRManager.pagePostId); // Notify server about typing

                clearTimeout(typingTimer); // Reset timer on each input
                typingTimer = setTimeout(async () => {
                    await signalRManager.stopTyping(signalRManager.pagePostId); // Notify server about stopping typing
                }, 1000); // 1-second delay after last input
            });
        }
    } else {
        console.error("Post ID element not found on the page (expected ID 'pagePostId' or data-post-id). Real-time features might be limited.");
    }
});

// Event listener for the main comment submission button
document.getElementById('submitMainComment')?.addEventListener('click', async function () {
    const postId = signalRManager.pagePostId;
    const commentContent = document.getElementById('mainCommentContent').value;

    if (!commentContent.trim()) {
        signalRManager.showNotification("Please enter a comment.", "error");
        return;
    }

    if (postId === null) {
        console.error("Main comment submission: Post ID is null. Cannot send comment.");
        signalRManager.showNotification("Error: Post ID missing for main comment. Please refresh.", "error");
        return;
    }

    // Send the comment to the server via SignalR
    await signalRManager.sendComment(postId, commentContent.trim());
    document.getElementById('mainCommentContent').value = ''; // Clear textarea after sending
});
