/**
 * Notification Handler - Real-Time Notifications for Navbar
 * Connects to NotificationHub and updates navbar notification badge in real-time
 */

class NotificationManager {
    constructor() {
        this.notificationConnection = null;
        this.isAdmin = false;
    }

    async initialize() {
        // Check if user is authenticated
        const isAuth = document.getElementById('isAuthenticated')?.value === 'true';
        if (!isAuth) {
            console.log('📌 User not authenticated, skipping notification hub');
            return;
        }

        try {
            // Build connection to NotificationHub
            this.notificationConnection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect()
                .build();

            // Setup event handlers
            this.setupEventHandlers();

            // Start connection
            await this.notificationConnection.start();
            console.log('✅ NotificationHub connected successfully');

            // Join notification group
            await this.notificationConnection.invoke("JoinNotificationGroup");
            
            // Check if user is admin and join admin group
            await this.checkAndJoinAdminGroup();

        } catch (err) {
            console.error('❌ Error connecting to NotificationHub:', err);
        }
    }

    async checkAndJoinAdminGroup() {
        // Check if user has admin role
        const adminElement = document.querySelector('.nav-link.text-warning .fa-crown');
        this.isAdmin = adminElement !== null;

        if (this.isAdmin) {
            try {
                await this.notificationConnection.invoke("JoinAdminGroup");
                console.log('👑 Joined admin-notifications group');
            } catch (err) {
                console.error('Error joining admin group:', err);
            }
        }
    }

    setupEventHandlers() {
        // General notification received
        this.notificationConnection.on("ReceiveNotification", (notification) => {
            console.log('🔔 Notification received:', notification);
            this.handleNewNotification(notification);
        });

        // Admin-specific notifications
        this.notificationConnection.on("ReceiveAdminNotification", (notification) => {
            console.log('👑 Admin notification received:', notification);
            this.handleAdminNotification(notification);
        });

        // Notification marked as read
        this.notificationConnection.on("NotificationMarkedAsRead", (notificationId) => {
            this.updateNotificationUI(notificationId, true);
        });

        // Unread count update
        this.notificationConnection.on("UnreadNotificationCount", (count) => {
            this.updateNotificationBadge(count);
        });

        // Connection events
        this.notificationConnection.onreconnecting(() => {
            console.log('🔄 Reconnecting to NotificationHub...');
        });

        this.notificationConnection.onreconnected(() => {
            console.log('✅ Reconnected to NotificationHub');
            this.checkAndJoinAdminGroup();
        });

        this.notificationConnection.onclose(() => {
            console.log('❌ NotificationHub connection closed');
        });
    }

    handleNewNotification(notification) {
        // Update notification badge
        this.incrementNotificationBadge();

        // Show toast notification
        this.showNotificationToast(notification);

        // Play notification sound (optional)
        // this.playNotificationSound();

        // Update notification dropdown if open
        this.prependNotificationToList(notification);
    }

    handleAdminNotification(notification) {
        if (!this.isAdmin) return;

        // Show admin-specific toast
        this.showAdminToast(notification);

        // Update admin notification counter if exists
        const adminBadge = document.querySelector('.admin-notification-badge');
        if (adminBadge) {
            const currentCount = parseInt(adminBadge.textContent) || 0;
            adminBadge.textContent = currentCount + 1;
            adminBadge.style.display = 'inline-block';
        }
    }

    updateNotificationBadge(count) {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        }
    }

    incrementNotificationBadge() {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            const currentCount = parseInt(badge.textContent) || 0;
            const newCount = currentCount + 1;
            badge.textContent = newCount > 99 ? '99+' : newCount;
            badge.style.display = 'inline-block';
        }
    }

    showNotificationToast(notification) {
        const toast = document.createElement('div');
        toast.className = 'position-fixed top-0 end-0 p-3';
        toast.style.zIndex = '9999';
        
        const toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center text-white bg-primary border-0';
        toastEl.setAttribute('role', 'alert');
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${notification.title || 'New Notification'}</strong><br>
                    <small>${notification.message || ''}</small>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        
        toast.appendChild(toastEl);
        document.body.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toastEl, { delay: 5000 });
        bsToast.show();
        
        toastEl.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    showAdminToast(notification) {
        const toast = document.createElement('div');
        toast.className = 'position-fixed top-0 end-0 p-3';
        toast.style.zIndex = '10000';
        
        const toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center text-white bg-warning border-0';
        toastEl.setAttribute('role', 'alert');
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body text-dark">
                    <i class="fas fa-crown me-2"></i>
                    <strong>${notification.title || 'Admin Notification'}</strong><br>
                    <small>${notification.message || ''}</small>
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        
        toast.appendChild(toastEl);
        document.body.appendChild(toast);
        
        const bsToast = new bootstrap.Toast(toastEl, { delay: 7000 });
        bsToast.show();
        
        toastEl.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    prependNotificationToList(notification) {
        const notificationList = document.querySelector('.notification-list');
        if (!notificationList) return;

        const notificationHTML = `
            <div class="notification-item unread" data-notification-id="${notification.notificationId || ''}">
                <div class="notification-title">${notification.title || 'New Notification'}</div>
                ${notification.message ? `<div class="notification-text">${notification.message}</div>` : ''}
                <div class="notification-info">
                    <span><i class="far fa-clock me-1"></i>Just now</span>
                    ${notification.url ? `<a href="${notification.url}" class="text-primary">View</a>` : ''}
                </div>
            </div>
        `;

        notificationList.insertAdjacentHTML('afterbegin', notificationHTML);

        // Remove "No new notifications" message if it exists
        const noNotifications = notificationList.querySelector('.notification-item:last-child');
        if (noNotifications && noNotifications.textContent.includes('No new notifications')) {
            noNotifications.remove();
        }
    }

    updateNotificationUI(notificationId, isRead) {
        const notificationItem = document.querySelector(`[data-notification-id="${notificationId}"]`);
        if (notificationItem) {
            if (isRead) {
                notificationItem.classList.remove('unread');
            } else {
                notificationItem.classList.add('unread');
            }
        }
    }

    playNotificationSound() {
        // Optional: Play a subtle notification sound
        try {
            const audio = new Audio('/sounds/notification.mp3');
            audio.volume = 0.3;
            audio.play().catch(err => {
                console.log('Could not play notification sound:', err);
            });
        } catch (err) {
            console.log('Notification sound not available');
        }
    }
}

// Initialize notification manager on page load
document.addEventListener('DOMContentLoaded', async function() {
    if (typeof signalR !== 'undefined') {
        window.notificationManager = new NotificationManager();
        await window.notificationManager.initialize();
    } else {
        console.warn('SignalR library not loaded, notifications will not work in real-time');
    }
});

