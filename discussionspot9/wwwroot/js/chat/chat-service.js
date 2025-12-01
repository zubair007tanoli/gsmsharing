/**
 * ChatService - Manages SignalR connection for chat
 */
class ChatService {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.callbacks = {
            onMessageReceived: [],
            onTyping: [],
            onUserOnline: [],
            onUserOffline: [],
            onError: [],
            onUserJoinedRoom: [],
            onUserLeftRoom: [],
            onConnectionStateChanged: []
        };
    }

    /**
     * Initialize SignalR connection with retry logic
     */
    async initialize(maxRetries = 5, retryDelay = 2000) {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: (retryContext) => {
                        // Exponential backoff: 0s, 2s, 4s, 8s, 16s
                        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                    }
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Register event handlers
            this.registerHandlers();

            // Start connection with retry logic
            let retryCount = 0;
            while (retryCount < maxRetries) {
                try {
                    await this.connection.start();
                    this.isConnected = true;
                    console.log('✅ ChatHub connected successfully');
                    return true;
                } catch (error) {
                    retryCount++;
                    if (retryCount >= maxRetries) {
                        console.error(`❌ ChatHub connection failed after ${maxRetries} attempts:`, error);
                        this.isConnected = false;
                        // Notify error callbacks
                        this.callbacks.onError.forEach(cb => cb(`Connection failed: ${error.message}`));
                        return false;
                    }
                    console.warn(`⚠️ ChatHub connection attempt ${retryCount}/${maxRetries} failed, retrying in ${retryDelay}ms...`);
                    await new Promise(resolve => setTimeout(resolve, retryDelay));
                    retryDelay *= 2; // Exponential backoff
                }
            }
            return false;
        } catch (error) {
            console.error('❌ ChatHub initialization failed:', error);
            this.isConnected = false;
            this.callbacks.onError.forEach(cb => cb(`Initialization failed: ${error.message}`));
            return false;
        }
    }

    /**
     * Register SignalR event handlers
     */
    registerHandlers() {
        // Receive direct message
        this.connection.on('ReceiveDirectMessage', (message) => {
            console.log('📩 Direct message received:', message);
            this.callbacks.onMessageReceived.forEach(cb => cb(message));
        });

        // Receive room message
        this.connection.on('ReceiveRoomMessage', (message) => {
            console.log('📩 Room message received:', message);
            this.callbacks.onMessageReceived.forEach(cb => cb(message));
        });

        // User typing indicator
        this.connection.on('UserTyping', (userId, isTyping) => {
            console.log('⌨️ User typing:', userId, isTyping);
            this.callbacks.onTyping.forEach(cb => cb(userId, isTyping));
        });

        // User came online
        this.connection.on('UserOnline', (userId) => {
            console.log('🟢 User online:', userId);
            this.callbacks.onUserOnline.forEach(cb => cb(userId));
        });

        // User went offline
        this.connection.on('UserOffline', (userId) => {
            console.log('⚫ User offline:', userId);
            this.callbacks.onUserOffline.forEach(cb => cb(userId));
        });

        // Error handling
        this.connection.on('Error', (error) => {
            console.error('❌ Chat error:', error);
            this.callbacks.onError.forEach(cb => cb(error));
        });

        // Reconnection events
        this.connection.onreconnecting((error) => {
            console.warn('🔄 ChatHub reconnecting...', error);
            this.isConnected = false;
            this.callbacks.onConnectionStateChanged.forEach(cb => cb('reconnecting'));
        });

        this.connection.onreconnected((connectionId) => {
            console.log('✅ ChatHub reconnected:', connectionId);
            this.isConnected = true;
            this.callbacks.onConnectionStateChanged.forEach(cb => cb('connected'));
        });

        // Room join/leave events
        this.connection.on('UserJoinedRoom', (userId, roomId) => {
            console.log('👤 User joined room:', userId, roomId);
            this.callbacks.onUserJoinedRoom.forEach(cb => cb(userId, roomId));
        });

        this.connection.on('UserLeftRoom', (userId, roomId) => {
            console.log('👤 User left room:', userId, roomId);
            this.callbacks.onUserLeftRoom.forEach(cb => cb(userId, roomId));
        });

        this.connection.onclose((error) => {
            console.error('❌ ChatHub connection closed:', error);
            this.isConnected = false;
            // Notify error callbacks with user-friendly message
            const errorMessage = error ? `Connection lost: ${error.message}` : 'Connection lost. Trying to reconnect...';
            this.callbacks.onError.forEach(cb => cb(errorMessage));
            this.callbacks.onConnectionStateChanged.forEach(cb => cb('disconnected'));
        });
    }

    /**
     * Send direct message
     */
    async sendDirectMessage(receiverId, content) {
        if (!this.isConnected) {
            console.error('❌ Cannot send message: Not connected');
            const error = { success: false, error: 'Not connected to chat server' };
            this.callbacks.onError.forEach(cb => cb(error.error));
            return error;
        }

        if (!receiverId || !content || !content.trim()) {
            const error = { success: false, error: 'Invalid message: receiver ID and content are required' };
            this.callbacks.onError.forEach(cb => cb(error.error));
            return error;
        }

        try {
            await this.connection.invoke('SendDirectMessage', receiverId, content.trim());
            console.log('📤 Direct message sent to:', receiverId);
            return { success: true };
        } catch (error) {
            console.error('❌ Error sending message:', error);
            const errorResult = { success: false, error: error.message || 'Failed to send message' };
            this.callbacks.onError.forEach(cb => cb(errorResult.error));
            return errorResult;
        }
    }

    /**
     * Send room message
     */
    async sendRoomMessage(roomId, content) {
        if (!this.isConnected) {
            console.error('❌ Cannot send message: Not connected');
            const error = { success: false, error: 'Not connected to chat server' };
            this.callbacks.onError.forEach(cb => cb(error.error));
            return error;
        }

        if (!roomId || !content || !content.trim()) {
            const error = { success: false, error: 'Invalid message: room ID and content are required' };
            this.callbacks.onError.forEach(cb => cb(error.error));
            return error;
        }

        try {
            await this.connection.invoke('SendRoomMessage', roomId, content.trim());
            console.log('📤 Room message sent to room:', roomId);
            return { success: true };
        } catch (error) {
            console.error('❌ Error sending room message:', error);
            const errorResult = { success: false, error: error.message || 'Failed to send message' };
            this.callbacks.onError.forEach(cb => cb(errorResult.error));
            return errorResult;
        }
    }

    /**
     * Notify typing
     */
    async notifyTyping(chatWithUserId) {
        if (!this.isConnected) return;

        try {
            await this.connection.invoke('UserTyping', chatWithUserId);
        } catch (error) {
            console.error('❌ Error notifying typing:', error);
        }
    }

    /**
     * Notify stopped typing
     */
    async notifyStoppedTyping(chatWithUserId) {
        if (!this.isConnected) return;

        try {
            await this.connection.invoke('UserStoppedTyping', chatWithUserId);
        } catch (error) {
            console.error('❌ Error notifying stopped typing:', error);
        }
    }

    /**
     * Mark message as read
     */
    async markAsRead(messageId) {
        if (!this.isConnected) return;

        try {
            await this.connection.invoke('MarkAsRead', messageId);
        } catch (error) {
            console.error('❌ Error marking as read:', error);
        }
    }

    /**
     * Get chat history
     */
    async getChatHistory(otherUserId, page = 1) {
        if (!this.isConnected) return [];

        try {
            const history = await this.connection.invoke('GetChatHistory', otherUserId, page, 50);
            console.log('📜 Chat history loaded:', history.length, 'messages');
            return history;
        } catch (error) {
            console.error('❌ Error getting chat history:', error);
            return [];
        }
    }

    /**
     * Get room chat history
     */
    async getRoomHistory(roomId, page = 1) {
        if (!this.isConnected) return [];

        try {
            const history = await this.connection.invoke('GetRoomHistory', roomId, page, 50);
            console.log('📜 Room history loaded:', history.length, 'messages');
            return history;
        } catch (error) {
            console.error('❌ Error getting room history:', error);
            return [];
        }
    }

    /**
     * Join a room (SignalR group)
     */
    async joinRoom(roomId) {
        if (!this.isConnected) return false;

        try {
            await this.connection.invoke('JoinRoom', roomId);
            console.log('✅ Joined room:', roomId);
            return true;
        } catch (error) {
            console.error('❌ Error joining room:', error);
            return false;
        }
    }

    /**
     * Leave a room (SignalR group)
     */
    async leaveRoom(roomId) {
        if (!this.isConnected) return false;

        try {
            await this.connection.invoke('LeaveRoom', roomId);
            console.log('✅ Left room:', roomId);
            return true;
        } catch (error) {
            console.error('❌ Error leaving room:', error);
            return false;
        }
    }

    /**
     * API Calls - HTTP endpoints
     */

    /**
     * Get user's direct chats (HTTP)
     */
    async fetchDirectChats() {
        try {
            const response = await fetch('/api/chat/direct', {
                credentials: 'include'
            });
            const data = await response.json();
            return data.success ? data.chats : [];
        } catch (error) {
            console.error('❌ Error fetching direct chats:', error);
            return [];
        }
    }

    /**
     * Get user's chat rooms (HTTP)
     */
    async fetchChatRooms() {
        try {
            const response = await fetch('/api/chat/rooms', {
                credentials: 'include'
            });
            const data = await response.json();
            return data.success ? data.rooms : [];
        } catch (error) {
            console.error('❌ Error fetching chat rooms:', error);
            return [];
        }
    }

    /**
     * Create a chat room (HTTP)
     */
    async createChatRoom(name, description, isPublic) {
        try {
            const response = await fetch('/api/chat/rooms', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: JSON.stringify({ name, description, isPublic })
            });
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('❌ Error creating chat room:', error);
            return { success: false, message: 'Failed to create room' };
        }
    }

    /**
     * Join a chat room (HTTP)
     */
    async joinChatRoom(roomId) {
        try {
            const response = await fetch(`/api/chat/rooms/${roomId}/join`, {
                method: 'POST',
                credentials: 'include'
            });
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('❌ Error joining chat room:', error);
            return { success: false, message: 'Failed to join room' };
        }
    }

    /**
     * Leave a chat room (HTTP)
     */
    async leaveChatRoom(roomId) {
        try {
            const response = await fetch(`/api/chat/rooms/${roomId}/leave`, {
                method: 'POST',
                credentials: 'include'
            });
            const data = await response.json();
            return data;
        } catch (error) {
            console.error('❌ Error leaving chat room:', error);
            return { success: false, message: 'Failed to leave room' };
        }
    }

    /**
     * Get unread count (HTTP)
     */
    async getUnreadCount() {
        try {
            const response = await fetch('/api/chat/unread-count', {
                credentials: 'include'
            });
            const data = await response.json();
            return data.success ? data.count : 0;
        } catch (error) {
            console.error('❌ Error getting unread count:', error);
            return 0;
        }
    }

    /**
     * Get online users (HTTP)
     */
    async fetchOnlineUsers() {
        try {
            const response = await fetch('/api/chat/online-users', {
                credentials: 'include'
            });
            const data = await response.json();
            return data.success ? data.users : [];
        } catch (error) {
            console.error('❌ Error fetching online users:', error);
            return [];
        }
    }

    /**
     * Subscribe to events
     */
    onMessageReceived(callback) {
        this.callbacks.onMessageReceived.push(callback);
    }

    onTyping(callback) {
        this.callbacks.onTyping.push(callback);
    }

    onUserOnline(callback) {
        this.callbacks.onUserOnline.push(callback);
    }

    onUserOffline(callback) {
        this.callbacks.onUserOffline.push(callback);
    }

    onError(callback) {
        this.callbacks.onError.push(callback);
    }

    onUserJoinedRoom(callback) {
        this.callbacks.onUserJoinedRoom.push(callback);
    }

    onUserLeftRoom(callback) {
        this.callbacks.onUserLeftRoom.push(callback);
    }

    onConnectionStateChanged(callback) {
        this.callbacks.onConnectionStateChanged.push(callback);
    }
}

// Export for use in other files
window.ChatService = ChatService;

