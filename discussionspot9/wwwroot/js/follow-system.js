/* ============================================
   FOLLOW SYSTEM - Client-Side Logic
   ============================================ */

class FollowManager {
    constructor() {
        this.initializeFollowButtons();
    }

    initializeFollowButtons() {
        document.addEventListener('DOMContentLoaded', () => {
            console.log('🔗 Follow system initialized');
            
            // Attach click handlers to all follow buttons
            document.querySelectorAll('[data-follow-btn]').forEach(btn => {
                btn.addEventListener('click', (e) => this.handleFollowClick(e));
            });
        });
    }

    async handleFollowClick(event) {
        event.preventDefault();
        const button = event.currentTarget;
        const userId = button.dataset.userId;

        if (!userId) {
            console.error('❌ No userId found on follow button');
            return;
        }

        // Disable button during request
        button.disabled = true;
        const originalHTML = button.innerHTML;

        try {
            const response = await fetch('/api/follow/toggle', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                body: JSON.stringify({ userId: userId })
            });

            const result = await response.json();

            if (result.success) {
                // Update button state
                this.updateButtonState(button, result.isFollowing);
                
                // Update counts if elements exist
                this.updateFollowCounts(userId, result.isFollowing);
                
                // Show toast notification
                this.showToast(result.message, 'success');
                
                console.log(`✅ ${result.isFollowing ? 'Followed' : 'Unfollowed'} user ${userId}`);
            } else {
                this.showToast(result.message || 'Failed to update follow status', 'error');
                button.innerHTML = originalHTML;
            }
        } catch (error) {
            console.error('❌ Error toggling follow:', error);
            this.showToast('Error updating follow status', 'error');
            button.innerHTML = originalHTML;
        } finally {
            button.disabled = false;
        }
    }

    updateButtonState(button, isFollowing) {
        if (isFollowing) {
            button.classList.remove('btn-follow-inactive');
            button.classList.add('btn-follow-active');
            button.innerHTML = '<i class="fas fa-user-check"></i> Following';
            button.title = 'Click to unfollow';
        } else {
            button.classList.remove('btn-follow-active');
            button.classList.add('btn-follow-inactive');
            button.innerHTML = '<i class="fas fa-user-plus"></i> Follow';
            button.title = 'Click to follow';
        }
    }

    async updateFollowCounts(userId, isFollowing) {
        try {
            const response = await fetch(`/api/follow/status/${userId}`);
            const data = await response.json();

            // Update follower count if element exists
            const followerCountEl = document.querySelector(`[data-follower-count="${userId}"]`);
            if (followerCountEl) {
                followerCountEl.textContent = this.formatCount(data.followerCount);
            }

            // Update following count if element exists
            const followingCountEl = document.querySelector(`[data-following-count="${userId}"]`);
            if (followingCountEl) {
                followingCountEl.textContent = this.formatCount(data.followingCount);
            }

            console.log(`📊 Counts updated: ${data.followerCount} followers, ${data.followingCount} following`);
        } catch (error) {
            console.error('Error updating follow counts:', error);
        }
    }

    formatCount(count) {
        if (count >= 1000000) return `${(count / 1000000).toFixed(1)}M`;
        if (count >= 1000) return `${(count / 1000).toFixed(1)}K`;
        return count.toString();
    }

    getAntiForgeryToken() {
        // Try to get from meta tag or hidden input
        const tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (tokenMeta) {
            return tokenMeta.content;
        }
        
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            return tokenInput.value;
        }
        
        return '';
    }

    showToast(message, type = 'info') {
        // Check if toast system exists
        if (typeof window.showToastNotification === 'function') {
            window.showToastNotification(message, type);
            return;
        }

        // Fallback: Simple toast
        const toast = document.createElement('div');
        toast.className = `follow-toast follow-toast-${type}`;
        toast.textContent = message;
        document.body.appendChild(toast);

        // Trigger animation
        setTimeout(() => toast.classList.add('show'), 10);

        // Remove after 3 seconds
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }
}

// Initialize on page load
if (typeof window.followManager === 'undefined') {
    window.followManager = new FollowManager();
}

/* ============================================
   HELPER FUNCTIONS
   ============================================ */

/**
 * Quick function to add follow button to any page
 * @param {string} targetUserId - User ID to follow
 * @param {boolean} isFollowing - Current follow state
 * @returns {HTMLElement} Follow button element
 */
function createFollowButton(targetUserId, isFollowing = false) {
    const button = document.createElement('button');
    button.className = `btn btn-sm ${isFollowing ? 'btn-follow-active' : 'btn-follow-inactive'}`;
    button.dataset.followBtn = 'true';
    button.dataset.userId = targetUserId;
    button.innerHTML = isFollowing 
        ? '<i class="fas fa-user-check"></i> Following'
        : '<i class="fas fa-user-plus"></i> Follow';
    button.title = isFollowing ? 'Click to unfollow' : 'Click to follow';
    
    return button;
}

/**
 * Load follow status and update button
 * @param {string} userId - User ID to check
 * @param {HTMLElement} button - Button element to update
 */
async function loadFollowStatus(userId, button) {
    try {
        const response = await fetch(`/api/follow/status/${userId}`);
        const data = await response.json();
        
        window.followManager.updateButtonState(button, data.isFollowing);
    } catch (error) {
        console.error('Error loading follow status:', error);
    }
}

/**
 * Display follower/following counts
 * @param {string} userId - User ID
 * @param {string} containerSelector - CSS selector for container
 */
async function displayFollowCounts(userId, containerSelector) {
    try {
        const response = await fetch(`/api/follow/status/${userId}`);
        const data = await response.json();

        const container = document.querySelector(containerSelector);
        if (container) {
            container.innerHTML = `
                <div class="follow-counts">
                    <a href="/u/${userId}/followers" class="follow-count-item">
                        <span class="count" data-follower-count="${userId}">${data.followerCount}</span>
                        <span class="label">Followers</span>
                    </a>
                    <a href="/u/${userId}/following" class="follow-count-item">
                        <span class="count" data-following-count="${userId}">${data.followingCount}</span>
                        <span class="label">Following</span>
                    </a>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error displaying follow counts:', error);
    }
}

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { FollowManager, createFollowButton, loadFollowStatus, displayFollowCounts };
}
