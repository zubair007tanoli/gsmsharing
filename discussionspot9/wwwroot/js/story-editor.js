(function() {
    'use strict';
    
    console.log('Story Editor Script Loading...');
    
    class StoryEditor {
        constructor() {
            console.log('StoryEditor Constructor Called');
            this.slides = [];
            this.currentSlideIndex = 0;
            this.selectedSlide = null;
            this.selectedElement = null;
            this.elementIdCounter = 0;
            this.zoomLevel = 1;
            this.history = [];
            this.historyIndex = -1;
            
            // Wait for DOM to be ready
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', () => this.init());
            } else {
                this.init();
            }
        }
        
        init() {
            console.log('Initializing Story Editor...');
            this.bindAllEvents();
            
            // Load existing story data if available
            const storyDataElement = document.getElementById('story-data');
            if (storyDataElement && storyDataElement.textContent) {
                try {
                    const storyData = JSON.parse(storyDataElement.textContent);
                    if (storyData && storyData.Slides && storyData.Slides.length > 0) {
                        console.log('Loading existing story with', storyData.Slides.length, 'slides');
                        this.loadExistingStory(storyData);
                    } else {
                        this.addInitialSlide();
                    }
                } catch (e) {
                    console.error('Error parsing story data:', e);
                    this.addInitialSlide();
                }
            } else {
                this.addInitialSlide();
            }
            
            console.log('Story Editor Initialized Successfully');
        }
        
        loadExistingStory(storyData) {
            // Populate title
            if (storyData.Title) {
                const titleInput = document.getElementById('story-title');
                if (titleInput) titleInput.value = storyData.Title;
            }
            
            // Convert slides to editor format
            this.slides = storyData.Slides.map((slide, index) => ({
                id: 'slide-' + slide.StorySlideId,
                backgroundColor: slide.BackgroundColor || '#667eea',
                backgroundType: 'color',
                duration: slide.Duration / 1000 || 5,
                transition: 'fade',
                elements: [],
                // Store original data for reference
                originalData: slide
            }));
            
            if (this.slides.length > 0) {
                this.currentSlideIndex = 0;
                this.selectedSlide = this.slides[0];
                this.updateUI();
            } else {
                this.addInitialSlide();
            }
        }
        
        bindAllEvents() {
            console.log('Binding events...');
            
            // Add slide button
            const addSlideBtn = document.getElementById('add-slide-btn');
            if (addSlideBtn) {
                addSlideBtn.addEventListener('click', () => {
                    console.log('Add slide clicked');
                    this.addSlide();
                });
            }
            
            // Navigation
            document.getElementById('prev-slide-btn')?.addEventListener('click', () => this.prevSlide());
            document.getElementById('next-slide-btn')?.addEventListener('click', () => this.nextSlide());
            document.getElementById('duplicate-slide-btn')?.addEventListener('click', () => this.duplicateSlide());
            document.getElementById('delete-slide-btn')?.addEventListener('click', () => this.deleteSlide());
            
            // AI Suggestions
            document.getElementById('ai-suggest-btn')?.addEventListener('click', () => this.getAISuggestions());
            
            // Templates
            document.querySelectorAll('.template-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    const template = e.currentTarget.dataset.template;
                    this.applyTemplate(template);
                });
            });
            
            // Add element buttons
            document.getElementById('add-text-btn')?.addEventListener('click', () => {
                console.log('Add text clicked');
                this.addTextElement();
            });
            document.getElementById('add-heading-btn')?.addEventListener('click', () => this.addHeadingElement());
            document.getElementById('add-image-btn')?.addEventListener('click', () => this.addImageElement());
            document.getElementById('add-shape-btn')?.addEventListener('click', () => this.addShapeElement());
            document.getElementById('add-sticker-btn')?.addEventListener('click', () => this.addStickerElement());
            document.getElementById('add-poll-btn')?.addEventListener('click', () => this.addPollElement());
            document.getElementById('add-quote-btn')?.addEventListener('click', () => this.addQuoteElement());
            
            // Background controls
            const bgType = document.getElementById('bg-type');
            if (bgType) {
                bgType.addEventListener('change', (e) => {
                    console.log('Background type changed:', e.target.value);
                    this.changeBgType(e.target.value);
                });
            }
            
            const bgColor = document.getElementById('bg-color');
            if (bgColor) {
                bgColor.addEventListener('input', (e) => {
                    console.log('Background color changed:', e.target.value);
                    this.updateSlideBackground(e.target.value);
                });
            }
            
            const bgColorText = document.getElementById('bg-color-text');
            if (bgColorText) {
                bgColorText.addEventListener('change', (e) => {
                    document.getElementById('bg-color').value = e.target.value;
                    this.updateSlideBackground(e.target.value);
                });
            }
            
            // Gradient controls
            document.getElementById('gradient-color-1')?.addEventListener('input', () => this.updateGradient());
            document.getElementById('gradient-color-2')?.addEventListener('input', () => this.updateGradient());
            document.getElementById('gradient-direction')?.addEventListener('change', () => this.updateGradient());
            
            // Element properties
            document.getElementById('element-text')?.addEventListener('input', (e) => {
                if (this.selectedElement) {
                    this.selectedElement.text = e.target.value;
                    this.updateCanvas();
                }
            });
            
            document.getElementById('font-size')?.addEventListener('input', (e) => {
                if (this.selectedElement) {
                    this.selectedElement.fontSize = e.target.value + 'px';
                    document.getElementById('font-size-value').textContent = e.target.value + 'px';
                    this.updateCanvas();
                }
            });
            
            document.getElementById('text-color')?.addEventListener('input', (e) => {
                if (this.selectedElement) {
                    this.selectedElement.color = e.target.value;
                    document.getElementById('text-color-text').value = e.target.value;
                    this.updateCanvas();
                }
            });
            
            document.getElementById('font-family')?.addEventListener('change', (e) => {
                if (this.selectedElement) {
                    this.selectedElement.fontFamily = e.target.value;
                    this.updateCanvas();
                }
            });
            
            document.getElementById('font-weight')?.addEventListener('change', (e) => {
                if (this.selectedElement) {
                    this.selectedElement.fontWeight = e.target.value;
                    this.updateCanvas();
                }
            });
            
            // Toolbar
            document.getElementById('undo-btn')?.addEventListener('click', () => this.undo());
            document.getElementById('redo-btn')?.addEventListener('click', () => this.redo());
            document.getElementById('zoom-in-btn')?.addEventListener('click', () => this.zoomIn());
            document.getElementById('zoom-out-btn')?.addEventListener('click', () => this.zoomOut());
            document.getElementById('duplicate-element-btn')?.addEventListener('click', () => this.duplicateElement());
            document.getElementById('delete-element-btn')?.addEventListener('click', () => this.deleteElement());
            
            // Save/Publish
            document.getElementById('save-draft-btn')?.addEventListener('click', () => this.saveDraft());
            document.getElementById('preview-btn')?.addEventListener('click', () => this.showPreview());
            document.getElementById('publish-btn')?.addEventListener('click', () => this.publishStory());
            
            console.log('All events bound successfully');
        }
        
        addInitialSlide() {
            console.log('Adding initial slide...');
            this.addSlide();
        }
        
        addSlide() {
            console.log('Creating new slide...');
            const slide = {
                id: 'slide-' + Date.now(),
                backgroundColor: '#667eea',
                backgroundType: 'color',
                duration: 5,
                transition: 'fade',
                elements: []
            };
            
            this.slides.push(slide);
            this.currentSlideIndex = this.slides.length - 1;
            this.selectedSlide = slide;
            
            console.log('Slide created:', slide);
            console.log('Total slides:', this.slides.length);
            
            this.updateUI();
            this.saveHistory();
        }
        
        prevSlide() {
            if (this.currentSlideIndex > 0) {
                this.currentSlideIndex--;
                this.selectedSlide = this.slides[this.currentSlideIndex];
                this.deselectElement();
                this.updateUI();
            }
        }
        
        nextSlide() {
            if (this.currentSlideIndex < this.slides.length - 1) {
                this.currentSlideIndex++;
                this.selectedSlide = this.slides[this.currentSlideIndex];
                this.deselectElement();
                this.updateUI();
            }
        }
        
        duplicateSlide() {
            if (!this.selectedSlide) return;
            
            const copy = JSON.parse(JSON.stringify(this.selectedSlide));
            copy.id = 'slide-' + Date.now();
            
            this.slides.splice(this.currentSlideIndex + 1, 0, copy);
            this.currentSlideIndex++;
            this.selectedSlide = copy;
            
            this.updateUI();
            this.saveHistory();
        }
        
        deleteSlide() {
            if (this.slides.length <= 1) {
                alert('You must have at least one slide!');
                return;
            }
            
            this.slides.splice(this.currentSlideIndex, 1);
            this.currentSlideIndex = Math.max(0, this.currentSlideIndex - 1);
            this.selectedSlide = this.slides[this.currentSlideIndex];
            
            this.updateUI();
            this.saveHistory();
        }
        
        addTextElement() {
            console.log('Adding text element...');
            if (!this.selectedSlide) {
                console.error('No slide selected!');
                return;
            }
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'text',
                text: 'Double click to edit',
                x: 80,
                y: 200,
                width: 200,
                height: 60,
                fontSize: '24px',
                fontFamily: 'Arial, sans-serif',
                fontWeight: '400',
                color: '#ffffff',
                textAlign: 'center',
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            console.log('Text element added:', element);
            console.log('Total elements:', this.selectedSlide.elements.length);
            
            this.updateCanvas();
            this.saveHistory();
        }
        
        addHeadingElement() {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'text',
                text: 'Your Headline',
                x: 20,
                y: 50,
                width: 320,
                height: 80,
                fontSize: '42px',
                fontFamily: 'Impact, sans-serif',
                fontWeight: '700',
                color: '#ffffff',
                textAlign: 'center',
                textShadow: '2px 2px 4px rgba(0,0,0,0.7)',
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.updateCanvas();
            this.saveHistory();
        }
        
        addImageElement() {
            if (!this.selectedSlide) return;
            
            const url = prompt('Enter image URL:');
            if (!url) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'image',
                src: url,
                x: 30,
                y: 100,
                width: 300,
                height: 300,
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.updateCanvas();
            this.saveHistory();
        }
        
        addShapeElement() {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'shape',
                shape: 'circle',
                x: 130,
                y: 270,
                width: 100,
                height: 100,
                backgroundColor: '#ff6b6b',
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.updateCanvas();
            this.saveHistory();
        }
        
        addStickerElement() {
            if (!this.selectedSlide) return;
            
            const stickers = ['😀', '😎', '🎉', '❤️', '⭐', '🔥', '💯', '✨'];
            const sticker = stickers[Math.floor(Math.random() * stickers.length)];
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'text',
                text: sticker,
                x: 160,
                y: 300,
                width: 60,
                height: 60,
                fontSize: '48px',
                fontFamily: 'Arial, sans-serif',
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.updateCanvas();
            this.saveHistory();
        }
        
        duplicateElement() {
            if (!this.selectedElement) return;
            
            const copy = JSON.parse(JSON.stringify(this.selectedElement));
            copy.id = 'element-' + (++this.elementIdCounter);
            copy.x += 20;
            copy.y += 20;
            
            this.selectedSlide.elements.push(copy);
            this.updateCanvas();
            this.saveHistory();
        }
        
        deleteElement() {
            if (!this.selectedElement) return;
            
            const index = this.selectedSlide.elements.findIndex(el => el.id === this.selectedElement.id);
            if (index !== -1) {
                this.selectedSlide.elements.splice(index, 1);
                this.selectedElement = null;
                this.updateCanvas();
                this.updatePropertiesPanel();
                this.saveHistory();
            }
        }
        
        changeBgType(type) {
            console.log('Changing background type to:', type);
            
            document.getElementById('bg-color-group').classList.add('d-none');
            document.getElementById('bg-gradient-group').classList.add('d-none');
            document.getElementById('bg-image-group').classList.add('d-none');
            
            if (type === 'color') {
                document.getElementById('bg-color-group').classList.remove('d-none');
                this.selectedSlide.backgroundType = 'color';
            } else if (type === 'gradient') {
                document.getElementById('bg-gradient-group').classList.remove('d-none');
                this.selectedSlide.backgroundType = 'gradient';
                this.updateGradient();
            } else if (type === 'image') {
                document.getElementById('bg-image-group').classList.remove('d-none');
                this.selectedSlide.backgroundType = 'image';
            }
            
            this.updateCanvas();
        }
        
        updateSlideBackground(color) {
            console.log('Updating slide background to:', color);
            if (!this.selectedSlide) {
                console.error('No slide selected!');
                return;
            }
            
            this.selectedSlide.backgroundColor = color;
            const bgColorText = document.getElementById('bg-color-text');
            if (bgColorText) {
                bgColorText.value = color;
            }
            
            console.log('Slide background updated:', this.selectedSlide.backgroundColor);
            console.log('Current slide:', this.selectedSlide);
            this.updateCanvas();
            this.saveHistory();
        }
        
        updateGradient() {
            if (!this.selectedSlide) return;
            
            const color1 = document.getElementById('gradient-color-1').value;
            const color2 = document.getElementById('gradient-color-2').value;
            const direction = document.getElementById('gradient-direction').value;
            
            this.selectedSlide.backgroundColor = `linear-gradient(${direction}, ${color1}, ${color2})`;
            this.updateCanvas();
        }
        
        selectElement(element) {
            console.log('Selecting element:', element);
            this.selectedElement = element;
            this.updatePropertiesPanel();
            this.highlightSelectedElement();
        }
        
        deselectElement() {
            this.selectedElement = null;
            this.updatePropertiesPanel();
            document.querySelectorAll('.story-element').forEach(el => el.classList.remove('selected'));
        }
        
        highlightSelectedElement() {
            document.querySelectorAll('.story-element').forEach(el => el.classList.remove('selected'));
            if (this.selectedElement) {
                const el = document.getElementById(this.selectedElement.id);
                if (el) el.classList.add('selected');
            }
        }
        
        updateUI() {
            console.log('Updating UI...');
            this.updateSidebarSlides();
            this.updateCanvas();
            this.updateSlideCounter();
            this.updateNavigationButtons();
            this.updatePropertiesPanel();
        }
        
        updateSidebarSlides() {
            const list = document.getElementById('slides-list');
            if (!list) return;
            
            list.innerHTML = '';
            
            this.slides.forEach((slide, index) => {
                const div = document.createElement('div');
                div.className = 'slide-thumbnail' + (index === this.currentSlideIndex ? ' active' : '');
                div.innerHTML = `
                    <div class="slide-number">${index + 1}</div>
                    <div style="font-size: 12px; font-weight: 600;">Slide ${index + 1}</div>
                    <div style="font-size: 10px; color: #999; margin-top: 4px;">${slide.duration}s · ${slide.elements.length} elements</div>
                `;
                div.onclick = () => {
                    this.currentSlideIndex = index;
                    this.selectedSlide = slide;
                    this.deselectElement();
                    this.updateUI();
                };
                list.appendChild(div);
            });
        }
        
        updateCanvas() {
            console.log('=== UPDATE CANVAS START ===');
            const canvas = document.getElementById('story-canvas');
            const overlay = document.getElementById('canvas-overlay');
            
            if (!canvas) {
                console.error('Canvas element not found!');
                return;
            }
            
            console.log('Current slide:', this.selectedSlide);
            console.log('Total slides:', this.slides.length);
            
            if (!this.selectedSlide || this.slides.length === 0) {
                if (overlay) overlay.style.display = 'block';
                console.log('No slides - showing overlay');
                return;
            }
            
            if (overlay) overlay.style.display = 'none';
            
            // Set background
            const slide = this.selectedSlide;
            console.log('Setting canvas background:', slide.backgroundColor);
            console.log('Background type:', slide.backgroundType);
            
            if (slide.backgroundType === 'color' || slide.backgroundType === 'gradient') {
                canvas.style.background = slide.backgroundColor;
                canvas.style.backgroundImage = 'none';
                console.log('Canvas background set to:', canvas.style.background);
            } else if (slide.backgroundType === 'image' && slide.backgroundImage) {
                canvas.style.background = 'none';
                canvas.style.backgroundImage = 'url(' + slide.backgroundImage + ')';
                canvas.style.backgroundSize = 'cover';
                canvas.style.backgroundPosition = 'center';
                console.log('Canvas background image set');
            }
            
            // Clear previous elements
            const existingElements = canvas.querySelectorAll('.story-element');
            existingElements.forEach(el => el.remove());
            console.log('Cleared', existingElements.length, 'existing elements');
            
            // Add elements
            console.log('Adding', slide.elements.length, 'elements to canvas');
            slide.elements.forEach(element => {
                console.log('Creating element:', element);
                const el = this.createElementNode(element);
                canvas.appendChild(el);
                console.log('Element added to canvas');
            });
            
            this.highlightSelectedElement();
            console.log('=== UPDATE CANVAS END ===');
        }
        
        createElementNode(element) {
            const el = document.createElement('div');
            el.id = element.id;
            el.className = 'story-element';
            el.style.position = 'absolute';
            el.style.left = element.x + 'px';
            el.style.top = element.y + 'px';
            el.style.width = (element.width === 'auto' ? 'auto' : element.width + 'px');
            el.style.height = (element.height === 'auto' ? 'auto' : element.height + 'px');
            el.style.opacity = element.opacity || 1;
            el.style.zIndex = element.zIndex || 0;
            el.style.cursor = 'move';
            el.style.userSelect = 'none';
            
            if (element.type === 'text') {
                el.classList.add('text-element');
                el.textContent = element.text;
                el.style.fontSize = element.fontSize;
                el.style.fontFamily = element.fontFamily;
                el.style.fontWeight = element.fontWeight;
                el.style.color = element.color;
                el.style.textAlign = element.textAlign || 'center';
                el.style.textShadow = element.textShadow || 'none';
                el.style.padding = '8px';
                el.style.display = 'flex';
                el.style.alignItems = 'center';
                el.style.justifyContent = 'center';
                el.style.wordBreak = 'break-word';
                
                // Double click to edit
                el.ondblclick = () => {
                    const newText = prompt('Edit text:', element.text);
                    if (newText !== null) {
                        element.text = newText;
                        this.updateCanvas();
                        this.saveHistory();
                    }
                };
            } else if (element.type === 'image') {
                el.classList.add('image-element');
                const img = document.createElement('img');
                img.src = element.src;
                img.style.width = '100%';
                img.style.height = '100%';
                img.style.objectFit = 'cover';
                img.style.pointerEvents = 'none';
                el.appendChild(img);
            } else if (element.type === 'shape') {
                el.classList.add('shape-element');
                el.style.backgroundColor = element.backgroundColor;
                if (element.shape === 'circle') {
                    el.style.borderRadius = '50%';
                }
            }
            
            // Click to select
            el.onclick = (e) => {
                e.stopPropagation();
                this.selectElement(element);
            };
            
            // Make draggable
            this.makeDraggable(el, element);
            
            return el;
        }
        
        makeDraggable(el, element) {
            let isDragging = false;
            let startX, startY, initialX, initialY;
            
            el.onmousedown = (e) => {
                if (e.target !== el && !el.classList.contains('text-element')) return;
                
                isDragging = true;
                startX = e.clientX;
                startY = e.clientY;
                initialX = element.x;
                initialY = element.y;
                
                el.style.cursor = 'grabbing';
                e.preventDefault();
            };
            
            document.onmousemove = (e) => {
                if (!isDragging) return;
                
                const dx = e.clientX - startX;
                const dy = e.clientY - startY;
                
                element.x = initialX + dx;
                element.y = initialY + dy;
                
                el.style.left = element.x + 'px';
                el.style.top = element.y + 'px';
            };
            
            document.onmouseup = () => {
                if (isDragging) {
                    isDragging = false;
                    el.style.cursor = 'move';
                    this.saveHistory();
                }
            };
        }
        
        updateSlideCounter() {
            const currentEl = document.getElementById('current-slide');
            const totalEl = document.getElementById('total-slides');
            
            if (currentEl) currentEl.textContent = this.currentSlideIndex + 1;
            if (totalEl) totalEl.textContent = this.slides.length;
        }
        
        updateNavigationButtons() {
            const prevBtn = document.getElementById('prev-slide-btn');
            const nextBtn = document.getElementById('next-slide-btn');
            
            if (prevBtn) prevBtn.disabled = this.currentSlideIndex === 0;
            if (nextBtn) nextBtn.disabled = this.currentSlideIndex === this.slides.length - 1;
        }
        
        updatePropertiesPanel() {
            const slideProps = document.getElementById('slide-properties');
            const elementProps = document.getElementById('element-properties');
            
            if (!slideProps || !elementProps) return;
            
            if (this.selectedElement) {
                slideProps.classList.add('d-none');
                elementProps.classList.remove('d-none');
                this.populateElementProperties();
            } else {
                slideProps.classList.remove('d-none');
                elementProps.classList.add('d-none');
                this.populateSlideProperties();
            }
        }
        
        populateSlideProperties() {
            if (!this.selectedSlide) return;
            
            const bgType = document.getElementById('bg-type');
            const bgColor = document.getElementById('bg-color');
            const bgColorText = document.getElementById('bg-color-text');
            
            if (bgType) bgType.value = this.selectedSlide.backgroundType || 'color';
            if (bgColor) bgColor.value = this.selectedSlide.backgroundColor || '#667eea';
            if (bgColorText) bgColorText.value = this.selectedSlide.backgroundColor || '#667eea';
        }
        
        populateElementProperties() {
            if (!this.selectedElement || this.selectedElement.type !== 'text') return;
            
            const textArea = document.getElementById('element-text');
            const fontSize = document.getElementById('font-size');
            const textColor = document.getElementById('text-color');
            const fontFamily = document.getElementById('font-family');
            const fontWeight = document.getElementById('font-weight');
            
            if (textArea) textArea.value = this.selectedElement.text || '';
            if (fontSize) {
                fontSize.value = parseInt(this.selectedElement.fontSize) || 24;
                document.getElementById('font-size-value').textContent = (parseInt(this.selectedElement.fontSize) || 24) + 'px';
            }
            if (textColor) {
                textColor.value = this.selectedElement.color || '#ffffff';
                document.getElementById('text-color-text').value = this.selectedElement.color || '#ffffff';
            }
            if (fontFamily) fontFamily.value = this.selectedElement.fontFamily || 'Arial, sans-serif';
            if (fontWeight) fontWeight.value = this.selectedElement.fontWeight || '400';
        }
        
        zoomIn() {
            this.zoomLevel = Math.min(this.zoomLevel + 0.1, 2);
            this.updateZoom();
        }
        
        zoomOut() {
            this.zoomLevel = Math.max(this.zoomLevel - 0.1, 0.5);
            this.updateZoom();
        }
        
        updateZoom() {
            const canvas = document.getElementById('story-canvas');
            if (canvas) {
                canvas.style.transform = `scale(${this.zoomLevel})`;
                document.getElementById('zoom-level').textContent = Math.round(this.zoomLevel * 100) + '%';
            }
        }
        
        saveHistory() {
            this.historyIndex++;
            this.history = this.history.slice(0, this.historyIndex);
            this.history.push(JSON.stringify(this.slides));
        }
        
        undo() {
            if (this.historyIndex > 0) {
                this.historyIndex--;
                this.slides = JSON.parse(this.history[this.historyIndex]);
                this.selectedSlide = this.slides[this.currentSlideIndex];
                this.updateUI();
            }
        }
        
        redo() {
            if (this.historyIndex < this.history.length - 1) {
                this.historyIndex++;
                this.slides = JSON.parse(this.history[this.historyIndex]);
                this.selectedSlide = this.slides[this.currentSlideIndex];
                this.updateUI();
            }
        }
        
        saveDraft() {
            const storyData = {
                title: document.getElementById('story-title')?.value || 'Untitled Story',
                slides: this.slides
            };
            
            localStorage.setItem('story-draft', JSON.stringify(storyData));
            alert('Draft saved successfully!');
        }
        
        showPreview() {
            alert('Preview mode - Full implementation coming soon!');
        }
        
        publishStory() {
            const title = document.getElementById('story-title')?.value;
            
            if (!title || title === 'Untitled Story') {
                alert('Please enter a story title before publishing!');
                return;
            }
            
            if (this.slides.length === 0) {
                alert('Please add at least one slide!');
                return;
            }
            
            const storyData = {
                title: title,
                slides: this.slides
            };
            
            console.log('Publishing story:', storyData);
            alert('Story published successfully! (Server integration pending)');
        }

        // AI Suggestions
        async getAISuggestions() {
            const suggestionsContainer = document.getElementById('ai-suggestions');
            if (!suggestionsContainer) return;
            
            // Show loading
            suggestionsContainer.innerHTML = '<div class="text-center text-muted small">Loading AI suggestions...</div>';
            
            // Simulate AI suggestions (replace with actual API call)
            setTimeout(() => {
                const suggestions = [
                    { text: 'Add a compelling headline', type: 'heading' },
                    { text: 'Include a call-to-action', type: 'text' },
                    { text: 'Use gradient background', type: 'background' },
                    { text: 'Add animated elements', type: 'effect' }
                ];
                
                suggestionsContainer.innerHTML = suggestions.map(s => 
                    `<div class="ai-suggestion-item" data-type="${s.type}">
                        <i class="fas fa-lightbulb me-2"></i>${s.text}
                    </div>`
                ).join('');
                
                // Add click handlers
                suggestionsContainer.querySelectorAll('.ai-suggestion-item').forEach(item => {
                    item.addEventListener('click', () => {
                        this.applyAISuggestion(item.dataset.type);
                    });
                });
            }, 500);
        }

        applyAISuggestion(type) {
            switch(type) {
                case 'heading':
                    this.addHeadingElement();
                    break;
                case 'text':
                    this.addTextElement();
                    break;
                case 'background':
                    if (this.selectedSlide) {
                        this.selectedSlide.backgroundColor = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
                        this.selectedSlide.backgroundType = 'gradient';
                        this.updateCanvas();
                    }
                    break;
                case 'effect':
                    alert('Animation effects coming soon!');
                    break;
            }
        }

        // Templates
        applyTemplate(templateName) {
            if (!this.selectedSlide) return;
            
            const templates = {
                minimal: {
                    backgroundColor: '#ffffff',
                    backgroundType: 'color',
                    elements: []
                },
                bold: {
                    backgroundColor: '#000000',
                    backgroundType: 'color',
                    elements: [{
                        id: 'element-' + (++this.elementIdCounter),
                        type: 'text',
                        text: 'BOLD STATEMENT',
                        x: 20,
                        y: 250,
                        width: 320,
                        height: 60,
                        fontSize: '42px',
                        fontFamily: 'Impact, sans-serif',
                        fontWeight: '900',
                        color: '#ffffff',
                        textAlign: 'center',
                        zIndex: 1
                    }]
                },
                gradient: {
                    backgroundColor: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    backgroundType: 'gradient',
                    elements: []
                },
                modern: {
                    backgroundColor: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
                    backgroundType: 'gradient',
                    elements: [{
                        id: 'element-' + (++this.elementIdCounter),
                        type: 'text',
                        text: 'Modern Design',
                        x: 20,
                        y: 280,
                        width: 320,
                        height: 60,
                        fontSize: '36px',
                        fontFamily: 'Arial, sans-serif',
                        fontWeight: '700',
                        color: '#ffffff',
                        textAlign: 'center',
                        textShadow: '2px 2px 8px rgba(0,0,0,0.5)',
                        zIndex: 1
                    }]
                }
            };
            
            const template = templates[templateName];
            if (template) {
                this.selectedSlide.backgroundColor = template.backgroundColor;
                this.selectedSlide.backgroundType = template.backgroundType;
                this.selectedSlide.elements = template.elements.map(el => ({...el}));
                this.updateCanvas();
                this.saveHistory();
            }
        }

        // New element types
        addPollElement() {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'poll',
                question: 'What do you think?',
                options: ['Option 1', 'Option 2'],
                x: 30,
                y: 200,
                width: 300,
                height: 150,
                backgroundColor: 'rgba(255,255,255,0.9)',
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.updateCanvas();
            this.saveHistory();
        }

        addQuoteElement() {
            if (!this.selectedSlide) return;
            
            const element = {
                id: 'element-' + (++this.elementIdCounter),
                type: 'text',
                text: '"Your inspirational quote here"',
                x: 40,
                y: 220,
                width: 280,
                height: 100,
                fontSize: '24px',
                fontFamily: 'Georgia, serif',
                fontWeight: '400',
                fontStyle: 'italic',
                color: '#ffffff',
                textAlign: 'center',
                opacity: 1,
                zIndex: this.selectedSlide.elements.length
            };
            
            this.selectedSlide.elements.push(element);
            this.updateCanvas();
            this.saveHistory();
        }
    }
    
    // Create global instance
    window.storyEditor = new StoryEditor();
    console.log('Story Editor instance created');
    
})();

