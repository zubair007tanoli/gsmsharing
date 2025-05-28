// post-details.js

document.addEventListener('DOMContentLoaded', function () {
    // Initialize
    initializeVoting();
    initializeCommentForm();
    initializeShareButtons();
    initializeCommentActions();
    initializeLoadMoreComments();
});

// Post Voting
function initializeVoting() {
    document.querySelectorAll('.vote-btn').forEach(button => {
        button.addEventListener('click', async function () {
            if (!isAuthenticated()) {
                window.location.href = '/login?returnUrl=' + encodeURIComponent(window.location.pathname);
                return;
            }

            const postId = this.dataset.postId;
            const voteType = parseInt(this.dataset.voteType);

            try {
                const response = await fetch('/Post/Vote', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': getAntiForgeryToken()
                    },
                    body: JSON.stringify({ postId, voteType })
                });

                const result = await response.json();

                if (result.success) {
                    updateVoteUI(postId, result.voteCount, result.userVote);
                } else {
                    showToast(result.message || 'Failed to vote', 'error');
                }
            } catch (error) {
                showToast('An error occurred while voting', 'error');
            }
        });
    });
}

// Comment Form
function initializeCommentForm() {
    const commentForm = document.getElementById('commentForm');
    if (!commentForm) return;

    commentForm.addEventListener('submit', async function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const content = formData.get('Content');
        const postId = formData.get('PostId');

        try {
            const response = await fetch('/Comment/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({ content, postId })
            });

            const result = await response.json();

            if (result.success) {
                addCommentToDOM(result.html);
                this.reset();
                showToast(result.message || 'Comment posted successfully!', 'success');
                updateCommentCount(1);
            } else {
                showToast(result.message || 'Failed to post comment', 'error');
            }
        } catch (error) {
            showToast('An error occurred while posting comment', 'error');
        }
    });
}

// Share Buttons
function initializeShareButtons() {
    document.querySelectorAll('.share-btn').forEach(button => {
        button.addEventListener('click', async function (e) {
            e.preventDefault();

            const platform = this.dataset.platform;
            const postId = document.querySelector('[data-post-id]').dataset.postId;
            const url = window.location.href;
            const title = document.title;

            if (platform === 'copy') {
                copyToClipboard(url);
                showToast('Link copied to clipboard!', 'success');
            } else {
                const shareUrl = getShareUrl(platform, url, title);
                if (shareUrl) {
                    window.open(shareUrl, '_blank', 'width=600,height=400');
                }
            }

            // Track share
            await fetch('/Post/Share', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({ postId, platform })
            });
        });
    });
}

// Comment Actions
function initializeCommentActions() {
    // Comment voting
    document.addEventListener('click', async function (e) {
        if (e.target.closest('.comment-vote-btn')) {
            const button = e.target.closest('.comment-vote-btn');
            const commentId = button.dataset.commentId;
            const voteType = parseInt(button.dataset.voteType);

            if (!isAuthenticated()) {
                window.location.href = '/login?returnUrl=' + encodeURIComponent(window.location.pathname);
                return;
            }

            try {
                const response = await fetch('/Comment/Vote', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': getAntiForgeryToken()
                    },
                    body: JSON.stringify({ commentId, voteType })
                });

                const result = await response.json();

                if (result.success) {
                    updateCommentVoteUI(commentId, result.voteCount, result.userVote);
                }
            } catch (error) {
                showToast('An error occurred while voting', 'error');
            }
        }

        // Reply button
        if (e.target.closest('.reply-btn')) {
            const button = e.target.closest('.reply-btn');
            const commentId = button.dataset.commentId;
            toggleReplyForm(commentId);
        }

        // Edit button
        if (e.target.closest('.edit-btn')) {
            const button = e.target.closest('.edit-btn');
            const commentId = button.dataset.commentId;
            showEditForm(commentId);
        }

        // Delete button
        if (e.target.closest('.delete-btn')) {
            const button = e.target.closest('.delete-btn');
            const commentId = button.dataset.commentId;

            if (confirm('Are you sure you want to delete this comment?')) {
                deleteComment(commentId);
            }
        }
    });

    // Reply form submission
    document.addEventListener('submit', async function (e) {
        if (e.target.classList.contains('reply-form')) {
            e.preventDefault();

            const formData = new FormData(e.target);
            const content = formData.get('Content');
            const postId = formData.get('PostId');
            const parentCommentId = formData.get('ParentCommentId');

            try {
                const response = await fetch('/Comment/Create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': getAntiForgeryToken()
                    },
                    body: JSON.stringify({ content, postId, parentCommentId })
                });

                const result = await response.json();

                if (result.success) {
                    addReplyToComment(parentCommentId, result.html);
                    e.target.reset();
                    toggleReplyForm(parentCommentId);
                    showToast('Reply posted successfully!', 'success');
                }
            } catch (error) {
                showToast('An error occurred while posting reply', 'error');
            }
        }
    });
}

// Load More Comments
function initializeLoadMoreComments() {
    const loadMoreBtn = document.querySelector('.load-more-comments');
    if (!loadMoreBtn) return;

    loadMoreBtn.addEventListener('click', async function () {
        const postId = this.dataset.postId;
        const page = parseInt(this.dataset.page);

        this.disabled = true;
        this.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';

        try {
            const response = await fetch(`/Comment/LoadMore?postId=${postId}&page=${page}`);
            const result = await response.json();

            if (result.success) {
                document.getElementById('commentsList').insertAdjacentHTML('beforeend', result.html);

                if (result.hasMore) {
                    this.dataset.page = page + 1;
                    this.disabled = false;
                    this.innerHTML = 'Load More Comments';
                } else {
                    this.remove();
                }
            }
        } catch (error) {
            this.disabled = false;
            this.innerHTML = 'Load More Comments';
            showToast('Failed to load comments', 'error');
        }
    });
}

// Helper Functions
function isAuthenticated() {
    return document.querySelector('meta[name="is-authenticated"]')?.content === 'true';
}

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
}

function updateVoteUI(postId, voteCount, userVote) {
    const voteContainer = document.querySelector(`[data-post-id="${postId}"]`).closest('.vote-buttons');
    const countElement = voteContainer.querySelector('.vote-count');
    const upvoteBtn = voteContainer.querySelector('.upvote');
    const downvoteBtn = voteContainer.querySelector('.downvote');

    countElement.textContent = voteCount;

    upvoteBtn.classList.toggle('active', userVote === 1);
    downvoteBtn.classList.toggle('active', userVote === -1);
}

function updateCommentVoteUI(commentId, voteCount, userVote) {
    const comment = document.querySelector(`[data-comment-id="${commentId}"]`).closest('.comment');
    const countElement = comment.querySelector('.comment-vote-count');
    const upvoteBtn = comment.querySelector('.comment-upvote');
    const downvoteBtn = comment.querySelector('.comment-downvote');

    countElement.textContent = voteCount;

    upvoteBtn.classList.toggle('active', userVote === 1);
    downvoteBtn.classList.toggle('active', userVote === -1);
}

function addCommentToDOM(html) {
    const commentsList = document.getElementById('commentsList');
    const noComments = commentsList.querySelector('.text-center');

    if (noComments) {
        noComments.remove();
    }

    commentsList.insertAdjacentHTML('afterbegin', html);
}

function addReplyToComment(parentId, html) {
    const parentComment = document.querySelector(`[data-comment-id="${parentId}"]`).closest('.comment');
    const repliesContainer = parentComment.querySelector('.replies') || createRepliesContainer(parentComment);

    repliesContainer.insertAdjacentHTML('beforeend', html);
}

function createRepliesContainer(parentComment) {
    const container = document.createElement('div');
    container.className = 'replies ms-4 mt-3';
    parentComment.appendChild(container);
    return container;
}

function toggleReplyForm(commentId) {
    const form = document.getElementById(`reply-form-${commentId}`);
    if (form) {
        form.classList.toggle('d-none');
        if (!form.classList.contains('d-none')) {
            form.querySelector('textarea').focus();
        }
    }
}

async function deleteComment(commentId) {
    try {
        const response = await fetch('/Comment/Delete', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ commentId })
        });

        const result = await response.json();

        if (result.success) {
            const comment = document.querySelector(`[data-comment-id="${commentId}"]`).closest('.comment');
            comment.querySelector('.comment-content').innerHTML = '<em class="text-muted">[deleted]</em>';
            comment.querySelector('.comment-actions').remove();
            showToast('Comment deleted successfully', 'success');
        }
    } catch (error) {
        showToast('Fail