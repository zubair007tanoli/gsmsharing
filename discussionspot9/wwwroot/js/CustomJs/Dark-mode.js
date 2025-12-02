// Dark Mode Toggle Functionality
// This script works with the inline script in _Layout.cshtml
// The theme is already applied by the inline script, so we just sync and add event listeners

document.addEventListener("DOMContentLoaded", () => {
    const html = document.documentElement;
    const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)");
    
    // Get current theme from html (already set by inline script)
    const currentTheme = html.getAttribute('data-theme') || 'light';
    const isDarkMode = currentTheme === 'dark';
    
    // Sync body class with html attribute (in case it wasn't synced)
    if (isDarkMode) {
        document.body.classList.add("dark-mode");
    } else {
        document.body.classList.remove("dark-mode");
    }
    
    // Update toggle icon
    updateDarkModeToggle(isDarkMode);

    // Add event listener to dark mode toggle
    const darkModeToggle = document.querySelector(".dark-mode-toggle");
    if (darkModeToggle) {
        darkModeToggle.addEventListener("click", toggleDarkMode);
    }

    // Listen for system preference changes (only if no saved preference)
    prefersDarkScheme.addEventListener("change", (e) => {
        if (!localStorage.getItem("theme")) {
            const newTheme = e.matches ? 'dark' : 'light';
            html.setAttribute('data-theme', newTheme);
            
            if (e.matches) {
                document.body.classList.add("dark-mode");
                updateDarkModeToggle(true);
            } else {
                document.body.classList.remove("dark-mode");
                updateDarkModeToggle(false);
            }
        }
    });
});

// Toggle dark mode
function toggleDarkMode() {
    const html = document.documentElement;
    const currentTheme = html.getAttribute('data-theme') || 'light';
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    
    // Update both html attribute and body class
    html.setAttribute('data-theme', newTheme);
    const isDarkMode = newTheme === 'dark';
    
    if (isDarkMode) {
        document.body.classList.add("dark-mode");
    } else {
        document.body.classList.remove("dark-mode");
    }

    // Save preference to localStorage
    localStorage.setItem("theme", newTheme);

    // Update toggle icon
    updateDarkModeToggle(isDarkMode);

    // Add animation effect
    const toggle = document.querySelector(".dark-mode-toggle");
    if (toggle) {
        toggle.style.transform = "scale(1.1)";
        setTimeout(() => {
            toggle.style.transform = "scale(1)";
        }, 200);
    }
}

// Update dark mode toggle icon
function updateDarkModeToggle(isDarkMode) {
    const moonIcon = document.querySelector(".dark-mode-toggle .fa-moon");
    const sunIcon = document.querySelector(".dark-mode-toggle .fa-sun");

    if (moonIcon && sunIcon) {
        if (isDarkMode) {
            moonIcon.style.display = "none";
            sunIcon.style.display = "inline-block";
            sunIcon.style.color = "#fbbf24"; // Amber color for sun
        } else {
            moonIcon.style.display = "inline-block";
            moonIcon.style.color = "#6366f1"; // Indigo color for moon
            sunIcon.style.display = "none";
        }
    }
}