/* ===== MODERN DISCUSSIONSPOT JAVASCRIPT ===== */

// Theme Management
class ThemeManager {
    constructor() {
        this.theme = localStorage.getItem('theme') || 
                    (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
        this.init();
    }

    init() {
        this.applyTheme(this.theme);
        this.setupThemeToggle();
        this.setupSystemThemeListener();
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);
        this.updateThemeIcons(theme);
        this.updateMetaThemeColor(theme);
        this.theme = theme;
    }

    updateThemeIcons(theme) {
        const icons = document.querySelectorAll('#themeIcon, #themeIconDesktop');
        icons.forEach(icon => {
            if (theme === 'dark') {
                icon.className = 'fas fa-sun';
            } else {
                icon.className = 'fas fa-moon';
            }
        });
    }

    updateMetaThemeColor(theme) {
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', theme === 'dark' ? '#1a1a1b' : '#ffffff');
        }
    }

    toggleTheme() {
        const newTheme = this.theme === 'light' ? 'dark' : 'light';
        this.applyTheme(newTheme);
        
        // Add a subtle animation effect
        document.body.style.transition = 'background-color 0.3s ease, color 0.3s ease';
        setTimeout(() => {
            document.body.style.transition = '';
        }, 300);
    }

    setupThemeToggle() {
        const toggleButtons = document.querySelectorAll('#themeToggle, #themeToggleDesktop');
        toggleButtons.forEach(button => {
            button.addEventListener('click', () => this.toggleTheme());
        });
    }

    setupSystemThemeListener() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        mediaQuery.addEventListener('change', (e) => {
            if (!localStorage.getItem('theme')) {
                this.applyTheme(e.matches ? 'dark' : 'light');
            }
        });
    }
}

// Navbar Enhancement
class NavbarManager {
    constructor() {
        this.navbar = document.getElementById('mainNavbar');
        this.lastScrollTop = 0;
        this.scrollThreshold = 10;
        this.init();
    }

    init() {
        this.setupScrollBehavior();
        this.setupMobileSearch();
        this.setupNotificationBadges();
        this.setupUserDropdown();
    }

    setupScrollBehavior() {
        let ticking = false;
        
        window.addEventListener('scroll', () => {
            if (!ticking) {
                requestAnimationFrame(() => {
                    this.handleScroll();
                    ticking = false;
                });
                ticking = true;
            }
        });
    }

    handleScroll() {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        
        // Add/remove shadow based on scroll position
        if (scrollTop > 10) {
            this.navbar.style.boxShadow = '0 2px 20px rgba(0, 0, 0, 0.1)';
        } else {
            this.navbar.style.boxShadow = '';
        }

        // Auto-hide navbar on mobile when scrolling down
        if (window.innerWidth <= 768) {
            if (scrollTop > this.lastScrollTop && scrollTop > 100) {
                // Scrolling down
                this.navbar.style.transform = 'translateY(-100%)';
            } else {
                // Scrolling up
                this.navbar.style.transform = 'translateY(0)';
            }
        }

        this.lastScrollTop = scrollTop;
    }

    setupMobileSearch() {
        const mobileSearchBtn = document.querySelector('.mobile-search-btn');
        const mobileSearchCollapse = document.getElementById('mobileSearch');
        
        if (mobileSearchBtn && mobileSearchCollapse) {
            mobileSearchBtn.addEventListener('click', () => {
                const isExpanded = mobileSearchCollapse.classList.contains('show');
                if (!isExpanded) {
                    // Focus on search input when opened
                    setTimeout(() => {
                        const searchInput = mobileSearchCollapse.querySelector('.search-input');
                        if (searchInput) searchInput.focus();
                    }, 300);
                }
            });
        }
    }

    setupNotificationBadges() {
        // Simulate real-time notification updates
        const badges = document.querySelectorAll('.notification-badge');
        badges.forEach(badge => {
            badge.addEventListener('click', () => {
                // Add a subtle click animation
                badge.style.transform = 'scale(0.8)';
                setTimeout(() => {
                    badge.style.transform = 'scale(1)';
                }, 150);
            });
        });
    }

    setupUserDropdown() {
        const userDropdown = document.querySelector('.user-menu-toggle');
        if (userDropdown) {
            userDropdown.addEventListener('click', (e) => {
                e.preventDefault();
                // Add custom dropdown animation
                const dropdown = userDropdown.nextElementSibling;
                if (dropdown) {
                    dropdown.style.opacity = '0';
                    dropdown.style.transform = 'translateY(-10px)';
                    setTimeout(() => {
                        dropdown.style.opacity = '1';
                        dropdown.style.transform = 'translateY(0)';
                    }, 10);
                }
            });
        }
    }
}

// Search Enhancement
class SearchManager {
    constructor() {
        this.searchInputs = document.querySelectorAll('.search-input');
        this.searchHistory = JSON.parse(localStorage.getItem('searchHistory') || '[]');
        this.init();
    }

    init() {
        this.setupSearchInputs();
        this.setupSearchSuggestions();
    }

    setupSearchInputs() {
        this.searchInputs.forEach(input => {
            input.addEventListener('focus', () => this.showSearchSuggestions(input));
            input.addEventListener('blur', () => this.hideSearchSuggestions(input));
            input.addEventListener('input', (e) => this.handleSearchInput(e));
            input.addEventListener('keydown', (e) => this.handleSearchKeydown(e));
        });
    }

    setupSearchSuggestions() {
        // Create suggestion dropdown for each search input
        this.searchInputs.forEach(input => {
            const suggestionDropdown = document.createElement('div');
            suggestionDropdown.className = 'search-suggestions';
            suggestionDropdown.style.cssText = `
                position: absolute;
                top: 100%;
                left: 0;
                right: 0;
                background: var(--card-bg);
                border: 1px solid var(--border-color);
                border-radius: 8px;
                box-shadow: var(--shadow-lg);
                z-index: 1000;
                display: none;
                max-height: 300px;
                overflow-y: auto;
            `;
            input.parentElement.appendChild(suggestionDropdown);
        });
    }

    showSearchSuggestions(input) {
        const dropdown = input.parentElement.querySelector('.search-suggestions');
        if (dropdown && this.searchHistory.length > 0) {
            this.populateSuggestions(dropdown);
            dropdown.style.display = 'block';
        }
    }

    hideSearchSuggestions(input) {
        setTimeout(() => {
            const dropdown = input.parentElement.querySelector('.search-suggestions');
            if (dropdown) {
                dropdown.style.display = 'none';
            }
        }, 200);
    }

    populateSuggestions(dropdown) {
        const recentSearches = this.searchHistory.slice(-5).reverse();
        dropdown.innerHTML = `
            <div class="suggestion-header">Recent Searches</div>
            ${recentSearches.map(search => `
                <div class="suggestion-item" data-search="${search}">
                    <i class="fas fa-history"></i>
                    <span>${search}</span>
                </div>
            `).join('')}
        `;

        // Add click handlers
        dropdown.querySelectorAll('.suggestion-item').forEach(item => {
            item.addEventListener('click', () => {
                const searchTerm = item.dataset.search;
                const input = dropdown.parentElement.querySelector('.search-input');
                input.value = searchTerm;
                input.form.submit();
            });
        });
    }

    handleSearchInput(e) {
        const query = e.target.value.trim();
        if (query.length > 2) {
            // Here you could implement real-time search suggestions
            this.debounce(() => {
                console.log('Searching for:', query);
            }, 300)();
        }
    }

    handleSearchKeydown(e) {
        if (e.key === 'Enter') {
            const query = e.target.value.trim();
            if (query && !this.searchHistory.includes(query)) {
                this.searchHistory.push(query);
                if (this.searchHistory.length > 10) {
                    this.searchHistory.shift();
                }
                localStorage.setItem('searchHistory', JSON.stringify(this.searchHistory));
            }
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

// Sidebar Enhancement
class SidebarManager {
    constructor() {
        this.sidebars = document.querySelectorAll('.sticky-sidebar');
        this.init();
    }

    init() {
        this.setupSmoothScrolling();
        this.setupCommunityJoinButtons();
        this.setupCollapsibleSections();
    }

    setupSmoothScrolling() {
        // Smooth scrolling for sidebar links
        document.querySelectorAll('.nav-item, .sidebar-card a').forEach(link => {
            link.addEventListener('click', (e) => {
                const href = link.getAttribute('href');
                if (href && href.startsWith('#')) {
                    e.preventDefault();
                    const target = document.querySelector(href);
                    if (target) {
                        target.scrollIntoView({ behavior: 'smooth' });
                    }
                }
            });
        });
    }

    setupCommunityJoinButtons() {
        document.querySelectorAll('.community-item button').forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                const isJoined = button.textContent.trim() === 'Joined';
                
                if (isJoined) {
                    button.textContent = 'Join';
                    button.className = 'btn btn-sm btn-outline-primary';
                } else {
                    button.textContent = 'Joined';
                    button.className = 'btn btn-sm btn-success';
                    
                    // Add success animation
                    button.style.transform = 'scale(0.95)';
                    setTimeout(() => {
                        button.style.transform = 'scale(1)';
                    }, 150);
                }
            });
        });
    }

    setupCollapsibleSections() {
        // Make sidebar sections collapsible on mobile
        if (window.innerWidth <= 768) {
            document.querySelectorAll('.sidebar-header').forEach(header => {
                header.style.cursor = 'pointer';
                header.addEventListener('click', () => {
                    const content = header.nextElementSibling;
                    const isCollapsed = content.style.display === 'none';
                    
                    content.style.display = isCollapsed ? 'block' : 'none';
                    
                    // Add arrow indicator
                    const arrow = header.querySelector('.collapse-arrow') || 
                                 document.createElement('i');
                    arrow.className = `fas fa-chevron-${isCollapsed ? 'up' : 'down'} collapse-arrow ms-auto`;
                    if (!header.querySelector('.collapse-arrow')) {
                        header.appendChild(arrow);
                    }
                });
            });
        }
    }
}

// Performance Optimization
class PerformanceManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupLazyLoading();
        this.setupImageOptimization();
        this.setupServiceWorker();
    }

    setupLazyLoading() {
        // Lazy load images and ads
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const element = entry.target;
                    
                    if (element.tagName === 'IMG') {
                        const src = element.dataset.src;
                        if (src) {
                            element.src = src;
                            element.removeAttribute('data-src');
                        }
                    }
                    
                    if (element.classList.contains('adsbygoogle')) {
                        // Trigger AdSense ad loading
                        if (window.adsbygoogle) {
                            window.adsbygoogle.push({});
                        }
                    }
                    
                    observer.unobserve(element);
                }
            });
        });

        // Observe images with data-src attribute
        document.querySelectorAll('img[data-src]').forEach(img => {
            observer.observe(img);
        });

        // Observe AdSense containers
        document.querySelectorAll('.adsbygoogle').forEach(ad => {
            observer.observe(ad);
        });
    }

    setupImageOptimization() {
        // Add loading="lazy" to images
        document.querySelectorAll('img').forEach(img => {
            if (!img.hasAttribute('loading')) {
                img.setAttribute('loading', 'lazy');
            }
        });
    }

    setupServiceWorker() {
        // Register service worker for caching
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js')
                .then(registration => {
                    console.log('SW registered: ', registration);
                })
                .catch(registrationError => {
                    console.log('SW registration failed: ', registrationError);
                });
        }
    }
}

// Accessibility Enhancement
class AccessibilityManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupKeyboardNavigation();
        this.setupAriaLabels();
        this.setupFocusManagement();
        this.setupReducedMotion();
    }

    setupKeyboardNavigation() {
        // Add keyboard navigation for custom elements
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                // Close any open dropdowns or modals
                document.querySelectorAll('.dropdown-menu.show').forEach(dropdown => {
                    dropdown.classList.remove('show');
                });
            }
        });
    }

    setupAriaLabels() {
        // Add missing aria labels
        document.querySelectorAll('.icon-btn').forEach(button => {
            if (!button.hasAttribute('aria-label') && !button.hasAttribute('title')) {
                const icon = button.querySelector('i');
                if (icon) {
                    const iconClass = icon.className;
                    let label = 'Button';
                    
                    if (iconClass.includes('bell')) label = 'Notifications';
                    else if (iconClass.includes('envelope')) label = 'Messages';
                    else if (iconClass.includes('search')) label = 'Search';
                    else if (iconClass.includes('moon') || iconClass.includes('sun')) label = 'Toggle theme';
                    
                    button.setAttribute('aria-label', label);
                }
            }
        });
    }

    setupFocusManagement() {
        // Improve focus visibility
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Tab') {
                document.body.classList.add('keyboard-navigation');
            }
        });

        document.addEventListener('mousedown', () => {
            document.body.classList.remove('keyboard-navigation');
        });
    }

    setupReducedMotion() {
        // Respect user's motion preferences
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            document.documentElement.style.setProperty('--transition-fast', '0.01ms');
            document.documentElement.style.setProperty('--transition-normal', '0.01ms');
            document.documentElement.style.setProperty('--transition-slow', '0.01ms');
        }
    }
}

// Analytics and Tracking
class AnalyticsManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupEventTracking();
        this.setupPerformanceTracking();
    }

    setupEventTracking() {
        // Track user interactions
        document.addEventListener('click', (e) => {
            const element = e.target.closest('[data-track]');
            if (element) {
                const action = element.dataset.track;
                this.trackEvent('click', action, {
                    element: element.tagName,
                    text: element.textContent?.substring(0, 50)
                });
            }
        });

        // Track theme changes
        document.addEventListener('themeChanged', (e) => {
            this.trackEvent('theme_change', e.detail.theme);
        });
    }

    setupPerformanceTracking() {
        // Track page load performance
        window.addEventListener('load', () => {
            setTimeout(() => {
                const perfData = performance.getEntriesByType('navigation')[0];
                this.trackEvent('performance', 'page_load', {
                    loadTime: perfData.loadEventEnd - perfData.loadEventStart,
                    domContentLoaded: perfData.domContentLoadedEventEnd - perfData.domContentLoadedEventStart
                });
            }, 0);
        });
    }

    trackEvent(category, action, data = {}) {
        // Send to analytics service (Google Analytics, etc.)
        if (typeof gtag !== 'undefined') {
            gtag('event', action, {
                event_category: category,
                ...data
            });
        }
        
        console.log('Analytics Event:', { category, action, data });
    }
}

// Error Handling
class ErrorManager {
    constructor() {
        this.init();
    }

    init() {
        this.setupGlobalErrorHandler();
        this.setupUnhandledRejectionHandler();
    }

    setupGlobalErrorHandler() {
        window.addEventListener('error', (e) => {
            console.error('Global error:', e.error);
            this.reportError(e.error, 'javascript_error');
        });
    }

    setupUnhandledRejectionHandler() {
        window.addEventListener('unhandledrejection', (e) => {
            console.error('Unhandled promise rejection:', e.reason);
            this.reportError(e.reason, 'promise_rejection');
        });
    }

    reportError(error, type) {
        // Report to error tracking service
        console.log('Error reported:', { error, type });
    }
}

// Initialize all managers when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Initialize all components
    new ThemeManager();
    new NavbarManager();
    new SearchManager();
    new SidebarManager();
    new PerformanceManager();
    new AccessibilityManager();
    new AnalyticsManager();
    new ErrorManager();

    // Add custom CSS for keyboard navigation
    const style = document.createElement('style');
    style.textContent = `
        .keyboard-navigation *:focus {
            outline: 2px solid var(--primary-color) !important;
            outline-offset: 2px !important;
        }
        
        .search-suggestions {
            background: var(--card-bg);
            border: 1px solid var(--border-color);
            border-radius: 8px;
            box-shadow: var(--shadow-lg);
        }
        
        .suggestion-header {
            padding: 0.75rem 1rem;
            font-weight: 600;
            font-size: 0.8rem;
            color: var(--text-muted);
            text-transform: uppercase;
            letter-spacing: 0.5px;
            border-bottom: 1px solid var(--border-color);
        }
        
        .suggestion-item {
            padding: 0.75rem 1rem;
            display: flex;
            align-items: center;
            gap: 0.75rem;
            cursor: pointer;
            transition: background-color var(--transition-fast);
        }
        
        .suggestion-item:hover {
            background-color: var(--hover-bg);
        }
        
        .suggestion-item i {
            color: var(--text-muted);
            width: 16px;
        }
    `;
    document.head.appendChild(style);

    console.log('DiscussionSpot modern theme initialized successfully!');
});

// Export for potential module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        ThemeManager,
        NavbarManager,
        SearchManager,
        SidebarManager,
        PerformanceManager,
        AccessibilityManager,
        AnalyticsManager,
        ErrorManager
    };
}