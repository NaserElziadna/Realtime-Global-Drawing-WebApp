// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Handle orientation changes on mobile devices
window.addEventListener('orientationchange', function() {
    // Wait for the orientation change to complete
    setTimeout(function() {
        // Trigger a resize event to redraw the canvas
        window.dispatchEvent(new Event('resize'));
        
        // Explicitly call redraw if available
        if (typeof redraw === 'function') {
            redraw();
        }
    }, 200);
});

// Update UI based on screen size
function updateUIForScreenSize() {
    const isMobile = window.innerWidth <= 768;
    
    // Adjust any UI elements based on screen size
    if (isMobile) {
        // Mobile-specific adjustments
        document.querySelectorAll('.btn-toolbar .btn').forEach(btn => {
            if (btn.textContent.trim() === 'Copy URL') {
                btn.textContent = '📋';
            }
        });
    } else {
        // Desktop-specific adjustments
        document.querySelectorAll('.btn-toolbar .btn').forEach(btn => {
            if (btn.textContent.trim() === '📋') {
                btn.textContent = 'Copy URL';
            }
        });
    }
}

// Call once on load and then on resize
document.addEventListener('DOMContentLoaded', updateUIForScreenSize);
window.addEventListener('resize', updateUIForScreenSize);
