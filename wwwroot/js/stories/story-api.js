/**
 * Story API Helper Functions
 * Provides safe, consistent API calls with error handling
 */

const StoryAPI = {
    /**
     * Safe fetch wrapper with error handling
     * @param {string} url - API endpoint
     * @param {Object} options - Fetch options
     * @returns {Promise<Object>} Response data
     */
    async safeFetch(url, options = {}) {
        try {
            const response = await fetch(url, {
                ...options,
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                }
            });

            if (!response.ok) {
                const errorText = await response.text();
                let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
                
                try {
                    const errorData = JSON.parse(errorText);
                    errorMessage = errorData.message || errorMessage;
                } catch {
                    errorMessage = errorText || errorMessage;
                }
                
                throw new Error(errorMessage);
            }

            const data = await response.json();
            return { success: true, data };
        } catch (error) {
            console.error('API Error:', error);
            return { 
                success: false, 
                error: error.message || 'An error occurred. Please try again.' 
            };
        }
    },

    /**
     * Upload a file
     * @param {File} file - File to upload
     * @param {string} category - Upload category ('stories', 'backgrounds', etc.)
     * @param {Function} onProgress - Progress callback (optional)
     * @returns {Promise<Object>} Upload result
     */
    async uploadFile(file, category = 'stories', onProgress = null) {
        // Validate file first
        const validation = StoryUtils.validateFile(file);
        if (!validation.valid) {
            return {
                success: false,
                error: validation.error
            };
        }

        const formData = new FormData();
        formData.append('file', file);
        formData.append('category', category);

        try {
            const xhr = new XMLHttpRequest();

            // Set up progress tracking
            if (onProgress) {
                xhr.upload.addEventListener('progress', (e) => {
                    if (e.lengthComputable) {
                        const percentComplete = (e.loaded / e.total) * 100;
                        onProgress(percentComplete);
                    }
                });
            }

            const response = await new Promise((resolve, reject) => {
                xhr.addEventListener('load', () => {
                    if (xhr.status >= 200 && xhr.status < 300) {
                        try {
                            resolve(JSON.parse(xhr.responseText));
                        } catch {
                            resolve({ success: true, filePath: xhr.responseText });
                        }
                    } else {
                        reject(new Error(`Upload failed: ${xhr.statusText}`));
                    }
                });

                xhr.addEventListener('error', () => {
                    reject(new Error('Network error during upload'));
                });

                xhr.addEventListener('abort', () => {
                    reject(new Error('Upload cancelled'));
                });

                xhr.open('POST', '/api/media/upload');
                xhr.send(formData);
            });

            if (!response.success) {
                return {
                    success: false,
                    error: response.message || 'Upload failed'
                };
            }

            return {
                success: true,
                filePath: response.filePath || response.url || response.data?.filePath
            };
        } catch (error) {
            console.error('Upload error:', error);
            return {
                success: false,
                error: error.message || 'Failed to upload file'
            };
        }
    },

    /**
     * Delete a story
     * @param {string} slug - Story slug
     * @returns {Promise<Object>} Delete result
     */
    async deleteStory(slug) {
        const result = await this.safeFetch(`/stories/${slug}/delete`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': this.getAntiForgeryToken()
            }
        });

        return result;
    },

    /**
     * Publish a story
     * @param {number} storyId - Story ID
     * @returns {Promise<Object>} Publish result
     */
    async publishStory(storyId) {
        return await this.safeFetch(`/api/stories/${storyId}/publish`, {
            method: 'POST'
        });
    },

    /**
     * Save story draft
     * @param {Object} data - Story data
     * @returns {Promise<Object>} Save result
     */
    async saveDraft(data) {
        return await this.safeFetch('/api/stories/save-draft', {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    /**
     * Get story slides (JSON)
     * @param {string} slug - Story slug
     * @returns {Promise<Object>} Slides data
     */
    async getSlides(slug) {
        return await this.safeFetch(`/api/stories/${slug}/slides`);
    },

    /**
     * Get anti-forgery token from page
     * @returns {string} Token value
     */
    getAntiForgeryToken() {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    }
};

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = StoryAPI;
}

