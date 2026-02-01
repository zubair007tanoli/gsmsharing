/**
 * Comment Link Preview Handler
 * Detects URLs in comment text and converts them to rich link previews.
 * Replaces raw URL text with "Link" (or configurable phrase) and shows preview cards.
 */

(function() {
    'use strict';

    // URL detection regex - matches http, https, and www URLs
    const urlRegex = /(https?:\/\/[^\s<]+)|(www\.[^\s<]+)/gi;
    // Text shown instead of raw URL (user-facing)
    const LINK_REPLACEMENT_TEXT = 'Link';
    // Selectors for comment body that contains the HTML content (order: try V1, then Reddit, then generic)
    const COMMENT_CONTENT_SELECTORS = '.comment-text, .comment-content-reddit, .comment-body';
    
    /**
     * Extract metadata from a URL using Open Graph Protocol
     * @param {string} url - The URL to fetch metadata for
     * @returns {Promise<Object>} - Link preview data
     */
    async function fetchLinkMetadata(url) {
        try {
            // Ensure URL has protocol
            const fullUrl = url.startsWith('http') ? url : `https://${url}`;
            
            // Call backend API to fetch metadata
            const response = await fetch('/api/LinkMetadata/GetMetadata', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ url: fullUrl })
            });

            if (!response.ok) {
                throw new Error('Failed to fetch metadata');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching link metadata:', error);
            
            // Return basic metadata if fetch fails
            const urlObj = new URL(url.startsWith('http') ? url : `https://${url}`);
            return {
                title: urlObj.hostname,
                description: '',
                url: urlObj.href,
                domain: urlObj.hostname,
                thumbnailUrl: '',
                faviconUrl: `${urlObj.protocol}//${urlObj.hostname}/favicon.ico`
            };
        }
    }

    /**
     * Fetch metadata for multiple URLs in a single batch request
     * @param {Array<string>} urls - Array of URLs
     * @returns {Promise<Array<Object>>} - Array of link preview data
     */
    async function fetchLinkMetadataBatch(urls) {
        try {
            // Ensure all URLs have protocol
            const fullUrls = urls.map(url => url.startsWith('http') ? url : `https://${url}`);
            
            // Call backend API to fetch metadata for all URLs
            const response = await fetch('/api/LinkMetadata/GetMetadataBatch', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ urls: fullUrls })
            });

            if (!response.ok) {
                throw new Error('Failed to fetch batch metadata');
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Error fetching batch link metadata:', error);
            
            // Return basic metadata for all URLs if fetch fails
            return urls.map(url => {
                const urlObj = new URL(url.startsWith('http') ? url : `https://${url}`);
                return {
                    title: urlObj.hostname,
                    description: '',
                    url: urlObj.href,
                    domain: urlObj.hostname,
                    thumbnailUrl: '',
                    faviconUrl: `${urlObj.protocol}//${urlObj.hostname}/favicon.ico`
                };
            });
        }
    }

    /**
     * Create a link preview card element
     * @param {Object} metadata - Link metadata
     * @returns {HTMLElement} - Link preview card
     */
    function createLinkPreviewCard(metadata) {
        const card = document.createElement('a');
        card.href = metadata.url;
        card.target = '_blank';
        card.rel = 'noopener noreferrer';
        card.className = 'comment-link-preview-card';
        
        const container = document.createElement('div');
        container.className = 'comment-link-preview-container';
        
        // Add thumbnail if available
        if (metadata.thumbnailUrl) {
            const thumbnailDiv = document.createElement('div');
            thumbnailDiv.className = 'comment-link-preview-thumbnail';
            
            const img = document.createElement('img');
            img.src = metadata.thumbnailUrl;
            img.alt = metadata.title || 'Link preview';
            img.loading = 'lazy';
            img.onerror = function() {
                this.parentElement.style.display = 'none';
            };
            
            thumbnailDiv.appendChild(img);
            container.appendChild(thumbnailDiv);
        }
        
        // Content section
        const contentDiv = document.createElement('div');
        contentDiv.className = 'comment-link-preview-content';
        
        // Header with favicon and domain
        const headerDiv = document.createElement('div');
        headerDiv.className = 'comment-link-preview-header';
        
        if (metadata.faviconUrl) {
            const favicon = document.createElement('img');
            favicon.src = metadata.faviconUrl;
            favicon.alt = 'favicon';
            favicon.className = 'comment-link-preview-favicon';
            favicon.loading = 'lazy';
            favicon.onerror = function() {
                this.style.display = 'none';
            };
            headerDiv.appendChild(favicon);
        }
        
        const domainSpan = document.createElement('span');
        domainSpan.className = 'comment-link-preview-domain';
        domainSpan.textContent = metadata.domain;
        headerDiv.appendChild(domainSpan);
        
        contentDiv.appendChild(headerDiv);
        
        // Title
        if (metadata.title) {
            const titleH4 = document.createElement('h4');
            titleH4.className = 'comment-link-preview-title';
            titleH4.textContent = metadata.title;
            contentDiv.appendChild(titleH4);
        }
        
        // Description
        if (metadata.description) {
            const descP = document.createElement('p');
            descP.className = 'comment-link-preview-description';
            descP.textContent = metadata.description;
            contentDiv.appendChild(descP);
        }
        
        // Footer
        const footerDiv = document.createElement('div');
        footerDiv.className = 'comment-link-preview-footer';
        
        const icon = document.createElement('i');
        icon.className = 'fas fa-external-link-alt';
        footerDiv.appendChild(icon);
        
        const visitSpan = document.createElement('span');
        visitSpan.textContent = 'Visit link';
        footerDiv.appendChild(visitSpan);
        
        contentDiv.appendChild(footerDiv);
        container.appendChild(contentDiv);
        card.appendChild(container);
        
        return card;
    }

    /**
     * Create a link icon to replace the original link text
     * @param {string} url - The URL
     * @returns {HTMLElement} - Link icon element
     */
    function createLinkIcon(url) {
        const icon = document.createElement('a');
        icon.href = url;
        icon.target = '_blank';
        icon.rel = 'noopener noreferrer';
        icon.className = 'comment-inline-link-icon';
        icon.title = url;
        icon.innerHTML = '<i class="fas fa-link"></i>';
        return icon;
    }

    /**
     * Check if position 'offset' in html is inside an <a> tag
     */
    function isInsideAnchor(html, offset) {
        const before = html.substring(0, offset);
        const openCount = (before.match(/<a\s/gi) || []).length;
        const closeCount = (before.match(/<\/a\s*>/gi) || []).length;
        return openCount > closeCount;
    }

    /**
     * Process a comment element to detect and convert links
     * @param {HTMLElement} commentElement - The comment element to process
     */
    async function processCommentLinks(commentElement) {
        const commentTextElement = commentElement.querySelector(COMMENT_CONTENT_SELECTORS);
        if (!commentTextElement) {
            console.debug('No comment content element found (tried: ' + COMMENT_CONTENT_SELECTORS + ')');
            return;
        }

        // Check if already processed
        if (commentTextElement.hasAttribute('data-links-processed')) {
            console.debug('Comment already processed for links');
            return;
        }

        // Mark as processing to prevent duplicate processing
        commentTextElement.setAttribute('data-links-processing', 'true');

        // Get the HTML content
        let htmlContent = commentTextElement.innerHTML;
        
        // Find all URLs in the content (from text nodes only)
        const urls = [];
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = htmlContent;
        
        function extractUrlsFromNode(node) {
            if (node.nodeType === Node.TEXT_NODE) {
                const text = node.textContent;
                const regex = /(https?:\/\/[^\s<]+)|(www\.[^\s<]+)/gi;
                let urlMatch;
                while ((urlMatch = regex.exec(text)) !== null) {
                    const url = urlMatch[0];
                    urls.push({
                        url: url,
                        fullUrl: url.startsWith('http') ? url : `https://${url}`
                    });
                }
            } else if (node.nodeType === Node.ELEMENT_NODE && node.tagName !== 'A') {
                for (let child of node.childNodes) {
                    extractUrlsFromNode(child);
                }
            }
        }
        
        extractUrlsFromNode(tempDiv);
        
        if (urls.length === 0) {
            commentTextElement.removeAttribute('data-links-processing');
            commentTextElement.setAttribute('data-links-processed', 'true');
            return;
        }

        // Single-pass replace: convert raw URLs to "Link" (skip if already inside <a>)
        htmlContent = htmlContent.replace(urlRegex, function(match) {
            const offset = arguments[arguments.length - 2];
            const fullString = arguments[arguments.length - 1];
            if (isInsideAnchor(fullString, offset)) return match;
            const fullUrl = match.startsWith('http') ? match : 'https://' + match;
            return '<a href="' + fullUrl + '" target="_blank" rel="noopener noreferrer" class="comment-inline-link-replacement" title="' + fullUrl + '">' + LINK_REPLACEMENT_TEXT + '</a>';
        });
        
        commentTextElement.innerHTML = htmlContent;

        // Create a container for link previews if it doesn't exist
        let previewContainer = commentElement.querySelector('.comment-link-previews');
        if (!previewContainer) {
            previewContainer = document.createElement('div');
            previewContainer.className = 'comment-link-previews';
            // V1: .comment-content has .comment-text, then .comment-actions
            const commentContent = commentElement.querySelector('.comment-content');
            const commentActions = commentElement.querySelector('.comment-actions');
            // Reddit: .comment-body-reddit has .comment-content-reddit, then .comment-actions-reddit
            const bodyReddit = commentElement.querySelector('.comment-body-reddit');
            const actionsReddit = commentElement.querySelector('.comment-actions-reddit');
            if (commentContent && commentActions) {
                commentContent.insertBefore(previewContainer, commentActions);
            } else if (bodyReddit && actionsReddit) {
                bodyReddit.insertBefore(previewContainer, actionsReddit);
            } else if (commentTextElement.parentElement) {
                commentTextElement.parentElement.insertBefore(previewContainer, commentTextElement.nextSibling);
            }
        }

        // Fetch metadata and create preview cards only if not already present (e.g. server-rendered)
        const hasExistingCards = previewContainer.querySelector('.comment-link-preview-card, .link-preview-card');
        const uniqueUrls = [...new Set(urls.map(u => u.fullUrl))];
        
        if (!hasExistingCards && uniqueUrls.length > 0) {
            if (uniqueUrls.length > 1 && uniqueUrls.length <= 10) {
                try {
                    const metadataArray = await fetchLinkMetadataBatch(uniqueUrls);
                    metadataArray.forEach(metadata => {
                        const previewCard = createLinkPreviewCard(metadata);
                        previewContainer.appendChild(previewCard);
                    });
                } catch (error) {
                    console.error('Failed to create previews in batch:', error);
                    for (const url of uniqueUrls) {
                        try {
                            const metadata = await fetchLinkMetadata(url);
                            previewContainer.appendChild(createLinkPreviewCard(metadata));
                        } catch (e) { console.error('Failed to create preview for', url, e); }
                    }
                }
            } else {
                for (const url of uniqueUrls) {
                    try {
                        const metadata = await fetchLinkMetadata(url);
                        previewContainer.appendChild(createLinkPreviewCard(metadata));
                    } catch (error) {
                        console.error(`Failed to create preview for ${url}:`, error);
                    }
                }
            }
        }

        // Mark as processed
        commentTextElement.removeAttribute('data-links-processing');
        commentTextElement.setAttribute('data-links-processed', 'true');
        console.log(`Successfully processed ${uniqueUrls.length} link preview(s) for comment`);
    }

    /**
     * Process all comments on the page
     */
    function processAllComments() {
        const commentElements = document.querySelectorAll('.comment-item, .comment-item-reddit');
        if (commentElements.length === 0) return;
        commentElements.forEach((commentElement) => processCommentLinks(commentElement));
    }

    /**
     * Initialize the link preview handler
     */
    function init() {
        console.log('🔗 Comment Link Preview: Initializing...');
        console.log('🔗 Document ready state:', document.readyState);
        
        // Process existing comments on page load
        if (document.readyState === 'loading') {
            console.log('🔗 Document still loading, waiting for DOMContentLoaded...');
            document.addEventListener('DOMContentLoaded', () => {
                console.log('🔗 DOMContentLoaded fired!');
                processAllComments();
            });
        } else {
            console.log('🔗 Document already loaded, processing immediately...');
            processAllComments();
        }

        // Watch for new comments added dynamically (via SignalR or AJAX)
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType !== Node.ELEMENT_NODE) return;
                    const isComment = node.classList && (node.classList.contains('comment-item') || node.classList.contains('comment-item-reddit'));
                    if (isComment) {
                        processCommentLinks(node);
                    } else if (node.querySelectorAll) {
                        const comments = node.querySelectorAll('.comment-item, .comment-item-reddit');
                        comments.forEach(comment => processCommentLinks(comment));
                    }
                });
            });
        });

        // Start observing the comment list (V1 and Reddit layouts)
        const commentList = document.querySelector('.comment-list, #commentsContainer, .comments-thread, #commentsThread');
        if (commentList) {
            console.log('🔗 MutationObserver started watching:', commentList);
            observer.observe(commentList, {
                childList: true,
                subtree: true
            });
        } else {
            console.warn('🔗 Comment list container not found! Will retry in 2 seconds...');
            // Retry after 2 seconds in case comments load late
            setTimeout(() => {
                const retryCommentList = document.querySelector('.comment-list, #commentsContainer, .comments-thread, #commentsThread');
                if (retryCommentList) {
                    console.log('🔗 Comment list found on retry, starting observer');
                    observer.observe(retryCommentList, {
                        childList: true,
                        subtree: true
                    });
                    // Process any comments that were added while waiting
                    processAllComments();
                } else {
                    console.error('🔗 Comment list still not found after retry!');
                    console.log('🔗 Available containers:', {
                        'body': document.body ? 'exists' : 'missing',
                        'comment-list': document.querySelector('.comment-list') ? 'exists' : 'missing',
                        'commentsContainer': document.querySelector('#commentsContainer') ? 'exists' : 'missing',
                        'comments-thread': document.querySelector('.comments-thread') ? 'exists' : 'missing'
                    });
                }
            }, 2000);
        }
    }

    // Export for use in other scripts
    window.CommentLinkPreview = {
        processComment: processCommentLinks,
        processAll: processAllComments,
        init: init
    };

    // Auto-initialize
    init();
    
    console.log('🔗 Comment Link Preview loaded successfully!');
    console.log('🔗 Available commands:');
    console.log('  - window.CommentLinkPreview.processAll() - Process all comments');
    console.log('  - window.CommentLinkPreview.processComment(element) - Process specific comment');
})();

