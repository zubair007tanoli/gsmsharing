    // Global state
    let selectedCategory = null;

    // Initialize dark mode from localStorage
    function initializeDarkMode() {
        const savedDarkMode = localStorage.getItem('darkMode');
    if (savedDarkMode === 'true') {
        document.body.classList.add('dark-mode');
        }
    }

    // Initialize
    document.addEventListener('DOMContentLoaded', function() {
        initializeDarkMode();
    initializeEventListeners();
    setCreatorId();
    updatePreview();
    });

    function setCreatorId() {
        // Set the current user ID - you'll need to get this from your authentication system
        document.getElementById('creatorId').value = 'current-user-id'; // Replace with actual user ID
    }

    function initializeEventListeners() {
        // Character count tracking
        document.getElementById('communityName').addEventListener('input', function (e) {
            updateCharCount(e.target, 'nameCount', 100);
            generateSlug();
            updatePreview();
        });

    document.getElementById('communityTitle').addEventListener('input', function(e) {
        updateCharCount(e.target, 'titleCount', 200);
    updatePreview();
        });

    document.getElementById('shortDescription').addEventListener('input', function(e) {
        updateCharCount(e.target, 'shortDescCount', 500);
    updatePreview();
        });

    // Category and type selection
    document.getElementById('categorySelect').addEventListener('change', updatePreview);
    document.getElementById('communityType').addEventListener('change', updatePreview);

    // Theme color
    document.getElementById('themeColor').addEventListener('change', function(e) {
        document.getElementById('previewIcon').style.background = e.target.value;
        });

    // File uploads
    document.getElementById('iconUpload').addEventListener('change', function(e) {
        handleImageUpload(e, 'iconPreview', 'iconImage');
        });

    document.getElementById('bannerUpload').addEventListener('change', function(e) {
        handleImageUpload(e, 'bannerPreview', 'bannerImage');
        });

    // Rules editor
    document.getElementById('rulesEditor').addEventListener('input', syncRulesContent);

    // Form submission
    document.getElementById('communityForm').addEventListener('submit', handleFormSubmit);

    // Save as draft
    document.getElementById('saveAsDraft').addEventListener('click', function() {
        showStatus('info', 'Saving as draft...');
            setTimeout(() => {
        showStatus('success', 'Draft saved successfully!');
            }, 1000);
        });

    // Preview
    document.getElementById('previewBtn').addEventListener('click', function() {
        showStatus('info', 'Opening preview...');
        });

        // Toggle switches
        document.querySelectorAll('.toggle-switch').forEach(toggle => {
        toggle.addEventListener('click', function () {
            const checkbox = this.querySelector('input[type="checkbox"]');
            checkbox.checked = !checkbox.checked;
        });
        });
    }

    function updateCharCount(input, counterId, maxLength) {
        const current = input.value.length;
    document.getElementById(counterId).textContent = current;

    const counter = document.getElementById(counterId);
        if (current > maxLength * 0.9) {
        counter.style.color = '#f59e0b';
        } else {
        counter.style.color = '#7c7c7c';
        }

        if (current >= maxLength) {
        counter.style.color = '#ef4444';
        }
    }

    function generateSlug() {
        const name = document.getElementById('communityName').value;
    const slug = name
    .toLowerCase()
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
    .trim('-');
    document.getElementById('slug').value = slug;
    }

    function updatePreview() {
        const name = document.getElementById('communityName').value || 'Community Name';
    const shortDesc = document.getElementById('shortDescription').value || 'Community description will appear here';
    const categorySelect = document.getElementById('categorySelect');
    const typeSelect = document.getElementById('communityType');

    const category = categorySelect.options[categorySelect.selectedIndex]?.text || 'Category';
    const type = typeSelect.options[typeSelect.selectedIndex]?.text.split(' -')[0] || 'Public';

    document.getElementById('previewName').textContent = name;
    document.getElementById('previewDescription').textContent = shortDesc;
    document.getElementById('previewCategory').textContent = category;
    document.getElementById('previewType').textContent = type;

    // Update icon with first letter
    const firstLetter = name.charAt(0).toUpperCase() || 'C';
    document.getElementById('previewIcon').textContent = firstLetter;
    }

    function handleImageUpload(event, previewId, imageId) {
        const file = event.target.files[0];
    if (file) {
            if (file.size > 5 * 1024 * 1024) {
        showStatus('error', 'File size must be less than 5MB');
    return;
            }

    const reader = new FileReader();
    reader.onload = function(e) {
        document.getElementById(imageId).src = e.target.result;
    document.getElementById(previewId).style.display = 'block';
            };
    reader.readAsDataURL(file);
        }
    }

    function syncRulesContent() {
        const rulesContent = document.getElementById('rulesEditor').innerHTML;
    document.getElementById('rulesHidden').value = rulesContent;
    }

    function handleFormSubmit(e) {
        e.preventDefault();

    // Validate required fields
    const name = document.getElementById('communityName').value.trim();
    const title = document.getElementById('communityTitle').value.trim();
    const shortDesc = document.getElementById('shortDescription').value.trim();
    const category = document.getElementById('categorySelect').value;

    if (!name || !title || !shortDesc || !category) {
        showStatus('error', 'Please fill in all required fields');
    return;
        }

    // Sync rules content
    syncRulesContent();

    showStatus('info', 'Creating community...');

        // Here you would submit the form to your server
        setTimeout(() => {
        showStatus('success', 'Community created successfully!');
        }, 2000);
    }

    function showStatus(type, message) {
        const statusMessage = document.getElementById('statusMessage');
    const statusText = document.getElementById('statusText');

    statusMessage.className = `status-message ${type}`;
    statusText.textContent = message;
    statusMessage.style.display = 'block';

        setTimeout(() => {
        statusMessage.style.display = 'none';
        }, 3000);
    }

    // Dark mode toggle functionality
    function toggleDarkMode() {
        document.body.classList.toggle('dark-mode');
    const isDarkMode = document.body.classList.contains('dark-mode');
    localStorage.setItem('darkMode', isDarkMode);
    showStatus('info', isDarkMode ? 'Dark mode enabled' : 'Light mode enabled');
    }