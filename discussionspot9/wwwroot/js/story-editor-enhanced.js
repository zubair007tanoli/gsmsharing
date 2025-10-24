// Enhanced Story Editor Features
// This file extends the base story-editor.js with advanced features

(function() {
    'use strict';
    
    console.log('Enhanced Story Editor Loading...');
    
    // Wait for both DOM and Bootstrap to be ready
    function initWhenReady() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initWhenReady);
            return;
        }
        
        // Check if Bootstrap is available
        if (typeof bootstrap === 'undefined') {
            console.warn('Bootstrap not loaded, retrying...');
            setTimeout(initWhenReady, 100);
            return;
        }
        
        // Initialize enhanced features
        setTimeout(() => {
            initEnhancedFeatures();
        }, 300);
    }
    
    initWhenReady();
    
    function initEnhancedFeatures() {
        console.log('Initializing Enhanced Features...');
        
        // Sticker System
        initStickerSystem();
        
        // Shape Selector
        initShapeSelector();
        
        // File Upload
        initFileUpload();
        
        // Link Handler
        initLinkHandler();
        
        console.log('Enhanced Features Initialized');
    }
    
    // Sticker System with Categories
    function initStickerSystem() {
        const stickerBtn = document.getElementById('add-sticker-btn');
        if (!stickerBtn) return;
        
        stickerBtn.addEventListener('click', () => {
            const modal = new bootstrap.Modal(document.getElementById('stickerModal'));
            modal.show();
            loadStickers('emojis');
        });
        
        // Tab switching
        document.querySelectorAll('.sticker-tab').forEach(tab => {
            tab.addEventListener('click', (e) => {
                document.querySelectorAll('.sticker-tab').forEach(t => t.classList.remove('active'));
                e.target.classList.add('active');
                loadStickers(e.target.dataset.category);
            });
        });
    }
    
    function loadStickers(category) {
        const grid = document.getElementById('sticker-grid');
        if (!grid) return;
        
        const stickers = {
            emojis: ['😀', '😃', '😄', '😁', '😅', '😂', '🤣', '😊', '😇', '🙂', '🙃', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓', '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟', '😕', '🙁', '☹️', '😣', '😖', '😫', '😩', '🥺', '😢', '😭', '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶', '😱', '😨', '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐', '😑', '😬', '🙄', '😯', '😦', '😧', '😮', '😲', '🥱', '😴', '🤤', '😪', '😵', '🤐', '🥴', '🤢', '🤮', '🤧', '😷', '🤒', '🤕', '🤑', '🤠', '😈', '👿', '👹', '👺', '🤡', '💩', '👻', '💀', '☠️', '👽', '👾', '🤖', '🎃', '😺', '😸', '😹', '😻', '😼', '😽', '🙀', '😿', '😾'],
            icons: ['⭐', '🔥', '💯', '✨', '💫', '🌟', '💥', '💢', '💖', '💝', '💗', '💓', '💕', '💞', '💟', '❣️', '💔', '❤️', '🧡', '💛', '💚', '💙', '💜', '🖤', '🤍', '🤎', '💋', '💌', '💍', '💎', '👑', '🎁', '🎀', '🎊', '🎉', '🎈', '🎆', '🎇', '💐', '🌸', '💮', '🏵️', '🌹', '🥀', '🌺', '🌻', '🌼', '🌷', '🌱', '🌿', '🍀', '🎄', '🎃', '🎂', '🍰', '🧁', '🍭', '🍬', '🍫', '🍪', '🍩', '🍨', '🍧', '🍦', '🥧', '🍰'],
            shapes: ['⚪', '⚫', '🔴', '🔵', '🟢', '🟡', '🟠', '🟣', '🔶', '🔷', '🔺', '🔻', '💠', '💫', '🌟', '⭐', '✨'],
            badges: ['🏆', '🥇', '🥈', '🥉', '🎖️', '🏅', '🎗️', '🎫', '🎟️', '🎀', '🎁', '🎂', '🎃', '🎄', '🎅', '🎆', '🎇', '🎈', '🎉', '🎊']
        };
        
        const categoryStickers = stickers[category] || stickers.emojis;
        
        grid.innerHTML = categoryStickers.map(sticker => 
            `<div class="sticker-item" data-sticker="${sticker}">${sticker}</div>`
        ).join('');
        
        // Add click handlers
        grid.querySelectorAll('.sticker-item').forEach(item => {
            item.addEventListener('click', () => {
                addStickerToSlide(item.dataset.sticker);
                bootstrap.Modal.getInstance(document.getElementById('stickerModal')).hide();
            });
        });
    }
    
    function addStickerToSlide(sticker) {
        if (!window.storyEditor || !window.storyEditor.selectedSlide) return;
        
        const element = {
            id: 'element-' + (++window.storyEditor.elementIdCounter),
            type: 'text',
            text: sticker,
            x: 160,
            y: 300,
            width: 60,
            height: 60,
            fontSize: '48px',
            fontFamily: 'Arial, sans-serif',
            opacity: 1,
            zIndex: window.storyEditor.selectedSlide.elements.length
        };
        
        window.storyEditor.selectedSlide.elements.push(element);
        window.storyEditor.updateCanvas();
        window.storyEditor.saveHistory();
    }
    
    // Shape Selector
    function initShapeSelector() {
        const shapeBtn = document.getElementById('add-shape-btn');
        if (!shapeBtn) return;
        
        shapeBtn.addEventListener('click', () => {
            const modal = new bootstrap.Modal(document.getElementById('shapeModal'));
            modal.show();
        });
        
        document.querySelectorAll('.shape-item').forEach(item => {
            item.addEventListener('click', () => {
                addShapeToSlide(item.dataset.shape);
                bootstrap.Modal.getInstance(document.getElementById('shapeModal')).hide();
            });
        });
    }
    
    function addShapeToSlide(shapeType) {
        if (!window.storyEditor || !window.storyEditor.selectedSlide) return;
        
        const element = {
            id: 'element-' + (++window.storyEditor.elementIdCounter),
            type: 'shape',
            shape: shapeType,
            x: 130,
            y: 270,
            width: 100,
            height: 100,
            backgroundColor: '#ff6b6b',
            opacity: 1,
            zIndex: window.storyEditor.selectedSlide.elements.length
        };
        
        window.storyEditor.selectedSlide.elements.push(element);
        window.storyEditor.updateCanvas();
        window.storyEditor.saveHistory();
    }
    
    // File Upload
    function initFileUpload() {
        const imageBtn = document.getElementById('add-image-btn');
        const videoBtn = document.getElementById('add-video-btn');
        const uploadBtn = document.getElementById('upload-btn');
        const fileInput = document.getElementById('file-input');
        const uploadArea = document.getElementById('upload-area');
        
        if (imageBtn) {
            imageBtn.addEventListener('click', () => openUploadModal('image'));
        }
        
        if (videoBtn) {
            videoBtn.addEventListener('click', () => openUploadModal('video'));
        }
        
        if (uploadArea) {
            uploadArea.addEventListener('click', () => fileInput?.click());
            uploadArea.addEventListener('dragover', (e) => {
                e.preventDefault();
                uploadArea.style.background = '#f0f8ff';
            });
            uploadArea.addEventListener('dragleave', () => {
                uploadArea.style.background = '';
            });
            uploadArea.addEventListener('drop', (e) => {
                e.preventDefault();
                uploadArea.style.background = '';
                handleFiles(e.dataTransfer.files);
            });
        }
        
        if (fileInput) {
            fileInput.addEventListener('change', (e) => handleFiles(e.target.files));
        }
        
        if (uploadBtn) {
            uploadBtn.addEventListener('click', () => {
                alert('Upload functionality - integrate with your backend');
            });
        }
    }
    
    function openUploadModal(type) {
        const modal = new bootstrap.Modal(document.getElementById('uploadModal'));
        modal.show();
        const fileInput = document.getElementById('file-input');
        if (fileInput) {
            fileInput.setAttribute('accept', type === 'image' ? 'image/*' : 'video/*');
        }
    }
    
    function handleFiles(files) {
        const preview = document.getElementById('upload-preview');
        if (!preview) return;
        
        Array.from(files).forEach(file => {
            const reader = new FileReader();
            reader.onload = (e) => {
                const div = document.createElement('div');
                div.className = 'upload-preview-item';
                
                if (file.type.startsWith('image/')) {
                    div.innerHTML = `<img src="${e.target.result}"><button class="remove-btn">×</button>`;
                } else if (file.type.startsWith('video/')) {
                    div.innerHTML = `<video src="${e.target.result}"></video><button class="remove-btn">×</button>`;
                }
                
                preview.appendChild(div);
            };
            reader.readAsDataURL(file);
        });
    }
    
    // Link Handler
    function initLinkHandler() {
        const linkBtn = document.getElementById('add-link-btn');
        if (!linkBtn) return;
        
        linkBtn.addEventListener('click', () => {
            const modal = new bootstrap.Modal(document.getElementById('linkModal'));
            modal.show();
        });
        
        const addLinkBtn = document.getElementById('add-link-btn-modal');
        if (addLinkBtn) {
            addLinkBtn.addEventListener('click', () => {
                const url = document.getElementById('link-url')?.value;
                const text = document.getElementById('link-text')?.value;
                const type = document.getElementById('link-type')?.value;
                
                if (url && text) {
                    addLinkToSlide(url, text, type);
                    bootstrap.Modal.getInstance(document.getElementById('linkModal')).hide();
                }
            });
        }
    }
    
    function addLinkToSlide(url, text, type) {
        if (!window.storyEditor || !window.storyEditor.selectedSlide) return;
        
        const element = {
            id: 'element-' + (++window.storyEditor.elementIdCounter),
            type: 'link',
            text: text,
            url: url,
            linkType: type,
            x: 40,
            y: 250,
            width: 280,
            height: 50,
            fontSize: '20px',
            fontFamily: 'Arial, sans-serif',
            color: '#0066cc',
            textDecoration: 'underline',
            opacity: 1,
            zIndex: window.storyEditor.selectedSlide.elements.length
        };
        
        window.storyEditor.selectedSlide.elements.push(element);
        window.storyEditor.updateCanvas();
        window.storyEditor.saveHistory();
    }
    
})();

