// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Conditional tooltips for truncated text
document.addEventListener('mouseover', function (e) {
    const target = e.target.closest('[data-bs-toggle="tooltip"]');
    if (!target) return;

    // Check if text is truncated (has ellipsis)
    // We check scrollWidth against offsetWidth to see if content is overflowing
    const isTruncated = target.scrollWidth > target.offsetWidth;
    
    let tooltip = bootstrap.Tooltip.getInstance(target);
    
    if (isTruncated) {
        // If truncated but no tooltip instance, create and show it
        if (!tooltip) {
            new bootstrap.Tooltip(target).show();
        }
    } else {
        // If not truncated but has tooltip instance, dispose it to prevent it from showing
        if (tooltip) {
            tooltip.dispose();
        }
    }
});
