(function () {
    "use strict";
    var header = document.getElementById("publicHeader");
    var toggle = document.getElementById("publicMenuToggle");
    var nav = document.getElementById("publicNavigation");
    if (!header || !toggle || !nav) { return; }
    var mobile = window.matchMedia("(max-width: 1000px)");
    header.classList.add("nav-ready");

    function closeMenu(restoreFocus) {
        header.classList.remove("menu-open");
        toggle.setAttribute("aria-expanded", "false");
        if (restoreFocus) { toggle.focus(); }
    }
    toggle.addEventListener("click", function () {
        var open = toggle.getAttribute("aria-expanded") !== "true";
        header.classList.toggle("menu-open", open);
        toggle.setAttribute("aria-expanded", String(open));
    });
    header.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && header.classList.contains("menu-open")) {
            event.preventDefault();
            closeMenu(true);
        }
    });
    document.addEventListener("click", function (event) {
        if (!header.contains(event.target)) {
            closeMenu(nav.contains(document.activeElement));
        }
    });
    nav.addEventListener("click", function (event) {
        var link = event.target.closest("a[href]");
        if (!link) { return; }
        if (mobile.matches) { closeMenu(true); }
        // Put keyboard focus at the selected section after an in-page jump.
        var destination = new URL(link.href, window.location.href);
        if (normalizePath(destination.pathname) === normalizePath(window.location.pathname) && destination.hash) {
            var target = document.getElementById(destination.hash.slice(1));
            if (target) {
                target.setAttribute("tabindex", "-1");
                target.focus({ preventScroll: true });
            }
        }
    });
    mobile.addEventListener("change", function () {
        var focusInNav = nav.contains(document.activeElement);
        var focusOnToggle = document.activeElement === toggle;
        closeMenu(mobile.matches && focusInNav);
        if (!mobile.matches && focusOnToggle) { nav.querySelector("a").focus(); }
    });
    function normalizePath(path) {
        return path.replace(/\/$/, "").replace(/\.aspx$/i, "").toLowerCase();
    }
    function updateCurrentLink() {
        nav.querySelectorAll("a[data-section]").forEach(function (link) {
            var destination = new URL(link.href, window.location.href);
            var samePage = normalizePath(destination.pathname) === normalizePath(window.location.pathname);
            var section = window.location.hash.slice(1) || "home";
            var active = samePage && link.dataset.section === section;
            if (active) { link.setAttribute("aria-current", section === "home" ? "page" : "location"); }
            else { link.removeAttribute("aria-current"); }
        });
    }
    window.addEventListener("hashchange", updateCurrentLink);
    updateCurrentLink();
}());
