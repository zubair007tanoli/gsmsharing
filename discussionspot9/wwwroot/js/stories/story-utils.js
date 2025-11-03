/**
 * Story Viewer Utilities
 * Enhanced story playback functionality with proper error handling
 */

class StoryViewer {
    constructor() {
        this.currentSlide = 0;
        this.totalSlides = 0;
        this.isPlaying = false;
        this.slideTimer = null;
        this.slides = [];
        this.progressBar = null;
        this.progressIndicators = null;
        
        this.init();
    }
    
    init() {
        try {
            // Initialize DOM elements
            this.slides = document.querySelectorAll('.story-slide');
            this.totalSlides = this.slides.length;
            this.progressBar = document.getElementById('progress-bar');
            this.progressIndicators = document.getElementById('progress-indicators');
            
            // Update total slides counter
            const totalSlidesSpan = document.getElementById('total-slides');
            if (totalSlidesSpan) {
                totalSlidesSpan.textContent = this.totalSlides;
            }
            
            // Create progress indicators
            this.createProgressIndicators();
            
            // Show first slide
            if (this.totalSlides > 0) {
                this.showSlide(0);
                this.startSlideTimer();
                this.isPlaying = true;
            }
            
            // Bind event listeners
            this.bindEvents();
            
            console.log(`✅ Story viewer initialized with ${this.totalSlides} slides`);
        } catch (error) {
            console.error('❌ Error initializing story viewer:', error);
        }
    }
    
    createProgressIndicators() {
        if (!this.progressIndicators) return;
        
        this.progressIndicators.innerHTML = '';
        for (let i = 0; i < this.totalSlides; i++) {
            const indicator = document.createElement('div');
            indicator.className = 'progress-indicator';
            indicator.dataset.slide = i;
            this.progressIndicators.appendChild(indicator);
        }
    }
    
    showSlide(index) {
        if (index < 0 || index >= this.totalSlides) return;
        
        // Hide all slides
        this.slides.forEach((slide, i) => {
            slide.classList.toggle('active', i === index);
        });
        
        // Update progress indicators
        const indicators = this.progressIndicators?.querySelectorAll('.progress-indicator');
        if (indicators) {
            indicators.forEach((indicator, i) => {
                indicator.classList.toggle('active', i <= index);
                indicator.classList.toggle('current', i === index);
            });
        }
        
        // Update current slide counter
        const currentSlideSpan = document.getElementById('current-slide');
        if (currentSlideSpan) {
            currentSlideSpan.textContent = index + 1;
        }
        
        this.currentSlide = index;
        
        // Handle video autoplay
        this.handleMediaPlayback(this.slides[index]);
        
        console.log(`📍 Showing slide ${index + 1}/${this.totalSlides}`);
    }
    
    handleMediaPlayback(slide) {
        // Pause all videos first
        document.querySelectorAll('.story-slide video').forEach(video => {
            video.pause();
        });
        
        // Play video in current slide if exists
        const video = slide.querySelector('video');
        if (video) {
            video.currentTime = 0;
            video.play().catch(e => console.warn('Video autoplay failed:', e));
        }
    }
    
    nextSlide() {
        if (this.currentSlide < this.totalSlides - 1) {
            this.clearSlideTimer();
            this.showSlide(this.currentSlide + 1);
            if (this.isPlaying) {
                this.startSlideTimer();
            }
        } else {
            // Story ended - restart or close
            this.clearSlideTimer();
            this.showSlide(0);
            this.togglePlayPause(); // Pause at end
        }
    }
    
    prevSlide() {
        if (this.currentSlide > 0) {
            this.clearSlideTimer();
            this.showSlide(this.currentSlide - 1);
            if (this.isPlaying) {
                this.startSlideTimer();
            }
        }
    }
    
    togglePlayPause() {
        this.isPlaying = !this.isPlaying;
        
        const playPauseBtn = document.getElementById('play-pause-btn');
        const icon = playPauseBtn?.querySelector('i');
        
        if (this.isPlaying) {
            icon?.setAttribute('class', 'fas fa-pause');
            this.startSlideTimer();
        } else {
            icon?.setAttribute('class', 'fas fa-play');
            this.clearSlideTimer();
        }
    }
    
    startSlideTimer() {
        this.clearSlideTimer();
        
        const currentSlideElement = this.slides[this.currentSlide];
        const duration = parseInt(currentSlideElement?.dataset.duration) || 5000;
        
        // Update progress bar
        this.animateProgressBar(duration);
        
        // Update timer display
        this.updateTimerDisplay(duration);
        
        this.slideTimer = setTimeout(() => {
            this.nextSlide();
        }, duration);
    }
    
    clearSlideTimer() {
        if (this.slideTimer) {
            clearTimeout(this.slideTimer);
            this.slideTimer = null;
        }
        
        // Stop progress bar animation
        if (this.progressBar) {
            this.progressBar.style.animation = 'none';
        }
    }
    
    animateProgressBar(duration) {
        if (!this.progressBar) return;
        
        this.progressBar.style.animation = 'none';
        // Force reflow
        this.progressBar.offsetHeight;
        this.progressBar.style.animation = `progress-fill ${duration}ms linear`;
    }
    
    updateTimerDisplay(duration) {
        const timerDisplay = document.getElementById('slide-timer');
        if (!timerDisplay) return;
        
        let remaining = Math.ceil(duration / 1000);
        timerDisplay.textContent = remaining;
        
        const timerInterval = setInterval(() => {
            remaining--;
            timerDisplay.textContent = Math.max(0, remaining);
            
            if (remaining <= 0) {
                clearInterval(timerInterval);
            }
        }, 1000);
    }
    
    bindEvents() {
        // Navigation buttons
        const prevBtn = document.getElementById('prev-btn');
        const nextBtn = document.getElementById('next-btn');
        const playPauseBtn = document.getElementById('play-pause-btn');
        const closeBtn = document.getElementById('close-btn');
        
        prevBtn?.addEventListener('click', () => this.prevSlide());
        nextBtn?.addEventListener('click', () => this.nextSlide());
        playPauseBtn?.addEventListener('click', () => this.togglePlayPause());
        closeBtn?.addEventListener('click', () => this.closeViewer());
        
        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            switch(e.key) {
                case 'ArrowRight':
                case ' ':
                    e.preventDefault();
                    this.nextSlide();
                    break;
                case 'ArrowLeft':
                    e.preventDefault();
                    this.prevSlide();
                    break;
                case 'Escape':
                    this.closeViewer();
                    break;
                case 'p':
                case 'P':
                    e.preventDefault();
                    this.togglePlayPause();
                    break;
            }
        });
        
        // Touch/swipe navigation
        this.bindTouchEvents();
        
        // Progress indicator clicks
        const indicators = this.progressIndicators?.querySelectorAll('.progress-indicator');
        indicators?.forEach((indicator, index) => {
            indicator.addEventListener('click', () => {
                this.clearSlideTimer();
                this.showSlide(index);
                if (this.isPlaying) {
                    this.startSlideTimer();
                }
            });
        });
        
        // Story info toggle functionality
        this.bindStoryInfoToggle();
    }
    
    bindStoryInfoToggle() {
        const storyInfo = document.getElementById('story-info');
        const storyInfoToggle = document.getElementById('story-info-toggle');
        const storyInfoClose = document.getElementById('story-info-close');
        
        if (storyInfoToggle) {
            storyInfoToggle.addEventListener('click', (e) => {
                e.stopPropagation();
                storyInfo?.classList.toggle('expanded');
            });
        }
        
        if (storyInfoClose) {
            storyInfoClose.addEventListener('click', (e) => {
                e.stopPropagation();
                storyInfo?.classList.remove('expanded');
            });
        }
        
        // Close story info when clicking outside
        document.addEventListener('click', (e) => {
            if (storyInfo && storyInfo.classList.contains('expanded')) {
                const clickedInside = storyInfo.contains(e.target);
                if (!clickedInside && e.target !== storyInfoToggle) {
                    storyInfo.classList.remove('expanded');
                }
            }
        });
    }
    
    bindTouchEvents() {
        let startX = 0;
        let startY = 0;
        
        const storyViewer = document.getElementById('story-viewer');
        if (!storyViewer) return;
        
        storyViewer.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
        }, { passive: true });
        
        storyViewer.addEventListener('touchend', (e) => {
            const endX = e.changedTouches[0].clientX;
            const endY = e.changedTouches[0].clientY;
            const diffX = startX - endX;
            const diffY = startY - endY;
            
            // Only handle horizontal swipes
            if (Math.abs(diffX) > Math.abs(diffY) && Math.abs(diffX) > 50) {
                if (diffX > 0) {
                    this.nextSlide(); // Swipe left - next slide
                } else {
                    this.prevSlide(); // Swipe right - previous slide
                }
            }
        }, { passive: true });
    }
    
    closeViewer() {
        this.clearSlideTimer();
        
        // Pause all videos
        document.querySelectorAll('.story-slide video').forEach(video => {
            video.pause();
        });
        
        // Go back in history or redirect
        if (window.history.length > 1) {
            window.history.back();
        } else {
            window.location.href = '/stories';
        }
    }
    
    // Public API methods
    goToSlide(index) {
        this.clearSlideTimer();
        this.showSlide(index);
        if (this.isPlaying) {
            this.startSlideTimer();
        }
    }
    
    play() {
        if (!this.isPlaying) {
            this.togglePlayPause();
        }
    }
    
    pause() {
        if (this.isPlaying) {
            this.togglePlayPause();
        }
    }
}

// Initialize story viewer when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.storyViewer = new StoryViewer();
});

// CSS animations for progress bar
const style = document.createElement('style');
style.textContent = `
@keyframes progress-fill {
    from { width: 0%; }
    to { width: 100%; }
}

.progress-indicator {
    cursor: pointer;
    transition: all 0.2s ease;
}

.progress-indicator:hover {
    transform: scale(1.1);
    opacity: 0.8;
}

.progress-indicator.current {
    background-color: #fff !important;
    box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.3);
}
`;
document.head.appendChild(style);
