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
    var sidebarStateKey = "cookinglah.sidebarCollapsed";
    // Keep the desktop preference across navigation and Web Forms postbacks.
    try {
        body.classList.toggle("sidebar-collapsed", window.sessionStorage.getItem(sidebarStateKey) === "true");
    } catch (error) {
        // Navigation still works when browser storage is unavailable.
    }
    body.classList.add("nav-ready");

    function updateToggle() {
        var expanded = mobile.matches
            ? body.classList.contains("sidebar-open")
            : !body.classList.contains("sidebar-collapsed");
        toggle.setAttribute("aria-expanded", String(expanded));
        var label = mobile.matches
            ? (expanded ? "Close navigation" : "Open navigation")
            : (expanded ? "Collapse navigation" : "Expand navigation");
        toggle.setAttribute("aria-label", label);
        toggle.setAttribute("title", label);
    }

    function closeSidebar(restoreFocus) {
        body.classList.remove("sidebar-open");
        updateToggle();
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
        updateToggle();
        overlay.hidden = false;
        sidebar.setAttribute("role", "dialog");
        sidebar.setAttribute("aria-modal", "true");
        main.inert = true;
        header.inert = true;
        close.focus();
    }
    toggle.addEventListener("click", function () {
        if (mobile.matches) {
            if (body.classList.contains("sidebar-open")) { closeSidebar(true); }
            else { openSidebar(); }
        } else {
            body.classList.toggle("sidebar-collapsed");
            try {
                window.sessionStorage.setItem(sidebarStateKey, String(body.classList.contains("sidebar-collapsed")));
            } catch (error) {
                // Storage restrictions must not prevent toggling the sidebar.
            }
            updateToggle();
        }
    });
    close.addEventListener("click", function () { closeSidebar(true); });
    overlay.addEventListener("click", function () { closeSidebar(true); });
    sidebar.addEventListener("click", function (event) {
        if (mobile.matches && event.target.closest("a[href]")) { closeSidebar(true); }
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
    // Icon-only links retain their names for assistive technology.
    sidebar.querySelectorAll(".master-nav-item").forEach(function (link) {
        var label = link.querySelector("span");
        if (label) {
            link.setAttribute("aria-label", label.textContent.trim()
                + (link.getAttribute("aria-disabled") === "true" ? " (Coming soon)" : ""));
        }
    });
    updateToggle();
    // Supports both .aspx and FriendlyUrls extensionless page routes.
    var current = window.location.pathname.replace(/\/$/, "").split("/").pop().replace(/\.aspx$/i, "").toLowerCase();
    sidebar.querySelectorAll("a[data-page]").forEach(function (link) {
        var active = link.dataset.page.toLowerCase().split(",").includes(current);
        link.classList.toggle("active", active);
        if (active) { link.setAttribute("aria-current", "page"); }
        else { link.removeAttribute("aria-current"); }
    });
}());
