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
            onError: []
        };
    }

    /**
     * Initialize SignalR connection
     */
    async initialize() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Register event handlers
            this.registerHandlers();

            // Start connection
            await this.connection.start();
            this.isConnected = true;
            console.log('✅ ChatHub connected successfully');

            return true;
        } catch (error) {
            console.error('❌ ChatHub connection failed:', error);
            this.isConnected = false;
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

        // Message sent confirmation
        this.connection.on('MessageSent', (message) => {
            console.log('✅ Message sent confirmation:', message);
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
        });

        this.connection.onreconnected((connectionId) => {
            console.log('✅ ChatHub reconnected:', connectionId);
            this.isConnected = true;
        });

        this.connection.onclose((error) => {
            console.error('❌ ChatHub connection closed:', error);
            this.isConnected = false;
        });
    }

    /**
     * Send direct message
     */
    async sendDirectMessage(receiverId, content) {
        if (!this.isConnected) {
            console.error('❌ Cannot send message: Not connected');
            return false;
        }

        try {
            await this.connection.invoke('SendDirectMessage', receiverId, content);
            console.log('📤 Direct message sent to:', receiverId);
            return true;
        } catch (error) {
            console.error('❌ Error sending message:', error);
            return false;
        }
    }

    /**
     * Send room message
     */
    async sendRoomMessage(roomId, content) {
        if (!this.isConnected) {
            console.error('❌ Cannot send message: Not connected');
            return false;
        }

        try {
            await this.connection.invoke('SendRoomMessage', roomId, content);
            console.log('📤 Room message sent to room:', roomId);
            return true;
        } catch (error) {
            console.error('❌ Error sending room message:', error);
            return false;
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
}

// Export for use in other files
window.ChatService = ChatService;

