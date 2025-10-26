/**
 * Admin Dashboard JavaScript
 * Handles sidebar toggle, search, and interactive features
 */

document.addEventListener('DOMContentLoaded', function() {
    initSidebarToggle();
    initAdminSearch();
    initReportsBadge();
    initThemeToggle();
});

// ========================================
// SIDEBAR TOGGLE
// ========================================
function initSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('adminSidebar');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('collapsed');
            
            // Save state to localStorage
            const isCollapsed = sidebar.classList.contains('collapsed');
            localStorage.setItem('admin-sidebar-collapsed', isCollapsed);
        });
        
        // Restore sidebar state
        const savedState = localStorage.getItem('admin-sidebar-collapsed');
        if (savedState === 'true') {
            sidebar.classList.add('collapsed');
        }
        
        // Mobile: Toggle sidebar
        if (window.innerWidth <= 768) {
            sidebarToggle.addEventListener('click', function(e) {
                e.stopPropagation();
                sidebar.classList.toggle('show');
            });
            
            // Close sidebar when clicking outside
            document.addEventListener('click', function(e) {
                if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                    sidebar.classList.remove('show');
                }
            });
        }
    }
}

// ========================================
// ADMIN SEARCH
// ========================================
function initAdminSearch() {
    const searchInput = document.getElementById('adminSearch');
    
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function(e) {
            clearTimeout(searchTimeout);
            
            searchTimeout = setTimeout(() => {
                const query = e.target.value.toLowerCase();
                
                if (query.length > 0) {
                    searchAdminItems(query);
                } else {
                    clearSearchHighlight();
                }
            }, 300);
        });
    }
}

function searchAdminItems(query) {
    const menuItems = document.querySelectorAll('.menu-item');
    let foundCount = 0;
    
    menuItems.forEach(item => {
        const text = item.textContent.toLowerCase();
        
        if (text.includes(query)) {
            item.style.background = 'rgba(102, 126, 234, 0.15)';
            item.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            foundCount++;
        } else {
            item.style.background = '';
        }
    });
    
    console.log(`Found ${foundCount} matching items for "${query}"`);
}

function clearSearchHighlight() {
    const menuItems = document.querySelectorAll('.menu-item');
    menuItems.forEach(item => {
        if (!item.classList.contains('active')) {
            item.style.background = '';
        }
    });
}

// ========================================
// REPORTS BADGE UPDATE
// ========================================
function initReportsBadge() {
    // Update reports badge count from API
    updateReportsBadge();
    
    // Refresh every 30 seconds
    setInterval(updateReportsBadge, 30000);
}

async function updateReportsBadge() {
    try {
        // TODO: Replace with actual API endpoint
        // const response = await fetch('/api/admin/pending-reports-count');
        // const data = await response.json();
        
        // Simulated data
        const pendingCount = 12;
        
        const badges = document.querySelectorAll('#sidebarReportsBadge, #adminReportsBadge');
        badges.forEach(badge => {
            if (pendingCount > 0) {
                badge.textContent = pendingCount;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        });
    } catch (error) {
        console.error('Error updating reports badge:', error);
    }
}

// ========================================
// THEME TOGGLE
// ========================================
function initThemeToggle() {
    const savedTheme = localStorage.getItem('admin-theme') || 'light';
    document.body.setAttribute('data-theme', savedTheme);
    
    // Listen for theme changes from main site
    window.addEventListener('storage', function(e) {
        if (e.key === 'theme') {
            document.body.setAttribute('data-theme', e.newValue || 'light');
        }
    });
}

// ========================================
// UTILITY FUNCTIONS
// ========================================

// Format numbers with commas
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

// Format currency
function formatCurrency(amount) {
    return '$' + formatNumber(Math.round(amount));
}

// Show toast notification
function showAdminToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} position-fixed`;
    toast.style.cssText = 'top: 80px; right: 20px; z-index: 9999; min-width: 300px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); animation: slideInRight 0.3s ease;';
    toast.innerHTML = `
        <div class="d-flex align-items-center justify-content-between">
            <span>${message}</span>
            <button type="button" class="btn-close" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        toast.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Add CSS for toast animations
if (!document.getElementById('admin-toast-animations')) {
    const style = document.createElement('style');
    style.id = 'admin-toast-animations';
    style.textContent = `
        @keyframes slideInRight {
            from { transform: translateX(400px); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }
        @keyframes slideOutRight {
            from { transform: translateX(0); opacity: 1; }
            to { transform: translateX(400px); opacity: 0; }
        }
    `;
    document.head.appendChild(style);
}

// Smooth scroll to top
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Export functions for external use
window.adminDashboard = {
    showToast: showAdminToast,
    formatNumber: formatNumber,
    formatCurrency: formatCurrency,
    scrollToTop: scrollToTop,
    updateReportsBadge: updateReportsBadge
};

