class SignalRManager {
    constructor() {
        this.postConnection = null;
        this.notificationConnection = null;
        this.pagePostId = null;
        this.isAuthenticated = false; // Track authentication status
        
        // Debouncing maps to prevent spam
        this.voteDebounceMap = new Map(); // Track ongoing votes
        this.lastVoteTime = new Map(); // Track last vote timestamp

        // Bind 'this' context for the main event handler
        this.handleDelegatedClick = this.handleDelegatedClick.bind(this);
    }
    
    // Utility: Debounce to prevent spam clicks
    debounce(key, func, wait = 300) {
        const now = Date.now();
        const lastTime = this.lastVoteTime.get(key) || 0;
        
        if (now - lastTime < wait) {
            console.log('⏱️ Debounced:', key, 'Wait', wait - (now - lastTime), 'ms');
            return false; // Too soon
        }
        
        this.lastVoteTime.set(key, now);
        return true; // Allow
    }

    async initializeConnections() {
        console.log('🔌 Initializing SignalR connections...');
        
        // Build connections with proper authentication
        this.postConnection = new signalR.HubConnectionBuilder()
            .withUrl("/postHub", {
                accessTokenFactory: () => {
                    // Get authentication token if available
                    const token = document.querySelector('meta[name="auth-token"]')?.content;
                    return token || '';
                }
            })
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount < 3) {
                        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 10000);
                    } else {
                        return null;
                    }
                }
            })
            .build();

        this.notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub", {
                accessTokenFactory: () => {
                    const token = document.querySelector('meta[name="auth-token"]')?.content;
                    return token || '';
                }
            })
            .withAutomaticReconnect()
            .build();

        this.setupPostEventHandlers();
        this.setupNotificationEventHandlers();

        try {
            await this.postConnection.start();
            console.log("✅ PostHub connection started successfully.");
            console.log("✅ Connection state:", this.postConnection.state);
            console.log("✅ Connection ID:", this.postConnection.connectionId);
            
            await this.notificationConnection.start();
            console.log("✅ NotificationHub connection started successfully.");
        } catch (err) {
            console.error("❌ SignalR connection start failed: ", err);
            console.error("❌ Error details:", err.message);
            console.error("❌ Error stack:", err.stack);
            this.showNotification("Failed to connect to real-time features.", 'error');
        }
    }

    setupPostEventHandlers() {
        // Vote update handler - THIS IS THE KEY ONE FOR VOTING
        this.postConnection.on("UpdatePostVotesUI", (postId, upvoteCount, downvoteCount, currentUserVote) => {
            const timestamp = new Date().toISOString();
            console.log(`🎯 [${timestamp}] === SIGNALR EVENT RECEIVED: UpdatePostVotesUI ===`);
            console.log('📥 Raw parameters received:', { postId, upvoteCount, downvoteCount, currentUserVote });
            console.log('📊 Current DOM score before update:', document.getElementById(`voteScore-${postId}`)?.textContent);
            this.updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote);
            console.log('📊 Current DOM score after update:', document.getElementById(`voteScore-${postId}`)?.textContent);
        });
        
        // CRITICAL: Poll update handler - receives poll data after voting
        this.postConnection.on("ReceivePollUpdate", (pollData) => {
            console.log('🎯🎯🎯 POLL UPDATE RECEIVED!', pollData);
            this.updatePollResultsUI(pollData);
        });
        
        this.postConnection.on("ReceivePollVoteError", (msg) => {
            console.log('❌ Poll vote error:', msg);
            this.showNotification(msg, 'error');
        });
        
        this.postConnection.on("ReceivePollVoteSuccess", (msg) => {
            console.log('✅ Poll vote success:', msg);
            this.showNotification(msg, 'success');
        });
        
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
                
                // Update comment count only for top-level comments
                if (!parentCommentId) {
                    this.updateCommentCount(1);
                }
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
        this.postConnection.on("CommentVoteUpdated", (commentId, up, down, score, userVote) => {
            console.log('💬 CommentVoteUpdated received:', { commentId, up, down, score, userVote });
            this.updateCommentVotes(commentId, up, down, score, userVote);
        });
        // NOTE: UpdatePostVotesUI is already registered above with better logging
        // NOTE: ReceivePollUpdate is already registered above with better logging
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
            console.log('🎯 Poll option clicked');
            const pollContainer = pollOption.closest('.poll-container');
            const postId = parseInt(pollContainer?.dataset.postId, 10);
            const optionId = parseInt(pollOption.dataset.optionId, 10);
            console.log('📊 Poll vote data - PostID:', postId, 'OptionID:', optionId);
            if (!isNaN(postId) && !isNaN(optionId)) {
                await this.castPollVote(postId, optionId);
            } else {
                console.error('❌ Invalid poll vote data');
            }
            return;
        }

        // --- Post Vote ---
        const postVoteBtn = target.closest('.post-container .vote-btn');
        if (postVoteBtn) {
            event.preventDefault();
            console.log('🎯 Post vote button clicked');
            const postId = parseInt(postVoteBtn.dataset.postId, 10);
            const voteType = parseInt(postVoteBtn.dataset.voteType, 10);
            console.log('🗳️ Post vote data - PostID:', postId, 'VoteType:', voteType);
            if (!isNaN(postId) && !isNaN(voteType)) {
                await this.votePost(postId, voteType);
            } else {
                console.error('❌ Invalid post vote data. PostID:', postId, 'VoteType:', voteType);
            }
            return;
        }

        // --- Comment Vote ---
        const commentVoteBtn = target.closest('.comment-vote-btn');
        if (commentVoteBtn) {
            event.preventDefault();
            console.log('🎯 Comment vote button clicked');
            const commentId = parseInt(commentVoteBtn.dataset.commentId, 10);
            const voteType = parseInt(commentVoteBtn.dataset.voteType, 10);
            console.log('💬 Comment vote data - CommentID:', commentId, 'VoteType:', voteType);
            if (!isNaN(commentId) && !isNaN(voteType)) {
                await this.voteComment(commentId, voteType);
            } else {
                console.error('❌ Invalid comment vote data');
            }
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
        console.log('💬 Voting on comment:', commentId, 'Type:', voteType);
        
        // Check authentication
        if (!this.isAuthenticated) {
            console.log('❌ User not authenticated, showing login prompt');
            this.showLoginPrompt();
            return;
        }
        
        // Debounce comment votes
        const debounceKey = `comment-${commentId}`;
        if (!this.debounce(debounceKey, null, 300)) {
            console.log('⏱️ Comment vote ignored (debounced)');
            return;
        }
        
        // Find comment vote elements (separate upvote/downvote counts)
        const upvoteCountEl = document.getElementById(`commentUpvote${commentId}`);
        const downvoteCountEl = document.getElementById(`commentDownvote${commentId}`);
        const upvoteBtn = document.querySelector(`[data-comment-id="${commentId}"][data-vote-type="1"]`);
        const downvoteBtn = document.querySelector(`[data-comment-id="${commentId}"][data-vote-type="-1"]`);
        
        console.log('🔍 Comment vote elements:', {
            upvoteCountEl: !!upvoteCountEl,
            downvoteCountEl: !!downvoteCountEl,
            upvoteBtn: !!upvoteBtn,
            downvoteBtn: !!downvoteBtn
        });
        
        // Store original state
        const originalState = {
            upvoteCount: upvoteCountEl?.textContent || '0',
            downvoteCount: downvoteCountEl?.textContent || '0',
            upvoteActive: upvoteBtn?.classList.contains('active') || false,
            downvoteActive: downvoteBtn?.classList.contains('active') || false
        };
        
        console.log('📊 Original comment state:', originalState);
        
        // Calculate optimistic count changes
        let currentUpvotes = parseInt(upvoteCountEl?.textContent || '0');
        let currentDownvotes = parseInt(downvoteCountEl?.textContent || '0');
        let newUpvotes = currentUpvotes;
        let newDownvotes = currentDownvotes;
        
        const wasUpvoted = upvoteBtn?.classList.contains('active');
        const wasDownvoted = downvoteBtn?.classList.contains('active');
        
        console.log('🔘 Comment button states - wasUpvoted:', wasUpvoted, 'wasDownvoted:', wasDownvoted);
        
        if (voteType === 1) {
            // Upvote clicked
            if (wasUpvoted) {
                console.log('➖ Removing comment upvote');
                newUpvotes = Math.max(0, newUpvotes - 1);
                upvoteBtn?.classList.remove('active');
            } else if (wasDownvoted) {
                console.log('🔄 Changing comment downvote to upvote');
                newUpvotes++;
                newDownvotes = Math.max(0, newDownvotes - 1);
                downvoteBtn?.classList.remove('active');
                upvoteBtn?.classList.add('active');
            } else {
                console.log('➕ Adding new comment upvote');
                newUpvotes++;
                upvoteBtn?.classList.add('active');
            }
        } else if (voteType === -1) {
            // Downvote clicked
            if (wasDownvoted) {
                console.log('➖ Removing comment downvote');
                newDownvotes = Math.max(0, newDownvotes - 1);
                downvoteBtn?.classList.remove('active');
            } else if (wasUpvoted) {
                console.log('🔄 Changing comment upvote to downvote');
                newUpvotes = Math.max(0, newUpvotes - 1);
                newDownvotes++;
                upvoteBtn?.classList.remove('active');
                downvoteBtn?.classList.add('active');
            } else {
                console.log('➕ Adding new comment downvote');
                newDownvotes++;
                downvoteBtn?.classList.add('active');
            }
        }
        
        console.log('🎯 Calculated new comment counts - Up:', newUpvotes, 'Down:', newDownvotes);
        
        // Update UI immediately (optimistic)
        if (upvoteCountEl) {
            upvoteCountEl.textContent = newUpvotes.toString();
            console.log('🔄 Optimistic comment upvote count set to:', newUpvotes);
        }
        
        if (downvoteCountEl) {
            downvoteCountEl.textContent = newDownvotes.toString();
            console.log('🔄 Optimistic comment downvote count set to:', newDownvotes);
        }
        
        console.log('✅ Comment UI updated optimistically');
        
        try { 
            await this.postConnection.invoke("VoteComment", commentId, voteType);
            console.log('✅ Comment vote sent to hub');
        }
        catch (err) { 
            console.error("❌ Failed to vote on comment:", err);
            
            // Revert UI on error
            if (upvoteCountEl) upvoteCountEl.textContent = originalState.upvoteCount;
            if (downvoteCountEl) downvoteCountEl.textContent = originalState.downvoteCount;
            if (upvoteBtn) {
                originalState.upvoteActive ? upvoteBtn.classList.add('active') : upvoteBtn.classList.remove('active');
            }
            if (downvoteBtn) {
                originalState.downvoteActive ? downvoteBtn.classList.add('active') : downvoteBtn.classList.remove('active');
            }
            
            this.showNotification('Failed to vote on comment', 'error');
            console.log('🔄 Comment UI reverted to original state');
        }
    }

    async votePost(postId, voteType) {
        const timestamp = new Date().toISOString();
        console.log(`🗳️ [${timestamp}] Voting on post:`, postId, 'Type:', voteType);
        
        // Check authentication
        if (!this.isAuthenticated) {
            console.log('❌ User not authenticated, showing login prompt');
            this.showLoginPrompt();
            return;
        }
        
        // ============================================
        // DEBOUNCE TO PREVENT SPAM
        // ============================================
        const debounceKey = `post-${postId}`;
        if (!this.debounce(debounceKey, null, 300)) {
            console.log('⏱️ Vote ignored (debounced)');
            return; // Prevent spam clicking
        }
        
        // ============================================
        // OPTIMISTIC UI UPDATE (INSTANT FEEDBACK)
        // ============================================
        const upvoteCountEl = document.getElementById(`upvoteCount-${postId}`);
        const downvoteCountEl = document.getElementById(`downvoteCount-${postId}`);
        const upvoteBtn = document.getElementById(`upvoteBtn-${postId}`);
        const downvoteBtn = document.getElementById(`downvoteBtn-${postId}`);
        
        if (!upvoteBtn || !downvoteBtn) {
            console.error('❌ Vote UI elements not found for post:', postId);
            console.log('Looking for:', `upvoteBtn-${postId}`, `downvoteBtn-${postId}`);
            return;
        }
        
        // Store original state for revert on error
        const originalState = {
            upvoteCount: upvoteCountEl?.textContent || '0',
            downvoteCount: downvoteCountEl?.textContent || '0',
            upvoteActive: upvoteBtn?.classList.contains('active') || false,
            downvoteActive: downvoteBtn?.classList.contains('active') || false
        };
        
        console.log('📊 Original state:', originalState);
        
        // Calculate optimistic count changes
        let currentUpvotes = parseInt(upvoteCountEl?.textContent || '0');
        let currentDownvotes = parseInt(downvoteCountEl?.textContent || '0');
        let newUpvotes = currentUpvotes;
        let newDownvotes = currentDownvotes;
        
        console.log('🔢 Current counts - Up:', currentUpvotes, 'Down:', currentDownvotes);
        
        const wasUpvoted = upvoteBtn?.classList.contains('active');
        const wasDownvoted = downvoteBtn?.classList.contains('active');
        
        console.log('🔘 Button states - wasUpvoted:', wasUpvoted, 'wasDownvoted:', wasDownvoted);
        
        if (voteType === 1) {
            // Upvote clicked
            if (wasUpvoted) {
                console.log('➖ Removing upvote');
                newUpvotes = Math.max(0, newUpvotes - 1);
                upvoteBtn.classList.remove('active');
            } else if (wasDownvoted) {
                console.log('🔄 Changing downvote to upvote');
                newUpvotes++;
                newDownvotes = Math.max(0, newDownvotes - 1);
                downvoteBtn.classList.remove('active');
                upvoteBtn.classList.add('active');
            } else {
                console.log('➕ Adding new upvote');
                newUpvotes++;
                upvoteBtn.classList.add('active');
            }
        } else if (voteType === -1) {
            // Downvote clicked
            if (wasDownvoted) {
                console.log('➖ Removing downvote');
                newDownvotes = Math.max(0, newDownvotes - 1);
                downvoteBtn.classList.remove('active');
            } else if (wasUpvoted) {
                console.log('🔄 Changing upvote to downvote');
                newUpvotes = Math.max(0, newUpvotes - 1);
                newDownvotes++;
                upvoteBtn.classList.remove('active');
                downvoteBtn.classList.add('active');
            } else {
                console.log('➕ Adding new downvote');
                newDownvotes++;
                downvoteBtn.classList.add('active');
            }
        }
        
        console.log('🎯 Calculated new counts - Up:', newUpvotes, 'Down:', newDownvotes);
        
        // Update UI IMMEDIATELY (optimistic)
        if (upvoteCountEl) {
            upvoteCountEl.textContent = newUpvotes.toString();
            console.log('🔄 Optimistic upvote count set to:', newUpvotes);
        }
        
        if (downvoteCountEl) {
            downvoteCountEl.textContent = newDownvotes.toString();
            console.log('🔄 Optimistic downvote count set to:', newDownvotes);
        }
        
        console.log('✅ UI updated optimistically');
        
        // ============================================
        // SEND TO SERVER (BACKGROUND)
        // ============================================
        try { 
            await this.postConnection.invoke("VotePost", postId, voteType);
            console.log('✅ Post vote synced to server successfully');
        }
        catch (err) { 
            console.error("❌ Failed to sync vote to server:", err);
            
            // REVERT UI on error
            if (upvoteCountEl) {
                upvoteCountEl.textContent = originalState.upvoteCount;
            }
            if (downvoteCountEl) {
                downvoteCountEl.textContent = originalState.downvoteCount;
            }
            if (upvoteBtn) {
                originalState.upvoteActive ? upvoteBtn.classList.add('active') : upvoteBtn.classList.remove('active');
            }
            if (downvoteBtn) {
                originalState.downvoteActive ? downvoteBtn.classList.add('active') : downvoteBtn.classList.remove('active');
            }
            
            this.showNotification('Failed to vote. Please try again.', 'error');
            console.log('🔄 UI reverted to original state');
        }
    }

    async castPollVote(postId, optionId) {
        console.log('📊 Voting on poll. Post:', postId, 'Option:', optionId);
        
        // CRITICAL FIX: Check authentication before voting
        if (!this.isAuthenticated) {
            console.log('❌ User not authenticated, showing login prompt');
            this.showLoginPrompt();
            return;
        }
        
        // Debounce poll votes
        const debounceKey = `poll-${postId}`;
        if (!this.debounce(debounceKey, null, 500)) {
            console.log('⏱️ Poll vote ignored (debounced)');
            this.showNotification('Please wait before voting again', 'warning');
            return;
        }
        
        // Find poll option element for visual feedback
        const pollOption = document.querySelector(`.poll-option[data-option-id="${optionId}"]`);
        const pollContainer = document.querySelector(`.poll-container[data-post-id="${postId}"]`);
        
        // Add loading state
        if (pollOption) {
            pollOption.style.opacity = '0.6';
            pollOption.style.pointerEvents = 'none';
        }
        if (pollContainer) {
            pollContainer.style.opacity = '0.8';
            pollContainer.style.pointerEvents = 'none';
        }
        
        try { 
            await this.postConnection.invoke("CastPollVote", postId, optionId);
            console.log('✅ Poll vote sent to hub successfully');
            
            // Success feedback
            if (pollOption) {
                pollOption.style.transition = 'all 0.3s ease';
                pollOption.style.backgroundColor = '#d4edda';
                setTimeout(() => {
                    pollOption.style.backgroundColor = '';
                }, 1000);
            }
        }
        catch (err) { 
            console.error("❌ Failed to cast poll vote:", err);
            this.showNotification('Failed to vote on poll. Please try again.', 'error');
            
            // Remove loading state on error
            if (pollOption) {
                pollOption.style.opacity = '1';
                pollOption.style.pointerEvents = 'auto';
            }
            if (pollContainer) {
                pollContainer.style.opacity = '1';
                pollContainer.style.pointerEvents = 'auto';
            }
        }
        finally {
            // Always restore after poll update or 2 seconds
            setTimeout(() => {
                if (pollOption) {
                    pollOption.style.opacity = '1';
                    pollOption.style.pointerEvents = 'auto';
                }
                if (pollContainer) {
                    pollContainer.style.opacity = '1';
                    pollContainer.style.pointerEvents = 'auto';
                }
            }, 2000);
        }
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
        console.log('🔵 showReplyForm called for comment:', commentId);
        
        const targetForm = document.getElementById(`replyForm${commentId}`);
        if (!targetForm) {
            console.error('❌ Reply form not found for comment:', commentId);
            return;
        }

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
        
        console.log('✅ Reply form toggled. Now visible:', isHidden);
        
        if (isHidden) { // Form is being shown
            // Check if Quill is available
            if (typeof Quill === 'undefined') {
                console.error('❌ Quill is not loaded!');
                this.showNotification('Editor not loaded. Please refresh the page.', 'error');
                return;
            }
            
            // Initialize Quill editor for this reply form if not already initialized
            if (!window[`replyQuill${commentId}`]) {
                try {
                    const editorElement = document.getElementById(`replyEditor${commentId}`);
                    if (!editorElement) {
                        console.error('❌ Reply editor element not found:', `replyEditor${commentId}`);
                        return;
                    }
                    
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

                    console.log('✅ Quill editor initialized for comment:', commentId);

                    // Update hidden textarea when editor content changes
                    window[`replyQuill${commentId}`].on('text-change', function() {
                        const content = window[`replyQuill${commentId}`].root.innerHTML;
                        const contentElement = document.getElementById(`replyContent${commentId}`);
                        if (contentElement) {
                            contentElement.value = content;
                        }
                    });
                } catch (error) {
                    console.error('❌ Error initializing Quill:', error);
                    this.showNotification('Error initializing editor: ' + error.message, 'error');
                    return;
                }
            }
            
            // Focus the editor
            try {
                if (window[`replyQuill${commentId}`]) {
                    window[`replyQuill${commentId}`].focus();
                    console.log('✅ Editor focused');
                }
            } catch (error) {
                console.error('❌ Error focusing editor:', error);
            }
        }
    }

    deleteCommentUI(commentId) {
        const el = document.querySelector(`.comment-item[data-comment-id="${commentId}"]`);
        if (el) {
            el.style.opacity = '0';
            setTimeout(() => el.remove(), 300);
        }
    }
    
    updateCommentCount(change) {
        // Update the comment count in the header
        const commentCountEl = document.querySelector('.comment-count');
        if (commentCountEl) {
            const currentText = commentCountEl.textContent;
            const match = currentText.match(/(\d+)/);
            if (match) {
                const currentCount = parseInt(match[0]);
                const newCount = Math.max(0, currentCount + change);
                commentCountEl.textContent = `${newCount} Comment${newCount !== 1 ? 's' : ''}`;
            }
        }
        
        // Update the comment button/display count in post actions
        const commentBtn = document.querySelector('.action-btn .fa-comment')?.parentElement || 
                          document.querySelector('.action-btn-display .fa-comment')?.parentElement ||
                          document.querySelector('#postCommentCount')?.parentElement;
        if (commentBtn) {
            const span = commentBtn.querySelector('span') || document.getElementById('postCommentCount');
            if (span) {
                const match = span.textContent.match(/(\d+)/);
                if (match) {
                    const currentCount = parseInt(match[0]);
                    const newCount = Math.max(0, currentCount + change);
                    span.textContent = `Comments (${newCount})`;
                }
            }
        }
    }

    updateCommentVotes(commentId, upvoteCount, downvoteCount, score, userVote) {
        console.log('🔄 Updating comment votes:', { commentId, upvoteCount, downvoteCount, score, userVote });
        
        const upvoteCountEl = document.getElementById(`commentUpvote${commentId}`);
        const downvoteCountEl = document.getElementById(`commentDownvote${commentId}`);
        const upvoteBtn = document.querySelector(`[data-comment-id="${commentId}"][data-vote-type="1"]`);
        const downvoteBtn = document.querySelector(`[data-comment-id="${commentId}"][data-vote-type="-1"]`);
        
        console.log('🔍 Comment elements found:', {
            upvoteCountEl: !!upvoteCountEl,
            downvoteCountEl: !!downvoteCountEl,
            upvoteBtn: !!upvoteBtn,
            downvoteBtn: !!downvoteBtn
        });
        
        // Ensure we have numbers
        const upCount = parseInt(upvoteCount) || 0;
        const downCount = parseInt(downvoteCount) || 0;
        
        console.log('📊 Comment calculated values:', { upCount, downCount, userVote });
        
        // Update individual counts (Reddit style)
        if (upvoteCountEl) {
            upvoteCountEl.textContent = upCount.toString();
            console.log('✅ Comment upvote count updated to:', upCount);
            
            // Animate upvote count
            upvoteCountEl.style.transform = 'scale(1.2)';
            upvoteCountEl.style.transition = 'transform 0.2s ease';
            setTimeout(() => {
                upvoteCountEl.style.transform = 'scale(1)';
            }, 200);
        } else {
            console.warn('⚠️ Comment upvote count element not found:', `#commentUpvote${commentId}`);
        }
        
        if (downvoteCountEl) {
            downvoteCountEl.textContent = downCount.toString();
            console.log('✅ Comment downvote count updated to:', downCount);
            
            // Animate downvote count
            downvoteCountEl.style.transform = 'scale(1.2)';
            downvoteCountEl.style.transition = 'transform 0.2s ease';
            setTimeout(() => {
                downvoteCountEl.style.transform = 'scale(1)';
            }, 200);
        } else {
            console.warn('⚠️ Comment downvote count element not found:', `#commentDownvote${commentId}`);
        }
        
        // Update button states based on user's vote
        if (upvoteBtn && downvoteBtn) {
            // Remove active states first
            upvoteBtn.classList.remove('active');
            downvoteBtn.classList.remove('active');
            
            // Add active state based on current user's vote
            if (userVote === 1) {
                upvoteBtn.classList.add('active');
                console.log('✅ Comment upvote button activated');
            } else if (userVote === -1) {
                downvoteBtn.classList.add('active');
                console.log('✅ Comment downvote button activated');
            }
            
            console.log('🔄 Comment vote buttons updated');
        } else {
            console.warn('⚠️ Comment vote buttons not found');
        }
    }

    updatePostVotesUI(postId, upvoteCount, downvoteCount, currentUserVote) {
        console.log('🔄 UpdatePostVotesUI called:', { postId, upvoteCount, downvoteCount, currentUserVote });
        console.log('📊 Received data types:', {
            upvoteCount: typeof upvoteCount,
            downvoteCount: typeof downvoteCount,
            currentUserVote: typeof currentUserVote
        });
        
        // Get vote display elements
        const upvoteCountEl = document.getElementById(`upvoteCount-${postId}`);
        const downvoteCountEl = document.getElementById(`downvoteCount-${postId}`);
        const upvoteBtn = document.getElementById(`upvoteBtn-${postId}`);
        const downvoteBtn = document.getElementById(`downvoteBtn-${postId}`);
        
        console.log('🔍 Elements found:', {
            upvoteCountEl: !!upvoteCountEl,
            downvoteCountEl: !!downvoteCountEl,
            upvoteBtn: !!upvoteBtn,
            downvoteBtn: !!downvoteBtn
        });
        
        // Ensure we have numbers
        const upCount = parseInt(upvoteCount) || 0;
        const downCount = parseInt(downvoteCount) || 0;
        
        console.log('📊 Calculated values:', {
            upCount,
            downCount,
            currentUserVote
        });
        
        // Update individual counts (Reddit/Stack Overflow style)
        if (upvoteCountEl) {
            upvoteCountEl.textContent = upCount.toString();
            console.log('✅ Upvote count updated to:', upCount);
            
            // Animate upvote count
            upvoteCountEl.style.transform = 'scale(1.2)';
            upvoteCountEl.style.transition = 'transform 0.2s ease';
            setTimeout(() => {
                upvoteCountEl.style.transform = 'scale(1)';
            }, 200);
        } else {
            console.warn('⚠️ Upvote count element not found');
        }
        
        if (downvoteCountEl) {
            downvoteCountEl.textContent = downCount.toString();
            console.log('✅ Downvote count updated to:', downCount);
            
            // Animate downvote count
            downvoteCountEl.style.transform = 'scale(1.2)';
            downvoteCountEl.style.transition = 'transform 0.2s ease';
            setTimeout(() => {
                downvoteCountEl.style.transform = 'scale(1)';
            }, 200);
        } else {
            console.warn('⚠️ Downvote count element not found');
        }
        
        // Update button states
        if (upvoteBtn) {
            if (currentUserVote === 1) {
                upvoteBtn.classList.add('active');
            } else {
                upvoteBtn.classList.remove('active');
            }
            console.log('✅ Upvote button active:', currentUserVote === 1);
        } else {
            console.warn('⚠️ Upvote button not found for post:', postId);
        }
        
        if (downvoteBtn) {
            if (currentUserVote === -1) {
                downvoteBtn.classList.add('active');
            } else {
                downvoteBtn.classList.remove('active');
            }
            console.log('✅ Downvote button active:', currentUserVote === -1);
        } else {
            console.warn('⚠️ Downvote button not found for post:', postId);
        }
        
        console.log('✅ Post votes UI update complete');
    }

    updatePollResultsUI(pollData) {
        console.log('📊📊📊 updatePollResultsUI CALLED!');
        console.log('📊 Poll data received:', JSON.stringify(pollData, null, 2));
        
        if (!pollData) {
            console.error('❌ pollData is null or undefined');
            return;
        }
        
        // Handle both PascalCase (C#) and camelCase (JavaScript) property names
        const data = {
            postId: pollData.postId || pollData.PostId,
            totalVotes: pollData.totalVotes || pollData.TotalVotes || 0,
            hasUserVoted: pollData.hasUserVoted || pollData.HasUserVoted || false,
            options: pollData.options || pollData.Options || []
        };
        
        console.log('📊 Normalized data:', data);
        
        if (!data.options || data.options.length === 0) {
            console.error('❌ No poll options found');
            console.log('Available keys in pollData:', Object.keys(pollData));
            return;
        }
        
        console.log(`🔍 Looking for poll container with data-post-id="${data.postId}"`);
        const pollContainer = document.querySelector(`.poll-container[data-post-id="${data.postId}"]`);
        if (!pollContainer) {
            console.error('❌ Poll container not found for postId:', data.postId);
            console.log('🔍 Available poll containers:', document.querySelectorAll('.poll-container'));
            return;
        }
        
        console.log('✅ Found poll container');
        
        // Update total votes
        const totalVotesElement = pollContainer.querySelector('.poll-total-votes');
        if (totalVotesElement) {
            const oldText = totalVotesElement.textContent;
            totalVotesElement.textContent = `${data.totalVotes.toLocaleString()} total votes`;
            console.log(`📊 Updated total votes: ${oldText} → ${totalVotesElement.textContent}`);
        } else {
            console.warn('⚠️ Total votes element not found');
        }
        
        // Update "You have voted" status
        const hasVotedMessage = pollContainer.querySelector('.voted-message');
        const clickToVoteMessage = pollContainer.querySelector('p.text-muted');
        if (data.hasUserVoted) {
            if (clickToVoteMessage) {
                clickToVoteMessage.style.display = 'none';
                console.log('✅ Hid "click to vote" message');
            }
            if (!hasVotedMessage) {
                const message = document.createElement('p');
                message.className = 'text-success text-center small mb-3 voted-message';
                message.innerHTML = '<i class="fas fa-check-circle"></i> You have voted in this poll';
                pollContainer.querySelector('.poll-header')?.after(message);
                console.log('✅ Added "you have voted" message');
            }
        }
        
        // Update each poll option
        console.log(`📊 Updating ${data.options.length} poll options...`);
        data.options.forEach((optionData, index) => {
            // Handle both PascalCase and camelCase
            const option = {
                pollOptionId: optionData.pollOptionId || optionData.PollOptionId,
                voteCount: optionData.voteCount || optionData.VoteCount || 0,
                votePercentage: optionData.votePercentage || optionData.VotePercentage || 0,
                hasUserVoted: optionData.hasUserVoted || optionData.HasUserVoted || false
            };
            
            console.log(`   Processing option ${index + 1}:`, option);
            
            const optionElement = pollContainer.querySelector(`.poll-option[data-option-id="${option.pollOptionId}"]`);
            if (!optionElement) {
                console.warn(`⚠️ Option element not found for optionId: ${option.pollOptionId}`);
                return;
            }
            
            // Update vote count
            const voteCountEl = optionElement.querySelector('.poll-vote-count, .poll-option-count');
            if (voteCountEl) {
                voteCountEl.textContent = `${option.voteCount.toLocaleString()} votes`;
                console.log(`   ✅ Updated vote count to: ${option.voteCount}`);
            }
            
            // Update percentage
            const percentageEl = optionElement.querySelector('.poll-percentage-badge, .poll-option-percent');
            if (percentageEl) {
                percentageEl.textContent = `${option.votePercentage.toFixed(1)}%`;
                console.log(`   ✅ Updated percentage to: ${option.votePercentage.toFixed(1)}%`);
            }
            
            // Update progress bar
            const progressBar = optionElement.querySelector('.poll-progress-bar, .poll-option-bar');
            if (progressBar) {
                progressBar.style.width = `${option.votePercentage}%`;
                progressBar.setAttribute('data-width', option.votePercentage.toFixed(1));
                console.log(`   ✅ Updated progress bar to: ${option.votePercentage}%`);
            }
            
            // Update selection state
            if (option.hasUserVoted) {
                optionElement.classList.add('selected', 'voted');
                const radioCircle = optionElement.querySelector('.radio-circle');
                if (radioCircle && !radioCircle.classList.contains('selected')) {
                    radioCircle.classList.add('selected');
                    radioCircle.innerHTML = '<i class="fas fa-check"></i>';
                    console.log(`   ✅ Marked option as selected`);
                }
            } else {
                optionElement.classList.remove('selected');
                const radioCircle = optionElement.querySelector('.radio-circle');
                if (radioCircle) {
                    radioCircle.classList.remove('selected');
                    radioCircle.innerHTML = '';
                }
            }
            
            // Switch to results mode if user voted
            if (data.hasUserVoted) {
                optionElement.classList.add('results-mode');
                // Hide vote button, show static display
                const voteBtn = optionElement.querySelector('.poll-option-vote-btn');
                if (voteBtn) voteBtn.style.display = 'none';
            }
        });
        
        console.log('✅✅✅ Poll UI updated successfully!');
    }
    
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
    console.log('🚀 === POST DETAIL PAGE INITIALIZATION ===');
    
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
            console.log('📄 Page Post ID:', signalRManager.pagePostId);
            if (!isNaN(signalRManager.pagePostId)) {
                await signalRManager.joinPostGroup(signalRManager.pagePostId);
                console.log('✅ Joined post group:', signalRManager.pagePostId);
            }
        } else {
            console.warn('⚠️ pagePostId element not found');
        }

        // This is the only event listener we need to set up now.
        signalRManager.initializeDelegatedListener();
        console.log('✅ Delegated click listener initialized');
        
        // Count vote buttons for debugging
        const voteButtons = document.querySelectorAll('.vote-btn');
        const commentVoteButtons = document.querySelectorAll('.comment-vote-btn');
        const pollOptions = document.querySelectorAll('.poll-option');
        
        console.log('📊 Found vote buttons:', voteButtons.length);
        console.log('💬 Found comment vote buttons:', commentVoteButtons.length);
        console.log('📊 Found poll options:', pollOptions.length);
        
        // Log button attributes for first vote button
        if (voteButtons.length > 0) {
            const firstBtn = voteButtons[0];
            console.log('🔍 First vote button attributes:');
            console.log('  - data-post-id:', firstBtn.dataset.postId);
            console.log('  - data-vote-type:', firstBtn.dataset.voteType);
            console.log('  - class:', firstBtn.className);
        }
        
        console.log('✅ === INITIALIZATION COMPLETE ===');

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

