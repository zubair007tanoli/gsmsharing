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
        this.initialized = false;
        
        // Don't auto-initialize in constructor - let the widget do it
        // this.initialize();
    }

    /**
     * Initialize chat system
     */
    async initialize() {
        // Prevent double initialization
        if (this.initialized) {
            console.log('⚠️ Chat controller already initialized, skipping...');
            return true;
        }
        
        console.log('🚀 Initializing chat controller...');
        
        // Connect to SignalR
        const connected = await this.chatService.initialize();
        
        if (connected) {
            this.setupEventHandlers();
            this.loadInitialData();
            this.initialized = true;
            console.log('✅ Chat system ready');
            return true;
        } else {
            console.error('❌ Failed to initialize chat');
            this.chatUI.showError('Failed to connect to chat server. Please refresh the page.');
            return false;
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

        // Handle room join/leave events
        this.chatService.onUserJoinedRoom((userId, roomId) => {
            if (roomId === this.currentChatRoomId || roomId === window.currentChatRoomId) {
                this.chatUI.showRoomMemberNotification(userId, roomId, 'joined');
            }
        });

        this.chatService.onUserLeftRoom((userId, roomId) => {
            if (roomId === this.currentChatRoomId || roomId === window.currentChatRoomId) {
                this.chatUI.showRoomMemberNotification(userId, roomId, 'left');
            }
        });

        // Handle connection state changes
        this.chatService.onConnectionStateChanged((state) => {
            this.chatUI.updateConnectionStatus(state);
        });
    }

    /**
     * Handle incoming message
     */
    handleIncomingMessage(message) {
        // Get current user ID from window or data attribute
        const currentUserId = window.currentUserId || document.body.getAttribute('data-user-id') || null;
        
        // Normalize property names (handle both camelCase and PascalCase)
        const senderId = message.senderId || message.SenderId;
        const receiverId = message.receiverId || message.ReceiverId;
        const chatRoomIdFromMsg = message.chatRoomId || message.ChatRoomId;
        const messageId = message.messageId || message.MessageId;
        const content = message.content || message.Content;
        
        // Determine isMine if not set by server (for room messages)
        // Set both camelCase and PascalCase for compatibility
        if (message.isMine === undefined && message.IsMine === undefined && currentUserId && senderId) {
            const isMine = senderId === currentUserId;
            message.isMine = isMine;
            message.IsMine = isMine;
        }
        
        // Check if this message is for the current chat (direct or room)
        const chatUserId = window.currentChatUserId || this.currentChatUserId;
        const chatRoomId = window.currentChatRoomId || this.currentChatRoomId;
        const isMine = message.isMine || message.IsMine || false;
        
        // Removed verbose logging for performance - only log in debug mode
        if (window.DEBUG_MODE) {
            console.log('📨 Processing incoming message:', { messageId, senderId, isMine });
        }
        
        // For direct messages (has ReceiverId)
        if (receiverId) {
            // Message is for this chat if:
            // 1. We're viewing this conversation (chatUserId matches sender or receiver)
            // 2. OR it's our own message (always show our own messages)
            const isViewingThisChat = chatUserId && (
                senderId === chatUserId || 
                receiverId === chatUserId
            );
            
            const shouldDisplay = isMine || isViewingThisChat;
            
            if (!shouldDisplay) {
                this.updateUnreadCount();
                return;
            }
        }
        // For room messages (has ChatRoomId, no ReceiverId)
        else if (chatRoomIdFromMsg) {
            // Only display if we're viewing this room
            if (chatRoomId && chatRoomIdFromMsg !== chatRoomId) {
                this.updateUnreadCount();
                return;
            }
            // If we're viewing this room, display it
        }
        
        // Check if message already exists (prevent duplicates)
        if (messageId) {
            const existingMessage = document.querySelector(`[data-message-id="${messageId}"]`);
            if (existingMessage) {
                return; // Skip duplicate
            }
        }
        
        // Remove optimistic message if it exists (temporary messages have Date.now() as ID)
        if (content) {
            const optimisticMessages = document.querySelectorAll('[data-message-id]');
            optimisticMessages.forEach(el => {
                const msgId = parseInt(el.getAttribute('data-message-id'));
                // If it's a temporary ID (very large number from Date.now()) and content matches, remove it
                if (msgId > 1000000000000 && el.textContent.includes(content)) {
                    el.remove();
                }
            });
        }
        
        // Ensure ChatUI container is found (might be in widget)
        this.chatUI.messagesContainer = document.getElementById('chatMessages');
        
        // Verify container exists before adding
        if (!this.chatUI.messagesContainer) {
            this.chatUI.messagesContainer = document.getElementById('chatMessages');
            if (!this.chatUI.messagesContainer) {
                // Retry once after a short delay
                setTimeout(() => {
                    this.chatUI.messagesContainer = document.getElementById('chatMessages');
                    if (this.chatUI.messagesContainer) {
                        this.chatUI.addMessage(message);
                    }
                }, 50);
                return;
            }
        }
        
        // Add message to UI
        this.chatUI.addMessage(message);
        
        // Increment message count
        this.messageCount++;
        
        // Check if we should show an ad (every 10 messages)
        if (this.messageCount % 10 === 0 && this.messageCount >= 10) {
            this.showChatAd();
        }

        // Mark as read if chat is open and message is for current user (direct messages only)
        if (receiverId && chatUserId && receiverId === chatUserId && !isMine && messageId) {
            this.chatService.markAsRead(messageId);
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
            console.log('📤 Sending room message to room:', roomId);
            await this.sendRoomMessage(roomId, content);
        } else if (userId) {
            console.log('📤 Sending direct message to user:', userId);
            await this.sendDirectMessage(userId, content);
        } else {
            console.warn('⚠️ Cannot send: No recipient or room selected');
            throw new Error('No recipient or room selected');
        }
    }

    /**
     * Send direct message
     */
    async sendDirectMessage(userId, content) {
        // Use provided userId or fallback to window/instance variables
        const targetUserId = userId || window.currentChatUserId || this.currentChatUserId;
        if (!targetUserId) {
            console.error('❌ Cannot send: No recipient selected');
            throw new Error('No recipient selected');
        }

        if (!content || !content.trim()) {
            console.warn('⚠️ Cannot send: Empty message');
            return;
        }

        // Store temporary message ID for removal later
        const tempMessageId = Date.now();
        
        // Ensure ChatUI is initialized
        this.chatUI.initialize();
        
        // Optimistic UI update (show message immediately - CRITICAL for perceived performance)
        const optimisticMessage = {
            MessageId: tempMessageId,
            SenderId: window.currentUserId || 'current-user',
            SenderName: 'You',
            Content: content.trim(),
            SentAt: new Date(),
            IsMine: true,
            TimeAgo: 'just now',
            FormattedTime: new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
        };

        // Show message IMMEDIATELY (don't wait for server) - CRITICAL for perceived performance
        if (this.chatUI.messagesContainer) {
            // Add message and force immediate render (bypass batching for single messages)
            const messageEl = this.chatUI.createMessageElement(optimisticMessage);
            if (messageEl && this.chatUI.messagesContainer) {
                this.chatUI.messagesContainer.appendChild(messageEl);
                // Use requestAnimationFrame for smooth rendering
                requestAnimationFrame(() => {
                    this.chatUI.scrollToBottom();
                });
            } else {
                // Fallback to normal addMessage if direct append fails
                this.chatUI.addMessage(optimisticMessage);
                this.chatUI.flushPendingMessages();
                this.chatUI.scrollToBottom();
            }
        }

        // Send to server asynchronously (non-blocking)
        // The real message from server will replace the optimistic one
        this.chatService.sendDirectMessage(targetUserId, content.trim())
            .then(result => {
                if (!result || !result.success) {
                    // Remove optimistic message on failure
                    const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
                    if (tempMsg) tempMsg.remove();
                    this.chatUI.showError(result?.error || 'Failed to send message. Please try again.');
                }
            })
            .catch(error => {
                // Remove optimistic message on error
                const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
                if (tempMsg) tempMsg.remove();
                this.chatUI.showError(error.message || 'Failed to send message. Please try again.');
            });
    }

    /**
     * Send room message
     */
    async sendRoomMessage(roomId, content) {
        // Use provided roomId or fallback to window/instance variables
        const targetRoomId = roomId || window.currentChatRoomId || this.currentChatRoomId;
        if (!targetRoomId) {
            console.error('❌ Cannot send: No room selected');
            throw new Error('No room selected');
        }

        if (!content || !content.trim()) {
            console.warn('⚠️ Cannot send: Empty message');
            return;
        }

        console.log('📤 Sending room message:', {
            roomId: targetRoomId,
            content: content.substring(0, 30)
        });

        // Store temporary message ID for removal later
        const tempMessageId = Date.now();
        
        // Ensure ChatUI is initialized
        this.chatUI.initialize();
        
        // Optimistic UI update
        const optimisticMessage = {
            MessageId: tempMessageId,
            SenderId: window.currentUserId || 'current-user',
            SenderName: 'You',
            Content: content.trim(),
            SentAt: new Date(),
            IsMine: true,
            TimeAgo: 'just now',
            FormattedTime: new Date().toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
        };

        if (this.chatUI.messagesContainer) {
            this.chatUI.addMessage(optimisticMessage);
            this.chatUI.scrollToBottom();
        } else {
            console.warn('⚠️ Messages container not found, message will appear when received from server');
        }

        try {
            // Send to server
            const result = await this.chatService.sendRoomMessage(targetRoomId, content.trim());
            
            console.log('📤 Send result:', result);
            
            if (!result || !result.success) {
                console.error('❌ Failed to send room message:', result?.error);
                // Remove optimistic message on failure
                const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
                if (tempMsg) tempMsg.remove();
                this.chatUI.showError(result?.error || 'Failed to send message. Please try again.');
            } else {
                console.log('✅ Room message sent successfully');
            }
        } catch (error) {
            console.error('❌ Error sending room message:', error);
            // Remove optimistic message on error
            const tempMsg = document.querySelector(`[data-message-id="${tempMessageId}"]`);
            if (tempMsg) tempMsg.remove();
            this.chatUI.showError(error.message || 'Failed to send message. Please try again.');
            throw error;
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
        
        // Ensure ChatUI is initialized with correct container
        this.chatUI.initialize();
        
        // Verify container is found
        if (!this.chatUI.messagesContainer) {
            // Try again after a delay (widget might be opening)
            setTimeout(() => {
                this.chatUI.initialize();
                if (this.chatUI.messagesContainer) {
                    this.loadChatHistory(userId);
                }
            }, 100);
            return;
        }
        
        // Show loading indicator
        this.chatUI.showLoading();
        
        // Get history from server
        const history = await this.chatService.getChatHistory(userId);
        
        // Clear loading and display messages
        this.chatUI.clearMessages();
        
        if (history && history.length > 0) {
            const currentUserId = window.currentUserId;
            
            // Process all messages first (prepare data)
            const processedMessages = history.map(message => {
                // Ensure isMine is set correctly (handle both camelCase and PascalCase)
                const senderId = message.senderId || message.SenderId;
                if (message.isMine === undefined && message.IsMine === undefined && currentUserId && senderId) {
                    const isMine = senderId === currentUserId;
                    message.isMine = isMine;
                    message.IsMine = isMine;
                }
                return message;
            });
            
            // Batch add all messages at once for better performance
            processedMessages.forEach(message => {
                this.chatUI.addMessage(message);
            });
            
            // Force flush any pending messages
            if (this.chatUI.flushPendingMessages) {
                this.chatUI.flushPendingMessages();
            }
            
            this.messageCount = history.length;
        } else {
            // Show empty state
            const emptyState = document.getElementById('chatEmptyMessages');
            if (emptyState) {
                emptyState.style.display = 'block';
            }
        }
        
        this.chatUI.scrollToBottom();
    }

    /**
     * Load room chat history
     */
    async loadRoomHistory(roomId) {
        // Leave previous room if switching
        if (this.currentChatRoomId && this.currentChatRoomId !== roomId) {
            await this.chatService.leaveRoom(this.currentChatRoomId);
        }

        this.currentChatRoomId = roomId;
        this.currentChatUserId = null;
        window.currentChatRoomId = roomId;
        window.currentChatUserId = null;
        
        console.log('📜 Loading room history for:', roomId);
        
        // Ensure ChatUI is initialized with correct container
        this.chatUI.initialize();
        
        // Verify container is found
        if (!this.chatUI.messagesContainer) {
            console.error('❌ ChatController: chatMessages container not found, retrying...');
            setTimeout(() => {
                this.chatUI.initialize();
                if (this.chatUI.messagesContainer) {
                    this.loadRoomHistory(roomId);
                } else {
                    console.error('❌ ChatController: Still cannot find container after retry');
                }
            }, 200);
            return;
        }
        
        console.log('✅ ChatController: Container found, loading room history...');
        
        // Join SignalR group
        const joined = await this.chatService.joinRoom(roomId);
        if (!joined) {
            console.error('❌ Failed to join room SignalR group');
            this.chatUI.showError('Failed to join room. Please try again.');
            return;
        }
        
        // Show loading indicator
        this.chatUI.showLoading();
        
        // Get history from server
        const history = await this.chatService.getRoomHistory(roomId);
        
        console.log(`📨 Received ${history?.length || 0} room messages from server`);
        
        // Clear loading and display messages
        this.chatUI.clearMessages();
        
        if (history && history.length > 0) {
            console.log(`📨 Loading ${history.length} room messages into UI`);
            const currentUserId = window.currentUserId;
            history.forEach(message => {
                // Ensure isMine is set correctly (handle both camelCase and PascalCase)
                const senderId = message.senderId || message.SenderId;
                if (message.isMine === undefined && message.IsMine === undefined && currentUserId && senderId) {
                    const isMine = senderId === currentUserId;
                    message.isMine = isMine;
                    message.IsMine = isMine;
                }
                const messageId = message.messageId || message.MessageId;
                const content = message.content || message.Content;
                const isMine = message.isMine || message.IsMine || false;
                
                console.log('📝 Adding room message:', {
                    messageId: messageId,
                    senderId: senderId,
                    isMine: isMine,
                    content: content?.substring(0, 30)
                });
                this.chatUI.addMessage(message);
            });
            this.messageCount = history.length;
            console.log(`✅ Loaded ${this.messageCount} room messages`);
        } else {
            console.log('📭 No room messages found in history');
            // Show empty state
            const emptyState = document.getElementById('chatEmptyMessages');
            if (emptyState) {
                emptyState.style.display = 'block';
            }
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
     * Show Google AdSense ad (non-intrusive)
     */
    showChatAd() {
        // Only show ads if AdSense is loaded
        if (typeof adsbygoogle === 'undefined') {
            console.warn('⚠️ Google AdSense not loaded');
            return;
        }

        const adId = 'chat-ad-' + Date.now();
        const adHtml = `
            <div class="chat-ad" id="${adId}">
                <div class="chat-ad-label">Advertisement</div>
                <ins class="adsbygoogle"
                     style="display:block"
                     data-ad-client="ca-pub-YOUR_PUBLISHER_ID"
                     data-ad-slot="YOUR_AD_SLOT_ID"
                     data-ad-format="auto"
                     data-full-width-responsive="true"></ins>
            </div>
        `;
        
        this.chatUI.addAdToMessages(adHtml);
        
        // Initialize AdSense after adding to DOM
        setTimeout(() => {
            try {
                (adsbygoogle = window.adsbygoogle || []).push({});
            } catch (e) {
                console.error('❌ Error loading AdSense ad:', e);
            }
        }, 100);
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

