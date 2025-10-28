/**
 * Follow System JavaScript
 * Handles all follow/unfollow functionality across the application
 */

(function() {
    'use strict';

    // ===========================================
    // Follow User Function
    // ===========================================
    window.followUser = async function(userId) {
        try {
            // Get the button element
            const btn = document.querySelector(`[onclick="followUser('${userId}')"]`);
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>Following...</span>';
            }

            const response = await fetch(`/api/FollowApi/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const data = await response.json();

            if (response.ok) {
                // Update button state
                if (btn) {
                    btn.classList.add('following');
                    btn.innerHTML = '<i class="fas fa-user-check"></i> <span>Following</span>';
                    btn.onclick = () => unfollowUser(userId);
                    btn.disabled = false;
                }

                // Update follower count
                updateFollowerCount(userId, data.followerCount);

                // Show success notification
                showNotification('Success', 'You are now following this user', 'success');
            } else {
                throw new Error(data.message || 'Failed to follow user');
            }
        } catch (error) {
            console.error('Error following user:', error);
            showNotification('Error', error.message || 'Failed to follow user', 'error');
            
            // Reset button
            const btn = document.querySelector(`[onclick="followUser('${userId}')"]`);
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-user-plus"></i> <span>Follow</span>';
            }
        }
    };

    // ===========================================
    // Unfollow User Function
    // ===========================================
    window.unfollowUser = async function(userId) {
        // Show confirmation dialog
        if (!confirm('Are you sure you want to unfollow this user?')) {
            return;
        }

        try {
            // Get the button element
            const btn = document.querySelector(`[onclick^="unfollowUser"]`);
            if (btn) {
                btn.disabled = true;
                btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> <span>Unfollowing...</span>';
            }

            const response = await fetch(`/api/FollowApi/${userId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const data = await response.json();

            if (response.ok) {
                // Update button state
                if (btn) {
                    btn.classList.remove('following');
                    btn.innerHTML = '<i class="fas fa-user-plus"></i> <span>Follow</span>';
                    btn.onclick = () => followUser(userId);
                    btn.disabled = false;
                }

                // Update follower count
                updateFollowerCount(userId, data.followerCount);

                // Show success notification
                showNotification('Success', 'You have unfollowed this user', 'info');
            } else {
                throw new Error(data.message || 'Failed to unfollow user');
            }
        } catch (error) {
            console.error('Error unfollowing user:', error);
            showNotification('Error', error.message || 'Failed to unfollow user', 'error');
            
            // Reset button
            const btn = document.querySelector(`[onclick^="unfollowUser"]`);
            if (btn) {
                btn.disabled = false;
                btn.classList.add('following');
                btn.innerHTML = '<i class="fas fa-user-check"></i> <span>Following</span>';
            }
        }
    };

    // ===========================================
    // Toggle Follow Function
    // ===========================================
    window.toggleFollow = async function(userId) {
        try {
            const response = await fetch(`/api/FollowApi/toggle/${userId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                }
            });

            const data = await response.json();

            if (response.ok) {
                // Update UI based on new state
                const btn = document.querySelector(`[onclick="toggleFollow('${userId}')"]`);
                if (btn) {
                    if (data.isFollowing) {
                        btn.classList.add('following');
                        btn.innerHTML = '<i class="fas fa-user-check"></i> <span>Following</span>';
                    } else {
                        btn.classList.remove('following');
                        btn.innerHTML = '<i class="fas fa-user-plus"></i> <span>Follow</span>';
                    }
                }

                // Update follower count
                updateFollowerCount(userId, data.followerCount);

                // Show notification
                const message = data.isFollowing ? 'You are now following this user' : 'You have unfollowed this user';
                showNotification('Success', message, data.isFollowing ? 'success' : 'info');
            } else {
                throw new Error(data.message || 'Failed to toggle follow');
            }
        } catch (error) {
            console.error('Error toggling follow:', error);
            showNotification('Error', error.message || 'Failed to update follow status', 'error');
        }
    };

    // ===========================================
    // Update Follower Count Display
    // ===========================================
    function updateFollowerCount(userId, count) {
        // Update all follower count displays on the page
        const countElements = document.querySelectorAll(`.follower-count[data-user-id="${userId}"]`);
        countElements.forEach(el => {
            el.textContent = formatCount(count);
        });

        // Update stat boxes
        const statElements = document.querySelectorAll(`.profile-stat-value.followers`);
        statElements.forEach(el => {
            el.textContent = formatCount(count);
        });
    }

    // ===========================================
    // Load Followers Modal
    // ===========================================
    window.loadFollowers = async function(userId, page = 1) {
        try {
            showModal('followersModal');
            document.getElementById('followersContent').innerHTML = '<div class="profile-loading"><div class="profile-spinner"></div></div>';

            const response = await fetch(`/api/FollowApi/followers/${userId}?page=${page}&pageSize=20`);
            const data = await response.json();

            if (response.ok) {
                renderUserList('followersContent', data.followers, 'followers');
                renderPagination('followersPagination', data.page, data.totalPages, (p) => loadFollowers(userId, p));
            } else {
                throw new Error('Failed to load followers');
            }
        } catch (error) {
            console.error('Error loading followers:', error);
            document.getElementById('followersContent').innerHTML = '<div class="alert alert-danger">Failed to load followers</div>';
        }
    };

    // ===========================================
    // Load Following Modal
    // ===========================================
    window.loadFollowing = async function(userId, page = 1) {
        try {
            showModal('followingModal');
            document.getElementById('followingContent').innerHTML = '<div class="profile-loading"><div class="profile-spinner"></div></div>';

            const response = await fetch(`/api/FollowApi/following/${userId}?page=${page}&pageSize=20`);
            const data = await response.json();

            if (response.ok) {
                renderUserList('followingContent', data.following, 'following');
                renderPagination('followingPagination', data.page, data.totalPages, (p) => loadFollowing(userId, p));
            } else {
                throw new Error('Failed to load following');
            }
        } catch (error) {
            console.error('Error loading following:', error);
            document.getElementById('followingContent').innerHTML = '<div class="alert alert-danger">Failed to load following</div>';
        }
    };

    // ===========================================
    // Load Suggested Users
    // ===========================================
    window.loadSuggestedUsers = async function() {
        try {
            const response = await fetch('/api/FollowApi/suggestions?count=5');
            const data = await response.json();

            if (response.ok && data.suggestions) {
                renderSuggestedUsers(data.suggestions);
            }
        } catch (error) {
            console.error('Error loading suggested users:', error);
        }
    };

    // ===========================================
    // Render User List
    // ===========================================
    function renderUserList(containerId, users, type) {
        const container = document.getElementById(containerId);
        
        if (!users || users.length === 0) {
            container.innerHTML = '<div class="profile-empty-state"><p>No users found</p></div>';
            return;
        }

        const html = users.map(user => `
            <div class="suggested-user-item">
                <img src="${user.avatarUrl || '/images/default-avatar.png'}" 
                     alt="${user.displayName}" 
                     class="suggested-user-avatar">
                <div class="suggested-user-info">
                    <div class="suggested-user-name">
                        <a href="/u/${user.displayName}">${user.displayName}</a>
                    </div>
                    <div class="suggested-user-stats">
                        ${user.karmaPoints} karma
                    </div>
                </div>
                ${user.isFollowing ? 
                    `<button class="suggested-user-follow following" onclick="unfollowUser('${user.userId}')">
                        <i class="fas fa-check"></i> Following
                    </button>` :
                    `<button class="suggested-user-follow" onclick="followUser('${user.userId}')">
                        <i class="fas fa-user-plus"></i> Follow
                    </button>`
                }
            </div>
        `).join('');

        container.innerHTML = html;
    }

    // ===========================================
    // Render Suggested Users
    // ===========================================
    function renderSuggestedUsers(users) {
        const container = document.getElementById('suggestedUsersContainer');
        if (!container) return;

        if (!users || users.length === 0) {
            container.innerHTML = '<p class="text-center text-muted">No suggestions available</p>';
            return;
        }

        const html = users.map(user => `
            <div class="suggested-user-item">
                <img src="${user.avatarUrl || '/images/default-avatar.png'}" 
                     alt="${user.displayName}" 
                     class="suggested-user-avatar">
                <div class="suggested-user-info">
                    <div class="suggested-user-name">
                        <a href="/u/${user.displayName}">${user.displayName}</a>
                    </div>
                    <div class="suggested-user-stats">
                        ${user.followerCount} followers
                        ${user.mutualFollowerCount > 0 ? ` • ${user.mutualFollowerCount} mutual` : ''}
                    </div>
                </div>
                <button class="suggested-user-follow" onclick="followUser('${user.userId}')">
                    <i class="fas fa-user-plus"></i> Follow
                </button>
            </div>
        `).join('');

        container.innerHTML = html;
    }

    // ===========================================
    // Render Pagination
    // ===========================================
    function renderPagination(containerId, currentPage, totalPages, callback) {
        const container = document.getElementById(containerId);
        if (!container || totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        let html = '<nav><ul class="pagination justify-content-center">';

        // Previous button
        if (currentPage > 1) {
            html += `<li class="page-item">
                <a class="page-link" href="#" onclick="event.preventDefault(); (${callback})(${currentPage - 1})">Previous</a>
            </li>`;
        }

        // Page numbers
        const maxPages = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxPages / 2));
        let endPage = Math.min(totalPages, startPage + maxPages - 1);

        if (endPage - startPage < maxPages - 1) {
            startPage = Math.max(1, endPage - maxPages + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="event.preventDefault(); (${callback})(${i})">${i}</a>
            </li>`;
        }

        // Next button
        if (currentPage < totalPages) {
            html += `<li class="page-item">
                <a class="page-link" href="#" onclick="event.preventDefault(); (${callback})(${currentPage + 1})">Next</a>
            </li>`;
        }

        html += '</ul></nav>';
        container.innerHTML = html;
    }

    // ===========================================
    // Utility Functions
    // ===========================================
    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    function formatCount(count) {
        if (count >= 1000000) {
            return (count / 1000000).toFixed(1) + 'M';
        } else if (count >= 1000) {
            return (count / 1000).toFixed(1) + 'K';
        }
        return count.toString();
    }

    function showModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        }
    }

    function showNotification(title, message, type = 'info') {
        // Check if toast container exists, if not create it
        let toastContainer = document.getElementById('toastContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toastContainer';
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        const toastId = 'toast-' + Date.now();
        const iconMap = {
            success: 'fa-check-circle text-success',
            error: 'fa-exclamation-circle text-danger',
            warning: 'fa-exclamation-triangle text-warning',
            info: 'fa-info-circle text-info'
        };

        const toast = document.createElement('div');
        toast.className = 'toast';
        toast.id = toastId;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        toast.innerHTML = `
            <div class="toast-header">
                <i class="fas ${iconMap[type]} me-2"></i>
                <strong class="me-auto">${title}</strong>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;

        toastContainer.appendChild(toast);

        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 3000
        });
        bsToast.show();

        // Remove toast from DOM after it's hidden
        toast.addEventListener('hidden.bs.toast', function() {
            toast.remove();
        });
    }

    // ===========================================
    // Profile Share Dropdown Toggle
    // ===========================================
    window.toggleProfileShare = function(event) {
        event.stopPropagation();
        const dropdown = document.getElementById('profileShareDropdown');
        const isVisible = dropdown.style.display !== 'none';
        
        // Close all other dropdowns first
        document.querySelectorAll('.profile-share-dropdown').forEach(d => {
            d.style.display = 'none';
        });
        
        // Toggle current dropdown
        dropdown.style.display = isVisible ? 'none' : 'block';
    };

    // ===========================================
    // Copy Profile Link
    // ===========================================
    window.copyProfileLink = async function(url) {
        try {
            await navigator.clipboard.writeText(url);
            showNotification('Success', 'Profile link copied to clipboard!', 'success');
            
            // Close dropdown
            document.getElementById('profileShareDropdown').style.display = 'none';
        } catch (error) {
            console.error('Error copying link:', error);
            showNotification('Error', 'Failed to copy link', 'error');
        }
    };

    // ===========================================
    // Track Share Action
    // ===========================================
    window.trackShare = function(type, contentId, platform) {
        // Track share activity (optional analytics)
        console.log(`Shared ${type} ${contentId} on ${platform}`);
        
        // Close dropdown after short delay
        setTimeout(() => {
            const dropdown = document.getElementById('profileShareDropdown');
            if (dropdown) {
                dropdown.style.display = 'none';
            }
        }, 300);
    };

    // ===========================================
    // Close Dropdowns on Outside Click
    // ===========================================
    document.addEventListener('click', function(event) {
        if (!event.target.closest('.profile-share-dropdown-wrapper')) {
            document.querySelectorAll('.profile-share-dropdown').forEach(d => {
                d.style.display = 'none';
            });
        }
    });

    // ===========================================
    // Initialize on Page Load
    // ===========================================
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Follow system initialized');
        
        // Load suggested users if container exists
        if (document.getElementById('suggestedUsersContainer')) {
            loadSuggestedUsers();
        }
    });

})();

