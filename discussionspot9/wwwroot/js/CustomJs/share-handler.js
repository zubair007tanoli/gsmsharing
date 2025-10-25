/**
 * Universal Share Handler
 * Handles all share button functionality across the application
 */

// Global function for inline share buttons (called from _ShareButtonsUnified partial)
function openSharePopup(button) {
    const wrapper = button.closest('.share-inline');
    if (!wrapper) {
        console.error('Share wrapper not found');
        return;
    }
    
    const shareUrl = wrapper.dataset.shareUrl;
    const shareTitle = wrapper.dataset.shareTitle;
    const contentType = wrapper.dataset.contentType;
    const contentId = wrapper.dataset.contentId;
    
    console.log('Opening share popup for:', { shareUrl, shareTitle, contentType, contentId });
    
    if (!shareUrl) {
        console.error('Share URL is missing');
        alert('Unable to share: URL not found');
        return;
    }
    
    showShareModal(shareUrl, shareTitle, contentType, contentId);
}

// Show share modal with all social media options
function showShareModal(url, title, contentType = 'content', contentId = '') {
    console.log('showShareModal called with:', { url, title, contentType, contentId });
    
    const encodedUrl = encodeURIComponent(url);
    const encodedTitle = encodeURIComponent(title);
    
    console.log('Encoded values:', { encodedUrl, encodedTitle });
    
    const modalHTML = `
        <div class="modal fade" id="universalShareModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header border-0">
                        <h5 class="modal-title fw-bold">
                            <i class="fas fa-share-alt me-2"></i> Share ${contentType}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body px-4">
                        <div class="share-options-grid" style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-bottom: 20px;">
                            <a href="https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}" 
                               target="_blank" 
                               rel="noopener" 
                               class="share-option-card text-center text-decoration-none p-3 rounded-3 border"
                               onclick="trackShare('${contentType}', '${contentId}', 'facebook')"
                               style="transition: all 0.2s;">
                                <i class="fab fa-facebook-f fa-2x mb-2" style="color: #1877f2;"></i>
                                <div class="small fw-medium">Facebook</div>
                            </a>
                            <a href="https://twitter.com/intent/tweet?url=${encodedUrl}&text=${encodedTitle}" 
                               target="_blank" 
                               rel="noopener" 
                               class="share-option-card text-center text-decoration-none p-3 rounded-3 border"
                               onclick="trackShare('${contentType}', '${contentId}', 'twitter')"
                               style="transition: all 0.2s;">
                                <i class="fab fa-twitter fa-2x mb-2" style="color: #1da1f2;"></i>
                                <div class="small fw-medium">Twitter</div>
                            </a>
                            <a href="https://www.linkedin.com/sharing/share-offsite/?url=${encodedUrl}" 
                               target="_blank" 
                               rel="noopener" 
                               class="share-option-card text-center text-decoration-none p-3 rounded-3 border"
                               onclick="trackShare('${contentType}', '${contentId}', 'linkedin')"
                               style="transition: all 0.2s;">
                                <i class="fab fa-linkedin-in fa-2x mb-2" style="color: #0077b5;"></i>
                                <div class="small fw-medium">LinkedIn</div>
                            </a>
                            <a href="https://reddit.com/submit?url=${encodedUrl}&title=${encodedTitle}" 
                               target="_blank" 
                               rel="noopener" 
                               class="share-option-card text-center text-decoration-none p-3 rounded-3 border"
                               onclick="trackShare('${contentType}', '${contentId}', 'reddit')"
                               style="transition: all 0.2s;">
                                <i class="fab fa-reddit-alien fa-2x mb-2" style="color: #ff4500;"></i>
                                <div class="small fw-medium">Reddit</div>
                            </a>
                            <a href="https://wa.me/?text=${encodedTitle}%20${encodedUrl}" 
                               target="_blank" 
                               rel="noopener" 
                               class="share-option-card text-center text-decoration-none p-3 rounded-3 border"
                               onclick="trackShare('${contentType}', '${contentId}', 'whatsapp')"
                               style="transition: all 0.2s;">
                                <i class="fab fa-whatsapp fa-2x mb-2" style="color: #25d366;"></i>
                                <div class="small fw-medium">WhatsApp</div>
                            </a>
                            <a href="https://t.me/share/url?url=${encodedUrl}&text=${encodedTitle}" 
                               target="_blank" 
                               rel="noopener" 
                               class="share-option-card text-center text-decoration-none p-3 rounded-3 border"
                               onclick="trackShare('${contentType}', '${contentId}', 'telegram')"
                               style="transition: all 0.2s;">
                                <i class="fab fa-telegram-plane fa-2x mb-2" style="color: #0088cc;"></i>
                                <div class="small fw-medium">Telegram</div>
                            </a>
                        </div>
                        
                        <div class="divider my-3">
                            <hr class="m-0">
                        </div>
                        
                        <div class="input-group">
                            <input type="text" 
                                   class="form-control" 
                                   id="universalShareLinkInput" 
                                   value="${url}" 
                                   readonly 
                                   style="font-size: 0.9rem;">
                            <button class="btn btn-primary" 
                                    type="button" 
                                    onclick="copyShareLink('${url}', '${contentType}', '${contentId}')">
                                <i class="fas fa-copy me-1"></i> Copy
                            </button>
                        </div>
                        
                        <div class="mt-3 text-center">
                            <small class="text-muted">
                                <i class="fas fa-info-circle me-1"></i>
                                Sharing: <strong id="shareUrlDisplay"></strong>
                            </small>
                        </div>
                        <div class="mt-2 text-center">
                            <small class="text-muted">
                                <i class="fas fa-shield-alt me-1"></i>
                                Share responsibly
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    const existingModal = document.getElementById('universalShareModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add modal to body
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('universalShareModal'));
    modal.show();
    
    // Display the URL being shared
    const urlDisplay = document.getElementById('shareUrlDisplay');
    if (urlDisplay) {
        urlDisplay.textContent = url;
    }
    
    // Clean up modal after hiding
    document.getElementById('universalShareModal').addEventListener('hidden.bs.modal', function() {
        this.remove();
    });
    
    // Add hover effects and click handlers to share option cards
    setTimeout(() => {
        document.querySelectorAll('.share-option-card').forEach(card => {
            card.addEventListener('mouseenter', function() {
                this.style.transform = 'translateY(-4px)';
                this.style.boxShadow = '0 6px 16px rgba(0,0,0,0.15)';
            });
            card.addEventListener('mouseleave', function() {
                this.style.transform = 'translateY(0)';
                this.style.boxShadow = 'none';
            });
            
            // Add click handler for proper popup window
            if (card.href && !card.href.startsWith('mailto:')) {
                card.addEventListener('click', function(e) {
                    e.preventDefault();
                    const shareWindow = window.open(
                        this.href,
                        'share-dialog',
                        'width=626,height=436,menubar=no,toolbar=no,resizable=yes,scrollbars=yes'
                    );
                    if (shareWindow) {
                        shareWindow.focus();
                    }
                });
            }
        });
    }, 100);
    
    // Try native share API on mobile
    if (isMobileDevice() && navigator.share) {
        // Show native share first on mobile
        navigator.share({
            title: title,
            url: url
        }).then(() => {
            modal.hide();
            trackShare(contentType, contentId, 'native');
        }).catch(err => {
            // User cancelled or error, show modal instead
            if (err.name !== 'AbortError') {
                console.log('Native share failed, showing modal');
            }
        });
    }
}

// Copy link to clipboard
function copyShareLink(url, contentType, contentId) {
    const input = document.getElementById('universalShareLinkInput');
    if (input) {
        input.select();
    }
    
    navigator.clipboard.writeText(url).then(() => {
        const button = event.target.closest('button');
        if (button) {
            const originalHTML = button.innerHTML;
            button.innerHTML = '<i class="fas fa-check me-1"></i> Copied!';
            button.classList.add('btn-success');
            button.classList.remove('btn-primary');
            
            setTimeout(() => {
                button.innerHTML = originalHTML;
                button.classList.remove('btn-success');
                button.classList.add('btn-primary');
            }, 2000);
        }
        
        // Show toast notification
        showShareToast('Link copied to clipboard!');
        
        // Track copy action
        trackShare(contentType, contentId, 'copy');
    }).catch(err => {
        console.error('Failed to copy:', err);
        showShareToast('Failed to copy link', 'error');
    });
}

// Track share actions
function trackShare(contentType, contentId, platform) {
    if (!contentId) return;
    
    fetch('/api/share/track', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            contentType: contentType,
            contentId: contentId,
            platform: platform,
            timestamp: new Date().toISOString()
        })
    }).catch(err => {
        console.log('Share tracking failed:', err);
    });
}

// Show toast notification
function showShareToast(message, type = 'success') {
    // Remove any existing toast
    const existingToast = document.getElementById('shareToast');
    if (existingToast) {
        existingToast.remove();
    }
    
    const toastHTML = `
        <div class="position-fixed bottom-0 end-0 p-3" style="z-index: 9999;">
            <div id="shareToast" class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : 'success'} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas fa-${type === 'error' ? 'exclamation-circle' : 'check-circle'} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', toastHTML);
    
    const toastElement = document.getElementById('shareToast');
    const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
    toast.show();
    
    // Remove from DOM after hiding
    toastElement.addEventListener('hidden.bs.toast', function() {
        this.closest('.position-fixed').remove();
    });
}

// Check if device is mobile
function isMobileDevice() {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
}

// Initialize share buttons on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeShareButtons();
    initializeSimpleVariantPopups();
});

// Initialize all share buttons
function initializeShareButtons() {
    // Handle inline share triggers
    document.querySelectorAll('.share-inline-trigger').forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            openSharePopup(this);
        });
    });
    
    // Handle share-btn class (for backward compatibility)
    document.querySelectorAll('.share-btn').forEach(button => {
        if (!button.hasAttribute('data-share-initialized')) {
            button.setAttribute('data-share-initialized', 'true');
            button.addEventListener('click', function(e) {
                e.preventDefault();
                
                const postId = this.dataset.postId;
                const postUrl = this.dataset.postUrl || window.location.href;
                const postTitle = this.dataset.postTitle || document.title;
                
                showShareModal(postUrl, postTitle, 'post', postId);
            });
        }
    });
}

// Initialize popup windows for simple variant share links
function initializeSimpleVariantPopups() {
    // Add click handlers to social media links in dropdown popups
    document.querySelectorAll('.share-popup-option').forEach(link => {
        if (link.href && !link.href.startsWith('mailto:') && link.tagName === 'A') {
            link.addEventListener('click', function(e) {
                e.preventDefault();
                const shareWindow = window.open(
                    this.href,
                    'share-dialog',
                    'width=626,height=436,menubar=no,toolbar=no,resizable=yes,scrollbars=yes'
                );
                if (shareWindow) {
                    shareWindow.focus();
                }
            });
        }
    });
}

// Reinitialize share buttons (useful for dynamically loaded content)
window.reinitializeShareButtons = function() {
    initializeShareButtons();
    initializeSimpleVariantPopups();
};

// Override the toggleShareDropdown to reinitialize popups when dropdown opens
const originalToggleShareDropdown = window.toggleShareDropdown;
window.toggleShareDropdown = function(button, event) {
    // Call original function if it exists (from _ShareButtonsUnified partial)
    if (originalToggleShareDropdown) {
        originalToggleShareDropdown(button, event);
    } else {
        // Fallback implementation
        if (event) event.stopPropagation();
        const wrapper = button.closest('.share-simple-wrapper');
        const dropdown = wrapper ? wrapper.querySelector('.share-dropdown-popup') : null;
        
        if (dropdown) {
            // Close all other dropdowns
            document.querySelectorAll('.share-dropdown-popup').forEach(d => {
                if (d !== dropdown) d.style.display = 'none';
            });
            
            // Toggle this dropdown
            const isOpen = dropdown.style.display !== 'none';
            dropdown.style.display = isOpen ? 'none' : 'block';
        }
    }
    
    // Reinitialize popup handlers after dropdown is shown
    setTimeout(initializeSimpleVariantPopups, 50);
};

// Export functions for global use
window.openSharePopup = openSharePopup;
window.showShareModal = showShareModal;
window.copyShareLink = copyShareLink;
window.trackShare = trackShare;

