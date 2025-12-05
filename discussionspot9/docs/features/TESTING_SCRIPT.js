/**
 * Comment Link Preview - Testing Script
 * 
 * Copy and paste this script into the browser console on a post detail page
 * to test the link preview functionality.
 * 
 * Usage:
 * 1. Navigate to a post detail page (e.g., /r/community/post/slug)
 * 2. Open browser console (F12)
 * 3. Copy and paste this entire script
 * 4. Run the test functions
 */

console.log('🧪 Comment Link Preview Testing Script Loaded');
console.log('Available test functions:');
console.log('  - testAPI() - Test the API endpoint');
console.log('  - testBatchAPI() - Test the batch API endpoint');
console.log('  - testLinkDetection() - Test URL detection in comments');
console.log('  - testAllComments() - Process all existing comments');
console.log('  - createTestComment() - Create a test comment with links');
console.log('  - checkSetup() - Verify all files are loaded');

/**
 * Check if all required files and functions are loaded
 */
function checkSetup() {
    console.log('🔍 Checking setup...');
    
    const checks = {
        'JavaScript loaded': typeof window.CommentLinkPreview !== 'undefined',
        'CSS loaded': document.querySelector('link[href*="comment-link-preview.css"]') !== null,
        'Font Awesome loaded': typeof FontAwesome !== 'undefined' || document.querySelector('link[href*="font-awesome"]') !== null,
        'Comment elements exist': document.querySelectorAll('.comment-item').length > 0,
        'Comment text elements exist': document.querySelectorAll('.comment-text').length > 0
    };
    
    console.table(checks);
    
    const allPassed = Object.values(checks).every(v => v);
    if (allPassed) {
        console.log('✅ All checks passed! System is ready.');
    } else {
        console.log('❌ Some checks failed. See table above.');
    }
    
    return allPassed;
}

/**
 * Test the API endpoint with a sample URL
 */
async function testAPI(url = 'https://github.com') {
    console.log(`🧪 Testing API with URL: ${url}`);
    
    try {
        const response = await fetch('/api/LinkMetadata/GetMetadata', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ url: url })
        });
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        console.log('✅ API Response:', data);
        console.table({
            'Title': data.title,
            'Domain': data.domain,
            'Has Thumbnail': !!data.thumbnailUrl,
            'Has Favicon': !!data.faviconUrl,
            'Has Description': !!data.description
        });
        
        return data;
    } catch (error) {
        console.error('❌ API Test Failed:', error);
        return null;
    }
}

/**
 * Test the batch API endpoint with multiple URLs
 */
async function testBatchAPI(urls = ['https://github.com', 'https://stackoverflow.com']) {
    console.log(`🧪 Testing Batch API with ${urls.length} URLs`);
    
    try {
        const response = await fetch('/api/LinkMetadata/GetMetadataBatch', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ urls: urls })
        });
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        
        const data = await response.json();
        console.log(`✅ Batch API Response: ${data.length} results`);
        data.forEach((item, index) => {
            console.log(`\n📄 Result ${index + 1}:`);
            console.table({
                'URL': item.url,
                'Title': item.title,
                'Domain': item.domain,
                'Has Thumbnail': !!item.thumbnailUrl
            });
        });
        
        return data;
    } catch (error) {
        console.error('❌ Batch API Test Failed:', error);
        return null;
    }
}

/**
 * Test URL detection in a sample text
 */
function testLinkDetection() {
    console.log('🧪 Testing URL detection...');
    
    const testCases = [
        'Check out https://github.com',
        'Visit www.example.com for more',
        'Multiple links: https://github.com and https://stackoverflow.com',
        'No links here',
        'Mixed: www.example.com and https://github.com'
    ];
    
    const urlRegex = /(https?:\/\/[^\s<]+)|(www\.[^\s<]+)/gi;
    
    testCases.forEach((text, index) => {
        const matches = text.match(urlRegex);
        console.log(`\nTest ${index + 1}: "${text}"`);
        console.log(`Found ${matches ? matches.length : 0} URL(s):`, matches || 'none');
    });
}

/**
 * Process all existing comments on the page
 */
function testAllComments() {
    console.log('🧪 Processing all comments...');
    
    if (typeof window.CommentLinkPreview === 'undefined') {
        console.error('❌ CommentLinkPreview not loaded!');
        return;
    }
    
    const comments = document.querySelectorAll('.comment-item');
    console.log(`Found ${comments.length} comment(s)`);
    
    if (comments.length === 0) {
        console.log('⚠️ No comments found on page');
        return;
    }
    
    // Reset all comments (remove processed flag)
    comments.forEach(comment => {
        const commentText = comment.querySelector('.comment-text');
        if (commentText) {
            commentText.removeAttribute('data-links-processed');
            commentText.removeAttribute('data-links-processing');
        }
    });
    
    // Process all comments
    window.CommentLinkPreview.processAll();
    
    console.log('✅ All comments processed');
    
    // Show results after a delay
    setTimeout(() => {
        const processed = document.querySelectorAll('.comment-text[data-links-processed="true"]').length;
        const withPreviews = document.querySelectorAll('.comment-link-previews').length;
        console.log(`📊 Results: ${processed} comments processed, ${withPreviews} with link previews`);
    }, 2000);
}

/**
 * Create a test comment element with links
 */
function createTestComment(text = 'Check out https://github.com and https://stackoverflow.com') {
    console.log('🧪 Creating test comment...');
    
    const commentList = document.querySelector('.comment-list');
    if (!commentList) {
        console.error('❌ Comment list not found!');
        return;
    }
    
    const testComment = document.createElement('div');
    testComment.className = 'comment-item';
    testComment.id = 'test-comment-' + Date.now();
    testComment.innerHTML = `
        <div class="comment-content">
            <div class="comment-meta">
                <div class="user-avatar small" style="background-color: #667eea;">TC</div>
                <span class="comment-author">Test User</span>
                <span class="comment-time">just now</span>
            </div>
            <div class="comment-text">
                ${text}
            </div>
            <div class="comment-actions">
                <div class="comment-vote">
                    <button class="comment-vote-btn upvote">
                        <i class="fas fa-arrow-up"></i>
                    </button>
                    <div class="comment-vote-counts">
                        <span class="comment-upvote-number">0</span>
                        <span class="comment-vote-separator">•</span>
                        <span class="comment-downvote-number">0</span>
                    </div>
                    <button class="comment-vote-btn downvote">
                        <i class="fas fa-arrow-down"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
    
    commentList.appendChild(testComment);
    console.log('✅ Test comment created with ID:', testComment.id);
    
    // Process the test comment
    if (window.CommentLinkPreview) {
        setTimeout(() => {
            window.CommentLinkPreview.processComment(testComment);
            console.log('✅ Test comment processed');
        }, 100);
    }
    
    return testComment;
}

/**
 * Run all tests
 */
async function runAllTests() {
    console.log('🧪🧪🧪 Running All Tests 🧪🧪🧪\n');
    
    console.log('=== Test 1: Setup Check ===');
    checkSetup();
    
    console.log('\n=== Test 2: URL Detection ===');
    testLinkDetection();
    
    console.log('\n=== Test 3: API Test ===');
    await testAPI('https://github.com');
    
    console.log('\n=== Test 4: Batch API Test ===');
    await testBatchAPI(['https://github.com', 'https://stackoverflow.com']);
    
    console.log('\n=== Test 5: Create Test Comment ===');
    createTestComment();
    
    console.log('\n=== Test 6: Process All Comments ===');
    setTimeout(() => {
        testAllComments();
    }, 1000);
    
    console.log('\n✅ All tests completed!');
}

/**
 * Performance test - measure processing time
 */
async function performanceTest() {
    console.log('⚡ Running performance test...');
    
    const startTime = performance.now();
    
    // Test API call
    const apiStart = performance.now();
    await testAPI('https://github.com');
    const apiTime = performance.now() - apiStart;
    
    // Test batch API call
    const batchStart = performance.now();
    await testBatchAPI(['https://github.com', 'https://stackoverflow.com', 'https://reddit.com']);
    const batchTime = performance.now() - batchStart;
    
    // Test comment processing
    const processStart = performance.now();
    const testComment = createTestComment();
    if (testComment && window.CommentLinkPreview) {
        await window.CommentLinkPreview.processComment(testComment);
    }
    const processTime = performance.now() - processStart;
    
    const totalTime = performance.now() - startTime;
    
    console.log('⚡ Performance Results:');
    console.table({
        'Single API Call': `${apiTime.toFixed(2)}ms`,
        'Batch API Call (3 URLs)': `${batchTime.toFixed(2)}ms`,
        'Comment Processing': `${processTime.toFixed(2)}ms`,
        'Total Time': `${totalTime.toFixed(2)}ms`
    });
}

/**
 * Visual test - highlight all processed comments
 */
function visualTest() {
    console.log('👁️ Running visual test...');
    
    const processedComments = document.querySelectorAll('.comment-text[data-links-processed="true"]');
    const linkIcons = document.querySelectorAll('.comment-inline-link-icon');
    const linkPreviews = document.querySelectorAll('.comment-link-preview-card');
    
    console.log(`Found ${processedComments.length} processed comments`);
    console.log(`Found ${linkIcons.length} link icons`);
    console.log(`Found ${linkPreviews.length} link preview cards`);
    
    // Highlight processed comments
    processedComments.forEach(comment => {
        comment.style.border = '2px solid #28a745';
        comment.style.padding = '5px';
    });
    
    // Highlight link icons
    linkIcons.forEach(icon => {
        icon.style.outline = '2px solid #ffc107';
    });
    
    // Highlight preview cards
    linkPreviews.forEach(preview => {
        preview.style.outline = '3px solid #0dcaf0';
    });
    
    console.log('✅ Visual test complete. Check highlighted elements:');
    console.log('  🟢 Green border = Processed comments');
    console.log('  🟡 Yellow outline = Link icons');
    console.log('  🔵 Blue outline = Preview cards');
    
    // Remove highlights after 5 seconds
    setTimeout(() => {
        processedComments.forEach(c => { c.style.border = ''; c.style.padding = ''; });
        linkIcons.forEach(i => i.style.outline = '');
        linkPreviews.forEach(p => p.style.outline = '');
        console.log('Highlights removed');
    }, 5000);
}

// Export functions to window for easy access
window.CommentLinkPreviewTests = {
    checkSetup,
    testAPI,
    testBatchAPI,
    testLinkDetection,
    testAllComments,
    createTestComment,
    runAllTests,
    performanceTest,
    visualTest
};

console.log('\n✅ Testing script ready!');
console.log('Run: CommentLinkPreviewTests.runAllTests() to run all tests');
console.log('Or run individual tests like: CommentLinkPreviewTests.testAPI()');

