/**
 * ChatController - Main chat logic controller
 */
class ChatController {
    constructor() {
        this.chatService = new ChatService();
        this.chatUI = new ChatUI();
        this.currentChatUserId = null;
        this.currentChatUserName = null;
        this.messageCount = 0;
        this.typingTimeout = null;
        
        this.initialize();
    }

    /**
     * Initialize chat system
     */
    async initialize() {
        console.log('🚀 Initializing chat controller...');
        
        // Connect to SignalR
        const connected = await this.chatService.initialize();
        
        if (connected) {
            this.setupEventHandlers();
            console.log('✅ Chat system ready');
        } else {
            console.error('❌ Failed to initialize chat');
        }
    }

    /**
     * Setup event handlers
     */
    setupEventHandlers() {
        // Handle incoming messages
        this.chatService.onMessageReceived((message) => {
            this.handleIncomingMessage(message);
        });

        // Handle typing indicator
        this.chatService.onTyping((userId, isTyping) => {
            if (userId === this.currentChatUserId) {
                this.chatUI.showTypingIndicator(isTyping);
            }
        });

        // Handle user online status
        this.chatService.onUserOnline((userId) => {
            this.chatUI.updateUserStatus(userId, 'online');
        });

        this.chatService.onUserOffline((userId) => {
            this.chatUI.updateUserStatus(userId, 'offline');
        });

        // Handle errors
        this.chatService.onError((error) => {
            this.chatUI.showError(error);
        });
    }

    /**
     * Handle incoming message
     */
    handleIncomingMessage(message) {
        // Add message to UI
        this.chatUI.addMessage(message);
        
        // Increment message count
        this.messageCount++;
        
        // Check if we should show an ad (every 10 messages)
        if (this.messageCount % 10 === 0 && this.messageCount >= 10) {
            this.showChatAd();
        }

        // Mark as read if chat is open
        if (message.SenderId === this.currentChatUserId && !message.IsMine) {
            this.chatService.markAsRead(message.MessageId);
        }

        // Update unread count
        this.updateUnreadCount();
        
        // Scroll to bottom
        this.chatUI.scrollToBottom();
    }

    /**
     * Send message
     */
    async sendMessage(content) {
        if (!content || !this.currentChatUserId) {
            console.warn('⚠️ Cannot send: No content or recipient');
            return;
        }

        // Optimistic UI update (show message immediately)
        const optimisticMessage = {
            MessageId: Date.now(), // Temporary ID
            SenderId: 'current-user',
            SenderName: 'You',
            Content: content,
            SentAt: new Date(),
            IsMine: true,
            TimeAgo: 'just now',
            FormattedTime: new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
        };

        this.chatUI.addMessage(optimisticMessage);
        this.chatUI.scrollToBottom();

        // Send to server
        const success = await this.chatService.sendDirectMessage(this.currentChatUserId, content);
        
        if (!success) {
            console.error('❌ Failed to send message');
            this.chatUI.showError('Failed to send message. Please try again.');
        }
    }

    /**
     * Load chat history
     */
    async loadChatHistory(userId) {
        this.currentChatUserId = userId;
        
        console.log('📜 Loading chat history for:', userId);
        
        // Show loading indicator
        this.chatUI.showLoading();
        
        // Get history from server
        const history = await this.chatService.getChatHistory(userId);
        
        // Clear loading and display messages
        this.chatUI.clearMessages();
        
        if (history && history.length > 0) {
            history.forEach(message => {
                this.chatUI.addMessage(message);
            });
            this.messageCount = history.length;
        }
        
        this.chatUI.scrollToBottom();
    }

    /**
     * Handle typing in input
     */
    handleTyping() {
        if (!this.currentChatUserId) return;

        // Notify server
        this.chatService.notifyTyping(this.currentChatUserId);

        // Clear existing timeout
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

        // Stop typing after 3 seconds of inactivity
        this.typingTimeout = setTimeout(() => {
            this.chatService.notifyStoppedTyping(this.currentChatUserId);
        }, 3000);
    }

    /**
     * Update unread count
     */
    updateUnreadCount() {
        // This would fetch from server in real implementation
        // For now, just update UI
        const badge = document.getElementById('chatUnreadBadge');
        if (badge) {
            // badge.textContent = count;
            // badge.style.display = count > 0 ? 'block' : 'none';
        }
    }

    /**
     * Show chat ad (non-intrusive)
     */
    showChatAd() {
        const adHtml = `
            <div class="chat-ad">
                <div class="chat-ad-label">Sponsored</div>
                <div class="chat-ad-content">
                    <div class="chat-ad-text">
                        <div class="chat-ad-title">Discover Premium Features</div>
                        <div class="chat-ad-description">Upgrade for ad-free experience and more!</div>
                    </div>
                </div>
            </div>
        `;
        
        this.chatUI.addAdToMessages(adHtml);
    }
}

// Export for use in chat widget
window.ChatController = ChatController;

