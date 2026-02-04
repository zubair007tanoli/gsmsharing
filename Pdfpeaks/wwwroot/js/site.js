/**
 * PdfPeaks - Interactive Image and Share Functionality
 */

// Initialize on document ready
document.addEventListener('DOMContentLoaded', function() {
    initializeInteractiveImages();
    initializeShareButtons();
    initializeFileDownloads();
    initializeImagePreview();
});

/**
 * Interactive Image Functionality - Zoom, Pan, Rotate
 */
function initializeInteractiveImages() {
    const interactiveContainers = document.querySelectorAll('.interactive-image-container');
    
    interactiveContainers.forEach(container => {
        setupInteractiveImage(container);
    });
}

function setupInteractiveImage(container) {
    const img = container.querySelector('img');
    if (!img) return;
    
    let scale = 1;
    let rotation = 0;
    let isDragging = false;
    let startX, startY, translateX = 0, translateY = 0;
    
    // Create controls
    const controls = document.createElement('div');
    controls.className = 'image-controls position-absolute top-0 end-0 m-2';
    controls.innerHTML = `
        <div class="btn-group btn-group-sm">
            <button class="btn btn-primary zoom-in" title="Zoom In"><i class="fas fa-plus"></i></button>
            <button class="btn btn-primary zoom-out" title="Zoom Out"><i class="fas fa-minus"></i></button>
            <button class="btn btn-primary rotate" title="Rotate"><i class="fas fa-redo"></i></button>
            <button class="btn btn-primary reset" title="Reset"><i class="fas fa-sync"></i></button>
            <button class="btn btn-primary fullscreen" title="Fullscreen"><i class="fas fa-expand"></i></button>
        </div>
    `;
    container.appendChild(controls);
    
    // Apply transforms to image
    function updateTransform() {
        img.style.transform = `translate(${translateX}px, ${translateY}px) scale(${scale}) rotate(${rotation}deg)`;
        img.style.transition = 'transform 0.2s ease';
    }
    
    // Zoom in
    controls.querySelector('.zoom-in').addEventListener('click', (e) => {
        e.stopPropagation();
        scale = Math.min(scale + 0.25, 4);
        updateTransform();
    });
    
    // Zoom out
    controls.querySelector('.zoom-out').addEventListener('click', (e) => {
        e.stopPropagation();
        scale = Math.max(scale - 0.25, 0.25);
        updateTransform();
    });
    
    // Rotate
    controls.querySelector('.rotate').addEventListener('click', (e) => {
        e.stopPropagation();
        rotation = (rotation + 90) % 360;
        updateTransform();
    });
    
    // Reset
    controls.querySelector('.reset').addEventListener('click', (e) => {
        e.stopPropagation();
        scale = 1;
        rotation = 0;
        translateX = 0;
        translateY = 0;
        updateTransform();
    });
    
    // Fullscreen
    controls.querySelector('.fullscreen').addEventListener('click', (e) => {
        e.stopPropagation();
        if (container.requestFullscreen) {
            container.requestFullscreen();
        } else if (container.webkitRequestFullscreen) {
            container.webkitRequestFullscreen();
        }
    });
    
    // Mouse wheel zoom
    img.addEventListener('wheel', (e) => {
        e.preventDefault();
        const delta = e.deltaY > 0 ? -0.1 : 0.1;
        scale = Math.max(0.25, Math.min(4, scale + delta));
        updateTransform();
    });
    
    // Pan functionality
    img.addEventListener('mousedown', (e) => {
        if (scale > 1) {
            isDragging = true;
            startX = e.clientX - translateX;
            startY = e.clientY - translateY;
            img.style.cursor = 'grabbing';
        }
    });
    
    document.addEventListener('mousemove', (e) => {
        if (isDragging) {
            translateX = e.clientX - startX;
            translateY = e.clientY - startY;
            updateTransform();
        }
    });
    
    document.addEventListener('mouseup', () => {
        isDragging = false;
        img.style.cursor = 'grab';
    });
    
    // Touch support for mobile
    let touchStartX, touchStartY;
    let initialScale = 1;
    
    img.addEventListener('touchstart', (e) => {
        if (e.touches.length === 1) {
            isDragging = true;
            touchStartX = e.touches[0].clientX - translateX;
            touchStartY = e.touches[0].clientY - translateY;
        } else if (e.touches.length === 2) {
            initialScale = scale;
        }
    });
    
    img.addEventListener('touchmove', (e) => {
        e.preventDefault();
        if (e.touches.length === 1 && isDragging) {
            translateX = e.touches[0].clientX - touchStartX;
            translateY = e.touches[0].clientY - touchStartY;
            updateTransform();
        } else if (e.touches.length === 2) {
            const distance = Math.hypot(
                e.touches[0].clientX - e.touches[1].clientX,
                e.touches[0].clientY - e.touches[1].clientY
            );
            scale = Math.max(0.25, Math.min(4, initialScale * distance / 200));
            updateTransform();
        }
    });
    
    img.addEventListener('touchend', () => {
        isDragging = false;
    });
}

/**
 * Share Functionality
 */
function initializeShareButtons() {
    const shareButtons = document.querySelectorAll('[data-share]');
    
    shareButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            e.preventDefault();
            const shareData = {
                title: button.dataset.shareTitle || document.title,
                text: button.dataset.shareText || '',
                url: button.dataset.shareUrl || window.location.href
            };
            
            shareViaPlatform(button.dataset.platform, shareData);
        });
    });
    
    // Initialize native share API where available
    initializeNativeShare();
}

function shareViaPlatform(platform, data) {
    const shareUrls = {
        facebook: `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(data.url)}`,
        twitter: `https://twitter.com/intent/tweet?text=${encodeURIComponent(data.text)}&url=${encodeURIComponent(data.url)}`,
        linkedin: `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(data.url)}`,
        whatsapp: `https://wa.me/?text=${encodeURIComponent(data.text + ' ' + data.url)}`,
        telegram: `https://t.me/share/url?url=${encodeURIComponent(data.url)}&text=${encodeURIComponent(data.text)}`,
        email: `mailto:?subject=${encodeURIComponent(data.title)}&body=${encodeURIComponent(data.text + '\n\n' + data.url)}`
    };
    
    if (platform === 'native') {
        if (navigator.share) {
            navigator.share(data).catch(console.error);
        } else {
            // Fallback to copy to clipboard
            copyToClipboard(data.url);
        }
    } else if (shareUrls[platform]) {
        window.open(shareUrls[platform], '_blank', 'width=600,height=400,noopener,noreferrer');
    }
}

function initializeNativeShare() {
    const nativeShareButtons = document.querySelectorAll('.share-native');
    
    nativeShareButtons.forEach(button => {
        if (!navigator.share) {
            button.style.display = 'none';
        } else {
            button.addEventListener('click', async () => {
                try {
                    await navigator.share({
                        title: button.dataset.title || document.title,
                        text: button.dataset.text || '',
                        url: button.dataset.url || window.location.href
                    });
                } catch (err) {
                    if (err.name !== 'AbortError') {
                        console.error('Share failed:', err);
                    }
                }
            });
        }
    });
}

function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showNotification('Link copied to clipboard!', 'success');
    }).catch(() => {
        // Fallback for older browsers
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);
        showNotification('Link copied to clipboard!', 'success');
    });
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} position-fixed bottom-0 end-0 m-4 shadow-lg`;
    notification.style.zIndex = '9999';
    notification.style.animation = 'slideIn 0.3s ease';
    notification.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close position-absolute end-0 me-2" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 3 seconds
    setTimeout(() => {
        notification.remove();
    }, 3000);
}

/**
 * File Download Functionality
 */
function initializeFileDownloads() {
    const downloadButtons = document.querySelectorAll('[data-download]');
    
    downloadButtons.forEach(button => {
        button.addEventListener('click', (e) => {
            e.preventDefault();
            const fileUrl = button.dataset.download;
            const fileName = button.dataset.filename || 'download';
            downloadFile(fileUrl, fileName);
        });
    });
}

function downloadFile(url, fileName) {
    // Create download link
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.target = '_blank';
    
    // For cross-origin files, use fetch and blob
    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error('Download failed');
            return response.blob();
        })
        .then(blob => {
            const blobUrl = window.URL.createObjectURL(blob);
            const tempLink = document.createElement('a');
            tempLink.href = blobUrl;
            tempLink.download = fileName;
            document.body.appendChild(tempLink);
            tempLink.click();
            document.body.removeChild(tempLink);
            window.URL.revokeObjectURL(blobUrl);
        })
        .catch(() => {
            // Fallback to direct download
            link.setAttribute('download', '');
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        });
}

/**
 * Image Preview with Drag and Drop
 */
function initializeImagePreview() {
    const dropZones = document.querySelectorAll('.drop-zone');
    
    dropZones.forEach(dropZone => {
        setupDropZone(dropZone);
    });
}

function setupDropZone(dropZone) {
    const fileInput = dropZone.querySelector('input[type="file"]');
    
    if (!fileInput) return;
    
    // Drag and drop events
    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('drag-over');
    });
    
    dropZone.addEventListener('dragleave', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
    });
    
    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('drag-over');
        
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            handleFileSelect(files[0], dropZone);
        }
    });
    
    // File input change
    fileInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
            handleFileSelect(e.target.files[0], dropZone);
        }
    });
    
    // Click to select
    dropZone.addEventListener('click', () => {
        fileInput.click();
    });
}

function handleFileSelect(file, dropZone) {
    const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'application/pdf'];
    
    if (!validTypes.includes(file.type)) {
        showNotification('Please select a valid file (JPEG, PNG, GIF, WebP, or PDF)', 'error');
        return;
    }
    
    // Trigger custom event
    const event = new CustomEvent('fileSelected', { detail: { file, dropZone } });
    document.dispatchEvent(event);
    
    // Show preview if image
    if (file.type.startsWith('image/')) {
        showImagePreview(file, dropZone);
    }
}

function showImagePreview(file, dropZone) {
    const reader = new FileReader();
    
    reader.onload = (e) => {
        let preview = dropZone.querySelector('.image-preview');
        
        if (!preview) {
            preview = document.createElement('div');
            preview.className = 'image-preview mt-3';
            dropZone.appendChild(preview);
        }
        
        preview.innerHTML = `
            <div class="interactive-image-container position-relative" style="cursor: grab; overflow: hidden; border-radius: 8px;">
                <img src="${e.target.result}" alt="Preview" class="img-fluid" style="max-height: 300px; object-fit: contain;">
            </div>
            <p class="text-muted mt-2"><small>${file.name} (${formatFileSize(file.size)})</small></p>
        `;
        
        // Initialize interactive functionality for new preview
        setTimeout(() => {
            initializeInteractiveImages();
        }, 100);
    };
    
    reader.readAsDataURL(file);
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * Share Modal Functionality
 */
function showShareModal(title, url, text = '') {
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.id = 'shareModal';
    modal.innerHTML = `
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="fas fa-share-alt me-2"></i>Share ${title}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p class="text-muted">Share this ${title} with others:</p>
                    
                    <div class="d-flex justify-content-center gap-3 mb-4">
                        <button class="btn btn-primary btn-lg rounded-circle share-facebook" data-platform="facebook" 
                            data-share-title="${title}" data-share-url="${url}">
                            <i class="fab fa-facebook-f"></i>
                        </button>
                        <button class="btn btn-info btn-lg rounded-circle text-white share-twitter" data-platform="twitter"
                            data-share-title="${title}" data-share-text="${text}" data-share-url="${url}">
                            <i class="fab fa-twitter"></i>
                        </button>
                        <button class="btn btn-success btn-lg rounded-circle share-whatsapp" data-platform="whatsapp"
                            data-share-title="${title}" data-share-text="${text}" data-share-url="${url}">
                            <i class="fab fa-whatsapp"></i>
                        </button>
                        <button class="btn btn-primary btn-lg rounded-circle share-linkedin" data-platform="linkedin"
                            data-share-url="${url}">
                            <i class="fab fa-linkedin-in"></i>
                        </button>
                        <button class="btn btn-secondary btn-lg rounded-circle share-email" data-platform="email"
                            data-share-title="${title}" data-share-text="${text}" data-share-url="${url}">
                            <i class="fas fa-envelope"></i>
                        </button>
                    </div>
                    
                    <div class="input-group">
                        <input type="text" class="form-control" value="${url}" readonly id="shareUrlInput">
                        <button class="btn btn-outline-primary" onclick="copyToClipboard('${url}')">
                            <i class="fas fa-copy"></i> Copy
                        </button>
                    </div>
                    
                    ${navigator.share ? `
                    <div class="text-center mt-3">
                        <button class="btn btn-outline-primary w-100 share-native" 
                            data-title="${title}" data-text="${text}" data-url="${url}">
                            <i class="fas fa-share-alt me-2"></i>Share using device
                        </button>
                    </div>
                    ` : ''}
                </div>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Initialize share buttons in modal
    setTimeout(() => {
        initializeShareButtons();
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        // Clean up on close
        modal.addEventListener('hidden.bs.modal', () => {
            modal.remove();
        });
    }, 100);
}

// Add animation keyframes dynamically
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    .interactive-image-container img {
        cursor: grab;
    }
    
    .interactive-image-container img:active {
        cursor: grabbing;
    }
    
    .image-controls {
        z-index: 10;
    }
`;
document.head.appendChild(style);

// Export functions for use in views
window.PdfPeaks = {
    showShareModal,
    downloadFile,
    copyToClipboard,
    initializeInteractiveImages,
    initializeShareButtons,
    initializeFileDownloads
};
