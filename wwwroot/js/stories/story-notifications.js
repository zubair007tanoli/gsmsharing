/**
 * Notification System for Stories
 * Provides user-friendly notifications for story operations
 */

const StoryNotifications = {
    /**
     * Show a notification to the user
     * @param {string} message - Message to display
     * @param {string} type - Type: 'success', 'error', 'warning', 'info'
     * @param {number} duration - Duration in milliseconds (default: 5000)
     */
    show(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show story-notification`;
        notification.setAttribute('role', 'alert');
        notification.setAttribute('aria-live', 'polite');
        
        // Add Bootstrap classes if available
        const typeClasses = {
            success: 'alert-success',
            error: 'alert-danger',
            warning: 'alert-warning',
            info: 'alert-info'
        };
        
        notification.className = `alert ${typeClasses[type] || typeClasses.info} alert-dismissible fade show story-notification`;
        
        // Create safe HTML structure
        const messageSpan = document.createElement('span');
        messageSpan.textContent = message;
        
        const closeButton = document.createElement('button');
        closeButton.type = 'button';
        closeButton.className = 'btn-close';
        closeButton.setAttribute('aria-label', 'Close');
        closeButton.addEventListener('click', () => notification.remove());
        
        notification.appendChild(messageSpan);
        notification.appendChild(closeButton);
        
        // Add styles if not already in DOM
        if (!document.getElementById('story-notification-styles')) {
            const style = document.createElement('style');
            style.id = 'story-notification-styles';
            style.textContent = `
                .story-notification {
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    z-index: 9999;
                    min-width: 300px;
                    max-width: 500px;
                    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                    animation: slideInRight 0.3s ease-out;
                }
                
                @keyframes slideInRight {
                    from {
                        transform: translateX(100%);
                        opacity: 0;
                    }
                    to {
                        transform: translateX(0);
                        opacity: 1;
                    }
                }
                
                .story-notification.fade {
                    animation: fadeOut 0.3s ease-out forwards;
                }
                
                @keyframes fadeOut {
                    to {
                        opacity: 0;
                        transform: translateX(100%);
                    }
                }
            `;
            document.head.appendChild(style);
        }
        
        document.body.appendChild(notification);
        
        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                notification.classList.add('fade');
                setTimeout(() => notification.remove(), 300);
            }, duration);
        }
        
        return notification;
    },

    /**
     * Show success notification
     */
    success(message, duration = 5000) {
        return this.show(message, 'success', duration);
    },

    /**
     * Show error notification
     */
    error(message, duration = 7000) {
        return this.show(message, 'error', duration);
    },

    /**
     * Show warning notification
     */
    warning(message, duration = 6000) {
        return this.show(message, 'warning', duration);
    },

    /**
     * Show info notification
     */
    info(message, duration = 5000) {
        return this.show(message, 'info', duration);
    }
};

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = StoryNotifications;
}

