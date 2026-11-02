(function () {
    "use strict";
    document.addEventListener("click", function (event) {
        var toggle = event.target.closest(".rating-breakdown-toggle");
        if (toggle) {
            var panel = document.getElementById(toggle.getAttribute("aria-controls"));
            var expanded = toggle.getAttribute("aria-expanded") === "true";
            toggle.setAttribute("aria-expanded", String(!expanded));
            toggle.querySelector("span:first-child").textContent = expanded ? "View rating breakdown" : "Hide rating breakdown";
            toggle.querySelector("span:last-child").textContent = expanded ? "▾" : "▴";
            panel.hidden = expanded;
        }
        var link = event.target.closest(".js-scroll-link");
        if (link && link.hash) {
            var target = document.querySelector(link.hash);
            if (target) {
                event.preventDefault();
                target.scrollIntoView({
                    behavior: "smooth",
                    block: "start"
                });
                target.focus({
                    preventScroll: true
                });
                history.replaceState(null, "", link.hash);
            }
        }
    });
    document.querySelectorAll(".star-rating-options input").forEach(function (radio) {
        radio.setAttribute("aria-label", radio.value + (radio.value === "1" ? " star" : " stars"));
    });
    var dialog = document.querySelector(".course-login-backdrop");
    if (dialog) {
        var close = dialog.querySelector(".course-dialog-close");
        if (close) close.focus();
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && close) close.click();
        });
    }
}());
