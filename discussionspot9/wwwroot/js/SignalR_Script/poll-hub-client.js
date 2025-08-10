// /js/PollHandler.js

class PollHandler {
    /**
     * @param {SignalRManager} signalRManager - An instance of the main SignalR manager.
     */
    constructor(signalRManager) {
        if (!signalRManager || !signalRManager.postConnection) {
            throw new Error("PollHandler requires a valid SignalRManager instance with an active postConnection.");
        }
        this.signalRManager = signalRManager;

        // Bind 'this' to event handlers
        this.handlePollOptionClick = this.handlePollOptionClick.bind(this);
    }

    /**
     * Binds click event listeners to all poll options on the page.
     * This should be called on initial page load and whenever new poll elements might be added.
     */
    rebindPollOptionClick() {
        document.querySelectorAll('.poll-option').forEach(option => {
            // Remove any existing listener to prevent duplicates
            option.removeEventListener('click', this.handlePollOptionClick);
            // Add the new listener
            option.addEventListener('click', this.handlePollOptionClick);
        });
        console.log("PollHandler: Re-bound click listeners to poll options.");
    }

    /**
     * Handles the click event on a poll option to cast a vote.
     * @param {Event} event - The click event.
     */
    handlePollOptionClick(event) {
        event.preventDefault();
        const optionDiv = event.currentTarget;

        // Prevent voting if the poll has already been voted on (indicated by the 'voted' class)
        if (optionDiv.closest('.poll-container')?.classList.contains('voted')) {
            console.log("Poll already voted on, ignoring click.");
            return;
        }

        const pollOptionId = parseInt(optionDiv.dataset.option);
        const postId = this.signalRManager.pagePostId;

        if (postId && !isNaN(pollOptionId)) {
            console.log(`PollHandler: Casting vote for PostId: ${postId}, PollOptionId: ${pollOptionId}`);
            this.signalRManager.postConnection.invoke("CastPollVote", postId, pollOptionId)
                .catch(err => {
                    console.error("Error invoking CastPollVote:", err);
                    this.signalRManager.showNotification("Error casting your vote.", 'error');
                });
        } else {
            console.error("PollHandler: Missing postId or pollOptionId. Cannot cast vote.");
        }
    }

    /**
     * Updates the entire poll's UI with new data received from the server.
     * @param {object} pollData - The poll data from the server.
     */
    updatePollUI(pollData) {
        if (!pollData || !pollData.options) {
            console.warn("PollHandler: Invalid pollData received for UI update.");
            return;
        }

        const pollContainer = document.querySelector('.poll-container');
        if (!pollContainer) {
            console.warn("PollHandler: Poll container not found on the page.");
            return;
        }

        // Mark the container as 'voted' to show results and disable further voting.
        pollContainer.classList.add('voted');

        const totalVotesElement = document.getElementById('totalVotes');
        if (totalVotesElement) {
            totalVotesElement.textContent = `${pollData.totalVotes.toLocaleString()} votes`;
        }

        pollData.options.forEach(optionData => {
            const optionElement = pollContainer.querySelector(`.poll-option[data-option="${optionData.pollOptionId}"]`);
            if (optionElement) {
                const percentageElement = optionElement.querySelector('.poll-percentage');
                const progressElement = optionElement.querySelector('.poll-progress');

                // Make the results visible
                if (percentageElement) percentageElement.style.display = 'block';
                if (progressElement) progressElement.style.display = 'block';

                if (percentageElement) {
                    percentageElement.textContent = `${Math.round(optionData.votePercentage)}%`;
                }
                if (progressElement) {
                    progressElement.style.width = `${optionData.votePercentage}%`;
                }

                // Highlight the option the user selected
                if (optionData.hasUserVoted) {
                    optionElement.classList.add('selected');
                }
            }
        });

        // Disable the main "Vote" button after voting.
        const voteButton = document.getElementById('submitVote');
        if (voteButton) {
            voteButton.disabled = true;
            voteButton.textContent = "Voted";
        }
    }
}
