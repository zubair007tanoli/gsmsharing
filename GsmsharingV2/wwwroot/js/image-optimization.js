// Image Optimization and Lazy Loading
(function() {
    'use strict';

    // Lazy load images
    function initLazyLoading() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.classList.add('loaded');
                            observer.unobserve(img);
                        }
                    }
                });
            }, {
                rootMargin: '50px'
            });

            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        } else {
            // Fallback for browsers without IntersectionObserver
            document.querySelectorAll('img[data-src]').forEach(img => {
                img.src = img.dataset.src;
            });
        }
    }

    // Lightbox functionality
    function initLightbox() {
        document.querySelectorAll('.lightbox-image').forEach(img => {
            img.addEventListener('click', function() {
                openLightbox(this.src, this.alt);
            });
        });
    }

    function openLightbox(src, alt) {
        const lightbox = document.createElement('div');
        lightbox.className = 'lightbox-overlay';
        lightbox.innerHTML = `
            <div class="lightbox-content">
                <button class="lightbox-close" aria-label="Close">&times;</button>
                <img src="${src}" alt="${alt}" class="lightbox-image-full">
                <div class="lightbox-caption">${alt}</div>
            </div>
        `;
        
        document.body.appendChild(lightbox);
        document.body.style.overflow = 'hidden';
        
        lightbox.addEventListener('click', function(e) {
            if (e.target === lightbox || e.target.classList.contains('lightbox-close')) {
                closeLightbox(lightbox);
            }
        });
        
        // Close on Escape key
        document.addEventListener('keydown', function escapeHandler(e) {
            if (e.key === 'Escape') {
                closeLightbox(lightbox);
                document.removeEventListener('keydown', escapeHandler);
            }
        });
    }

    function closeLightbox(lightbox) {
        lightbox.classList.add('fade-out');
        setTimeout(() => {
            document.body.removeChild(lightbox);
            document.body.style.overflow = '';
        }, 300);
    }

    // Progressive image loading
    function initProgressiveLoading() {
        document.querySelectorAll('img[data-progressive]').forEach(img => {
            const lowResSrc = img.src;
            const highResSrc = img.dataset.progressive;
            
            // Load high-res image
            const highResImg = new Image();
            highResImg.onload = function() {
                img.src = highResSrc;
                img.classList.add('loaded');
            };
            highResImg.src = highResSrc;
        });
    }

    // Image optimization helper
    function optimizeImageUrl(url, width, height, quality = 80) {
        // If using a CDN or image service, add optimization parameters
        if (url.includes('cloudinary.com') || url.includes('imgix.net')) {
            const separator = url.includes('?') ? '&' : '?';
            return `${url}${separator}w=${width}&h=${height}&q=${quality}&auto=format`;
        }
        return url;
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initLazyLoading();
            initLightbox();
            initProgressiveLoading();
        });
    } else {
        initLazyLoading();
        initLightbox();
        initProgressiveLoading();
    }

    // Export functions
    window.imageOptimization = {
        optimizeUrl: optimizeImageUrl,
        openLightbox: openLightbox
    };
})();
