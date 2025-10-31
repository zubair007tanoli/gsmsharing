/**
 * Story Utility Functions
 * Shared utilities for all story-related views
 */

const StoryUtils = {
    /**
     * Check if a file URL is a video file
     * @param {string} url - File URL to check
     * @returns {boolean} True if video file
     */
    isVideoFile(url) {
        if (!url) return false;
        const videoExtensions = ['.mp4', '.webm', '.ogg', '.avi', '.mov', '.mkv'];
        const urlLower = url.toLowerCase();
        return videoExtensions.some(ext => urlLower.endsWith(ext));
    },

    /**
     * Make a URL absolute
     * @param {string} url - Relative or absolute URL
     * @param {string} scheme - Protocol (http/https)
     * @param {string} host - Host name
     * @returns {string} Absolute URL
     */
    makeAbsoluteUrl(url, scheme, host) {
        if (!url) return '';
        if (url.startsWith('http://') || url.startsWith('https://')) return url;
        if (url.startsWith('/')) return `${scheme}://${host}${url}`;
        return `${scheme}://${host}/${url}`;
    },

    /**
     * Truncate text to specified length
     * @param {string} text - Text to truncate
     * @param {number} maxLength - Maximum length
     * @param {string} suffix - Suffix to add (default: '…')
     * @returns {string} Truncated text
     */
    truncate(text, maxLength, suffix = '…') {
        if (!text) return '';
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + suffix;
    },

    /**
     * Format duration in milliseconds to readable format
     * @param {number} ms - Duration in milliseconds
     * @returns {string} Formatted duration (e.g., "5s", "1m 30s")
     */
    formatDuration(ms) {
        const seconds = Math.floor(ms / 1000);
        if (seconds < 60) return `${seconds}s`;
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        return remainingSeconds > 0 ? `${minutes}m ${remainingSeconds}s` : `${minutes}m`;
    },

    /**
     * Validate file upload
     * @param {File} file - File object to validate
     * @param {number} maxSize - Maximum size in bytes (default: 10MB)
     * @param {string[]} allowedTypes - Allowed MIME types
     * @returns {{valid: boolean, error?: string}}
     */
    validateFile(file, maxSize = 10485760, allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif', 'video/mp4']) {
        if (!file) {
            return { valid: false, error: 'No file provided' };
        }

        if (file.size > maxSize) {
            const maxSizeMB = (maxSize / 1048576).toFixed(1);
            return { valid: false, error: `File size exceeds ${maxSizeMB}MB limit` };
        }

        if (allowedTypes.length > 0 && !allowedTypes.includes(file.type)) {
            return { valid: false, error: 'Invalid file type. Only images and MP4 videos allowed.' };
        }

        return { valid: true };
    },

    /**
     * Safe element creation (prevents XSS)
     * @param {string} tag - HTML tag name
     * @param {Object} attributes - Attributes object
     * @param {string} textContent - Text content (will be escaped)
     * @returns {HTMLElement} Created element
     */
    createSafeElement(tag, attributes = {}, textContent = '') {
        const element = document.createElement(tag);
        
        for (const [key, value] of Object.entries(attributes)) {
            if (key === 'innerHTML' || key === 'textContent') continue;
            element.setAttribute(key, value);
        }
        
        if (textContent) {
            element.textContent = textContent;
        }
        
        return element;
    },

    /**
     * Safe HTML insertion (prevents XSS)
     * @param {HTMLElement} container - Container element
     * @param {string} html - HTML string (will be sanitized)
     */
    safeSetHTML(container, html) {
        // Use DOMPurify in production, or create text node for simple cases
        const temp = document.createElement('div');
        temp.textContent = html;
        container.textContent = ''; // Clear first
        container.appendChild(document.createTextNode(html));
    }
};

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = StoryUtils;
}

