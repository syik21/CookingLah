(function () {
    "use strict";
    var body = document.body;
    var sidebar = document.getElementById("masterSidebar");
    var toggle = document.getElementById("sidebarToggle");
    var close = document.getElementById("sidebarClose");
    var overlay = document.getElementById("sidebarOverlay");
    var main = document.getElementById("masterMain");
    var header = document.getElementById("masterHeader");
    var mobile = window.matchMedia("(max-width: 860px)");
    var previousFocus;
    body.classList.add("nav-ready");

    function closeSidebar(restoreFocus) {
        body.classList.remove("sidebar-open");
        toggle.setAttribute("aria-expanded", "false");
        overlay.hidden = true;
        main.inert = false;
        header.inert = false;
        sidebar.removeAttribute("role");
        sidebar.removeAttribute("aria-modal");
        if (restoreFocus && previousFocus) { previousFocus.focus(); }
    }
    function openSidebar() {
        if (!mobile.matches) { return; }
        previousFocus = document.activeElement;
        body.classList.add("sidebar-open");
        toggle.setAttribute("aria-expanded", "true");
        overlay.hidden = false;
        sidebar.setAttribute("role", "dialog");
        sidebar.setAttribute("aria-modal", "true");
        main.inert = true;
        header.inert = true;
        close.focus();
    }
    toggle.addEventListener("click", openSidebar);
    close.addEventListener("click", function () { closeSidebar(true); });
    overlay.addEventListener("click", function () { closeSidebar(true); });
    sidebar.addEventListener("click", function (event) {
        if (event.target.closest("a[href]")) { closeSidebar(true); }
    });
    document.addEventListener("keydown", function (event) {
        if (!body.classList.contains("sidebar-open")) { return; }
        if (event.key === "Escape") { event.preventDefault(); closeSidebar(true); }
        if (event.key === "Tab") {
            var items = sidebar.querySelectorAll("button, a[href]");
            var first = items[0], last = items[items.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        }
    });
    mobile.addEventListener("change", function () {
        var focusInSidebar = sidebar.contains(document.activeElement);
        closeSidebar(false);
        if (focusInSidebar) {
            if (mobile.matches) { toggle.focus(); }
            else { sidebar.querySelector("a[href]").focus(); }
        }
    });
    // Supports both .aspx and FriendlyUrls extensionless page routes.
    var current = window.location.pathname.replace(/\/$/, "").split("/").pop().replace(/\.aspx$/i, "").toLowerCase();
    sidebar.querySelectorAll("a[data-page]").forEach(function (link) {
        var active = link.dataset.page.toLowerCase() === current;
        link.classList.toggle("active", active);
        if (active) { link.setAttribute("aria-current", "page"); }
        else { link.removeAttribute("aria-current"); }
    });
}());
