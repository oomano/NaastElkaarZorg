window.scrollToTop = () => {
    window.scrollTo({
        top: 0,
        behavior: "instant"
    });
};

// Close the mobile nav menu when any nav link is clicked.
// Uses event delegation so it survives Blazor enhanced-nav DOM updates.
// Pure vanilla JS — no Blazor circuit dependency, runs immediately on load.
document.addEventListener('click', function (e) {
    if (e.target.closest('nav ul li a')) {
        var checkbox = document.getElementById('nav-toggle');
        if (checkbox) checkbox.checked = false;
    }
});