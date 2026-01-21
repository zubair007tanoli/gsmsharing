// Voting System JavaScript
(function() {
    'use strict';

    // Initialize voting for all vote buttons
    function initializeVoting() {
        document.querySelectorAll('.vote-btn').forEach(btn => {
            btn.addEventListener('click', handleVote);
        });
    }

    async function handleVote(e) {
        e.preventDefault();
        const btn = e.currentTarget;
        const contentType = btn.dataset.contentType;
        const contentId = parseInt(btn.dataset.contentId);
        const voteType = parseInt(btn.dataset.voteType);
        
        // Disable button during request
        btn.disabled = true;
        
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const response = await fetch('/api/Vote/Vote', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token || ''
                },
                body: JSON.stringify({
                    contentType: contentType,
                    contentID: contentId,
                    voteType: voteType
                })
            });
            
            if (!response.ok) {
                throw new Error('Vote failed');
            }
            
            const data = await response.json();
            
            if (data.success) {
                // Update score
                const scoreElement = document.querySelector(`#${contentType}Score-${contentId}`);
                if (scoreElement) {
                    const newScore = data.upvotes - data.downvotes;
                    scoreElement.textContent = newScore;
                    
                    // Add animation
                    scoreElement.classList.add('pulse-animation');
                    setTimeout(() => {
                        scoreElement.classList.remove('pulse-animation');
                    }, 500);
                }
                
                // Update button states
                const container = btn.closest('.vote-section');
                if (container) {
                    const upBtn = container.querySelector('.vote-up');
                    const downBtn = container.querySelector('.vote-down');
                    
                    upBtn?.classList.remove('active');
                    downBtn?.classList.remove('active');
                    
                    if (data.userVote === 1 && upBtn) {
                        upBtn.classList.add('active');
                    } else if (data.userVote === -1 && downBtn) {
                        downBtn.classList.add('active');
                    }
                }
            }
        } catch (error) {
            console.error('Vote error:', error);
            alert('Failed to submit vote. Please try again.');
        } finally {
            btn.disabled = false;
        }
    }

    // Export for global use
    window.initializeVoting = initializeVoting;
    
    // Auto-initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeVoting);
    } else {
        initializeVoting();
    }
})();
