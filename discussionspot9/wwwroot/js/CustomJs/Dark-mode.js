// Dark Mode Toggle Functionality

document.addEventListener("DOMContentLoaded", () => {
    // Check for saved theme preference or use system preference
    const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)");
    const savedTheme = localStorage.getItem("theme");

    // Apply dark mode if saved or system preference is dark
    if (savedTheme === "dark" || (!savedTheme && prefersDarkScheme.matches)) {
        document.body.classList.add("dark-mode");
        updateDarkModeToggle(true);
    } else {
        document.body.classList.remove("dark-mode");
        updateDarkModeToggle(false);
    }

    // Add event listener to dark mode toggle
    const darkModeToggle = document.querySelector(".dark-mode-toggle");
    if (darkModeToggle) {
        darkModeToggle.addEventListener("click", toggleDarkMode);
    } else {
        console.error("Dark mode toggle button not found!");
    }

    // Listen for system preference changes
    prefersDarkScheme.addEventListener("change", (e) => {
        if (!localStorage.getItem("theme")) {
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
    document.body.classList.toggle("dark-mode");
    const isDarkMode = document.body.classList.contains("dark-mode");

    // Save preference to localStorage
    localStorage.setItem("theme", isDarkMode ? "dark" : "light");

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