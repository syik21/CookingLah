(function () {
    "use strict";

    var storageKey = "CookingLah.MyReviews.ViewMode";
    var reviewGrid = document.getElementById("reviewCardGrid");
    var viewButtons = document.querySelectorAll("[data-review-view]");

    function getSavedView() {
        try {
            return window.localStorage.getItem(storageKey) === "one" ? "one" : "two";
        } catch (error) {
            return "two";
        }
    }

    function saveView(viewMode) {
        try {
            window.localStorage.setItem(storageKey, viewMode);
        } catch (error) {
            return;
        }
    }

    function applyView(viewMode, shouldSave) {
        if (!reviewGrid) {
            return;
        }

        reviewGrid.classList.toggle("view-one", viewMode === "one");
        reviewGrid.classList.toggle("view-two", viewMode === "two");

        viewButtons.forEach(function (button) {
            var selected = button.getAttribute("data-review-view") === viewMode;
            button.classList.toggle("is-active", selected);
            button.setAttribute("aria-pressed", selected ? "true" : "false");
        });

        if (shouldSave) {
            saveView(viewMode);
        }
    }

    viewButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            applyView(button.getAttribute("data-review-view"), true);
        });
    });

    document.querySelectorAll(".review-star-options input[type='radio']").forEach(function (radio) {
        radio.required = true;
    });

    document.querySelectorAll(".review-edit-form textarea").forEach(function (textarea) {
        textarea.addEventListener("input", function () {
            textarea.setCustomValidity("");
        });
    });

    var pageForm = document.forms[0];

    if (pageForm) {
        pageForm.addEventListener("submit", function (event) {
            var editor = document.querySelector(".review-card.is-editing");

            if (!editor) {
                return;
            }

            var reviewText = editor.querySelector("textarea");

            if (reviewText && reviewText.value.trim().length === 0) {
                reviewText.setCustomValidity("Enter your review before saving.");
                reviewText.reportValidity();
                event.preventDefault();
            }
        });
    }

    applyView(getSavedView(), false);
}());
