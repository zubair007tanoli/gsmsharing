// View Post Page JavaScript - Reddit Style

document.addEventListener("DOMContentLoaded", () => {
    // Initialize bootstrap components
    const bootstrap = window.bootstrap

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl))

    // Handle voting buttons
    initVotingButtons()

    // Handle reply buttons
    initReplyButtons()

    // Handle load more comments button
    initLoadMoreComments()

    // Handle post actions
    initPostActions()

    // Handle share buttons
    initShareButtons()
})

// Initialize voting buttons - Fixed for proper icon handling
function initVotingButtons() {
    const voteButtons = document.querySelectorAll(".vote-btn")

    voteButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const isLike = this.classList.contains("upvote")
            const parentElement = this.closest(".post-voting") || this.closest(".comment-voting")
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

// Initialize reply buttons
function initReplyButtons() {
    const replyButtons = document.querySelectorAll(".reply-btn")
    const cancelReplyButtons = document.querySelectorAll(".cancel-reply-btn")

    // Show reply form when reply button is clicked
    replyButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const commentId = this.getAttribute("data-comment-id")
            const replyForm = document.getElementById(`reply-form-${commentId}`)

            // Hide all other reply forms first
            document.querySelectorAll(".reply-form").forEach((form) => {
                if (form.id !== `reply-form-${commentId}`) {
                    form.style.display = "none"
                }
            })

            // Toggle this reply form
            if (replyForm.style.display === "none") {
                replyForm.style.display = "block"
                replyForm.querySelector("textarea").focus()
            } else {
                replyForm.style.display = "none"
            }
        })
    })

    // Hide reply form when cancel button is clicked
    cancelReplyButtons.forEach((button) => {
        button.addEventListener("click", function () {
            const commentId = this.getAttribute("data-comment-id")
            const replyForm = document.getElementById(`reply-form-${commentId}`)
            replyForm.style.display = "none"
        })
    })

    // Handle reply form submissions
    document.querySelectorAll(".reply-form form").forEach((form) => {
        form.addEventListener("submit", function (e) {
            e.preventDefault()

            const textarea = this.querySelector("textarea")
            const replyText = textarea.value.trim()

            if (replyText) {
                // In a real application, you would send this to the server
                // For demo purposes, we'll just show a success message
                textarea.value = ""

                // Hide the reply form
                this.closest(".reply-form").style.display = "none"

                // Show success message
                showToast("Reply submitted successfully!")
            }
        })
    })
}

// Initialize load more comments button
function initLoadMoreComments() {
    const loadMoreButton = document.getElementById("loadMoreComments")

    if (loadMoreButton) {
        loadMoreButton.addEventListener("click", function () {
            // In a real application, you would fetch more comments from the server
            // For demo purposes, we'll just show a message
            this.innerHTML =
                '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...'
            this.disabled = true

            setTimeout(() => {
                this.innerHTML = "No More Comments"
                showToast("All comments have been loaded")
            }, 1500)
        })
    }
}

// Initialize post actions
function initPostActions() {
    // Handle main comment form submission
    const mainCommentForm = document.getElementById("mainCommentForm")

    if (mainCommentForm) {
        mainCommentForm.addEventListener("submit", function (e) {
            e.preventDefault()

            const textarea = this.querySelector("textarea")
            const commentText = textarea.value.trim()

            if (commentText) {
                // In a real application, you would send this to the server
                // For demo purposes, we'll just show a success message
                textarea.value = ""

                // Show success message
                showToast("Comment submitted successfully!")
            }
        })
    }

    // Handle bookmark button
    const bookmarkButton = document.querySelector(".btn-action i.fa-bookmark")?.closest("button")
    if (bookmarkButton) {
        bookmarkButton.addEventListener("click", function () {
            const icon = this.querySelector("i")

            if (icon.classList.contains("far")) {
                // Bookmark
                icon.classList.remove("far")
                icon.classList.add("fas")
                this.classList.add("active")
                showToast("Post bookmarked!")
            } else {
                // Unbookmark
                icon.classList.remove("fas")
                icon.classList.add("far")
                this.classList.remove("active")
                showToast("Bookmark removed")
            }
        })
    }
}

// Initialize share buttons
function initShareButtons() {
    // Handle copy link button
    const copyLinkButton = document.getElementById("copyLinkBtn")
    if (copyLinkButton) {
        copyLinkButton.addEventListener("click", () => {
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
    }

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
            }
        })
    })
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
