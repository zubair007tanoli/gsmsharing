// Dark Mode Toggle - Consolidated Script
// This script handles all dark mode functionality
(function() {
    let isInitialized = false;
    
    // Wait for DOM to be ready
    function init() {
        if (isInitialized) return; // Prevent double initialization
        isInitialized = true;
        
        const darkModeToggle = document.querySelector('.dark-mode-toggle');
        const html = document.documentElement;
        const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)");

        // Get current theme from html (already set by inline script)
        const currentTheme = html.getAttribute('data-theme') || 'light';
        
        // Sync body class with html attribute
        syncBodyClass(currentTheme);
        
        // Update toggle icon
        updateToggleIcon(currentTheme);

        // Add click event listener (only once)
        if (darkModeToggle) {
            // Remove any existing listeners by cloning the element
            const newToggle = darkModeToggle.cloneNode(true);
            darkModeToggle.parentNode.replaceChild(newToggle, darkModeToggle);
            
            // Add fresh event listener
            newToggle.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                
                const currentTheme = html.getAttribute('data-theme') || 'light';
                const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
                
                // Update theme
                html.setAttribute('data-theme', newTheme);
                localStorage.setItem('theme', newTheme);
                syncBodyClass(newTheme);
                updateToggleIcon(newTheme);
                
                // Add animation effect
                newToggle.style.transform = "scale(1.1)";
                setTimeout(() => {
                    newToggle.style.transform = "scale(1)";
                }, 200);
            });
        }

        // Listen for system preference changes (only if no saved preference)
        prefersDarkScheme.addEventListener("change", (e) => {
            if (!localStorage.getItem("theme")) {
                const newTheme = e.matches ? 'dark' : 'light';
                html.setAttribute('data-theme', newTheme);
                syncBodyClass(newTheme);
                updateToggleIcon(newTheme);
            }
        });
    }

    function syncBodyClass(theme) {
        if (theme === 'dark') {
            document.body.classList.add('dark-mode');
        } else {
            document.body.classList.remove('dark-mode');
        }
    }

    function updateToggleIcon(theme) {
        const darkModeToggle = document.querySelector('.dark-mode-toggle');
        if (darkModeToggle) {
            const moonIcon = darkModeToggle.querySelector('.fa-moon');
            const sunIcon = darkModeToggle.querySelector('.fa-sun');
            
            if (moonIcon && sunIcon) {
                if (theme === 'dark') {
                    moonIcon.style.display = 'none';
                    sunIcon.style.display = 'inline-block';
                    sunIcon.style.color = '#fbbf24';
                } else {
                    moonIcon.style.display = 'inline-block';
                    moonIcon.style.color = '#6366f1';
                    sunIcon.style.display = 'none';
                }
            }
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        // DOM already loaded
        init();
    }
})();

