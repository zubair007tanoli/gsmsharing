/*!
 * Poll Vote Handler
 * Clean, focused handler for poll voting via SignalR
 * Works with PostHub.CastPollVote method
 */

const signalR = window.signalR // Declare the signalR variable

class PollVoteHandler {
    constructor() {
        this.signalRConnection = null
        this.postId = null
        this.isInitialized = false
        this.boundButtons = new Set()

        // Bind methods to preserve 'this' context
        this.handlePollVoteClick = this.handlePollVoteClick.bind(this)
        this.onPollVoteSuccess = this.onPollVoteSuccess.bind(this)
        this.onPollVoteError = this.onPollVoteError.bind(this)
        this.onPollUpdate = this.onPollUpdate.bind(this)
    }

    /**
     * Initialize the poll vote handler
     * @param {number} postId - The post ID containing the poll
     * @param {object} signalRConnection - Existing SignalR connection to PostHub
     */
    async init(postId, signalRConnection = null) {
        this.postId = Number.parseInt(postId)

        if (!this.postId || isNaN(this.postId)) {
            console.error("PollVoteHandler: Invalid postId provided")
            return false
        }

        // Use provided connection or try to get from global SignalRManager
        if (signalRConnection) {
            this.signalRConnection = signalRConnection
        } else if (window.signalRManager && window.signalRManager.postConnection) {
            this.signalRConnection = window.signalRManager.postConnection
        } else {
            console.error("PollVoteHandler: No SignalR connection available")
            return false
        }

        // Set up event listeners for poll-specific events
        this.setupPollEventHandlers()

        // Bind vote buttons
        this.bindPollVoteButtons()

        this.isInitialized = true
        console.log(`PollVoteHandler initialized for post ${this.postId}`)
        return true
    }

    /**
     * Set up SignalR event handlers for poll events
     */
    setupPollEventHandlers() {
        if (!this.signalRConnection) return

        // Remove existing listeners to prevent duplicates
        try {
            this.signalRConnection.off("ReceivePollVoteSuccess")
            this.signalRConnection.off("ReceivePollVoteError")
            this.signalRConnection.off("ReceivePollUpdate")
        } catch (e) {
            // Ignore errors if listeners don't exist
        }

        // Add new listeners
        this.signalRConnection.on("ReceivePollVoteSuccess", this.onPollVoteSuccess)
        this.signalRConnection.on("ReceivePollVoteError", this.onPollVoteError)
        this.signalRConnection.on("ReceivePollUpdate", this.onPollUpdate)

        console.log("PollVoteHandler: Event handlers registered")
    }

    /**
     * Bind click handlers to poll vote buttons
     */
    bindPollVoteButtons() {
        // Look for poll vote buttons with the correct data attributes
        const selectors = [
            ".poll-option-vote-btn",
            ".poll-vote-btn",
            ".poll-option[data-option-id] button",
            ".poll-option[data-option-id]", // In case the whole option is clickable
        ]

        selectors.forEach((selector) => {
            document.querySelectorAll(selector).forEach((button) => {
                if (this.boundButtons.has(button)) return

                // Check if this button has the required data attributes
                const optionId = this.getOptionIdFromElement(button)
                const buttonPostId = this.getPostIdFromElement(button)

                if (optionId && (buttonPostId === this.postId || !buttonPostId)) {
                    button.addEventListener("click", this.handlePollVoteClick)
                    button.setAttribute("data-poll-bound", "true")
                    this.boundButtons.add(button)
                    console.log(`Bound poll vote button for option ${optionId}`)
                }
            })
        })
    }

    /**
     * Handle poll vote button clicks
     */
    async handlePollVoteClick(event) {
        event.preventDefault()
        event.stopPropagation()

        const button = event.currentTarget
        const optionId = this.getOptionIdFromElement(button)

        if (!optionId) {
            this.showNotification("Invalid poll option", "error")
            return
        }

        // Check if SignalR is connected
        if (!this.signalRConnection || this.signalRConnection.state !== signalR.HubConnectionState.Connected) {
            this.showNotification("Not connected to server. Please refresh the page.", "error")
            return
        }

        // Disable all poll buttons immediately for better UX
        this.disableAllPollButtons()

        try {
            console.log(`Casting poll vote: PostId=${this.postId}, OptionId=${optionId}`)

            // Call the PostHub.CastPollVote method
            await this.signalRConnection.invoke("CastPollVote", this.postId, optionId)

            // Success/error handling will be done via SignalR event handlers
        } catch (error) {
            console.error("Error casting poll vote:", error)
            this.showNotification("Failed to cast vote. Please try again.", "error")
            this.enableAllPollButtons() // Re-enable on error
        }
    }

    /**
     * Handle successful poll vote response
     */
    onPollVoteSuccess(message) {
        console.log("Poll vote successful:", message)
        this.showNotification(message || "Your vote has been recorded!", "success")

        // Keep buttons disabled after successful vote (single-choice poll)
        this.markPollAsVoted()
    }

    /**
     * Handle poll vote error response
     */
    onPollVoteError(message) {
        console.error("Poll vote error:", message)
        this.showNotification(message || "Failed to cast your vote.", "error")

        // Re-enable buttons on error
        this.enableAllPollButtons()
    }

    /**
     * Handle poll update broadcast (when someone else votes)
     */
    onPollUpdate(updatedVoteCounts) {
        console.log("Received poll update:", updatedVoteCounts)
        this.updatePollDisplay(updatedVoteCounts)
    }

    /**
     * Update the poll display with new vote counts
     */
    updatePollDisplay(updatedData) {
        if (!updatedData) return

        // Handle different data formats
        let options = []
        if (Array.isArray(updatedData)) {
            options = updatedData
        } else if (updatedData.options) {
            options = updatedData.options
        } else if (typeof updatedData === "object") {
            // Convert object to array format
            options = Object.keys(updatedData).map((key) => ({
                optionId: Number.parseInt(key),
                voteCount: updatedData[key],
            }))
        }

        let totalVotes = 0
        options.forEach((option) => {
            totalVotes += option.voteCount || option.votes || 0
        })

        // Update each option
        options.forEach((option) => {
            const optionId = option.optionId || option.id || option.pollOptionId
            const voteCount = option.voteCount || option.votes || 0
            const percentage = totalVotes > 0 ? Math.round((voteCount / totalVotes) * 100) : 0

            this.updateOptionDisplay(optionId, voteCount, percentage)
        })

        // Update total votes if element exists
        const totalElement = document.querySelector(".poll-total-votes")
        if (totalElement) {
            totalElement.textContent = `${totalVotes.toLocaleString()} votes`
        }
    }

    /**
     * Update individual option display
     */
    updateOptionDisplay(optionId, voteCount, percentage) {
        // Find the option element
        const optionElement =
            document.querySelector(`[data-option-id="${optionId}"]`) || document.getElementById(`poll-option-${optionId}`)

        if (!optionElement) return

        // Update vote count
        const countElement = optionElement.querySelector(".poll-option-count") || optionElement.querySelector(".vote-count")
        if (countElement) {
            countElement.textContent = voteCount
        }

        // Update percentage
        const percentElement =
            optionElement.querySelector(".poll-option-percent") || optionElement.querySelector(".percentage")
        if (percentElement) {
            percentElement.textContent = `${percentage}%`
        }

        // Update progress bar
        const progressBar =
            optionElement.querySelector(".poll-option-bar") ||
            optionElement.querySelector(".progress-bar") ||
            optionElement.querySelector(".poll-progress")
        if (progressBar) {
            progressBar.style.width = `${percentage}%`
        }
    }

    /**
     * Disable all poll buttons
     */
    disableAllPollButtons() {
        document.querySelectorAll(".poll-option-vote-btn, .poll-vote-btn").forEach((button) => {
            button.disabled = true
            button.classList.add("disabled")
        })
    }

    /**
     * Enable all poll buttons
     */
    enableAllPollButtons() {
        document.querySelectorAll(".poll-option-vote-btn, .poll-vote-btn").forEach((button) => {
            button.disabled = false
            button.classList.remove("disabled")
        })
    }

    /**
     * Mark poll as voted (for single-choice polls)
     */
    markPollAsVoted() {
        const pollContainer = document.querySelector(".poll-container")
        if (pollContainer) {
            pollContainer.classList.add("poll-voted")
        }

        // Update any instruction text
        const instructionElement = document.querySelector(".poll-instruction")
        if (instructionElement) {
            instructionElement.innerHTML = '<i class="fas fa-check-circle text-success"></i> You have voted in this poll'
        }
    }

    /**
     * Get option ID from button or its parent elements
     */
    getOptionIdFromElement(element) {
        // Try different ways to get the option ID
        let optionId =
            element.getAttribute("data-option-id") ||
            element.getAttribute("data-poll-option-id") ||
            element.dataset.optionId ||
            element.dataset.pollOptionId

        // If not found on button, check parent elements
        if (!optionId) {
            const optionContainer = element.closest("[data-option-id]") || element.closest("[data-poll-option-id]")
            if (optionContainer) {
                optionId = optionContainer.getAttribute("data-option-id") || optionContainer.getAttribute("data-poll-option-id")
            }
        }

        return optionId ? Number.parseInt(optionId) : null
    }

    /**
     * Get post ID from button or its parent elements
     */
    getPostIdFromElement(element) {
        let postId = element.getAttribute("data-post-id") || element.dataset.postId

        if (!postId) {
            const postContainer = element.closest("[data-post-id]")
            if (postContainer) {
                postId = postContainer.getAttribute("data-post-id")
            }
        }

        return postId ? Number.parseInt(postId) : null
    }

    /**
     * Show notification to user
     */
    showNotification(message, type = "info") {
        // Try to use existing notification system
        if (window.signalRManager && typeof window.signalRManager.showNotification === "function") {
            window.signalRManager.showNotification(message, type)
            return
        }

        // Fallback notification
        console.log(`[${type.toUpperCase()}] ${message}`)

        // Simple toast notification
        const toast = document.createElement("div")
        toast.className = `alert alert-${type === "error" ? "danger" : type === "success" ? "success" : "info"} alert-dismissible fade show`
        toast.style.cssText = "position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;"
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `

        document.body.appendChild(toast)

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast)
            }
        }, 5000)
    }

    /**
     * Clean up event handlers
     */
    destroy() {
        if (this.signalRConnection) {
            try {
                this.signalRConnection.off("ReceivePollVoteSuccess", this.onPollVoteSuccess)
                this.signalRConnection.off("ReceivePollVoteError", this.onPollVoteError)
                this.signalRConnection.off("ReceivePollUpdate", this.onPollUpdate)
            } catch (e) {
                // Ignore errors
            }
        }

        this.boundButtons.forEach((button) => {
            button.removeEventListener("click", this.handlePollVoteClick)
            button.removeAttribute("data-poll-bound")
        })

        this.boundButtons.clear()
        this.isInitialized = false

        console.log("PollVoteHandler destroyed")
    }
}

// Auto-initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    // Wait a bit for SignalR to initialize
    setTimeout(() => {
        // Check if we have a post with a poll
        const postIdElement = document.getElementById("pagePostId") || document.querySelector("[data-post-id]")

        if (postIdElement) {
            const postId = Number.parseInt(postIdElement.value || postIdElement.getAttribute("data-post-id"))

            // Check if there are any poll elements on the page
            const hasPoll = document.querySelector(".poll-container, .poll-option, .poll-vote-btn")

            if (postId && hasPoll) {
                const pollHandler = new PollVoteHandler()
                pollHandler.init(postId).then((success) => {
                    if (success) {
                        window.pollVoteHandler = pollHandler
                        console.log("PollVoteHandler auto-initialized successfully")
                    }
                })
            }
        }
    }, 1000) // Wait 1 second for SignalR to be ready
})

// Export for manual initialization
window.PollVoteHandler = PollVoteHandler
