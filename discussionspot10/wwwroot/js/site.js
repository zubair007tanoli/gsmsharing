// DiscussionSpot - Enhanced JavaScript functionality

// Theme Management
class ThemeManager {
    constructor() {
        this.themeToggle = document.getElementById('themeToggle');
        this.themeIcon = document.getElementById('themeIcon');
        this.html = document.documentElement;
        this.init();
    }

    init() {
        // Check for saved theme preference or default to light mode
        const currentTheme = localStorage.getItem('theme') || 'light';
        this.html.setAttribute('data-theme', currentTheme);
        this.updateThemeIcon(currentTheme);

        if (this.themeToggle) {
            this.themeToggle.addEventListener('click', () => this.toggleTheme());
        }
    }

    toggleTheme() {
        const currentTheme = this.html.getAttribute('data-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        
        this.html.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        this.updateThemeIcon(newTheme);
        
        // Dispatch custom event for other components
        window.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme: newTheme } }));
    }

    updateThemeIcon(theme) {
        if (this.themeIcon) {
            if (theme === 'light') {
                this.themeIcon.className = 'fas fa-moon';
                this.themeToggle.title = 'Switch to dark mode';
            } else {
                this.themeIcon.className = 'fas fa-sun';
                this.themeToggle.title = 'Switch to light mode';
            }
        }
    }
}

// Search Enhancement
class SearchEnhancement {
    constructor() {
        this.searchInputs = document.querySelectorAll('.search-input');
        this.init();
    }

    init() {
        this.searchInputs.forEach(input => {
            // Add search suggestions (placeholder for future implementation)
            input.addEventListener('focus', () => this.showSuggestions(input));
            input.addEventListener('blur', () => this.hideSuggestions(input));
            input.addEventListener('input', () => this.handleSearchInput(input));
        });
    }

    showSuggestions(input) {
        // Placeholder for search suggestions
        console.log('Show search suggestions');
    }

    hideSuggestions(input) {
        // Placeholder for hiding suggestions
        console.log('Hide search suggestions');
    }

    handleSearchInput(input) {
        // Placeholder for search input handling
        const query = input.value.trim();
        if (query.length > 2) {
            // Could implement live search here
            console.log('Search query:', query);
        }
    }
}

// Notification System
class NotificationSystem {
    constructor() {
        this.notificationBell = document.querySelector('[title="Notifications"]');
        this.init();
    }

    init() {
        if (this.notificationBell) {
            this.notificationBell.addEventListener('click', (e) => {
                // Add visual feedback
                this.notificationBell.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    this.notificationBell.style.transform = 'scale(1)';
                }, 150);
            });
        }
    }

    showNotification(message, type = 'info') {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 5000);
    }
}

// Mobile Menu Enhancement
class MobileMenuEnhancement {
    constructor() {
        this.navbarToggler = document.querySelector('.navbar-toggler');
        this.navbarCollapse = document.querySelector('.navbar-collapse');
        this.init();
    }

    init() {
        if (this.navbarToggler && this.navbarCollapse) {
            this.navbarToggler.addEventListener('click', () => {
                // Add smooth animation
                this.navbarCollapse.style.transition = 'all 0.3s ease';
            });

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
    }

    closeMobileMenu() {
        if (this.navbarCollapse.classList.contains('show')) {
            this.navbarToggler.click();
        }
    }
}

// Smooth Scrolling
class SmoothScrolling {
    constructor() {
        this.init();
    }

    init() {
        // Smooth scroll for anchor links
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

// Lazy Loading for Images
class LazyLoading {
    constructor() {
        this.images = document.querySelectorAll('img[data-src]');
        this.init();
    }

    init() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        imageObserver.unobserve(img);
                    }
                });
            });

            this.images.forEach(img => imageObserver.observe(img));
        } else {
            // Fallback for older browsers
            this.images.forEach(img => {
                img.src = img.dataset.src;
                img.classList.remove('lazy');
            });
        }
    }
}

// Performance Monitoring
class PerformanceMonitor {
    constructor() {
        this.init();
    }

    init() {
        // Monitor page load performance
        window.addEventListener('load', () => {
            if ('performance' in window) {
                const perfData = performance.getEntriesByType('navigation')[0];
                console.log('Page load time:', perfData.loadEventEnd - perfData.loadEventStart, 'ms');
            }
        });

        // Monitor scroll performance
        let scrollTimeout;
        window.addEventListener('scroll', () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                // Throttled scroll handling
                this.handleScroll();
            }, 100);
        });
    }

    handleScroll() {
        // Add scroll-based animations or effects here
        const scrollTop = window.pageYOffset;
        const navbar = document.querySelector('.navbar');
        
        if (navbar) {
            if (scrollTop > 100) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        }
    }
}

// Initialize all components when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all components
    new ThemeManager();
    new SearchEnhancement();
    new NotificationSystem();
    new MobileMenuEnhancement();
    new SmoothScrolling();
    new LazyLoading();
    new PerformanceMonitor();

    // Initialize Google AdSense
    if (typeof adsbygoogle !== 'undefined') {
        (adsbygoogle = window.adsbygoogle || []).push({});
    }

    // Add loading complete class
    document.body.classList.add('loaded');
    
    console.log('DiscussionSpot enhanced functionality loaded successfully!');
});

// Utility functions
window.DiscussionSpot = {
    // Show notification
    notify: function(message, type = 'info') {
        const notificationSystem = new NotificationSystem();
        notificationSystem.showNotification(message, type);
    },

    // Toggle theme programmatically
    toggleTheme: function() {
        const themeManager = new ThemeManager();
        themeManager.toggleTheme();
    },

    // Smooth scroll to element
    scrollTo: function(selector) {
        const element = document.querySelector(selector);
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    }
};