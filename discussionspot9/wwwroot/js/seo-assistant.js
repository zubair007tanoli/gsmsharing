/**
 * Real-Time SEO Assistant with Google Search API + AI
 * Auto-suggests keywords, optimizes content, and maximizes SEO score
 */

class SeoAssistant {
    constructor() {
        this.currentKeywords = [];
        this.debounceTimer = null;
        this.isAnalyzing = false;
        this.lastAnalyzedTitle = '';
        this.lastAnalyzedContent = '';
        
        this.init();
    }

    init() {
        // Get form elements - try multiple ID variations
        this.titleInput = document.getElementById('Title') || 
                         document.getElementById('title') ||
                         document.querySelector('input[asp-for="Title"]');
        
        this.contentInput = document.getElementById('Content') || 
                           document.getElementById('contentTextarea') ||
                           document.querySelector('textarea[asp-for="Content"]');
        
        this.tagsInput = document.querySelector('input[name="Tags"]') || 
                        document.getElementById('Tags') ||
                        document.getElementById('tags') ||
                        document.querySelector('.tags-input');
        
        // Check if Quill editor is being used
        this.quillEditor = window.contentQuill || null;
        
        if (!this.titleInput) {
            console.warn('SEO Assistant: Title input not found. Available IDs:', 
                Array.from(document.querySelectorAll('input')).map(el => el.id));
            return;
        }

        // Attach event listeners
        this.attachEventListeners();
        
        // Initial analysis if title exists
        if (this.titleInput.value) {
            this.scheduleDraftAnalysis();
        }

        console.log('✅ SEO Assistant initialized');
    }

    attachEventListeners() {
        // Title input - analyze on change
        this.titleInput?.addEventListener('input', () => {
            this.scheduleDraftAnalysis();
        });

        // Content input - handle both textarea and Quill editor
        if (this.quillEditor) {
            // Quill editor - listen to text changes
            this.quillEditor.on('text-change', () => {
                this.scheduleDraftAnalysis(2000);
            });
        } else if (this.contentInput) {
            // Regular textarea - analyze on change
            this.contentInput.addEventListener('input', () => {
                this.scheduleDraftAnalysis(2000);
            });
        }

        // Form submit - ensure keywords are applied
        const form = this.titleInput.closest('form');
        if (form) {
            form.addEventListener('submit', (e) => {
                this.onFormSubmit(e);
            });
        }
    }

    scheduleDraftAnalysis(delay = 1500) {
        clearTimeout(this.debounceTimer);
        this.debounceTimer = setTimeout(() => {
            this.analyzeDraft();
        }, delay);
    }

    async analyzeDraft() {
        const title = this.titleInput?.value || '';
        
        // Get content from Quill or textarea
        let content = '';
        if (this.quillEditor) {
            content = this.quillEditor.getText() || ''; // Get plain text from Quill
        } else if (this.contentInput) {
            content = this.contentInput.value || '';
        }

        // Skip if no title or already analyzing same content
        if (!title || this.isAnalyzing) return;
        if (title === this.lastAnalyzedTitle && content === this.lastAnalyzedContent) return;

        this.lastAnalyzedTitle = title;
        this.lastAnalyzedContent = content;
        this.isAnalyzing = true;

        this.updateStatus('analyzing', 'Analyzing with Google AI...');

        try {
            const response = await fetch(
                `/admin/seo/api/suggest-keywords-realtime?title=${encodeURIComponent(title)}&content=${encodeURIComponent(content)}`
            );
            
            if (!response.ok) throw new Error('API request failed');
            
            const data = await response.json();

            if (data.success) {
                this.currentKeywords = data.keywords || [];
                this.updateUI(data);
            } else {
                this.updateStatus('error', data.error || 'Failed to analyze');
            }
        } catch (error) {
            console.error('SEO Assistant error:', error);
            this.updateStatus('error', 'Connection error. Please try again.');
        } finally {
            this.isAnalyzing = false;
        }
    }

    updateUI(data) {
        // Update SEO score
        this.updateSeoScore(data.seoScore || 0);

        // Update status
        this.updateStatus('success', `Analysis complete! Found ${this.currentKeywords.length} keywords`);

        // Display keywords
        this.displayKeywords(this.currentKeywords);

        // Show suggestions
        this.displaySuggestions(data.suggestions || []);

        // Show the suggestions panel
        const suggestionsPanel = document.getElementById('keywordSuggestions');
        if (suggestionsPanel) {
            suggestionsPanel.style.display = 'block';
        }
    }

    updateSeoScore(score) {
        const scoreBar = document.getElementById('seoScoreBar');
        const scoreText = document.getElementById('seoScoreText');
        
        if (!scoreBar || !scoreText) return;

        // Animate score
        scoreBar.style.width = score + '%';
        scoreText.textContent = Math.round(score) + '/100';

        // Update color based on score
        scoreBar.className = 'progress-bar';
        if (score >= 80) {
            scoreBar.classList.add('bg-success');
        } else if (score >= 60) {
            scoreBar.classList.add('bg-warning');
        } else {
            scoreBar.classList.add('bg-danger');
        }
    }

    updateStatus(type, message) {
        const statusDiv = document.getElementById('seoStatus');
        if (!statusDiv) return;

        let icon = 'fa-info-circle';
        let className = 'alert alert-info';

        switch (type) {
            case 'analyzing':
                icon = 'fa-spinner fa-spin';
                className = 'alert alert-info';
                break;
            case 'success':
                icon = 'fa-check-circle';
                className = 'alert alert-success';
                break;
            case 'error':
                icon = 'fa-exclamation-triangle';
                className = 'alert alert-danger';
                break;
        }

        statusDiv.className = className;
        statusDiv.innerHTML = `<i class="fas ${icon} me-2"></i>${message}`;
    }

    displayKeywords(keywords) {
        const keywordList = document.getElementById('keywordList');
        if (!keywordList) return;

        keywordList.innerHTML = '';

        keywords.slice(0, 10).forEach(keyword => {
            const badge = document.createElement('span');
            badge.className = 'badge bg-primary keyword-badge';
            badge.textContent = keyword;
            badge.style.cursor = 'pointer';
            badge.title = 'Click to insert into content';
            badge.onclick = () => this.insertKeywordIntoContent(keyword);
            keywordList.appendChild(badge);
        });
    }

    displaySuggestions(suggestions) {
        const tipsList = document.getElementById('tipsList');
        if (!tipsList) return;

        tipsList.innerHTML = '';

        if (suggestions.length === 0) {
            tipsList.innerHTML = '<li class="text-success">✅ Great! No issues found.</li>';
            return;
        }

        suggestions.forEach(tip => {
            const li = document.createElement('li');
            li.textContent = tip;
            tipsList.appendChild(li);
        });
    }

    insertKeywordIntoContent(keyword) {
        if (!this.contentInput) return;

        const cursorPos = this.contentInput.selectionStart || 0;
        const textBefore = this.contentInput.value.substring(0, cursorPos);
        const textAfter = this.contentInput.value.substring(cursorPos);
        
        // Insert keyword with spaces
        this.contentInput.value = textBefore + ' ' + keyword + ' ' + textAfter;
        
        // Move cursor after inserted keyword
        this.contentInput.selectionStart = this.contentInput.selectionEnd = cursorPos + keyword.length + 2;
        this.contentInput.focus();

        // Show notification
        this.showNotification(`✅ Inserted "${keyword}" into content`);
    }

    autoApplyAllKeywords() {
        if (this.currentKeywords.length === 0) {
            alert('❌ No keywords available. Type a title first!');
            return;
        }

        let applied = false;

        // 1. Apply to Tags input
        if (this.tagsInput) {
            const existingTags = this.tagsInput.value.split(',').map(t => t.trim()).filter(t => t);
            const newTags = this.currentKeywords.slice(0, 5).filter(k => !existingTags.includes(k));
            
            if (newTags.length > 0) {
                const allTags = [...existingTags, ...newTags].join(', ');
                this.tagsInput.value = allTags;
                applied = true;
            }
        }

        // 2. Ensure keywords are in content
        if (this.contentInput) {
            const content = this.contentInput.value.toLowerCase();
            const missingKeywords = this.currentKeywords.slice(0, 3).filter(k => 
                !content.includes(k.toLowerCase())
            );

            if (missingKeywords.length > 0) {
                // Append keywords naturally to content
                const keywordText = `\n\nRelated: ${missingKeywords.join(', ')}`;
                this.contentInput.value += keywordText;
                applied = true;
            }
        }

        if (applied) {
            this.showNotification('✅ Keywords auto-applied successfully!', 'success');
            
            // Re-analyze to update score
            this.scheduleDraftAnalysis(500);
        } else {
            this.showNotification('ℹ️ Keywords already present!', 'info');
        }
    }

    onFormSubmit(e) {
        // Optionally auto-apply keywords before submit
        if (this.currentKeywords.length > 0 && this.tagsInput && !this.tagsInput.value) {
            e.preventDefault();
            
            if (confirm('Apply suggested keywords before publishing?')) {
                this.autoApplyAllKeywords();
                
                // Submit after short delay
                setTimeout(() => {
                    e.target.submit();
                }, 500);
            }
        }
    }

    showNotification(message, type = 'success') {
        // Create toast notification
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
            ${message}
        `;
        
        document.body.appendChild(toast);

        // Auto-remove after 3 seconds
        setTimeout(() => {
            if (toast.parentElement) {
                toast.remove();
            }
        }, 3000);
    }
}

// Global function for button onclick
function autoApplyKeywords() {
    if (window.seoAssistant) {
        window.seoAssistant.autoApplyAllKeywords();
    }
}

// Initialize when DOM is ready AND after Quill
function initSeoAssistant() {
    // Check if Quill is ready (for CreateTest page)
    if (typeof contentQuill !== 'undefined' && contentQuill !== null) {
        console.log('✅ Quill detected, initializing SEO Assistant...');
        window.seoAssistant = new SeoAssistant();
    } else if (document.getElementById('Title') || document.getElementById('title')) {
        // No Quill, but title input exists (regular Create page)
        console.log('✅ No Quill, initializing SEO Assistant for regular form...');
        window.seoAssistant = new SeoAssistant();
    } else {
        // Retry after delay
        console.log('⏳ Waiting for form elements...');
        setTimeout(initSeoAssistant, 500);
    }
}

// Try to initialize immediately
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(initSeoAssistant, 1000); // Delay to let Quill initialize
    });
} else {
    setTimeout(initSeoAssistant, 1000);
}

