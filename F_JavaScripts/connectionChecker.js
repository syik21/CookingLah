(function () {
    "use strict";

    if (window.cookingLahConnectionCheckerLoaded) return;
    window.cookingLahConnectionCheckerLoaded = true;

    var wasOffline = !navigator.onLine;
    var hideTimer = null;
    var banner = document.createElement("div");
    var message = document.createElement("span");
    var reloadButton = document.createElement("button");

    banner.className = "connection-status";
    banner.setAttribute("role", "status");
    banner.setAttribute("aria-live", "polite");
    banner.hidden = true;

    reloadButton.type = "button";
    reloadButton.textContent = "Reload page";
    reloadButton.addEventListener("click", function () {
        window.location.reload();
    });

    banner.appendChild(message);
    banner.appendChild(reloadButton);
    document.body.appendChild(banner);

    function showOffline() {
        clearTimeout(hideTimer);
        wasOffline = true;
        message.textContent = "You are offline. Check your internet connection.";
        banner.className = "connection-status connection-status--offline";
        banner.hidden = false;
        reloadButton.hidden = false;
    }

    function showOnline() {
        if (!wasOffline) return;
        wasOffline = false;
        message.textContent = "Connection restored.";
        banner.className = "connection-status connection-status--online";
        banner.hidden = false;
        reloadButton.hidden = true;
        hideTimer = setTimeout(function () {
            banner.hidden = true;
        }, 3500);
    }

    window.addEventListener("offline", showOffline);
    window.addEventListener("online", showOnline);

    if (!navigator.onLine) showOffline();
}());
