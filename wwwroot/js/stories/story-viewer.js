/**
 * Story Viewer JavaScript
 * Handles story viewing functionality with slide navigation, progress tracking, and controls
 */

(function() {
    'use strict';

    // Wait for DOM and utility files to be available
    if (typeof document === 'undefined') return;

    document.addEventListener('DOMContentLoaded', function() {
        const storyViewer = document.getElementById('story-viewer');
        if (!storyViewer) return;

        const slides = document.querySelectorAll('.story-slide');
        const progressBar = document.getElementById('progress-bar');
        const currentSlideSpan = document.getElementById('current-slide');
        const totalSlidesSpan = document.getElementById('total-slides');
        const playPauseBtn = document.getElementById('play-pause-btn');
        const prevBtn = document.getElementById('prev-btn');
        const nextBtn = document.getElementById('next-btn');
        const closeBtn = document.getElementById('close-btn');
        
        if (!slides.length) return;

        let currentSlideIndex = 0;
        let isPlaying = true;
        let slideTimer = null;
        let slideStartTime = null;
        let totalSlides = slides.length;
        let viewCount = 0;
        let likeCount = 0;
        let isLiked = false;
        let isBookmarked = false;
        let hideControlsTimer = null;
        
        totalSlidesSpan.textContent = totalSlides;
        
        // Initialize
        initializeProgressIndicators();
        updateStats();
        showSlide(0);
        
        if (isPlaying) {
            startSlideTimer();
        }
        
        function showSlide(index) {
            // Hide all slides
            slides.forEach(slide => slide.classList.remove('active'));
            
            // Show current slide
            if (slides[index]) {
                slides[index].classList.add('active');
                currentSlideIndex = index;
                if (currentSlideSpan) {
                    currentSlideSpan.textContent = index + 1;
                }
                
                // Update progress bar
                if (progressBar) {
                    const progress = ((index + 1) / totalSlides) * 100;
                    progressBar.style.width = progress + '%';
                }
                
                // Update progress indicators
                updateProgressIndicators();
                
                // Reset slide timer
                slideStartTime = Date.now();
                const timerElement = document.getElementById('slide-timer');
                if (timerElement) {
                    timerElement.textContent = '0';
                }
                
                // Increment view count on first view
                if (index === 0 && viewCount === 0) {
                    viewCount++;
                    updateStats();
                }
            }
        }
        
        function nextSlide() {
            const nextIndex = (currentSlideIndex + 1) % totalSlides;
            showSlide(nextIndex);
            
            if (isPlaying) {
                startSlideTimer();
            }
        }
        
        function prevSlide() {
            const prevIndex = currentSlideIndex === 0 ? totalSlides - 1 : currentSlideIndex - 1;
            showSlide(prevIndex);
            
            if (isPlaying) {
                startSlideTimer();
            }
        }
        
        function startSlideTimer() {
            if (slideTimer) {
                clearTimeout(slideTimer);
            }
            
            const currentSlide = slides[currentSlideIndex];
            const duration = currentSlide ? (parseInt(currentSlide.dataset.duration) || 5000) : 5000;
            
            slideTimer = setTimeout(() => {
                if (isPlaying) {
                    nextSlide();
                }
            }, duration);
        }
        
        function togglePlayPause() {
            isPlaying = !isPlaying;
            if (playPauseBtn) {
                const icon = playPauseBtn.querySelector('i');
                if (icon) {
                    icon.className = isPlaying ? 'fas fa-pause' : 'fas fa-play';
                }
            }
            
            if (isPlaying) {
                startSlideTimer();
            } else {
                if (slideTimer) {
                    clearTimeout(slideTimer);
                }
            }
        }
        
        // Event listeners
        if (playPauseBtn) {
            playPauseBtn.addEventListener('click', togglePlayPause);
        }
        
        if (nextBtn) {
            nextBtn.addEventListener('click', nextSlide);
        }
        
        if (prevBtn) {
            prevBtn.addEventListener('click', prevSlide);
        }
        
        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                window.history.back();
            });
        }
        
        // Keyboard navigation
        document.addEventListener('keydown', function(e) {
            switch(e.key) {
                case 'ArrowRight':
                case ' ':
                    e.preventDefault();
                    nextSlide();
                    break;
                case 'ArrowLeft':
                    e.preventDefault();
                    prevSlide();
                    break;
                case 'Escape':
                    window.history.back();
                    break;
            }
        });
        
        // Touch/swipe navigation
        let startX = 0;
        let startY = 0;
        
        storyViewer.addEventListener('touchstart', function(e) {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        });
        
        storyViewer.addEventListener('touchend', function(e) {
            const endX = e.changedTouches[0].clientX;
            const endY = e.changedTouches[0].clientY;
            const diffX = startX - endX;
            const diffY = startY - endY;
            
            // Only handle horizontal swipes
            if (Math.abs(diffX) > Math.abs(diffY) && Math.abs(diffX) > 50) {
                if (diffX > 0) {
                    nextSlide(); // Swipe left - next slide
                } else {
                    prevSlide(); // Swipe right - previous slide
                }
            }
        });
        
        // Auto-hide controls
        function showControls() {
            const controls = document.querySelector('.story-controls');
            const info = document.querySelector('.story-info');
            const close = document.querySelector('.close-btn');
            
            if (controls) controls.style.opacity = '1';
            if (info) info.style.opacity = '1';
            if (close) close.style.opacity = '1';
            
            if (hideControlsTimer) {
                clearTimeout(hideControlsTimer);
            }
            
            hideControlsTimer = setTimeout(() => {
                if (controls) controls.style.opacity = '0.3';
                if (info) info.style.opacity = '0.3';
                if (close) close.style.opacity = '0.3';
            }, 3000);
        }
        
        function hideControls() {
            const controls = document.querySelector('.story-controls');
            const info = document.querySelector('.story-info');
            const close = document.querySelector('.close-btn');
            
            if (controls) controls.style.opacity = '0';
            if (info) info.style.opacity = '0';
            if (close) close.style.opacity = '0';
        }
        
        // Show controls on interaction
        storyViewer.addEventListener('click', showControls);
        storyViewer.addEventListener('touchstart', showControls);
        
        // Initialize controls visibility
        showControls();
        
        // Initialize action buttons
        initializeActionButtons();
        
        // Helper functions
        function initializeProgressIndicators() {
            const indicatorsContainer = document.getElementById('progress-indicators');
            if (!indicatorsContainer) return;
            
            // Clear using safe method (not innerHTML)
            while (indicatorsContainer.firstChild) {
                indicatorsContainer.removeChild(indicatorsContainer.firstChild);
            }
            
            for (let i = 0; i < totalSlides; i++) {
                const indicator = document.createElement('div');
                indicator.className = 'progress-indicator';
                if (i === 0) indicator.classList.add('active');
                indicatorsContainer.appendChild(indicator);
            }
        }
        
        function updateProgressIndicators() {
            const indicators = document.querySelectorAll('.progress-indicator');
            indicators.forEach((indicator, index) => {
                indicator.classList.toggle('active', index === currentSlideIndex);
            });
        }
        
        function updateStats() {
            const viewCountEl = document.getElementById('view-count');
            const likeCountEl = document.getElementById('like-count');
            
            if (viewCountEl) viewCountEl.textContent = viewCount;
            if (likeCountEl) likeCountEl.textContent = likeCount;
        }
        
        function initializeActionButtons() {
            // Share button
            const shareBtn = document.getElementById('share-btn');
            if (shareBtn) {
                shareBtn.addEventListener('click', () => {
                    const titleEl = document.querySelector('.story-title');
                    const descEl = document.querySelector('.story-description');
                    
                    if (navigator.share) {
                        navigator.share({
                            title: titleEl ? titleEl.textContent : '',
                            text: descEl ? descEl.textContent : '',
                            url: window.location.href
                        }).catch(err => console.error('Error sharing:', err));
                    } else {
                        // Fallback to copying URL
                        navigator.clipboard.writeText(window.location.href)
                            .then(() => {
                                showNotification('Link copied to clipboard!');
                            })
                            .catch(err => {
                                console.error('Error copying:', err);
                                showNotification('Failed to copy link', 'error');
                            });
                    }
                });
            }
            
            // Like button
            const likeBtn = document.getElementById('like-btn');
            if (likeBtn) {
                likeBtn.addEventListener('click', () => {
                    isLiked = !isLiked;
                    likeCount += isLiked ? 1 : -1;
                    updateStats();
                    
                    likeBtn.style.color = isLiked ? '#ff6b6b' : 'white';
                    likeBtn.style.transform = isLiked ? 'scale(1.2)' : 'scale(1)';
                    
                    showNotification(isLiked ? 'Story liked!' : 'Story unliked!');
                });
            }
            
            // Bookmark button
            const bookmarkBtn = document.getElementById('bookmark-btn');
            if (bookmarkBtn) {
                bookmarkBtn.addEventListener('click', () => {
                    isBookmarked = !isBookmarked;
                    
                    bookmarkBtn.style.color = isBookmarked ? '#ffd700' : 'white';
                    bookmarkBtn.style.transform = isBookmarked ? 'scale(1.2)' : 'scale(1)';
                    
                    showNotification(isBookmarked ? 'Story bookmarked!' : 'Story unbookmarked!');
                });
            }
        }
        
        function showNotification(message, type = 'info') {
            // Use StoryNotifications if available, otherwise create simple notification
            if (typeof StoryNotifications !== 'undefined') {
                StoryNotifications.show(message, type);
            } else {
                const notification = document.createElement('div');
                notification.style.cssText = `
                    position: fixed;
                    top: 20px;
                    right: 20px;
                    background: rgba(0, 0, 0, 0.8);
                    color: white;
                    padding: 1rem 1.5rem;
                    border-radius: 10px;
                    z-index: 10001;
                    font-size: 0.9rem;
                    backdrop-filter: blur(10px);
                    border: 1px solid rgba(255, 255, 255, 0.1);
                `;
                notification.textContent = message;
                
                document.body.appendChild(notification);
                
                setTimeout(() => {
                    notification.remove();
                }, 3000);
            }
        }
        
        function updateSlideTimer() {
            if (slideStartTime) {
                const elapsed = Math.floor((Date.now() - slideStartTime) / 1000);
                const timerElement = document.getElementById('slide-timer');
                if (timerElement) {
                    timerElement.textContent = elapsed;
                }
            }
        }
        
        // Update timer every second
        setInterval(updateSlideTimer, 1000);
    });
})();

