class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.pagePostId = null;
        this.isAuthenticated = false; // Track authentication status

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

        // --- Submit Reply with loading animation ---
        const submitReplyBtn = target.closest('.reply-submit-btn');
        if (submitReplyBtn) {
            event.preventDefault();
            const form = submitReplyBtn.closest('.reply-form');
            const parentId = parseInt(form?.dataset.commentId, 10);
            
            // Get content from Quill editor if available
            let content = '';
            if (window[`replyQuill${parentId}`]) {
                content = window[`replyQuill${parentId}`].root.innerHTML.trim();
                const text = window[`replyQuill${parentId}`].getText().trim();
                if (!text || text.length === 0) {
                    this.showNotification('Please enter a reply', 'warning');
                    return;
                }
            } else {
                // Fallback to textarea
                const textarea = form?.querySelector('textarea');
                content = textarea?.value.trim();
                if (!content) {
                    this.showNotification('Please enter a reply', 'warning');
                    return;
                }
            }
            
            if (content && this.pagePostId && !isNaN(parentId)) {
                // Add loading state
                submitReplyBtn.classList.add('loading');
                submitReplyBtn.disabled = true;
                const originalHTML = submitReplyBtn.innerHTML;
                submitReplyBtn.innerHTML = '<span>Replying...</span>';
                
                try {
                    await this.sendComment(this.pagePostId, content, parentId);
                    
                    // Show success state
                    submitReplyBtn.classList.remove('loading');
                    submitReplyBtn.classList.add('success');
                    submitReplyBtn.innerHTML = '<span>Posted!</span>';
                    
                    // Clear the editor and hide form after brief delay
                    setTimeout(() => {
                        if (window[`replyQuill${parentId}`]) {
                            window[`replyQuill${parentId}`].setText('');
                            delete window[`replyQuill${parentId}`];
                        } else {
                            const textarea = form?.querySelector('textarea');
                            if (textarea) textarea.value = '';
                        }
                        
                        form.classList.add('d-none');
                        submitReplyBtn.classList.remove('success');
                        submitReplyBtn.disabled = false;
                        submitReplyBtn.innerHTML = originalHTML;
                    }, 1000);
                } catch (error) {
                    // Handle error
                    submitReplyBtn.classList.remove('loading');
                    submitReplyBtn.disabled = false;
                    submitReplyBtn.innerHTML = originalHTML;
                    this.showNotification('Failed to post reply', 'error');
                }
            }
            return;
        }

        // --- Cancel Reply ---
        const cancelReplyBtn = target.closest('.reply-cancel-btn');
        if (cancelReplyBtn) {
            event.preventDefault();
            const form = cancelReplyBtn.closest('.reply-form');
            if (form) {
                const commentId = form.dataset.commentId;
                
                // Clear and destroy Quill editor if it exists
                if (window[`replyQuill${commentId}`]) {
                    window[`replyQuill${commentId}`].setText('');
                    delete window[`replyQuill${commentId}`];
                }
                
                // Clear textarea fallback
                const textarea = form.querySelector('textarea');
                if (textarea) textarea.value = '';
                
                form.classList.add('d-none');
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
        // Check authentication before sending comment
        console.log('🔐 Checking authentication before comment:', this.isAuthenticated);
        
        if (!this.isAuthenticated) {
            console.log('❌ User not authenticated, showing login prompt');
            this.showLoginPrompt();
            return;
        }

        // Sanitize HTML content for display in placeholder
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = content;
        const previewText = tempDiv.textContent || tempDiv.innerText || "";
        
        const placeholderHtml = `<div id="optimistic-comment-placeholder" style="opacity: 0.6;"><p><strong>You</strong> <small>sending...</small></p><p>${previewText.substring(0, 100)}${previewText.length > 100 ? '...' : ''}</p><hr/></div>`;
        const targetContainer = parentId ? document.getElementById(`commentReplies-${parentId}`) : document.querySelector('.comment-list');
        targetContainer?.insertAdjacentHTML('beforeend', placeholderHtml);

        try {
            await this.postConnection.invoke("SendComment", postId, content, parentId);
        } catch (err) {
            console.error("Failed to send comment:", err);
            document.getElementById('optimistic-comment-placeholder')?.remove();
            this.showNotification('Failed to post comment. Please try again.', 'error');
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

    // AUTHENTICATION METHODS
    showLoginPrompt() {
        // Get current page URL for return navigation
        const currentUrl = window.location.pathname + window.location.search;
        const returnUrl = encodeURIComponent(currentUrl);
        
        console.log('🔗 Current URL:', currentUrl);
        console.log('🔗 Encoded ReturnUrl:', returnUrl);
        
        // Update modal button URLs with return URL
        const loginBtn = document.getElementById('loginPromptLoginBtn');
        const registerBtn = document.getElementById('loginPromptRegisterBtn');
        
        // Use the simpler route pattern defined in Program.cs
        if (loginBtn) {
            loginBtn.href = `/auth?returnUrl=${returnUrl}`;
            console.log('🔗 Login button href set to:', loginBtn.href);
        }
        if (registerBtn) {
            registerBtn.href = `/auth?returnUrl=${returnUrl}#register`;
            console.log('🔗 Register button href set to:', registerBtn.href);
        }
        
        // Show the modal using Bootstrap
        const modalElement = document.getElementById('loginPromptModal');
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
            console.log('✅ Login modal shown');
        } else {
            // Fallback: direct redirect if modal not found
            console.log('⚠️ Modal not found, redirecting directly');
            this.showNotification('Please login to comment', 'info');
            setTimeout(() => {
                window.location.href = `/auth?returnUrl=${returnUrl}`;
            }, 1500);
        }
    }

    // UI UPDATE METHODS
    showReplyForm(commentId) {
        const targetForm = document.getElementById(`replyForm${commentId}`);
        if (!targetForm) return;

        // Close all other reply forms and destroy their editors
        document.querySelectorAll('.reply-form').forEach(form => {
            if (form.id !== targetForm.id && !form.classList.contains('d-none')) {
                form.classList.add('d-none');
                const formCommentId = form.dataset.commentId;
                // Destroy the Quill instance if it exists
                if (window[`replyQuill${formCommentId}`]) {
                    delete window[`replyQuill${formCommentId}`];
                }
            }
        });

        // Toggle visibility of target form
        const isHidden = targetForm.classList.contains('d-none');
        targetForm.classList.toggle('d-none');
        
        if (isHidden) { // Form is being shown
            // Initialize Quill editor for this reply form if not already initialized
            if (!window[`replyQuill${commentId}`]) {
                const toolbarOptions = [
                    ['bold', 'italic', 'underline'],
                    ['link', 'blockquote', 'code-block'],
                    [{ 'list': 'ordered'}, { 'list': 'bullet' }]
                ];

                window[`replyQuill${commentId}`] = new Quill(`#replyEditor${commentId}`, {
                    theme: 'snow',
                    modules: {
                        toolbar: toolbarOptions
                    },
                    placeholder: 'Write your reply...'
                });

                // Update hidden textarea when editor content changes
                window[`replyQuill${commentId}`].on('text-change', function() {
                    const content = window[`replyQuill${commentId}`].root.innerHTML;
                    document.getElementById(`replyContent${commentId}`).value = content;
                });
            }
            
            // Focus the editor
            window[`replyQuill${commentId}`].focus();
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
    
    showNotification(message, type = 'info') {
        // Create toast notification
        const toastContainer = document.getElementById('toast-container') || (() => {
            const container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'position-fixed top-0 end-0 p-3';
            container.style.zIndex = '1055';
            document.body.appendChild(container);
            return container;
        })();

        const toastId = `toast-${Date.now()}`;
        const icons = {
            'success': 'fa-check-circle text-success',
            'error': 'fa-exclamation-circle text-danger',
            'warning': 'fa-exclamation-triangle text-warning',
            'info': 'fa-info-circle text-info'
        };
        
        const icon = icons[type] || icons['info'];
        
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas ${icon} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
        toast.show();
        
        // Remove toast element after it's hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
}


// INITIALIZATION
document.addEventListener('DOMContentLoaded', async () => {
    try {
        const signalRManager = new SignalRManager();
        window.signalRManager = signalRManager;

        // Set authentication status from page
        const isAuthElement = document.getElementById('isAuthenticated');
        const authValue = isAuthElement ? isAuthElement.value : 'false';
        signalRManager.isAuthenticated = authValue === 'true' || authValue === 'True';
        
        console.log('🔐 Raw Auth Value:', authValue);
        console.log('🔐 Authentication Status:', signalRManager.isAuthenticated);
        console.log('🔐 Element exists:', !!isAuthElement);

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

        // Main comment form submission with loading animation
        const submitButton = document.getElementById('submitMainComment');
        if (submitButton) {
            submitButton.addEventListener('click', async (e) => {
                e.preventDefault();
                
                // Get content from Quill editor if available, otherwise fall back to textarea
                let content = '';
                if (window.mainCommentQuill) {
                    content = window.mainCommentQuill.root.innerHTML.trim();
                    // Check if editor is empty (just contains <p><br></p> or similar)
                    const text = window.mainCommentQuill.getText().trim();
                    if (!text || text.length === 0) {
                        signalRManager.showNotification('Please enter a comment', 'warning');
                        return;
                    }
                } else {
                    // Fallback to textarea
                    const contentElement = document.getElementById('mainCommentContent');
                    content = contentElement?.value.trim();
                    if (!content) {
                        signalRManager.showNotification('Please enter a comment', 'warning');
                        return;
                    }
                }
                
                if (content && signalRManager.pagePostId) {
                    // Add loading state
                    submitButton.classList.add('loading');
                    submitButton.disabled = true;
                    const originalText = submitButton.innerHTML;
                    submitButton.innerHTML = '<span>Posting...</span>';
                    
                    try {
                        await signalRManager.sendComment(signalRManager.pagePostId, content);
                        
                        // Show success state
                        submitButton.classList.remove('loading');
                        submitButton.classList.add('success');
                        submitButton.innerHTML = '<span>Posted!</span>';
                        
                        // Clear the editor after successful submission
                        if (window.mainCommentQuill) {
                            window.mainCommentQuill.setText('');
                        } else {
                            document.getElementById('mainCommentContent').value = '';
                        }
                        
                        // Reset button after animation
                        setTimeout(() => {
                            submitButton.classList.remove('success');
                            submitButton.disabled = false;
                            submitButton.innerHTML = originalText;
                        }, 1500);
                    } catch (error) {
                        // Handle error
                        submitButton.classList.remove('loading');
                        submitButton.disabled = false;
                        submitButton.innerHTML = originalText;
                        signalRManager.showNotification('Failed to post comment', 'error');
                    }
                }
            });
        }

    } catch (error) {
        console.error('Fatal error during initialization:', error);
    }
});

