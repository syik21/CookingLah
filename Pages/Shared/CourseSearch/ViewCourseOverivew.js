(function () {
    "use strict";

    var dialog = document.querySelector(".course-login-backdrop");
    if (!dialog) return;

    var closeButton = dialog.querySelector(".course-dialog-close");
    if (closeButton) closeButton.focus();

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && closeButton) closeButton.click();
    });
}());

