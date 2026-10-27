(function () {
    "use strict";
    // The enclosing Web Forms panel handles Enter with its Search default button.
    document.querySelectorAll(".review-course-thumbnail img").forEach(function (image) {
        image.addEventListener("error", function () {
            image.hidden = true;
            var fallback = document.createElement("span");
            fallback.textContent = "Thumbnail unavailable";
            image.parentNode.appendChild(fallback);
        });
    });
}());
