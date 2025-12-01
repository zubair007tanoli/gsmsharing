/**
 * ChatUI - Handles all UI updates for chat
 */
class ChatUI {
    constructor() {
        this.messagesContainer = null;
        this.typingIndicator = null;
    }

    /**
     * Initialize UI elements
     */
    initialize() {
        // Try to find messages container (could be in widget or page)
        // Try widget first, then dedicated page
        this.messagesContainer = document.getElementById('chatMessages');
        
        // If not found, the widget might be minimized - try to find it anyway
        if (!this.messagesContainer) {
            // Check if widget exists but is hidden
            const widget = document.getElementById('chatWidget');
            if (widget) {
                // Container exists in DOM even if hidden, try again
                this.messagesContainer = document.getElementById('chatMessages');
            }
        }
        
        this.typingIndicator = document.getElementById('chatTypingIndicator');
        
        // Log for debugging
        if (!this.messagesContainer) {
            console.warn('⚠️ ChatUI: chatMessages container not found');
            console.warn('⚠️ Available elements:', {
                chatWidget: !!document.getElementById('chatWidget'),
                chatWindow: !!document.getElementById('chatWindow'),
                chatContent: !!document.getElementById('chatContent')
            });
        } else {
            console.log('✅ ChatUI: Found messages container:', {
                id: this.messagesContainer.id,
                visible: this.messagesContainer.offsetParent !== null,
                parent: this.messagesContainer.parentElement?.id
            });
        }
        
        if (!this.typingIndicator) {
            console.warn('⚠️ ChatUI: chatTypingIndicator not found');
        }
    }

    /**
     * Add message to chat
     */
    addMessage(message) {
        // Re-initialize if container not found (might be in widget that was just opened)
        if (!this.messagesContainer) {
            this.initialize();
        }
        
        // If still not found, try again after a short delay (widget might be opening)
        if (!this.messagesContainer) {
            console.warn('⚠️ ChatUI: Container not found, retrying in 100ms...');
            setTimeout(() => {
                this.initialize();
                if (this.messagesContainer) {
                    this.addMessage(message); // Retry adding the message
                } else {
                    console.error('❌ ChatUI: Cannot add message - container not found after retry');
                    console.error('❌ Available containers:', {
                        chatMessages: document.getElementById('chatMessages'),
                        chatWindow: document.getElementById('chatWindow'),
                        chatWidget: document.getElementById('chatWidget')
                    });
                }
            }, 100);
            return;
        }

        // Check if message already exists (prevent duplicates)
        if (message.MessageId) {
            const existing = this.messagesContainer.querySelector(`[data-message-id="${message.MessageId}"]`);
            if (existing) {
                console.log('📝 ChatUI: Message already exists, skipping:', message.MessageId);
                return;
            }
        }

        try {
            const messageEl = this.createMessageElement(message);
            this.messagesContainer.appendChild(messageEl);
            
            // Hide empty state if it exists
            const emptyState = document.getElementById('chatEmptyMessages');
            if (emptyState) {
                emptyState.style.display = 'none';
            }
            
            console.log('✅ ChatUI: Message added successfully:', {
                MessageId: message.MessageId,
                Container: this.messagesContainer.id,
                TotalMessages: this.messagesContainer.children.length
            });
            
            this.scrollToBottom();
        } catch (error) {
            console.error('❌ ChatUI: Error adding message:', error);
        }
    }

    /**
     * Create message HTML element
     */
    createMessageElement(message) {
        if (!message || !message.Content) {
            console.error('❌ ChatUI: Invalid message object:', message);
            return null;
        }

        const div = document.createElement('div');
        div.className = `chat-message ${message.IsMine ? 'mine' : ''}`;
        
        // Use MessageId if available, otherwise generate a temporary one
        const messageId = message.MessageId || Date.now();
        div.setAttribute('data-message-id', messageId);

        // Get initials from sender name
        let initials = 'U';
        if (message.SenderName) {
            const parts = message.SenderName.split(' ').filter(p => p.length > 0);
            if (parts.length >= 2) {
                initials = (parts[0][0] + parts[1][0]).toUpperCase();
            } else if (parts.length === 1 && parts[0].length > 0) {
                initials = parts[0].substring(0, 2).toUpperCase();
            }
        }

        div.innerHTML = `
            <div class="chat-message-avatar">${initials}</div>
            <div class="chat-message-content">
                <div class="chat-message-bubble">
                    <p class="chat-message-text">${this.escapeHtml(message.Content)}</p>
                </div>
                <div class="chat-message-time">${message.FormattedTime || message.TimeAgo || 'now'}</div>
            </div>
        `;

        return div;
    }

    /**
     * Add ad to messages
     */
    addAdToMessages(adHtml) {
        if (!this.messagesContainer) return;

        const adDiv = document.createElement('div');
        adDiv.innerHTML = adHtml;
        this.messagesContainer.appendChild(adDiv.firstElementChild);
    }

    /**
     * Clear all messages
     */
    clearMessages() {
        if (this.messagesContainer) {
            this.messagesContainer.innerHTML = '';
            // Show empty state if it exists
            const emptyState = document.getElementById('chatEmptyMessages');
            if (emptyState) {
                emptyState.style.display = 'block';
            }
        }
    }

    /**
     * Show typing indicator
     */
    showTypingIndicator(show) {
        if (this.typingIndicator) {
            if (show) {
                this.typingIndicator.classList.add('active');
            } else {
                this.typingIndicator.classList.remove('active');
            }
        }
    }

    /**
     * Scroll to bottom of messages
     */
    scrollToBottom() {
        if (this.messagesContainer) {
            // Use requestAnimationFrame for smooth scrolling
            requestAnimationFrame(() => {
                this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
            });
        }
    }

    /**
     * Show loading state
     */
    showLoading() {
        if (this.messagesContainer) {
            this.messagesContainer.innerHTML = `
                <div class="chat-empty-state">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            `;
        }
    }

    /**
     * Show error message
     */
    showError(message) {
        if (this.messagesContainer) {
            const errorDiv = document.createElement('div');
            errorDiv.className = 'chat-error-message alert alert-danger alert-dismissible fade show';
            errorDiv.setAttribute('role', 'alert');
            errorDiv.innerHTML = `
                <i class="fas fa-exclamation-circle"></i>
                <span>${this.escapeHtml(message)}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            `;
            this.messagesContainer.appendChild(errorDiv);
            
            // Auto-dismiss after 5 seconds
            setTimeout(() => {
                if (errorDiv.parentNode) {
                    errorDiv.remove();
                }
            }, 5000);
        }
    }

    /**
     * Update user online status
     */
    updateUserStatus(userId, status) {
        const statusEl = document.getElementById('chatWindowStatus');
        if (statusEl) {
            statusEl.className = `chat-window-user-status ${status}`;
            statusEl.innerHTML = `<i class="fas fa-circle" style="font-size: 0.5rem;"></i> ${status.charAt(0).toUpperCase() + status.slice(1)}`;
        }

        // Update in conversation list
        const conversationItem = document.querySelector(`.chat-conversation-item[data-user-id="${userId}"]`);
        if (conversationItem) {
            const dot = conversationItem.querySelector('.chat-online-dot');
            if (dot) {
                if (status === 'online') {
                    dot.style.background = '#46d160';
                } else if (status === 'away') {
                    dot.style.background = '#ffa500';
                } else {
                    dot.style.display = 'none';
                }
            }
        }
    }

    /**
     * Escape HTML to prevent XSS
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Render direct chats list
     */
    renderDirectChatsList(chats) {
        const listEl = document.getElementById('directChatList');
        if (!listEl) return;

        // Remove empty state if exists
        const emptyState = listEl.querySelector('.chat-empty-state');
        if (emptyState && chats.length > 0) {
            emptyState.remove();
        }

        if (chats.length === 0) {
            return; // Keep empty state
        }

        // Clear and render
        listEl.innerHTML = '';
        chats.forEach(chat => {
            const initials = chat.displayName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
            const lastMessageTime = chat.lastMessage ? this.formatTime(chat.lastMessage.sentAt) : '';
            const lastMessageText = chat.lastMessage ? this.escapeHtml(chat.lastMessage.content.substring(0, 50)) : 'No messages yet';
            
            const conversationHtml = `
                <div class="chat-conversation-item" data-user-id="${chat.userId}" onclick="window.openChat('${chat.userId}', '${this.escapeHtml(chat.displayName)}')">
                    <div class="chat-user-avatar" style="background: ${this.getColorFromName(chat.displayName)}">
                        ${initials}
                        <div class="chat-online-dot" style="background: ${chat.status === 'online' ? '#46d160' : 'transparent'}"></div>
                    </div>
                    <div class="chat-conversation-info">
                        <div class="chat-conversation-name">${this.escapeHtml(chat.displayName)}</div>
                        <div class="chat-conversation-preview">${lastMessageText}${lastMessageText.length >= 50 ? '...' : ''}</div>
                    </div>
                    <div class="chat-conversation-meta">
                        ${chat.unreadCount > 0 ? `<span class="chat-unread-badge-small">${chat.unreadCount > 99 ? '99+' : chat.unreadCount}</span>` : ''}
                        <div class="chat-conversation-time">${lastMessageTime}</div>
                    </div>
                </div>
            `;
            listEl.insertAdjacentHTML('beforeend', conversationHtml);
        });
    }

    /**
     * Render chat rooms list
     */
    renderChatRoomsList(rooms) {
        const listEl = document.getElementById('roomsListContent');
        if (!listEl) return;

        // Remove empty state if exists
        const emptyState = listEl.querySelector('.chat-empty-state');
        if (emptyState && rooms.length > 0) {
            emptyState.remove();
        }

        if (rooms.length === 0) {
            return; // Keep empty state
        }

        // Clear and render
        listEl.innerHTML = '';
        rooms.forEach(room => {
            const initials = room.name.substring(0, 2).toUpperCase();
            const lastMessageTime = room.lastMessage ? this.formatTime(room.lastMessage.sentAt) : '';
            const lastMessageText = room.lastMessage ? this.escapeHtml(room.lastMessage.content.substring(0, 50)) : 'No messages yet';
            
            const roomHtml = `
                <div class="chat-room-item" data-room-id="${room.chatRoomId}" onclick="openChatRoomWindow(${room.chatRoomId}, '${this.escapeHtml(room.name)}', '${room.iconUrl || ''}')">
                    <div class="chat-room-avatar" style="background: ${this.getColorFromName(room.name)}">
                        ${initials}
                    </div>
                    <div class="chat-room-info">
                        <div class="chat-room-name">
                            ${this.escapeHtml(room.name)}
                            ${room.isPublic ? '<i class="fas fa-globe" title="Public Room"></i>' : '<i class="fas fa-lock" title="Private Room"></i>'}
                        </div>
                        <div class="chat-room-preview">${lastMessageText}${lastMessageText.length >= 50 ? '...' : ''}</div>
                        <div class="chat-room-meta-info">
                            <span><i class="fas fa-users"></i> ${room.memberCount} members</span>
                        </div>
                    </div>
                    <div class="chat-room-meta">
                        ${room.unreadCount > 0 ? `<span class="chat-unread-badge-small">${room.unreadCount > 99 ? '99+' : room.unreadCount}</span>` : ''}
                        <div class="chat-room-time">${lastMessageTime}</div>
                    </div>
                </div>
            `;
            listEl.insertAdjacentHTML('beforeend', roomHtml);
        });
    }

    /**
     * Format time for display
     */
    formatTime(dateString) {
        if (!dateString) return '';
        
        const date = new Date(dateString);
        const now = new Date();
        const diff = now - date;
        
        if (diff < 60000) return 'now';
        if (diff < 3600000) return `${Math.floor(diff / 60000)}m`;
        if (diff < 86400000) return `${Math.floor(diff / 3600000)}h`;
        if (diff < 604800000) return `${Math.floor(diff / 86400000)}d`;
        
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    }

    /**
     * Get color from name (deterministic)
     */
    getColorFromName(name) {
        const colors = ['#0079d3', '#ff4500', '#46d160', '#ffa500', '#7c7c7c', '#0056b3', '#8b5cf6', '#ec4899'];
        let hash = 0;
        for (let i = 0; i < name.length; i++) {
            hash = name.charCodeAt(i) + ((hash << 5) - hash);
        }
        return colors[Math.abs(hash) % colors.length];
    }

    /**
     * Show room member join/leave notification
     */
    showRoomMemberNotification(userId, roomId, action) {
        if (!this.messagesContainer) return;

        // Get user display name (would need to fetch or pass as parameter)
        const notificationDiv = document.createElement('div');
        notificationDiv.className = 'chat-system-message';
        notificationDiv.innerHTML = `
            <div class="chat-system-message-content">
                <i class="fas fa-user-${action === 'joined' ? 'plus' : 'minus'}"></i>
                <span>User ${action === 'joined' ? 'joined' : 'left'} the room</span>
            </div>
        `;
        this.messagesContainer.appendChild(notificationDiv);
        this.scrollToBottom();

        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notificationDiv.parentNode) {
                notificationDiv.remove();
            }
        }, 5000);
    }

    /**
     * Update connection status indicator
     */
    updateConnectionStatus(state) {
        const statusEl = document.getElementById('chatConnectionStatus');
        if (!statusEl) return;

        const statusConfig = {
            'connected': { text: 'Connected', class: 'status-connected', icon: 'fa-check-circle' },
            'disconnected': { text: 'Disconnected', class: 'status-disconnected', icon: 'fa-times-circle' },
            'reconnecting': { text: 'Reconnecting...', class: 'status-reconnecting', icon: 'fa-sync fa-spin' }
        };

        const config = statusConfig[state] || statusConfig['disconnected'];
        statusEl.className = `chat-connection-status ${config.class}`;
        statusEl.innerHTML = `<i class="fas ${config.icon}"></i> ${config.text}`;
    }

    /**
     * Render online users list
     */
    renderOnlineUsersList(users) {
        const listEl = document.getElementById('onlineUsersContent');
        const emptyState = document.getElementById('onlineUsersEmpty');
        
        if (!listEl) return;

        // Show/hide empty state
        if (users.length === 0) {
            if (emptyState) emptyState.style.display = 'block';
            listEl.innerHTML = '';
            return;
        }

        if (emptyState) emptyState.style.display = 'none';

        // Clear and render
        listEl.innerHTML = '';
        users.forEach(user => {
            const initials = user.displayName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
            const avatarColor = this.getColorFromName(user.displayName);
            
            const userHtml = `
                <div class="chat-conversation-item" onclick="window.openChat('${user.userId}', '${this.escapeHtml(user.displayName)}')">
                    <div class="chat-user-avatar" style="background: ${avatarColor}; position: relative;">
                        ${user.avatarUrl 
                            ? `<img src="${this.escapeHtml(user.avatarUrl)}" alt="${this.escapeHtml(user.displayName)}" style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover;">`
                            : `${initials}`}
                        <div class="chat-online-dot" style="background: #46d160; display: block;"></div>
                    </div>
                    <div class="chat-conversation-info">
                        <div class="chat-conversation-name">${this.escapeHtml(user.displayName)}</div>
                        <div class="chat-conversation-preview" style="color: #46d160;">
                            <i class="fas fa-circle" style="font-size: 0.5rem;"></i> Online
                        </div>
                    </div>
                </div>
            `;
            listEl.insertAdjacentHTML('beforeend', userHtml);
        });
    }
}

// Export for use in chat widget
window.ChatUI = ChatUI;

