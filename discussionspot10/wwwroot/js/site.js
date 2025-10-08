// DiscussionSpot - Modern JavaScript Functionality

// Theme Management
class ThemeManager {
    constructor() {
        this.theme = localStorage.getItem('theme') || 'light';
        this.init();
    }

    init() {
        this.applyTheme(this.theme);
        this.setupEventListeners();
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        this.updateThemeIcon(theme);
        this.theme = theme;
        localStorage.setItem('theme', theme);
    }

    toggleTheme() {
        const newTheme = this.theme === 'dark' ? 'light' : 'dark';
        this.applyTheme(newTheme);
    }

    updateThemeIcon(theme) {
        const themeToggle = document.getElementById('themeToggle');
        if (!themeToggle) return;

        const sunIcon = themeToggle.querySelector('.fa-sun');
        const moonIcon = themeToggle.querySelector('.fa-moon');

        if (theme === 'dark') {
            sunIcon?.classList.remove('d-none');
            moonIcon?.classList.add('d-none');
        } else {
            sunIcon?.classList.add('d-none');
            moonIcon?.classList.remove('d-none');
        }
    }

    setupEventListeners() {
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => this.toggleTheme());
        }
    }
}

// Search Enhancement
class SearchEnhancer {
    constructor() {
        this.searchInput = document.querySelector('.search-input');
        this.setupSearch();
    }

    setupSearch() {
        if (!this.searchInput) return;

        // Add search suggestions
        this.searchInput.addEventListener('input', this.debounce(this.handleSearchInput.bind(this), 300));
        
        // Add keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.key === 'k') {
                e.preventDefault();
                this.searchInput.focus();
            }
        });
    }

    handleSearchInput(e) {
        const query = e.target.value.trim();
        if (query.length > 2) {
            // Here you could implement search suggestions
            console.log('Search query:', query);
        }
    }

    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
}

// Notification System
class NotificationManager {
    constructor() {
        this.notifications = [];
        this.setupNotifications();
    }

    setupNotifications() {
        // Check for new notifications periodically
        setInterval(() => this.checkNotifications(), 30000); // Check every 30 seconds
    }

    checkNotifications() {
        // Simulate notification check
        const hasNewNotifications = Math.random() > 0.8; // 20% chance
        if (hasNewNotifications) {
            this.updateNotificationBadge();
        }
    }

    updateNotificationBadge() {
        const badge = document.querySelector('.badge');
        if (badge) {
            const currentCount = parseInt(badge.textContent) || 0;
            badge.textContent = currentCount + 1;
            badge.classList.add('pulse');
            
            // Remove pulse animation after 2 seconds
            setTimeout(() => {
                badge.classList.remove('pulse');
            }, 2000);
        }
    }
}

// Mobile Navigation Enhancement
class MobileNavigation {
    constructor() {
        this.navbarToggler = document.querySelector('.navbar-toggler');
        this.navbarCollapse = document.querySelector('.navbar-collapse');
        this.setupMobileNav();
    }

    setupMobileNav() {
        if (!this.navbarToggler || !this.navbarCollapse) return;

        // Close mobile menu when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.navbarCollapse.contains(e.target) && 
                !this.navbarToggler.contains(e.target) &&
                this.navbarCollapse.classList.contains('show')) {
                this.closeMobileMenu();
            }
        });

        // Close mobile menu when clicking on nav links
        const navLinks = this.navbarCollapse.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', () => this.closeMobileMenu());
        });
    }

    closeMobileMenu() {
        const bsCollapse = new bootstrap.Collapse(this.navbarCollapse, {
            toggle: false
        });
        bsCollapse.hide();
    }
}

// Smooth Scrolling
class SmoothScrolling {
    constructor() {
        this.setupSmoothScrolling();
    }

    setupSmoothScrolling() {
        // Add smooth scrolling to all anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                e.preventDefault();
                const target = document.querySelector(anchor.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }
}

// Loading States
class LoadingManager {
    constructor() {
        this.setupLoadingStates();
    }

    setupLoadingStates() {
        // Add loading states to forms
        document.querySelectorAll('form').forEach(form => {
            form.addEventListener('submit', (e) => {
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn) {
                    submitBtn.classList.add('loading');
                    submitBtn.disabled = true;
                }
            });
        });
    }
}

// Card Animations
class CardAnimations {
    constructor() {
        this.setupCardAnimations();
    }

    setupCardAnimations() {
        // Add hover effects to cards
        document.querySelectorAll('.card').forEach(card => {
            card.addEventListener('mouseenter', () => {
                card.style.transform = 'translateY(-2px)';
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = 'translateY(0)';
            });
        });
    }
}

// Initialize all components when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all managers
    new ThemeManager();
    new SearchEnhancer();
    new NotificationManager();
    new MobileNavigation();
    new SmoothScrolling();
    new LoadingManager();
    new CardAnimations();

    // Add fade-in animation to cards
    const cards = document.querySelectorAll('.card');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    cards.forEach(card => {
        observer.observe(card);
    });

    // Add pulse animation to notification badge
    const style = document.createElement('style');
    style.textContent = `
        .pulse {
            animation: pulse 1s ease-in-out;
        }
        
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.1); }
            100% { transform: scale(1); }
        }
    `;
    document.head.appendChild(style);
});

// Utility functions
const Utils = {
    // Debounce function
    debounce: function(func, wait, immediate) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func(...args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func(...args);
        };
    },

    // Throttle function
    throttle: function(func, limit) {
        let inThrottle;
        return function(...args) {
            if (!inThrottle) {
                func.apply(this, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    // Format date
    formatDate: function(date) {
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(new Date(date));
    }
};

// Export for use in other scripts
window.DiscussionSpot = {
    ThemeManager,
    SearchEnhancer,
    NotificationManager,
    MobileNavigation,
    SmoothScrolling,
    LoadingManager,
    CardAnimations,
    Utils
};