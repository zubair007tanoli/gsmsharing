/**
 * Advanced Story Editor Pro - Subscription-worthy features
 * Features: File upload, Stock photos, Advanced animations, Templates, Auto-save
 */

(function() {
    'use strict';
    
    class StoryEditorPro {
        constructor() {
            this.slides = [];
            this.currentSlideIndex = 0;
            this.selectedSlide = null;
            this.selectedElement = null;
            this.elementIdCounter = 0;
            this.zoomLevel = 1;
            this.history = [];
            this.historyIndex = -1;
            this.storyId = null;
            this.isUploading = false;
            this.autoSaveInterval = null;
            
            // Initialize when DOM is ready
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', () => this.init());
            } else {
                this.init();
            }
        }
        
        init() {
            console.log('🎨 Initializing Story Editor Pro...');
            this.loadStoryData();
            this.bindEvents();
            this.setupAutoSave();
            this.setupDragDrop();
            console.log('✅ Story Editor Pro Ready!');
        }
        
        loadStoryData() {
            const storyDataElement = document.getElementById('story-data');
            if (storyDataElement && storyDataElement.textContent) {
                try {
                    const storyData = JSON.parse(storyDataElement.textContent);
                    this.storyId = storyData.StoryId;
                    
                    if (storyData.Title) {
                        document.getElementById('story-title').value = storyData.Title;
                    }
                    
                    if (storyData.Slides && storyData.Slides.length > 0) {
                        this.loadExistingSlides(storyData.Slides);
                    } else {
                        this.addSlide();
                    }
                } catch (e) {
                    console.error('Error loading story:', e);
                    this.addSlide();
                }
            } else {
                this.addSlide();
            }
        }
        
        loadExistingSlides(slidesData) {
            this.slides = slidesData.map(slide => ({
                id: 'slide-' + slide.StorySlideId,
                storySlideId: slide.StorySlideId,
                backgroundColor: slide.BackgroundColor || '#667eea',
                backgroundType: 'color',
                backgroundImage: slide.MediaUrl || '',
                duration: (slide.Duration || 5000) / 1000,
                transition: 'fade',
                elements: []
            }));
            
            this.currentSlideIndex = 0;
            this.selectedSlide = this.slides[0];
            this.updateUI();
        }
        
        bindEvents() {
            // Slide management
            this.on('add-slide-btn', 'click', () => this.addSlide());
            this.on('prev-slide-btn', 'click', () => this.navigateSlide(-1));
            this.on('next-slide-btn', 'click', () => this.navigateSlide(1));
            this.on('duplicate-slide-btn', 'click', () => this.duplicateSlide());
            this.on('delete-slide-btn', 'click', () => this.deleteSlide());
            
            // Element buttons
            this.on('add-text-btn', 'click', () => this.addTextElement());
            this.on('add-heading-btn', 'click', () => this.addHeadingElement());
            this.on('add-image-btn', 'click', () => this.showImagePicker());
            this.on('add-video-btn', 'click', () => this.showVideoPicker());
            
            // Background controls
            this.on('bg-type', 'change', (e) => this.changeBgType(e.target.value));
            this.on('bg-color', 'input', (e) => this.updateSlideBackground(e.target.value));
            this.on('bg-image-upload', 'change', (e) => this.uploadBackgroundImage(e));
            
            // Save/Publish
            this.on('save-draft-btn', 'click', () => this.saveDraft());
            this.on('preview-btn', 'click', () => this.showPreview());
            this.on('publish-btn', 'click', () => this.publishStory());
            
            // Stock photos button
            this.on('stock-photos-btn', 'click', () => this.showStockPhotos());
        }
        
        // ==================== FILE UPLOAD FUNCTIONS ====================
        
        async uploadFile(file, category = 'stories') {
            if (!file) return null;
            
            this.showUploadProgress(true);
            
            const formData = new FormData();
            formData.append('file', file);
            formData.append('category', category);
            
            try {
                const response = await fetch('/api/media/upload', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': this.getAntiForgeryToken()
                    }
                });
                
                if (!response.ok) {
                    throw new Error('Upload failed');
                }
                
                const result = await response.json();
                
                if (result.success) {
                    this.showNotification('✅ File uploaded successfully!', 'success');
                    return result;
                } else {
                    throw new Error(result.errorMessage || 'Upload failed');
                }
            } catch (error) {
                console.error('Upload error:', error);
                this.showNotification('❌ Upload failed: ' + error.message, 'error');
                return null;
            } finally {
                this.showUploadProgress(false);
            }
        }
        
        async uploadBackgroundImage(event) {
            const file = event.target.files[0];
            if (!file) return;
            
            if (!file.type.startsWith('image/')) {
                this.showNotification('Please select an image file', 'error');
                return;
            }
            
            const result = await this.uploadFile(file, 'backgrounds');
            if (result && this.selectedSlide) {
                this.selectedSlide.backgroundImage = result.filePath;
                this.selectedSlide.backgroundType = 'image';
                this.updateCanvas();
                this.saveHistory();
            }
        }
        
        setupDragDrop() {
            const canvasWrapper = document.getElementById('canvas-wrapper');
            if (!canvasWrapper) return;
            
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                canvasWrapper.addEventListener(eventName, (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                }, false);
            });
            
            ['dragenter', 'dragover'].forEach(eventName => {
                canvasWrapper.addEventListener(eventName, () => {
                    canvasWrapper.classList.add('drag-over');
                }, false);
            });
            
            ['dragleave', 'drop'].forEach(eventName => {
                canvasWrapper.addEventListener(eventName, () => {
                    canvasWrapper.classList.remove('drag-over');
                }, false);
            });
            
            canvasWrapper.addEventListener('drop', async (e) => {
                const files = e.dataTransfer.files;
                if (files.length > 0) {
                    await this.handleDroppedFiles(files);
                }
            }, false);
        }
        
        async handleDroppedFiles(files) {
            for (const file of files) {
                if (file.type.startsWith('image/')) {
                    const result = await this.uploadFile(file, 'stories');
                    if (result) {
                        this.addImageElement(result.filePath);
                    }
                } else if (file.type.startsWith('video/')) {
                    const result = await this.uploadFile(file, 'videos');
                    if (result) {
                        this.addVideoElement(result.filePath);
                    }
                }
            }
        }
        
        // ==================== IMAGE PICKER WITH STOCK PHOTOS ====================
        
        showImagePicker() {
            const modal = this.createModal('Add Image', `
                <div class="image-picker">
                    <ul class="nav nav-tabs mb-3" role="tablist">
                        <li class="nav-item">
                            <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#upload-tab">
                                <i class="fas fa-upload me-2"></i>Upload
                            </button>
                        </li>
                        <li class="nav-item">
                            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#url-tab">
                                <i class="fas fa-link me-2"></i>URL
                            </button>
                        </li>
                        <li class="nav-item">
                            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#stock-tab">
                                <i class="fas fa-images me-2"></i>Stock Photos
                            </button>
                        </li>
                    </ul>
                    
                    <div class="tab-content">
                        <!-- Upload Tab -->
                        <div class="tab-pane fade show active" id="upload-tab">
                            <div class="upload-zone" id="image-upload-zone">
                                <i class="fas fa-cloud-upload-alt fa-3x mb-3"></i>
                                <p class="mb-2">Drag & Drop your image here</p>
                                <p class="small text-muted mb-3">or click to browse</p>
                                <input type="file" id="image-file-input" accept="image/*" style="display: none;">
                                <button type="button" class="btn btn-primary" onclick="document.getElementById('image-file-input').click()">
                                    <i class="fas fa-folder-open me-2"></i>Choose File
                                </button>
                            </div>
                            <div id="image-preview" class="mt-3" style="display: none;">
                                <img id="preview-img" style="max-width: 100%; max-height: 300px;">
                            </div>
                        </div>
                        
                        <!-- URL Tab -->
                        <div class="tab-pane fade" id="url-tab">
                            <div class="form-group">
                                <label>Image URL</label>
                                <input type="url" id="image-url-input" class="form-control" placeholder="https://example.com/image.jpg">
                                <small class="form-text text-muted">Enter the URL of an image</small>
                            </div>
                        </div>
                        
                        <!-- Stock Photos Tab -->
                        <div class="tab-pane fade" id="stock-tab">
                            <div class="form-group mb-3">
                                <input type="text" id="stock-search" class="form-control" placeholder="Search for images...">
                            </div>
                            <div id="stock-photos-grid" class="stock-photos-grid">
                                <p class="text-muted text-center">Search for stock photos above</p>
                            </div>
                        </div>
                    </div>
                </div>
            `, [
                {
                    text: 'Add Image',
                    class: 'btn-primary',
                    onClick: () => this.handleImagePickerAdd(modal)
                }
            ]);
            
            this.setupImagePickerEvents(modal);
        }
        
        setupImagePickerEvents(modal) {
            // File upload
            const fileInput = modal.querySelector('#image-file-input');
            const uploadZone = modal.querySelector('#image-upload-zone');
            
            if (fileInput && uploadZone) {
                fileInput.addEventListener('change', async (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        modal.dataset.uploadedFile = JSON.stringify(await this.uploadFile(file, 'stories'));
                        this.showImagePreview(file, modal.querySelector('#preview-img'));
                        modal.querySelector('#image-preview').style.display = 'block';
                    }
                });
                
                ['dragenter', 'dragover'].forEach(evt => {
                    uploadZone.addEventListener(evt, (e) => {
                        e.preventDefault();
                        uploadZone.classList.add('drag-over');
                    });
                });
                
                ['dragleave', 'drop'].forEach(evt => {
                    uploadZone.addEventListener(evt, (e) => {
                        e.preventDefault();
                        uploadZone.classList.remove('drag-over');
                    });
                });
                
                uploadZone.addEventListener('drop', async (e) => {
                    const files = e.dataTransfer.files;
                    if (files.length > 0) {
                        const file = files[0];
                        modal.dataset.uploadedFile = JSON.stringify(await this.uploadFile(file, 'stories'));
                        this.showImagePreview(file, modal.querySelector('#preview-img'));
                        modal.querySelector('#image-preview').style.display = 'block';
                    }
                });
            }
            
            // Stock photos search
            const stockSearch = modal.querySelector('#stock-search');
            if (stockSearch) {
                let searchTimeout;
                stockSearch.addEventListener('input', (e) => {
                    clearTimeout(searchTimeout);
                    searchTimeout = setTimeout(() => {
                        this.searchStockPhotos(e.target.value, modal);
                    }, 500);
                });
            }
        }
        
        async searchStockPhotos(query, modal) {
            if (!query.trim()) return;
            
            const grid = modal.querySelector('#stock-photos-grid');
            grid.innerHTML = '<div class="text-center"><div class="spinner-border" role="status"></div><p class="mt-2">Searching...</p></div>';
            
            // Unsplash API (you'll need to add your API key)
            const photos = await this.fetchUnsplashPhotos(query);
            
            if (photos && photos.length > 0) {
                grid.innerHTML = photos.map(photo => `
                    <div class="stock-photo-item" data-url="${photo.urls.regular}" data-thumbnail="${photo.urls.small}">
                        <img src="${photo.urls.small}" alt="${photo.alt_description || query}">
                        <div class="stock-photo-overlay">
                            <p class="photographer">by ${photo.user.name}</p>
                        </div>
                    </div>
                `).join('');
                
                // Add click handlers
                grid.querySelectorAll('.stock-photo-item').forEach(item => {
                    item.addEventListener('click', () => {
                        grid.querySelectorAll('.stock-photo-item').forEach(i => i.classList.remove('selected'));
                        item.classList.add('selected');
                        modal.dataset.stockPhotoUrl = item.dataset.url;
                    });
                });
            } else {
                grid.innerHTML = '<p class="text-muted text-center">No photos found</p>';
            }
        }
        
        async fetchUnsplashPhotos(query) {
            try {
                // You'll need to add your Unsplash API key
                const apiKey = 'YOUR_UNSPLASH_API_KEY'; // TODO: Move to config
                const response = await fetch(`https://api.unsplash.com/search/photos?query=${encodeURIComponent(query)}&per_page=12&client_id=${apiKey}`);
                const data = await response.json();
                return data.results || [];
            } catch (error) {
                console.error('Error fetching Unsplash photos:', error);
                return [];
            }
        }
        
        handleImagePickerAdd(modal) {
            let imageUrl = null;
            
            // Check uploaded file
            if (modal.dataset.uploadedFile) {
                const uploadResult = JSON.parse(modal.dataset.uploadedFile);
                imageUrl = uploadResult.filePath;
            }
            // Check URL input
            else if (modal.querySelector('#image-url-input').value) {
                imageUrl = modal.querySelector('#image-url-input').value;
            }
            // Check stock photo
            else if (modal.dataset.stockPhotoUrl) {
                imageUrl = modal.dataset.stockPhotoUrl;
            }
            
            if (imageUrl) {
                this.addImageElement(imageUrl);
                bootstrap.Modal.getInstance(modal).hide();
            } else {
                this.showNotification('Please select or upload an image', 'warning');
            }
        }
        
        showImagePreview(file, imgElement) {
            const reader = new FileReader();
            reader.onload = (e) => {
                imgElement.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
        
        // ==================== VIDEO PICKER ====================
        
        showVideoPicker() {
            const modal = this.createModal('Add Video', `
                <div class="video-picker">
                    <ul class="nav nav-tabs mb-3" role="tablist">
                        <li class="nav-item">
                            <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#video-upload-tab">
                                <i class="fas fa-upload me-2"></i>Upload Video
                            </button>
                        </li>
                        <li class="nav-item">
                            <button class="nav-link" data-bs-toggle="tab" data-bs-target="#video-url-tab">
                                <i class="fas fa-link me-2"></i>Video URL
                            </button>
                        </li>
                    </ul>
                    
                    <div class="tab-content">
                        <div class="tab-pane fade show active" id="video-upload-tab">
                            <div class="upload-zone" id="video-upload-zone">
                                <i class="fas fa-video fa-3x mb-3"></i>
                                <p class="mb-2">Drag & Drop your video here</p>
                                <p class="small text-muted mb-3">Maximum size: 100MB</p>
                                <input type="file" id="video-file-input" accept="video/*" style="display: none;">
                                <button type="button" class="btn btn-primary" onclick="document.getElementById('video-file-input').click()">
                                    <i class="fas fa-folder-open me-2"></i>Choose Video
                                </button>
                            </div>
                            <div id="video-preview" class="mt-3" style="display: none;">
                                <video id="preview-video" controls style="max-width: 100%; max-height: 300px;"></video>
                            </div>
                        </div>
                        
                        <div class="tab-pane fade" id="video-url-tab">
                            <div class="form-group">
                                <label>Video URL</label>
                                <input type="url" id="video-url-input" class="form-control" placeholder="https://example.com/video.mp4">
                                <small class="form-text text-muted">Enter the URL of a video (MP4, WebM)</small>
                            </div>
                        </div>
                    </div>
                </div>
            `, [
                {
                    text: 'Add Video',
                    class: 'btn-primary',
                    onClick: () => this.handleVideoPickerAdd(modal)
                }
            ]);
            
            this.setupVideoPickerEvents(modal);
        }
        
        setupVideoPickerEvents(modal) {
            const fileInput = modal.querySelector('#video-file-input');
            const uploadZone = modal.querySelector('#video-upload-zone');
            
            if (fileInput && uploadZone) {
                fileInput.addEventListener('change', async (e) => {
                    const file = e.target.files[0];
                    if (file) {
                        if (file.size > 100 * 1024 * 1024) {
                            this.showNotification('Video size must be less than 100MB', 'error');
                            return;
                        }
                        modal.dataset.uploadedVideo = JSON.stringify(await this.uploadFile(file, 'videos'));
                        this.showVideoPreview(file, modal.querySelector('#preview-video'));
                        modal.querySelector('#video-preview').style.display = 'block';
                    }
                });
                
                ['dragenter', 'dragover'].forEach(evt => {
                    uploadZone.addEventListener(evt, (e) => {
                        e.preventDefault();
                        uploadZone.classList.add('drag-over');
                    });
                });
                
                ['dragleave', 'drop'].forEach(evt => {
                    uploadZone.addEventListener(evt, (e) => {
                        e.preventDefault();
                        uploadZone.classList.remove('drag-over');
                    });
                });
                
                uploadZone.addEventListener('drop', async (e) => {
                    const files = e.dataTransfer.files;
                    if (files.length > 0) {
                        const file = files[0];
                        if (file.size > 100 * 1024 * 1024) {
                            this.showNotification('Video size must be less than 100MB', 'error');
                            return;
                        }
                        modal.dataset.uploadedVideo = JSON.stringify(await this.uploadFile(file, 'videos'));
                        this.showVideoPreview(file, modal.querySelector('#preview-video'));
                        modal.querySelector('#video-preview').style.display = 'block';
                    }
                });
            }
        }
        
        showVideoPreview(file, videoElement) {
            const reader = new FileReader();
            reader.onload = (e) => {
                videoElement.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
        
        handleVideoPickerAdd(modal) {
            let videoUrl = null;
            
            if (modal.dataset.uploadedVideo) {
                const uploadResult = JSON.parse(modal.dataset.uploadedVideo);
                videoUrl = uploadResult.filePath;
            } else if (modal.querySelector('#video-url-input').value) {
                videoUrl = modal.querySelector('#video-url-input').value;
            }
            
            if (videoUrl) {
                this.addVideoElement(videoUrl);
                bootstrap.Modal.getInstance(modal).hide();
            } else {
                this.showNotification('Please select or upload a video', 'warning');
            }
        }
        
        // ==================== ADD ELEMENTS ====================
        
        addTextElement() {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'text',
                text: 'Click to edit text',
                x: 50,
                y: 200,
                fontSize: '24px',
                fontFamily: 'Arial, sans-serif',
                fontWeight: '400',
                color: '#ffffff',
                textAlign: 'center',
                opacity: 1,
                animation: 'fade-in',
                animationDelay: 0,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.selectedElement = element;
            this.updateCanvas();
            this.updatePropertiesPanel();
            this.saveHistory();
        }
        
        addHeadingElement() {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'text',
                text: 'Your Heading',
                x: 30,
                y: 100,
                fontSize: '48px',
                fontFamily: 'Arial, sans-serif',
                fontWeight: '700',
                color: '#ffffff',
                textAlign: 'center',
                textShadow: '2px 2px 4px rgba(0,0,0,0.5)',
                opacity: 1,
                animation: 'slide-in-down',
                animationDelay: 0,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.selectedElement = element;
            this.updateCanvas();
            this.updatePropertiesPanel();
            this.saveHistory();
        }
        
        addImageElement(src) {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'image',
                src: src,
                x: 30,
                y: 150,
                width: 300,
                height: 300,
                opacity: 1,
                animation: 'zoom-in',
                animationDelay: 0,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.selectedElement = element;
            this.updateCanvas();
            this.saveHistory();
        }
        
        addVideoElement(src) {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'video',
                src: src,
                x: 30,
                y: 100,
                width: 300,
                height: 200,
                opacity: 1,
                autoplay: true,
                muted: true,
                loop: true,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.selectedElement = element;
            this.updateCanvas();
            this.saveHistory();
        }
        
        // ==================== SLIDE MANAGEMENT ====================
        
        addSlide() {
            const slide = {
                id: 'slide-' + Date.now(),
                backgroundColor: '#667eea',
                backgroundType: 'color',
                backgroundImage: '',
                duration: 5,
                transition: 'fade',
                elements: []
            };
            
            this.slides.push(slide);
            this.currentSlideIndex = this.slides.length - 1;
            this.selectedSlide = slide;
            this.selectedElement = null;
            
            this.updateUI();
            this.saveHistory();
        }
        
        navigateSlide(direction) {
            const newIndex = this.currentSlideIndex + direction;
            if (newIndex >= 0 && newIndex < this.slides.length) {
                this.currentSlideIndex = newIndex;
                this.selectedSlide = this.slides[newIndex];
                this.selectedElement = null;
                this.updateUI();
            }
        }
        
        duplicateSlide() {
            if (!this.selectedSlide) return;
            
            const duplicated = JSON.parse(JSON.stringify(this.selectedSlide));
            duplicated.id = 'slide-' + Date.now();
            duplicated.elements.forEach(el => {
                el.id = 'element-' + (++this.elementIdCounter);
            });
            
            this.slides.splice(this.currentSlideIndex + 1, 0, duplicated);
            this.currentSlideIndex++;
            this.selectedSlide = duplicated;
            
            this.updateUI();
            this.saveHistory();
        }
        
        deleteSlide() {
            if (this.slides.length === 1) {
                this.showNotification('Cannot delete the last slide', 'warning');
                return;
            }
            
            if (confirm('Delete this slide?')) {
                this.slides.splice(this.currentSlideIndex, 1);
                this.currentSlideIndex = Math.max(0, this.currentSlideIndex - 1);
                this.selectedSlide = this.slides[this.currentSlideIndex];
                this.selectedElement = null;
                
                this.updateUI();
                this.saveHistory();
            }
        }
        
        // ==================== AUTO-SAVE ====================
        
        setupAutoSave() {
            this.autoSaveInterval = setInterval(() => {
                if (this.slides.length > 0) {
                    this.saveDraft(true); // Silent save
                }
            }, 30000); // Auto-save every 30 seconds
        }
        
        async saveDraft(silent = false) {
            if (!silent) {
                this.showNotification('💾 Saving draft...', 'info');
            }
            
            const data = {
                storyId: this.storyId,
                title: document.getElementById('story-title').value || 'Untitled Story',
                slides: this.slides
            };
            
            try {
                const response = await fetch('/api/stories/save-draft', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': this.getAntiForgeryToken()
                    },
                    body: JSON.stringify(data)
                });
                
                const result = await response.json();
                
                if (result.success) {
                    if (!silent) {
                        this.showNotification('✅ Draft saved!', 'success');
                    }
                    if (!this.storyId) {
                        this.storyId = result.storyId;
                    }
                } else {
                    throw new Error(result.message || 'Save failed');
                }
            } catch (error) {
                console.error('Save error:', error);
                if (!silent) {
                    this.showNotification('❌ Failed to save: ' + error.message, 'error');
                }
            }
        }
        
        async publishStory() {
            if (this.slides.length === 0) {
                this.showNotification('Add at least one slide before publishing', 'warning');
                return;
            }
            
            if (confirm('Publish this story? It will be visible to everyone.')) {
                await this.saveDraft();
                
                try {
                    const response = await fetch(`/api/stories/${this.storyId}/publish`, {
                        method: 'POST',
                        headers: {
                            'RequestVerificationToken': this.getAntiForgeryToken()
                        }
                    });
                    
                    const result = await response.json();
                    
                    if (result.success) {
                        this.showNotification('🚀 Story published!', 'success');
                        setTimeout(() => {
                            window.location.href = `/stories/viewer/${result.slug}`;
                        }, 1500);
                    } else {
                        throw new Error(result.message || 'Publish failed');
                    }
                } catch (error) {
                    this.showNotification('❌ Failed to publish: ' + error.message, 'error');
                }
            }
        }
        
        // ==================== UI UPDATE ====================
        
        updateUI() {
            this.updateSlidesList();
            this.updateCanvas();
            this.updatePropertiesPanel();
            this.updateNavButtons();
        }
        
        updateSlidesList() {
            const slidesList = document.getElementById('slides-list');
            if (!slidesList) return;
            
            slidesList.innerHTML = this.slides.map((slide, index) => `
                <div class="slide-thumbnail ${index === this.currentSlideIndex ? 'active' : ''}" 
                     data-slide-index="${index}" onclick="storyEditorPro.selectSlide(${index})">
                    <div class="slide-number">${index + 1}</div>
                    <div class="slide-preview" style="background: ${slide.backgroundColor}; ${slide.backgroundImage ? `background-image: url(${slide.backgroundImage})` : ''}"}>
                        <div class="slide-preview-content">
                            ${slide.elements.length} elements
                        </div>
                    </div>
                </div>
            `).join('');
        }
        
        updateCanvas() {
            const canvas = document.getElementById('story-canvas');
            if (!canvas || !this.selectedSlide) return;
            
            const bgStyle = this.selectedSlide.backgroundType === 'image' && this.selectedSlide.backgroundImage
                ? `background-image: url(${this.selectedSlide.backgroundImage}); background-size: cover; background-position: center;`
                : `background-color: ${this.selectedSlide.backgroundColor};`;
            
            canvas.innerHTML = `
                <div class="story-slide" style="${bgStyle}">
                    ${this.selectedSlide.elements.map(element => this.renderElement(element)).join('')}
                </div>
            `;
            
            this.attachElementHandlers(canvas);
        }
        
        renderElement(element) {
            const style = `
                position: absolute;
                left: ${element.x}px;
                top: ${element.y}px;
                opacity: ${element.opacity || 1};
                z-index: ${element.zIndex || 0};
                ${element.width ? `width: ${element.width}px;` : ''}
                ${element.height ? `height: ${element.height}px;` : ''}
                ${element.fontSize ? `font-size: ${element.fontSize};` : ''}
                ${element.fontFamily ? `font-family: ${element.fontFamily};` : ''}
                ${element.fontWeight ? `font-weight: ${element.fontWeight};` : ''}
                ${element.color ? `color: ${element.color};` : ''}
                ${element.textAlign ? `text-align: ${element.textAlign};` : ''}
                ${element.textShadow ? `text-shadow: ${element.textShadow};` : ''}
            `.trim();
            
            if (element.type === 'text') {
                return `<div class="story-element text-element ${element.id === this.selectedElement?.id ? 'selected' : ''}" 
                             data-element-id="${element.id}" style="${style}">${element.text}</div>`;
            } else if (element.type === 'image') {
                return `<div class="story-element image-element ${element.id === this.selectedElement?.id ? 'selected' : ''}" 
                             data-element-id="${element.id}" style="${style}">
                            <img src="${element.src}" style="width: 100%; height: 100%; object-fit: cover;">
                        </div>`;
            } else if (element.type === 'video') {
                return `<div class="story-element video-element ${element.id === this.selectedElement?.id ? 'selected' : ''}" 
                             data-element-id="${element.id}" style="${style}">
                            <video src="${element.src}" ${element.autoplay ? 'autoplay' : ''} 
                                   ${element.muted ? 'muted' : ''} ${element.loop ? 'loop' : ''}
                                   style="width: 100%; height: 100%; object-fit: cover;"></video>
                        </div>`;
            }
            
            return '';
        }
        
        attachElementHandlers(canvas) {
            canvas.querySelectorAll('.story-element').forEach(el => {
                el.addEventListener('click', (e) => {
                    e.stopPropagation();
                    const elementId = el.dataset.elementId;
                    this.selectedElement = this.selectedSlide.elements.find(e => e.id === elementId);
                    this.updateCanvas();
                    this.updatePropertiesPanel();
                });
            });
        }
        
        updatePropertiesPanel() {
            // Update element properties panel
            const elementProps = document.getElementById('element-properties');
            if (elementProps) {
                elementProps.style.display = this.selectedElement ? 'block' : 'none';
            }
            
            const slideProps = document.getElementById('slide-properties');
            if (slideProps) {
                slideProps.style.display = this.selectedElement ? 'none' : 'block';
            }
        }
        
        updateNavButtons() {
            const prevBtn = document.getElementById('prev-slide-btn');
            const nextBtn = document.getElementById('next-slide-btn');
            const currentSlide = document.getElementById('current-slide');
            const totalSlides = document.getElementById('total-slides');
            
            if (prevBtn) prevBtn.disabled = this.currentSlideIndex === 0;
            if (nextBtn) nextBtn.disabled = this.currentSlideIndex === this.slides.length - 1;
            if (currentSlide) currentSlide.textContent = this.currentSlideIndex + 1;
            if (totalSlides) totalSlides.textContent = this.slides.length;
        }
        
        selectSlide(index) {
            this.currentSlideIndex = index;
            this.selectedSlide = this.slides[index];
            this.selectedElement = null;
            this.updateUI();
        }
        
        changeBgType(type) {
            if (!this.selectedSlide) return;
            
            this.selectedSlide.backgroundType = type;
            
            document.getElementById('bg-color-group').classList.toggle('d-none', type !== 'color');
            document.getElementById('bg-gradient-group')?.classList.toggle('d-none', type !== 'gradient');
            document.getElementById('bg-image-group').classList.toggle('d-none', type !== 'image');
            
            this.updateCanvas();
        }
        
        updateSlideBackground(color) {
            if (!this.selectedSlide) return;
            
            this.selectedSlide.backgroundColor = color;
            document.getElementById('bg-color-text').value = color;
            this.updateCanvas();
            this.saveHistory();
        }
        
        // ==================== HISTORY ====================
        
        saveHistory() {
            const state = JSON.stringify(this.slides);
            
            if (this.historyIndex < this.history.length - 1) {
                this.history = this.history.slice(0, this.historyIndex + 1);
            }
            
            this.history.push(state);
            this.historyIndex++;
            
            // Limit history to 50 states
            if (this.history.length > 50) {
                this.history.shift();
                this.historyIndex--;
            }
        }
        
        // ==================== UTILITIES ====================
        
        createModal(title, content, buttons = []) {
            const modalId = 'modal-' + Date.now();
            const modalHtml = `
                <div class="modal fade" id="${modalId}" tabindex="-1">
                    <div class="modal-dialog modal-lg">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${title}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">${content}</div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                                ${buttons.map((btn, i) => `
                                    <button type="button" class="btn ${btn.class}" data-action="${i}">${btn.text}</button>
                                `).join('')}
                            </div>
                        </div>
                    </div>
                </div>
            `;
            
            document.body.insertAdjacentHTML('beforeend', modalHtml);
            const modalElement = document.getElementById(modalId);
            const modalInstance = new bootstrap.Modal(modalElement);
            
            buttons.forEach((btn, i) => {
                modalElement.querySelector(`[data-action="${i}"]`).addEventListener('click', () => {
                    btn.onClick(modalElement);
                });
            });
            
            modalElement.addEventListener('hidden.bs.modal', () => {
                modalElement.remove();
            });
            
            modalInstance.show();
            return modalElement;
        }
        
        showNotification(message, type = 'info') {
            const toast = document.createElement('div');
            toast.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0`;
            toast.setAttribute('role', 'alert');
            toast.innerHTML = `
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            `;
            
            let container = document.querySelector('.toast-container');
            if (!container) {
                container = document.createElement('div');
                container.className = 'toast-container position-fixed top-0 end-0 p-3';
                document.body.appendChild(container);
            }
            
            container.appendChild(toast);
            const toastInstance = new bootstrap.Toast(toast);
            toastInstance.show();
        }
        
        showUploadProgress(show) {
            let overlay = document.getElementById('upload-overlay');
            if (!overlay && show) {
                overlay = document.createElement('div');
                overlay.id = 'upload-overlay';
                overlay.className = 'upload-overlay';
                overlay.innerHTML = `
                    <div class="upload-progress">
                        <div class="spinner-border text-primary" role="status"></div>
                        <p class="mt-3">Uploading...</p>
                    </div>
                `;
                document.body.appendChild(overlay);
            } else if (overlay && !show) {
                overlay.remove();
            }
        }
        
        getAntiForgeryToken() {
            return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
        }
        
        on(id, event, handler) {
            const element = document.getElementById(id);
            if (element) {
                element.addEventListener(event, handler);
            }
        }
        
        showPreview() {
            this.showNotification('Preview feature coming soon!', 'info');
        }
    }
    
    // Initialize the editor
    window.storyEditorPro = new StoryEditorPro();
})();

