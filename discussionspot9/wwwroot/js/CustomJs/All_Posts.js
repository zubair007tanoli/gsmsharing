// All Posts Page JavaScript

document.addEventListener("DOMContentLoaded", () => {
    // Initialize bootstrap components
    const bootstrap = window.bootstrap

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl))

    // Handle view toggle (card vs compact)
    initViewToggle()

    // Handle voting buttons
    initVotingButtons()

    // Handle share buttons
    initShareButtons()

    // Handle filters
    initFilters()

    // Initialize dark mode toggle
    initDarkModeToggle()

    // Initialize nested categories toggle
    initNestedCategories()
})

// Initialize view toggle
function initViewToggle() {
    const viewButtons = document.querySelectorAll('.btn-group[role="group"] .btn')
    const postsFeed = document.querySelector(".posts-feed")

    viewButtons.forEach((button) => {
        button.addEventListener("click", function () {
            // Remove active class from all buttons
            viewButtons.forEach((btn) => btn.classList.remove("active"))

            // Add active class to clicked button
            this.classList.add("active")

            // Toggle compact view based on which button was clicked
            if (this.querySelector(".fa-list")) {
                postsFeed.classList.add("compact-view")
            } else {
                postsFeed.classList.remove("compact-view")
            }
        })
    })
}

// Initialize voting buttons
function initVotingButtons() {
    const voteButtons = document.querySelectorAll(".vote-btn")

    voteButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const isLike = this.classList.contains("upvote")
            const parentElement = this.closest(".post-votes")
            const countElement = parentElement.querySelector(".vote-count")
            const count = Number.parseInt(countElement.textContent)

            const likeButton = parentElement.querySelector(".upvote")
            const dislikeButton = parentElement.querySelector(".downvote")

            if (isLike) {
                if (this.classList.contains("active")) {
                    // Remove like
                    this.classList.remove("active")
                    countElement.textContent = count - 1
                } else {
                    // Add like
                    this.classList.add("active")
                    if (dislikeButton.classList.contains("active")) {
                        dislikeButton.classList.remove("active")
                        countElement.textContent = count + 2
                    } else {
                        countElement.textContent = count + 1
                    }
                }
            } else {
                if (this.classList.contains("active")) {
                    // Remove dislike
                    this.classList.remove("active")
                    countElement.textContent = count + 1
                } else {
                    // Add dislike
                    this.classList.add("active")
                    if (likeButton.classList.contains("active")) {
                        likeButton.classList.remove("active")
                        countElement.textContent = count - 2
                    } else {
                        countElement.textContent = count - 1
                    }
                }
            }
        })
    })
}

// Initialize share buttons
function initShareButtons() {
    // Handle copy link buttons
    const copyLinkButtons = document.querySelectorAll(".copy-link-btn")
    copyLinkButtons.forEach((button) => {
        button.addEventListener("click", () => {
            // Create a temporary input to copy the URL
            const tempInput = document.createElement("input")
            tempInput.value = window.location.href
            document.body.appendChild(tempInput)
            tempInput.select()
            document.execCommand("copy")
            document.body.removeChild(tempInput)

            // Show toast
            showToast("Link copied to clipboard!")
        })
    })

    // Handle social share buttons
    const socialShareButtons = document.querySelectorAll(".social-share-btn")
    socialShareButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const platform = this.getAttribute("data-platform")
            const url = encodeURIComponent(window.location.href)
            const title = encodeURIComponent(document.title)
            let shareUrl = ""

            switch (platform) {
                case "facebook":
                    shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${url}`
                    break
                case "twitter":
                    shareUrl = `https://twitter.com/intent/tweet?url=${url}&text=${title}`
                    break
                case "linkedin":
                    shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${url}`
                    break
                case "reddit":
                    shareUrl = `https://www.reddit.com/submit?url=${url}&title=${title}`
                    break
                case "email":
                    shareUrl = `mailto:?subject=${title}&body=${url}`
                    break
            }

            if (shareUrl) {
                window.open(shareUrl, "_blank", "width=600,height=400")

                // Increment share count (in a real app, this would be an API call)
                const shareBtn = this.closest(".share-dropdown").querySelector(".share-btn")
                const shareCount = Number.parseInt(shareBtn.textContent.trim().split(" ")[1])
                shareBtn.innerHTML = `<i class="fas fa-share-alt me-1" aria-hidden="true"></i> ${shareCount + 1}`

                showToast(`Shared on ${platform}!`)
            }
        })
    })
}

// Initialize filters
function initFilters() {
    const filterForm = document.querySelector(".card-body button.btn-outline-secondary")
    if (filterForm) {
        filterForm.addEventListener("click", (e) => {
            e.preventDefault()

            // In a real application, you would apply the filters here
            // For demo purposes, we'll just show a message
            showToast("Filters applied!")
        })
    }

    // Time period filter
    const timeFilter = document.getElementById("timeFilter")
    if (timeFilter) {
        timeFilter.addEventListener("change", function () {
            // In a real application, you would apply the time filter here
            showToast(`Showing posts from: ${this.options[this.selectedIndex].text}`)
        })
    }
}

// Initialize dark mode toggle
function initDarkModeToggle() {
    const darkModeToggle = document.querySelector(".dark-mode-toggle")

    if (darkModeToggle) {
        darkModeToggle.addEventListener("click", () => {
            document.body.classList.toggle("dark-mode")

            // Save preference to localStorage
            if (document.body.classList.contains("dark-mode")) {
                localStorage.setItem("darkMode", "enabled")
            } else {
                localStorage.setItem("darkMode", "disabled")
            }
        })

        // Check for saved preference
        if (localStorage.getItem("darkMode") === "enabled") {
            document.body.classList.add("dark-mode")
        }
    }
}

// Initialize nested categories toggle
function initNestedCategories() {
    const categoryToggles = document.querySelectorAll(".category-toggle")

    categoryToggles.forEach((toggle) => {
        toggle.addEventListener("click", function (e) {
            e.preventDefault()
            e.stopPropagation()

            const parent = this.closest(".category-parent")
            const children = parent.nextElementSibling

            // Toggle the active class
            this.classList.toggle("active")
            children.classList.toggle("active")

            // Change the icon direction
            if (this.classList.contains("active")) {
                this.style.transform = "rotate(180deg)"
            } else {
                this.style.transform = "rotate(0deg)"
            }
        })
    })

    // Open the first category by default
    if (categoryToggles.length > 0) {
        categoryToggles[0].click()
    }
}

// Show toast message
function showToast(message) {
    // Create toast container if it doesn't exist
    let toastContainer = document.querySelector(".toast-container")

    if (!toastContainer) {
        toastContainer = document.createElement("div")
        toastContainer.className = "toast-container position-fixed bottom-0 end-0 p-3"
        document.body.appendChild(toastContainer)
    }

    // Create toast
    const toastId = "toast-" + Date.now()
    const toastHTML = `
      <div id="${toastId}" class="toast align-items-center text-white bg-primary border-0" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
          <div class="toast-body">
            ${message}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
      </div>
    `

    toastContainer.insertAdjacentHTML("beforeend", toastHTML)

    // Initialize and show toast
    const toastElement = document.getElementById(toastId)
    const bootstrap = window.bootstrap
    const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 3000 })
    toast.show()

    // Remove toast after it's hidden
    toastElement.addEventListener("hidden.bs.toast", function () {
        this.remove()
    })
}
