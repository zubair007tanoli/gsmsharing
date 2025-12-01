/**
 * Chat Debug Helper - For troubleshooting chat issues
 */
window.ChatDebug = {
    /**
     * Check if chat system is properly initialized
     */
    checkSystemStatus: function() {
        const status = {
            signalrLoaded: typeof signalR !== 'undefined',
            chatServiceExists: typeof ChatService !== 'undefined',
            chatControllerExists: typeof ChatController !== 'undefined',
            chatUIExists: typeof ChatUI !== 'undefined',
            currentUserId: window.currentUserId || 'NOT SET',
            connection: null,
            isConnected: false
        };

        // Check if connection exists
        if (window.chatController && window.chatController.chatService) {
            status.connection = window.chatController.chatService.connection;
            status.isConnected = window.chatController.chatService.isConnected;
        }

        console.log('🔍 Chat System Status:', status);
        return status;
    },

    /**
     * Test message sending
     */
    testSendMessage: async function(receiverId, content) {
        if (!window.chatController) {
            console.error('❌ ChatController not initialized');
            return false;
        }

        try {
            console.log('🧪 Testing message send:', { receiverId, content });
            const result = await window.chatController.sendMessage(content);
            console.log('✅ Test result:', result);
            return result;
        } catch (error) {
            console.error('❌ Test failed:', error);
            return false;
        }
    },

    /**
     * Check database connection (via API)
     */
    checkDatabase: async function() {
        try {
            const response = await fetch('/api/chat/direct', {
                credentials: 'include'
            });
            const data = await response.json();
            console.log('📊 Database check (Direct chats):', data);
            return data;
        } catch (error) {
            console.error('❌ Database check failed:', error);
            return null;
        }
    },

    /**
     * Log all chat-related errors
     */
    enableErrorLogging: function() {
        window.addEventListener('error', function(e) {
            if (e.message && e.message.includes('chat')) {
                console.error('🚨 Chat Error:', e);
            }
        });

        // Override console.error for chat-related errors
        const originalError = console.error;
        console.error = function(...args) {
            if (args.some(arg => typeof arg === 'string' && arg.toLowerCase().includes('chat'))) {
                originalError.apply(console, ['🚨 CHAT ERROR:', ...args]);
            } else {
                originalError.apply(console, args);
            }
        };
    }
};

// Auto-enable error logging in development
if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    ChatDebug.enableErrorLogging();
    console.log('🔧 Chat Debug mode enabled');
}

