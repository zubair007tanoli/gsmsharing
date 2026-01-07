// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('htmx:beforeRequest', function(evt) {
    // Show a global spinner or progress bar if needed
    // The 'detail' property is typical for CustomEvent, but HTMX might use a different event constructor.
    // For pure JS without TypeScript, 'evt.detail' usually works if the event has it.
    console.log("HTMX request started:", (evt as CustomEvent).detail?.elt);
});

document.addEventListener('htmx:afterRequest', function(evt) {
    // Hide global spinner, handle errors, etc.
    console.log("HTMX request finished:", (evt as CustomEvent).detail?.elt);
});


// Generic drag-and-drop enhancements for file input
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.file-drop-area').forEach(dropArea => {
        // Ensure fileInput exists before proceeding
        const fileInput = dropArea.querySelector('input[type="file"]') as HTMLInputElement | null;
        const dropTextElement = dropArea.querySelector('.drop-text') as HTMLElement | null;
        const defaultText = dropTextElement ? dropTextElement.textContent : 'Drag and drop files here, or click to browse';

        if (fileInput && dropTextElement) { // Null check for both fileInput and dropTextElement
            dropArea.addEventListener('dragover', (e: DragEvent) => { // Cast to DragEvent
                e.preventDefault();
                dropArea.classList.add('drag-over');
                dropTextElement.textContent = 'Drop files here!';
            });

            dropArea.addEventListener('dragleave', () => {
                dropArea.classList.remove('drag-over');
                dropTextElement.textContent = defaultText;
            });

            dropArea.addEventListener('drop', (e: DragEvent) => { // Cast to DragEvent
                e.preventDefault();
                dropArea.classList.remove('drag-over');
                // Check if dataTransfer and files exist
                const files = e.dataTransfer?.files;
                if (files && files.length > 0) {
                    fileInput.files = files; // files property exists on HTMLInputElement
                    dropTextElement.textContent = `${files.length} file(s) selected.`;
                    // Trigger HTMX if file input has hx-trigger="change"
                    if (fileInput.hasAttribute('hx-trigger') && fileInput.getAttribute('hx-trigger')?.includes('change')) {
                        fileInput.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                }
            });

            fileInput.addEventListener('change', () => {
                // Check if files exist on fileInput
                const files = fileInput.files;
                if (files && files.length > 0) {
                    dropTextElement.textContent = `${files.length} file(s) selected.`;
                } else {
                    dropTextElement.textContent = defaultText;
                }
            });
        }
    });
});

// SignalR setup - Basic client-side connection
// This would be more complex to integrate with specific job IDs etc.
// let connection = new signalR.HubConnectionBuilder().withUrl("/progressHub").build();

// connection.on("ReceiveProgress", function (message) {
//     console.log("Progress update: " + message);
//     // Update a specific progress bar element on the page
//     // document.getElementById("progressBar").style.width = message + "%";
//     // document.getElementById("progressBarText").innerText = message + "%";
// });

// connection.start().catch(function (err) {
//     return console.error(err.toString());
// });


        [HttpGet("jobs/{id}")] // Route: /api/jobs/{id}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JobStatus(string id)
        {
            // ... implementation ...
        }
