(function () {
    "use strict";
    document.querySelectorAll("input[type=file][data-max-file-mb]").forEach(function (input) {
        var limitMb = Number(input.dataset.maxFileMb);
        var error = document.createElement("p");
        error.id = input.id + "_sizeError";
        error.className = "upload-size-error";
        error.setAttribute("role", "alert");
        error.hidden = true;
        input.insertAdjacentElement("afterend", error);
        input.setAttribute("aria-describedby", ((input.getAttribute("aria-describedby") || "") + " " + error.id).trim());
        // Capture runs before image-preview handlers, so rejected files never get previewed.
        input.addEventListener("change", function () {
            var file = input.files[0];
            if (file && file.size > limitMb * 1024 * 1024) {
                error.textContent = "This file is too large (" + (file.size / (1024 * 1024)).toFixed(1) +
                    " MB). Choose a file of " + limitMb + " MB or less. The selection has been cleared.";
                error.hidden = false;
                input.value = "";
            } else {
                error.textContent = "";
                error.hidden = true;
            }
        }, true);
    });
}());
