/**
 * ChatController - Main chat logic controller
 */
class ChatController {
    constructor() {
        this.chatService = new ChatService();
        this.chatUI = new ChatUI();
        this.currentChatUserId = null;
        this.currentChatUserName = null;
        this.currentChatRoomId = null;
        this.currentChatRoomName = null;
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
            this.loadInitialData();
            console.log('✅ Chat system ready');
        } else {
            console.error('❌ Failed to initialize chat');
        }
    }

    /**
     * Load initial chat data
     */
    async loadInitialData() {
        // Load direct chats
        await this.loadDirectChats();
        
        // Load user's chat rooms
        await this.loadChatRooms();
        
        // Load unread count
        await this.updateUnreadCount();
    }

    /**
     * Load direct chats list
     */
    async loadDirectChats() {
        const chats = await this.chatService.fetchDirectChats();
        this.chatUI.renderDirectChatsList(chats);
    }

    /**
     * Load chat rooms list
     */
    async loadChatRooms() {
        const rooms = await this.chatService.fetchChatRooms();
        this.chatUI.renderChatRoomsList(rooms);
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
        // Get current user ID from window or data attribute
        const currentUserId = window.currentUserId || document.body.getAttribute('data-user-id') || null;
        
        // Determine IsMine if not set by server (for room messages)
        if (message.IsMine === undefined && currentUserId && message.SenderId) {
            message.IsMine = message.SenderId === currentUserId;
        }
        
        // Check if message already exists (prevent duplicates from optimistic updates)
        const existingMessage = document.querySelector(`[data-message-id="${message.MessageId}"]`);
        if (existingMessage) {
            // Update existing message with server data
            console.log('📝 Updating existing message:', message.MessageId);
            return;
        }
        
        // Remove optimistic message if it exists (temporary messages have Date.now() as ID)
        const optimisticMessages = document.querySelectorAll('[data-message-id]');
        optimisticMessages.forEach(el => {
            const msgId = parseInt(el.getAttribute('data-message-id'));
            // If it's a temporary ID (very large number from Date.now()) and content matches, remove it
            if (msgId > 1000000000000 && el.textContent.includes(message.Content)) {
                el.remove();
            }
        });
        
        console.log('📨 Adding message to UI:', {
            MessageId: message.MessageId,
            SenderId: message.SenderId,
            Content: message.Content?.substring(0, 50),
            IsMine: message.IsMine
        });
        
        // Add message to UI
        this.chatUI.addMessage(message);
        
        // Increment message count
        this.messageCount++;
        
        // Check if we should show an ad (every 10 messages)
        if (this.messageCount % 10 === 0 && this.messageCount >= 10) {
            this.showChatAd();
        }

        // Mark as read if chat is open and message is for current user (direct messages only)
        const chatUserId = window.currentChatUserId || this.currentChatUserId;
        if (message.ReceiverId && message.ReceiverId === chatUserId && !message.IsMine && message.MessageId) {
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
        if (!content) {
            console.warn('⚠️ Cannot send: No content');
            return;
        }

        // Check window variables first (set by UI), then instance variables
        const roomId = window.currentChatRoomId || this.currentChatRoomId;
        const userId = window.currentChatUserId || this.currentChatUserId;

        // Determine if it's a direct message or room message
        if (roomId) {
            await this.sendRoomMessage(content);
        } else if (userId) {
            await this.sendDirectMessage(content);
        } else {
            console.warn('⚠️ Cannot send: No recipient or room selected');
        }
    }

    /**
     * Send direct message
     */
    async sendDirectMessage(content) {
        const userId = window.currentChatUserId || this.currentChatUserId;
        if (!userId) {
            console.error('❌ Cannot send: No recipient selected');
            return;
        }

        if (!content || !content.trim()) {
            console.warn('⚠️ Cannot send: Empty message');
            return;
        }

        // Store temporary message ID for removal later
        const tempMessageId = Date.now();
        
        // Optimistic UI update
        const optimisticMessage = {
            MessageId: tempMessageId,
            SenderId: 'current-user',
            SenderName: 'You',
            Content: content.trim(),
            SentAt: new Date(),
            IsMine: true,
            TimeAgo: 'just now',
            FormattedTime: new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
        };

        this.chatUI.addMessage(optimisticMessage);
        this.chatUI.scrollToBottom();

        try {
            // Send to server
            const success = await this.chatService.sendDirectMessage(userId, content.trim());
            
            if (!success) {
                console.error('❌ Failed to send message');
                // Remove optimistic message on failure
                const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
                if (tempMsg) tempMsg.remove();
                this.chatUI.showError('Failed to send message. Please try again.');
            }
        } catch (error) {
            console.error('❌ Error sending message:', error);
            // Remove optimistic message on error
            const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
            if (tempMsg) tempMsg.remove();
            this.chatUI.showError('Failed to send message. Please try again.');
        }
    }

    /**
     * Send room message
     */
    async sendRoomMessage(content) {
        const roomId = window.currentChatRoomId || this.currentChatRoomId;
        if (!roomId) {
            console.error('❌ Cannot send: No room selected');
            return;
        }

        if (!content || !content.trim()) {
            console.warn('⚠️ Cannot send: Empty message');
            return;
        }

        // Store temporary message ID for removal later
        const tempMessageId = Date.now();
        
        // Optimistic UI update
        const optimisticMessage = {
            MessageId: tempMessageId,
            SenderId: 'current-user',
            SenderName: 'You',
            Content: content.trim(),
            SentAt: new Date(),
            IsMine: true,
            TimeAgo: 'just now',
            FormattedTime: new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
        };

        this.chatUI.addMessage(optimisticMessage);
        this.chatUI.scrollToBottom();

        try {
            // Send to server
            const success = await this.chatService.sendRoomMessage(roomId, content.trim());
            
            if (!success) {
                console.error('❌ Failed to send room message');
                // Remove optimistic message on failure
                const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
                if (tempMsg) tempMsg.remove();
                this.chatUI.showError('Failed to send message. Please try again.');
            }
        } catch (error) {
            console.error('❌ Error sending room message:', error);
            // Remove optimistic message on error
            const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
            if (tempMsg) tempMsg.remove();
            this.chatUI.showError('Failed to send message. Please try again.');
        }
    }

    /**
     * Load chat history
     */
    async loadChatHistory(userId) {
        this.currentChatUserId = userId;
        this.currentChatRoomId = null;
        window.currentChatUserId = userId;
        window.currentChatRoomId = null;
        
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
     * Load room chat history
     */
    async loadRoomHistory(roomId) {
        this.currentChatRoomId = roomId;
        this.currentChatUserId = null;
        window.currentChatRoomId = roomId;
        window.currentChatUserId = null;
        
        console.log('📜 Loading room history for:', roomId);
        
        // Join SignalR group
        await this.chatService.joinRoom(roomId);
        
        // Show loading indicator
        this.chatUI.showLoading();
        
        // Get history from server
        const history = await this.chatService.getRoomHistory(roomId);
        
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
     * Create chat room
     */
    async createChatRoom(name, description, isPublic) {
        try {
            const result = await this.chatService.createChatRoom(name, description, isPublic);
            
            if (result.success) {
                // Reload rooms list
                await this.loadChatRooms();
                
                // Open the newly created room
                if (result.room) {
                    window.openChatRoomWindow(result.room.chatRoomId, result.room.name, result.room.iconUrl);
                }
                
                return { success: true, room: result.room };
            } else {
                return { success: false, message: result.message || 'Failed to create room' };
            }
        } catch (error) {
            console.error('❌ Error creating chat room:', error);
            return { success: false, message: 'Failed to create room' };
        }
    }

    /**
     * Join chat room
     */
    async joinChatRoom(roomId) {
        try {
            const result = await this.chatService.joinChatRoom(roomId);
            
            if (result.success) {
                // Reload rooms list
                await this.loadChatRooms();
                
                // Open the room
                const rooms = await this.chatService.fetchChatRooms();
                const room = rooms.find(r => r.chatRoomId === roomId);
                if (room) {
                    window.openChatRoomWindow(room.chatRoomId, room.name, room.iconUrl);
                }
                
                return { success: true };
            } else {
                return { success: false, message: result.message || 'Failed to join room' };
            }
        } catch (error) {
            console.error('❌ Error joining chat room:', error);
            return { success: false, message: 'Failed to join room' };
        }
    }

    /**
     * Handle typing in input
     */
    handleTyping() {
        const userId = window.currentChatUserId || this.currentChatUserId;
        if (!userId) return;

        // Notify server
        this.chatService.notifyTyping(userId);

        // Clear existing timeout
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

        // Stop typing after 3 seconds of inactivity
        this.typingTimeout = setTimeout(() => {
            this.chatService.notifyStoppedTyping(userId);
        }, 3000);
    }

    /**
     * Update unread count
     */
    async updateUnreadCount() {
        try {
            const count = await this.chatService.getUnreadCount();
            const badge = document.getElementById('chatUnreadBadge');
            if (badge) {
                if (count > 0) {
                    badge.textContent = count > 99 ? '99+' : count.toString();
                    badge.style.display = 'block';
                } else {
                    badge.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('❌ Error updating unread count:', error);
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

    /**
     * Load online users
     */
    async loadOnlineUsers() {
        console.log('👥 Loading online users...');
        const users = await this.chatService.fetchOnlineUsers();
        this.chatUI.renderOnlineUsersList(users);
    }
}

// Export for use in chat widget
window.ChatController = ChatController;

