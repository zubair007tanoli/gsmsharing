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
        this.messagesContainer = document.getElementById('chatMessages');
        this.typingIndicator = document.getElementById('chatTypingIndicator');
    }

    /**
     * Add message to chat
     */
    addMessage(message) {
        if (!this.messagesContainer) {
            this.initialize();
        }

        const messageEl = this.createMessageElement(message);
        this.messagesContainer.appendChild(messageEl);
    }

    /**
     * Create message HTML element
     */
    createMessageElement(message) {
        const div = document.createElement('div');
        div.className = `chat-message ${message.IsMine ? 'mine' : ''}`;
        div.setAttribute('data-message-id', message.MessageId);

        const initials = message.SenderName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();

        div.innerHTML = `
            <div class="chat-message-avatar">${initials}</div>
            <div class="chat-message-content">
                <div class="chat-message-bubble">
                    <p class="chat-message-text">${this.escapeHtml(message.Content)}</p>
                </div>
                <div class="chat-message-time">${message.FormattedTime || message.TimeAgo}</div>
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
            this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
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
            errorDiv.className = 'alert alert-danger alert-dismissible fade show';
            errorDiv.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            this.messagesContainer.appendChild(errorDiv);
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
     * Add conversation to list
     */
    addConversationToList(user) {
        const listEl = document.getElementById('directChatList');
        if (!listEl) return;

        // Remove empty state
        const emptyState = listEl.querySelector('.chat-empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        const conversationHtml = `
            <div class="chat-conversation-item" data-user-id="${user.userId}" onclick="window.openChat('${user.userId}', '${user.displayName}')">
                <div class="chat-user-avatar" style="background: ${user.color || '#0079d3'}">
                    ${user.initials}
                    <div class="chat-online-dot"></div>
                </div>
                <div class="chat-conversation-info">
                    <div class="chat-conversation-name">${user.displayName}</div>
                    <div class="chat-conversation-preview">Click to start chatting...</div>
                </div>
                <div class="chat-conversation-time">now</div>
            </div>
        `;

        listEl.insertAdjacentHTML('afterbegin', conversationHtml);
    }
}

// Export for use in chat widget
window.ChatUI = ChatUI;

