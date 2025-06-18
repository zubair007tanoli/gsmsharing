// Global state
let currentPostType = 'text';
let tags = [];
let uploadedFiles = [];

// Initialize dark mode from localStorage
function initializeDarkMode() {
    const savedDarkMode = localStorage.getItem('darkMode');
    if (savedDarkMode === 'true') {
        document.body.classList.add('dark-mode');
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    initializeDarkMode();
    initializeEventListeners();
    generateSlug();

    // Initialize from model
    if (document.getElementById('communityId').value) {
        const communityName = document.getElementById('communityName').value;
        document.getElementById('communitySearch').value = communityName;

        // Update rules header
        document.getElementById('rulesHeader').innerHTML =
            `<i class="fas fa-shield-alt"></i> ${communityName} Rules`;
    }
});

function initializeEventListeners() {
    // Post type tabs
    document.querySelectorAll('.post-tab').forEach(tab => {
        tab.addEventListener('click', handleTabClick);
    });

    // Community search and selection
    const communitySearch = document.getElementById('communitySearch');
    const communityDropdown = document.getElementById('communityDropdown');

    communitySearch.addEventListener('focus', () => {
        communityDropdown.style.display = 'block';
    });

    communitySearch.addEventListener('input', handleCommunitySearch);

    document.querySelectorAll('.community-item').forEach(item => {
        item.addEventListener('click', handleCommunitySelect);
    });

    // Title and slug generation
    document.getElementById('title').addEventListener('input', function (e) {
        updateCharCount(e.target, 'titleCount', 300);
        generateSlug();
    });

    // Tag input
    document.getElementById('tagInput').addEventListener('keypress', handleTagInput);

    // Poll options
    document.getElementById('addPollOption').addEventListener('click', addPollOption);

    // Media upload
    document.getElementById('mediaUpload').addEventListener('change', handleMediaUpload);

    // Form submission
    document.getElementById('postForm').addEventListener('submit', handleFormSubmit);

    // Editor content sync
    document.getElementById('textEditor').addEventListener('input', syncEditorContent);

    // Click outside to close dropdowns
    document.addEventListener('click', handleOutsideClick);

    // Save as draft
    document.getElementById('saveAsDraft').addEventListener('click', function () {
        document.querySelector('input[name="Status"]').value = 'draft';
        showStatus('info', 'Saving as draft...');
        document.getElementById('postForm').submit();
    });

    // Preview
    document.getElementById('previewBtn').addEventListener('click', function () {
        showStatus('info', 'Opening preview...');
    });

    // SEO Toggle
    document.getElementById('seoToggle').addEventListener('click', function () {
        const seoSection = document.getElementById('seoSection');
        const isVisible = seoSection.style.display !== 'none';
        seoSection.style.display = isVisible ? 'none' : 'block';
        this.innerHTML = isVisible ?
            '<i class="fas fa-chevron-down"></i> Show SEO Settings' :
            '<i class="fas fa-chevron-up"></i> Hide SEO Settings';
    });

    // Toggle switches
    document.querySelectorAll('.toggle-switch').forEach(toggle => {
        toggle.addEventListener('click', function () {
            const checkbox = this.querySelector('input[type="checkbox"]');
            checkbox.checked = !checkbox.checked;
        });
    });
}

function handleTabClick(e) {
    e.preventDefault();

    // Remove active class from all tabs
    document.querySelectorAll('.post-tab').forEach(tab => {
        tab.classList.remove('active');
    });

    // Hide all content sections
    document.querySelectorAll('.content-section').forEach(section => {
        section.classList.remove('active');
    });

    // Add active class to clicked tab
    e.currentTarget.classList.add('active');
    currentPostType = e.currentTarget.getAttribute('data-type');
    document.getElementById('postType').value = currentPostType;

    // Show corresponding content section
    const targetSection = document.getElementById(currentPostType + 'Content');
    if (targetSection) {
        targetSection.classList.add('active');
    }

    // Update required fields based on post type
    updateRequiredFields();
}

function updateRequiredFields() {
    // Reset all required fields
    document.querySelectorAll('[required]').forEach(field => {
        if (field.id !== 'title' && field.id !== 'communitySearch' && field.id !== 'communityId') {
            field.removeAttribute('required');
        }
    });

    // Set required fields based on post type
    switch (currentPostType) {
        case 'image':
            document.getElementById('mediaUpload').setAttribute('required', 'required');
            break;
        case 'link':
            document.getElementById('linkUrl').setAttribute('required', 'required');
            break;
        case 'poll':
            document.querySelectorAll('.poll-option-input').forEach(input => {
                input.setAttribute('required', 'required');
            });
            break;
    }
}

function handleCommunitySearch(e) {
    const searchTerm = e.target.value.toLowerCase();
    const communityItems = document.querySelectorAll('.community-item');

    communityItems.forEach(item => {
        const communityName = item.querySelector('h4').textContent.toLowerCase();
        if (communityName.includes(searchTerm)) {
            item.style.display = 'flex';
        } else {
            item.style.display = 'none';
        }
    });
}

function handleCommunitySelect(e) {
    const communityItem = e.currentTarget;
    const communityName = communityItem.querySelector('h4').textContent;
    const communitySlug = communityItem.getAttribute('data-slug');
    const communityId = communityItem.getAttribute('data-id');

    // Update form fields
    document.getElementById('communityId').value = communityId;
    document.getElementById('communityName').value = communityName;
    document.getElementById('communitySlug').value = communitySlug;
    document.getElementById('communitySearch').value = communityName;

    // Update UI
    document.getElementById('communityDropdown').style.display = 'none';
    document.getElementById('rulesHeader').innerHTML =
        `<i class="fas fa-shield-alt"></i> ${communityName} Rules`;

    showStatus('success', `Selected ${communityName}`);
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
    const title = document.getElementById('title').value;
    const slug = title
        .toLowerCase()
        .replace(/[^a-z0-9\s-]/g, '')
        .replace(/\s+/g, '-')
        .replace(/-+/g, '-')
        .trim('-');
    // Note: Slug will be generated server-side for database consistency
}

function handleTagInput(e) {
    if (e.key === 'Enter' && e.target.value.trim() !== '') {
        e.preventDefault();

        if (tags.length < 5) {
            const tagText = e.target.value.trim();

            if (!tags.includes(tagText)) {
                tags.push(tagText);

                const tagElement = document.createElement('div');
                tagElement.className = 'tag';
                tagElement.innerHTML = `
                    ${tagText}
                    <button type="button" class="tag-remove" onclick="removeTag('${tagText}', this)">
                        <i class="fas fa-times"></i>
                    </button>
                `;

                const container = document.getElementById('tagContainer');
                container.insertBefore(tagElement, e.target);
                e.target.value = '';

                // Update hidden input
                document.getElementById('tagsInputHidden').value = tags.join(',');
            }
        } else {
            showStatus('warning', 'Maximum 5 tags allowed');
        }
    }
}

function removeTag(tagText, button) {
    const index = tags.indexOf(tagText);
    if (index > -1) {
        tags.splice(index, 1);
    }
    button.parentElement.remove();
    document.getElementById('tagsInputHidden').value = tags.join(',');
}

function addPollOption() {
    const pollOptions = document.getElementById('pollOptions');
    const optionCount = pollOptions.children.length;

    if (optionCount < 6) {
        const newOption = document.createElement('div');
        newOption.className = 'poll-option';
        newOption.innerHTML = `
            <input type="text" class="form-input poll-option-input" name="PollOptions" placeholder="Option ${optionCount + 1}" required>
            <button type="button" class="poll-remove" onclick="removePollOption(this)">
                <i class="fas fa-times"></i>
            </button>
        `;

        pollOptions.appendChild(newOption);
        updatePollRemoveButtons();
    } else {
        showStatus('warning', 'Maximum 6 poll options allowed');
    }
}

function removePollOption(button) {
    const pollOptions = document.getElementById('pollOptions');
    if (pollOptions.children.length > 2) {
        button.parentElement.remove();
        updatePollRemoveButtons();
    }
}

function updatePollRemoveButtons() {
    const pollOptions = document.getElementById('pollOptions');
    const removeButtons = pollOptions.querySelectorAll('.poll-remove');

    removeButtons.forEach((button, index) => {
        if (pollOptions.children.length <= 2) {
            button.disabled = true;
        } else {
            button.disabled = false;
        }
    });
}

function handleMediaUpload(e) {
    const files = Array.from(e.target.files);
    const preview = document.getElementById('mediaPreview');

    files.forEach(file => {
        if (file.size > 10 * 1024 * 1024) {
            showStatus('error', `File ${file.name} is too large. Maximum size is 10MB.`);
            return;
        }

        const mediaItem = document.createElement('div');
        mediaItem.className = 'media-item';

        if (file.type.startsWith('image/')) {
            const img = document.createElement('img');
            img.src = URL.createObjectURL(file);
            mediaItem.appendChild(img);
        } else if (file.type.startsWith('video/')) {
            const video = document.createElement('video');
            video.src = URL.createObjectURL(file);
            video.controls = true;
            mediaItem.appendChild(video);
        }

        const removeBtn = document.createElement('button');
        removeBtn.className = 'media-remove';
        removeBtn.innerHTML = '<i class="fas fa-times"></i>';
        removeBtn.onclick = () => mediaItem.remove();

        mediaItem.appendChild(removeBtn);
        preview.appendChild(mediaItem);
    });
}

function syncEditorContent() {
    const editorContent = document.getElementById('textEditor').innerHTML;
    document.getElementById('contentHidden').value = editorContent;
}

function handleFormSubmit(e) {
    // Validate required fields
    if (!document.getElementById('communityId').value) {
        e.preventDefault();
        showStatus('error', 'Please select a community');
        return;
    }

    const title = document.getElementById('title').value.trim();
    if (!title) {
        e.preventDefault();
        showStatus('error', 'Please enter a title');
        return;
    }

    // Sync editor content
    syncEditorContent();

    showStatus('info', 'Publishing post...');
}

function handleOutsideClick(e) {
    const communityDropdown = document.getElementById('communityDropdown');
    const communitySearch = document.getElementById('communitySearch');

    if (!communitySearch.contains(e.target) && !communityDropdown.contains(e.target)) {
        communityDropdown.style.display = 'none';
    }
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

function showCreateCommunityModal() {
    showStatus('info', 'Opening create community modal...');
    // Here you would open a modal for creating a new community
}