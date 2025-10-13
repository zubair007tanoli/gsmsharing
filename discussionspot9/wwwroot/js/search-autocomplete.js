// Search Autocomplete
(function() {
    const searchInput = document.getElementById('headerSearchInput');
    const suggestionsBox = document.getElementById('searchSuggestions');
    
    if (!searchInput || !suggestionsBox) return;

    let debounceTimer;
    let currentFocus = -1;

    searchInput.addEventListener('input', function() {
        clearTimeout(debounceTimer);
        const query = this.value.trim();

        if (query.length < 2) {
            suggestionsBox.style.display = 'none';
            return;
        }

        debounceTimer = setTimeout(() => fetchSuggestions(query), 300);
    });

    searchInput.addEventListener('keydown', function(e) {
        const items = suggestionsBox.getElementsByClassName('suggestion-item');
        
        if (e.keyCode === 40) { // Down arrow
            e.preventDefault();
            currentFocus++;
            if (currentFocus >= items.length) currentFocus = 0;
            setActive(items);
        } else if (e.keyCode === 38) { // Up arrow
            e.preventDefault();
            currentFocus--;
            if (currentFocus < 0) currentFocus = items.length - 1;
            setActive(items);
        } else if (e.keyCode === 13) { // Enter
            if (currentFocus > -1 && items[currentFocus]) {
                e.preventDefault();
                items[currentFocus].click();
            }
        } else if (e.keyCode === 27) { // Escape
            suggestionsBox.style.display = 'none';
            currentFocus = -1;
        }
    });

    function setActive(items) {
        for (let i = 0; i < items.length; i++) {
            items[i].classList.remove('active');
        }
        if (items[currentFocus]) {
            items[currentFocus].classList.add('active');
        }
    }

    async function fetchSuggestions(query) {
        try {
            const response = await fetch(`/api/search/suggestions?q=${encodeURIComponent(query)}`);
            const data = await response.json();

            if (data.suggestions && data.suggestions.length > 0) {
                renderSuggestions(data.suggestions);
            } else {
                suggestionsBox.style.display = 'none';
            }
        } catch (error) {
            console.error('Error fetching suggestions:', error);
        }
    }

    function renderSuggestions(suggestions) {
        suggestionsBox.innerHTML = '';
        currentFocus = -1;

        suggestions.forEach(suggestion => {
            const div = document.createElement('div');
            div.className = 'suggestion-item';
            div.innerHTML = `
                <div class="d-flex align-items-center">
                    <span class="me-2">${suggestion.icon}</span>
                    <div class="flex-grow-1">
                        <div class="suggestion-title">${suggestion.title}</div>
                        <small class="text-muted">${suggestion.meta}</small>
                    </div>
                </div>
            `;
            div.addEventListener('click', () => {
                window.location.href = suggestion.url;
            });
            suggestionsBox.appendChild(div);
        });

        suggestionsBox.style.display = 'block';
    }

    // Close suggestions when clicking outside
    document.addEventListener('click', function(e) {
        if (!searchInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
            suggestionsBox.style.display = 'none';
        }
    });
})();

