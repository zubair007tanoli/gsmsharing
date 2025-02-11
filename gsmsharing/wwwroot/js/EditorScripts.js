
    let currentColorCommand = null;

    function formatText(command, value = null) {
        document.execCommand(command, false, value);
    document.getElementById('editor').focus();
        }

    function openLinkModal() {
        new bootstrap.Modal(document.getElementById('linkModal')).show();
        }

    function insertLink() {
            const url = document.getElementById('linkUrl').value;
    if (url) {
                const selection = window.getSelection();
                if (selection.rangeCount > 0) {
                    const range = selection.getRangeAt(0);
    const selectedText = range.extractContents();
    const a = document.createElement('a');
    a.href = url;
    a.target = "_blank";
    a.appendChild(selectedText);
    range.insertNode(a);
                }
            }
    bootstrap.Modal.getInstance(document.getElementById('linkModal')).hide();
        }

    function openImageModal() {
        new bootstrap.Modal(document.getElementById('imageModal')).show();
        }

    function insertImage() {
            const fileInput = document.getElementById('imageFile');
    const alt = document.getElementById('imageAlt').value;

    if (fileInput.files && fileInput.files[0]) {
                const reader = new FileReader();
    reader.onload = function(e) {
                    const image = document.createElement('img');
    image.src = e.target.result;
    image.alt = alt;
    image.className = 'resizable-image';
    image.style.maxWidth = '100%';
    document.getElementById('editor').appendChild(image);
    makeImageResizable(image);
                };
    reader.readAsDataURL(fileInput.files[0]);
            } else if (document.getElementById('imageUrl').value) {
                const image = document.createElement('img');
    image.src = document.getElementById('imageUrl').value;
    image.alt = alt;
    image.className = 'resizable-image';
    image.style.maxWidth = '100%';
    document.getElementById('editor').appendChild(image);
    makeImageResizable(image);
            }

    bootstrap.Modal.getInstance(document.getElementById('imageModal')).hide();
        }

    function makeImageResizable(image) {
        image.addEventListener('mousedown', function (e) {
            e.preventDefault();
            const startX = e.clientX;
            const startY = e.clientY;
            const startWidth = parseInt(document.defaultView.getComputedStyle(image).width, 10);
            const startHeight = parseInt(document.defaultView.getComputedStyle(image).height, 10);

            function onMouseMove(e) {
                const width = startWidth + (e.clientX - startX);
                const height = startHeight + (e.clientY - startY);
                image.style.width = width + 'px';
                image.style.height = height + 'px';
            }

            function onMouseUp() {
                document.removeEventListener('mousemove', onMouseMove);
                document.removeEventListener('mouseup', onMouseUp);
            }

            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
        });
        }

    function openColorPicker(command) {
        currentColorCommand = command;
    new bootstrap.Modal(document.getElementById('colorPickerModal')).show();
        }

    function applyColor() {
            const color = document.getElementById('colorPicker').value;
    formatText(currentColorCommand, color);
    bootstrap.Modal.getInstance(document.getElementById('colorPickerModal')).hide();
        }

    function insertTable() {
        new bootstrap.Modal(document.getElementById('tableModal')).show();
        }

    function createTable() {
            const rows = document.getElementById('rows').value;
    const columns = document.getElementById('columns').value;
    const table = document.createElement('table');
    table.className = 'table table-bordered';
    for (let i = 0; i < rows; i++) {
                const tr = document.createElement('tr');
    for (let j = 0; j < columns; j++) {
                    const td = document.createElement('td');
    td.contentEditable = true;
    tr.appendChild(td);
                }
    table.appendChild(tr);
            }
    document.getElementById('editor').appendChild(table);
    bootstrap.Modal.getInstance(document.getElementById('tableModal')).hide();
    addTableControls(table);
        }

    function addTableControls(table) {
        table.addEventListener('click', function (e) {
            const target = e.target;
            if (target.tagName === 'TD' || target.tagName === 'TH') {
                // Add row
                if (e.offsetX > target.offsetWidth - 20 && e.offsetY > target.offsetHeight - 20) {
                    const row = target.parentElement.cloneNode(true);
                    Array.from(row.children).forEach(cell => cell.innerHTML = '');
                    target.parentElement.parentElement.appendChild(row);
                }
                // Remove row
                if (e.offsetX > target.offsetWidth - 20 && e.offsetY < 20) {
                    if (target.parentElement.parentElement.children.length > 1) {
                        target.parentElement.remove();
                    }
                }
                // Add column
                if (e.offsetX < 20 && e.offsetY > target.offsetHeight - 20) {
                    Array.from(target.parentElement.parentElement.children).forEach(row => {
                        const cell = document.createElement(target.tagName);
                        cell.contentEditable = true;
                        row.appendChild(cell);
                    });
                }
                // Remove column
                if (e.offsetX < 20 && e.offsetY < 20) {
                    if (target.parentElement.children.length > 1) {
                        Array.from(target.parentElement.parentElement.children).forEach(row => {
                            row.removeChild(row.lastElementChild);
                        });
                    }
                }
            }
        });
        }

    function submitComment() {
        const commentContent = document.getElementById('editor').innerHTML;
        document.getElementById('Post_Content').value = commentContent;

        alert(document.getElementById('Post_Content').value);
    document.getElementById('editor').innerHTML = '';
}

//purify
async function submitComment() {
    // Get raw HTML from editor
    const rawHtml = document.getElementById('editor').innerHTML;

    // Sanitize HTML
    const cleanHtml = sanitizeHtml(rawHtml);

    // Optional: Further cleaning for empty tags
    const finalHtml = removeEmptyTags(cleanHtml);
    document.getElementById('Post_Content').value = finalHtml;
    alert(finalHtml);
    // Submit to controller (example using fetch)
}

// HTML Sanitization using DOMPurify
function sanitizeHtml(dirty) {
    return DOMPurify.sanitize(dirty, {
        ALLOWED_TAGS: [
            'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
            'strong', 'em', 'u', 'a', 'img', 'ul',
            'ol', 'li', 'p', 'div', 'span', 'br',
            'table', 'tr', 'td', 'th', 'tbody', 'thead','b'
        ],
        ALLOWED_ATTR: [
            'href', 'target', 'src', 'alt', 'title',
            'style', 'class', 'width', 'height',
            'contenteditable', 'data-*'
        ],
        FORBID_TAGS: ['script', 'iframe', 'object', 'embed'],
        FORBID_ATTR: ['onclick', 'onerror', 'onload'],
        ADD_ATTR: ['target']
    });
}

// Additional cleaning for empty tags
function removeEmptyTags(html) {
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = html;

    // Remove empty tags
    const elements = tempDiv.querySelectorAll('p, div, span');
    elements.forEach(el => {
        if (!el.textContent.trim() && el.children.length === 0) {
            el.remove();
        }
    });

    // Clean up line breaks
    return tempDiv.innerHTML
        .replace(/<br>\s*<\/br>/g, '')
        .replace(/(<br[^>]*>){2,}/g, '<br>')
        .trim();
}
//purify ends

    document.getElementById('headingSelect').addEventListener('change', function() {
        formatText('formatBlock', this.value);
    this.selectedIndex = 0;
        });

    document.getElementById('fontSelect').addEventListener('change', function() {
        formatText('fontName', this.value);
    this.selectedIndex = 0;
        });

    document.getElementById('imageFile').addEventListener('change', function(e) {
            const file = e.target.files[0];
    if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
    reader.onload = function(e) {
                    const preview = document.getElementById('imagePreview');
    preview.src = e.target.result;
    preview.style.display = 'block';
    document.getElementById('imageUrl').value = '';
                };
    reader.readAsDataURL(file);
            }
        });

    document.getElementById('imageUrl').addEventListener('input', function(e) {
            const preview = document.getElementById('imagePreview');
    preview.src = e.target.value;
    preview.style.display = e.target.value ? 'block' : 'none';
    document.getElementById('imageFile').value = '';
        });
