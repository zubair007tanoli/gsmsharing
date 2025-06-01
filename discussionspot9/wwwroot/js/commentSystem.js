class CommentSystem {
    constructor(postId) {
        this.postId = postId;
        this.connection = null;
        this.initializeSignalR();
        this.bindEvents();
    }

    async initializeSignalR() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/posthub")
            .build();

        this.connection.on("ReceiveComment", (comment) => {
            this.addCommentToDOM(comment);
            this.updateCommentCount(1);
        });

        this.connection.on("UpdateVoteCount", (newCount) => {
            document.querySelector('.vote-count').textContent = newCount;
        });

        try {
            await this.connection.start();
            await this.connection.invoke("JoinPostGroup", this.postId);
        } catch (err) {
            console.error(err);
        }
    }

    bindEvents() {
        // Comment form submission
        document.getElementById('commentForm')?.addEventListener('submit', async (e) => {
            e.preventDefault();
            await this.submitComment(e.target);
        });

        // Reply forms
        document.addEventListener('click', (e) => {
            if (e.target.closest('.reply-btn')) {
                this.showReplyForm(e.target.closest('.reply-btn'));
            }
            if (e.target.closest('.cancel-reply')) {
                this.hideReplyForm(e.target.closest('.cancel-reply'));
            }
        });

        // Vote buttons
        document.addEventListener('click', async (e) => {
            if (e.target.closest('.comment-upvote, .comment-downvote')) {
                await this.voteComment(e.target.closest('.vote-btn'));
            }
        });
    }

    async submitComment(form) {
        const formData = new FormData(form);
        const content = formData.get('Content');
        const parentCommentId = formData.get('ParentCommentId');

        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke("SendComment", this.postId, content, parentCommentId);
            form.reset();
        } else {
            // Fallback to AJAX
            await this.submitCommentAjax(content, parentCommentId);
        }
    }

    async submitCommentAjax(content, parentCommentId) {
        const response = await fetch('/Comment/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                PostId: this.postId,
                Content: content,
                ParentCommentId: parentCommentId
            })
        });

        const result = await response.json();
        if (result.success) {
            location.reload(); // Fallback behavior
        }
    }

    addCommentToDOM(comment) {
        // Implementation to add comment to the DOM
        // This would use the _CommentItem partial view rendered from server
    }

    updateCommentCount(change) {
        const countElement = document.getElementById('commentCount');
        const currentCount = parseInt(countElement.textContent);
        countElement.textContent = currentCount + change;
    }

    showReplyForm(button) {
        const commentId = button.dataset.commentId;
        const replyForm = document.getElementById(`reply-form-${commentId}`);
        replyForm.classList.remove('d-none');
        replyForm.querySelector('textarea').focus();
    }

    hideReplyForm(button) {
        const replyForm = button.closest('.reply-form');
        replyForm.classList.add('d-none');
    }

    async voteComment(button) {
        const commentId = button.dataset.commentId;
        const voteType = parseInt(button.dataset.voteType);

        const response = await fetch('/Comment/Vote', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ commentId, voteType })
        });

        const result = await response.json();
        if (result.success) {
            this.updateVoteDisplay(commentId, result);
        }
    }

    updateVoteDisplay(commentId, result) {
        const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
        const voteCount = commentElement.querySelector('.vote-count');
        const upvoteBtn = commentElement.querySelector('.comment-upvote');
        const downvoteBtn = commentElement.querySelector('.comment-downvote');

        voteCount.textContent = result.voteCount;
        upvoteBtn.classList.toggle('active', result.userVote === 1);
        downvoteBtn.classList.toggle('active', result.userVote === -1);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    const postId = document.querySelector('[data-post-id]')?.dataset.postId;
    if (postId) {
        new CommentSystem(parseInt(postId));
    }
});