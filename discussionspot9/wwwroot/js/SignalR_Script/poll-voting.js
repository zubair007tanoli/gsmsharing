/*!
 * Poll Voting Handler
 * Handles poll voting without SignalR for immediate functionality
 */

class PollVotingHandler {
    constructor() {
        this.boundButtons = new Set()
        this.init()
    }

    init() {
        // Bind existing vote buttons
        this.bindVoteButtons()

        // Watch for dynamically added content
        this.observeChanges()

        console.log("PollVotingHandler initialized")
    }

    bindVoteButtons() {
        const buttons = document.querySelectorAll(".poll-option-vote-btn:not([data-poll-bound])")

        buttons.forEach((button) => {
            if (this.boundButtons.has(button)) return

            button.addEventListener("click", this.handleVoteClick.bind(this))
            button.setAttribute("data-poll-bound", "true")
            this.boundButtons.add(button)
        })

        console.log(`Bound ${buttons.length} poll vote buttons`)
    }

    async handleVoteClick(event) {
        event.preventDefault()
        event.stopPropagation()

        const button = event.currentTarget
        const postId = Number.parseInt(button.getAttribute("data-post-id"))
        const optionId = Number.parseInt(button.getAttribute("data-option-id"))

        if (!postId || !optionId) {
            this.showNotification("Invalid poll data", "error")
            return
        }

        // Disable all buttons in this poll immediately
        this.disablePollButtons(postId)

        try {
            // Make AJAX call to vote
            const response = await fetch("/api/polls/vote", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    RequestVerificationToken: this.getAntiForgeryToken(),
                },
                body: JSON.stringify({
                    postId: postId,
                    optionId: optionId,
                }),
            })

            const result = await response.json()

            if (result.success) {
                this.showNotification("Vote cast successfully!", "success")
                this.updatePollUI(postId, result.pollData)
            } else {
                this.showNotification(result.message || "Failed to cast vote", "error")
                this.enablePollButtons(postId) // Re-enable on error
            }
        } catch (error) {
            console.error("Error casting vote:", error)
            this.showNotification("Network error. Please try again.", "error")
            this.enablePollButtons(postId) // Re-enable on error
        }
    }

    disablePollButtons(postId) {
        const buttons = document.querySelectorAll(`.poll-option-vote-btn[data-post-id="${postId}"]`)
        buttons.forEach((btn) => {
            btn.disabled = true
            btn.classList.add("disabled")
        })
    }

    enablePollButtons(postId) {
        const buttons = document.querySelectorAll(`.poll-option-vote-btn[data-post-id="${postId}"]`)
        buttons.forEach((btn) => {
            btn.disabled = false
            btn.classList.remove("disabled")
        })
    }

    updatePollUI(postId, pollData) {
        const container = document.querySelector(`[data-post-id="${postId}"]`)
        if (!container) return

        // Update total votes
        const totalVotesEl = container.querySelector(".poll-total-votes")
        if (totalVotesEl && pollData.totalVotes !== undefined) {
            totalVotesEl.textContent = `${pollData.totalVotes.toLocaleString()} votes`
        }

        // Update each option
        if (pollData.options) {
            pollData.options.forEach((option) => {
                const optionEl = container.querySelector(`[data-option-id="${option.optionId}"]`)
                if (!optionEl) return

                // Update count
                const countEl = optionEl.querySelector(".poll-option-count")
                if (countEl) countEl.textContent = option.voteCount

                // Update percentage
                const percentEl = optionEl.querySelector(".poll-option-percent")
                if (percentEl) percentEl.textContent = `${Math.round(option.percentage)}%`

                // Update progress bar
                const barEl = optionEl.querySelector(".poll-option-bar")
                if (barEl) barEl.style.width = `${Math.round(option.percentage)}%`

                // Mark as voted if this was the selected option
                if (option.userVoted) {
                    optionEl.classList.add("selected", "voted")
                    const radioCircle = optionEl.querySelector(".radio-circle")
                    if (radioCircle) {
                        radioCircle.classList.add("selected")
                        radioCircle.innerHTML = '<i class="fas fa-check"></i>'
                    }
                }
            })
        }

        // Replace vote buttons with static display
        this.convertToStaticDisplay(container)

        // Update message
        const messageEl = container.querySelector(".text-muted")
        if (messageEl) {
            messageEl.className = "text-success text-center small mb-3"
            messageEl.innerHTML = '<i class="fas fa-check-circle"></i> You have voted in this poll'
        }
    }

    convertToStaticDisplay(container) {
        const buttons = container.querySelectorAll(".poll-option-vote-btn")
        buttons.forEach((button) => {
            const inner = button.querySelector(".poll-option-inner")
            if (inner) {
                const newDiv = document.createElement("div")
                newDiv.className = "poll-option-inner"
                newDiv.innerHTML = inner.innerHTML
                button.parentNode.replaceChild(newDiv, button)
            }
        })
    }

    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')
        return token ? token.value : ""
    }

    showNotification(message, type = "info") {
        // Try to use existing notification system or create a simple one
        if (window.signalRManager && typeof window.signalRManager.showNotification === "function") {
            window.signalRManager.showNotification(message, type)
            return
        }

        // Fallback notification
        const toast = document.createElement("div")
        toast.className = `alert alert-${type === "error" ? "danger" : type === "success" ? "success" : "info"} alert-dismissible fade show`
        toast.style.position = "fixed"
        toast.style.top = "20px"
        toast.style.right = "20px"
        toast.style.zIndex = "9999"
        toast.style.minWidth = "300px"

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

    observeChanges() {
        // Watch for new poll content being added to the page
        const observer = new MutationObserver((mutations) => {
            let shouldRebind = false

            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1) {
                        // Element node
                        if (node.querySelector && node.querySelector(".poll-option-vote-btn")) {
                            shouldRebind = true
                        }
                    }
                })
            })

            if (shouldRebind) {
                setTimeout(() => this.bindVoteButtons(), 100)
            }
        })

        observer.observe(document.body, {
            childList: true,
            subtree: true,
        })
    }
}

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    window.pollVotingHandler = new PollVotingHandler()
})

// Also initialize if DOM is already loaded
if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => {
        if (!window.pollVotingHandler) {
            window.pollVotingHandler = new PollVotingHandler()
        }
    })
} else {
    if (!window.pollVotingHandler) {
        window.pollVotingHandler = new PollVotingHandler()
    }
}
