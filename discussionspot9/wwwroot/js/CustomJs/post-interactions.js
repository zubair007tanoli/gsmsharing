// Initialize post interactions
function initializePostInteractions(voteUrl, shareUrl) {
    // Voting functionality
    document.querySelectorAll('.vote-btn').forEach(btn => {
        btn.addEventListener('click', async function () {
            if (this.disabled) {
                window.location.href = '/Identity/Account/Login';
                return;
            }

            const postCard = this.closest('.post-card');
            const postId = postCard.dataset.postId;
            const voteType = parseInt(this.dataset.voteType);
            const voteButtons = this.parentElement;
            const voteCount = voteButtons.querySelector('.vote-count');

            // Add loading state
            voteButtons.classList.add('loading');

            try {
                const response = await fetch(voteUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': window.csrfToken
                    },
                    body: JSON.stringify({
                        postId: parseInt(postId),
                        voteType: voteType
                    })
                });

                const result = await response.json();

                if (result.success) {
                    // Update UI
                    voteButtons.querySelectorAll('.vote-btn').forEach(b => b.classList.remove('active'));
                    if (result.userVote !== 0) {
                        this.classList.add('active');
                    }

                    // Update vote count
                    voteCount.textContent = formatNumber(result.voteCount);
                }
            } catch (error) {
                console.error('Error voting:', error);
            } finally {
                voteButtons.classList.remove('loading');
            }
        });
    });

    // Share functionality
    document.querySelectorAll('.share-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const postUrl = this.dataset.postUrl;
            const postCard = this.closest('.post-card');
            const postId = postCard.dataset.postId;

            // Create share menu if it doesn't exist
            let shareMenu = this.parentElement.querySelector('.share-menu');
            if (!shareMenu) {
                shareMenu = createShareMenu(postUrl, postId);
                this.parentElement.style.position = 'relative';
                this.parentElement.appendChild(shareMenu);
            }

            // Toggle menu
            shareMenu.classList.toggle('show');

            // Track share
            fetch(shareUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': window.csrfToken
                },
                body: JSON.stringify({
                    postId: parseInt(postId),
                    platform: 'menu_opened'
                })
            });
        });
    });

    // Save functionality
    document.querySelectorAll('.save-btn').forEach(btn => {
        btn.addEventListener('click', async function () {
            const postCard = this.closest('.post-card');
            const postId = postCard.dataset.postId;
            const isSaved = this.classList.contains('saved');

            try {
                const response = await fetch('/Post/ToggleSave', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': window.csrfToken
                    },
                    body: JSON.stringify({ postId: parseInt(postId) })
                });

                const result = await response.json();

                if (result.success) {
                    this.classList.toggle('saved');
                    const icon = this.querySelector('i');
                    icon.classList.toggle('fas');
                    icon.classList.toggle('far');
                    this.innerHTML = `<i class="${icon.className}"></i> ${result.isSaved ? 'Saved' : 'Save'}`;
                }
            } catch (error) {
                console.error('Error saving post:', error);
            }
        });
    });

    // Close share menu when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.share-btn') && !e.target.closest('.share-menu')) {
            document.querySelectorAll('.share-menu.show').forEach(menu => {
                menu.classList.remove('show');
            });
        }
    });
}

// Create share menu
function createShareMenu(postUrl, postId) {
    const menu = document.createElement('div');
    menu.className = 'share-menu';

    const platforms = [
        { name: 'Copy Link', icon: 'fas fa-link', action: 'copy' },
        { name: 'Twitter', icon: 'fab fa-twitter', action: 'twitter' },
        { name: 'Facebook', icon: 'fab fa-facebook', action: 'facebook' },
        { name: 'Reddit', icon: 'fab fa-reddit', action: 'reddit' }
    ];

    platforms.forEach(platform => {
        const option = document.createElement('div');
        option.className = 'share-option';
        option.innerHTML = `<i class="${platform.icon}"></i> ${platform.name}`;

        option.addEventListener('click', function () {
            sharePost(postUrl, platform.action, postId);
        });

        menu.appendChild(option);
    });

    return menu;
}

// Handle sharing
function sharePost(url, platform, postId) {
    const fullUrl = window.location.origin + url;

    switch (platform) {
        case 'copy':
            navigator.clipboard.writeText(fullUrl).then(() => {
                showToast('Link copied to clipboard!');
            });
            break;
        case 'twitter':
            window.open(`https://twitter.com/intent/tweet?url=${encodeURIComponent(fullUrl)}`, '_blank');
            break;
        case 'facebook':
            window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(fullUrl)}`, '_blank');
            break;
        case 'reddit':
            window.open(`https://www.reddit.com/submit?url=${encodeURIComponent(fullUrl)}`, '_blank');
            break;
    }

    // Track share
    fetch(window.shareUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': window.csrfToken
        },
        body: JSON.stringify({
            postId: parseInt(postId),
            platform: platform
        })
    });
}

// Format numbers
function formatNumber(num) {
    if (num >= 1000000) {
        return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
        return (num / 1000).toFixed(1) + 'k';
    }
    return num.toString();
}

// Show toast notification
function showToast(message) {
    // Implementation for toast notification
    console.log(message);
}