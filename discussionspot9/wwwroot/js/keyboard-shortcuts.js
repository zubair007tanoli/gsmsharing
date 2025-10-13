// Keyboard Shortcuts
(function() {
    const shortcuts = {
        '/': 'Focus search',
        'g h': 'Go to home',
        'g p': 'Go to popular',
        'g c': 'Go to communities',
        'c': 'Create post',
        'n': 'Open notifications',
        '?': 'Show shortcuts help',
        'esc': 'Close modals/clear focus'
    };

    let keySequence = '';
    let sequenceTimeout;

    // Keyboard shortcuts help modal
    function showShortcutsHelp() {
        const modal = document.createElement('div');
        modal.className = 'shortcuts-modal';
        modal.innerHTML = `
            <div class="shortcuts-modal-content">
                <div class="shortcuts-header">
                    <h3><i class="fas fa-keyboard"></i> Keyboard Shortcuts</h3>
                    <button class="shortcuts-close" onclick="this.parentElement.parentElement.parentElement.remove()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="shortcuts-body">
                    ${Object.entries(shortcuts).map(([key, desc]) => `
                        <div class="shortcut-item">
                            <kbd>${key}</kbd>
                            <span>${desc}</span>
                        </div>
                    `).join('')}
                </div>
            </div>
        `;
        document.body.appendChild(modal);
        
        // Close on click outside
        modal.addEventListener('click', (e) => {
            if (e.target === modal) modal.remove();
        });
    }

    document.addEventListener('keydown', function(e) {
        // Ignore if typing in an input field
        if (e.target.matches('input, textarea, select')) {
            if (e.key === 'Escape') {
                e.target.blur();
            }
            return;
        }

        // Handle single key shortcuts
        if (e.key === '/') {
            e.preventDefault();
            document.getElementById('headerSearchInput')?.focus();
            return;
        }

        if (e.key === 'Escape') {
            // Close any open modals
            document.querySelectorAll('.modal.show').forEach(modal => {
                const backdrop = document.querySelector('.modal-backdrop');
                modal.classList.remove('show');
                backdrop?.remove();
            });
            // Close shortcuts help
            document.querySelector('.shortcuts-modal')?.remove();
            // Blur active element
            document.activeElement?.blur();
            return;
        }

        if (e.key === '?') {
            e.preventDefault();
            showShortcutsHelp();
            return;
        }

        if (e.key === 'c' && !e.ctrlKey && !e.metaKey) {
            e.preventDefault();
            window.location.href = '/create';
            return;
        }

        if (e.key === 'n' && !e.ctrlKey && !e.metaKey) {
            e.preventDefault();
            document.querySelector('.notification-icon')?.click();
            return;
        }

        // Handle two-key sequences (g + something)
        clearTimeout(sequenceTimeout);
        keySequence += e.key;

        sequenceTimeout = setTimeout(() => {
            keySequence = '';
        }, 1000);

        if (keySequence === 'gh') {
            e.preventDefault();
            window.location.href = '/';
            keySequence = '';
        } else if (keySequence === 'gp') {
            e.preventDefault();
            window.location.href = '/popular';
            keySequence = '';
        } else if (keySequence === 'gc') {
            e.preventDefault();
            window.location.href = '/communities';
            keySequence = '';
        }
    });
})();

