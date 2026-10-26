(function () {
    "use strict";
    function initSection(selector) {
        var section = document.querySelector(selector);
        if (!section) return;
        var panel = section.matches(".mc-modal") ? section : section.querySelector(".mc-modal");
        if (panel) {
            var dialog = document.createElement("dialog");
            if (typeof dialog.showModal === "function") {
                dialog.className = "mc-dialog";
                var heading = panel.querySelector("h2");
                heading.id = panel.id + "_heading";
                dialog.setAttribute("aria-labelledby", heading.id);
                panel.parentNode.insertBefore(dialog, panel);
                dialog.appendChild(panel);
                var feedback = document.getElementById("pnlMessage");
                if (feedback) heading.insertAdjacentElement("afterend", feedback);
                dialog.showModal();
                document.documentElement.classList.add("mc-dialog-open");
                var cancel = panel.querySelector("[data-cancel]");
                dialog.addEventListener("cancel", function (event) {
                    event.preventDefault();
                    if (cancel) cancel.click();
                });
                // Enter in a text field saves this editor, never a header action.
                panel.addEventListener("keydown", function (event) {
                    if (event.key === "Enter" && event.target.matches("input[type=text]")) {
                        var save = panel.querySelector("input.btn-primary");
                        if (save) { event.preventDefault(); save.click(); }
                    }
                });
                var initial = feedback || (panel.classList.contains("mc-delete") ? cancel : panel.querySelector("input[type=text], textarea"));
                if (initial) initial.focus();
            }
        }
        section.querySelectorAll("[data-image-input]").forEach(function (input) {
            var editor = input.closest(".mc-modal");
            var preview = editor && editor.querySelector("[data-image-preview]");
            if (!preview) return;
            var imageUrl;
            input.addEventListener("change", function () {
                if (imageUrl) URL.revokeObjectURL(imageUrl);
                var file = input.files[0];
                preview.hidden = !file || !/^image\/(jpeg|png)$/.test(file.type);
                if (!preview.hidden) { imageUrl = URL.createObjectURL(file); preview.src = imageUrl; }
            });
        });
    }
    window.CookingLahManage = { initSection: initSection };
    initSection("[id$='pnlDeleteCourse']");
    var feedback = document.getElementById("pnlMessage");
    if (feedback) feedback.focus();

    var traditionalFoodSection = document.querySelector(".mc-traditional-food");
    if (traditionalFoodSection) {
        var editor = traditionalFoodSection.closest(".mc-modal");
        var categorySelect = editor && editor.querySelector("select[id$='ddlCategory']");
        var foodModeInputs = traditionalFoodSection.querySelectorAll("input[name$='rblFoodCultureMode']");
        var foodFields = traditionalFoodSection.querySelector("[data-traditional-food-fields]");
        var existingFields = traditionalFoodSection.querySelector("[data-food-existing-fields]");
        var newFields = traditionalFoodSection.querySelector("[data-food-new-fields]");

        function updateFoodFields() {
            var selectedCategory = categorySelect && categorySelect.selectedIndex >= 0
                ? categorySelect.options[categorySelect.selectedIndex].text.trim()
                : "";
            var isTraditionalDishes = selectedCategory.toLowerCase() === "traditional dishes";
            var addNewFood = Array.prototype.some.call(foodModeInputs, function (input) {
                return input.checked && input.value === "New";
            });

            traditionalFoodSection.hidden = !isTraditionalDishes;
            foodFields.hidden = !isTraditionalDishes;
            existingFields.hidden = !isTraditionalDishes || addNewFood;
            newFields.hidden = !isTraditionalDishes || !addNewFood;
        }

        if (categorySelect) categorySelect.addEventListener("change", updateFoodFields);
        foodModeInputs.forEach(function (input) { input.addEventListener("change", updateFoodFields); });
        updateFoodFields();
    }

    var workspace = document.querySelector("[id$='pnlWorkspace']");
    var reviewDialog = document.getElementById("publishedChangesDialog");
    if (workspace && reviewDialog && workspace.dataset.reviewRequired === "true") {
        var destination = null;
        var stayButton = reviewDialog.querySelector("[data-review-stay]");
        reviewDialog.querySelector("[data-review-stay]").addEventListener("click", function () {
            reviewDialog.close();
        });
        var leaveDestination = document.getElementById("reviewLeaveDestination");
        reviewDialog.querySelector("[data-review-submit]").addEventListener("click", function () {
            reviewDialog.close();
            var submitButton = workspace.querySelector("[id$='btnSubmitReview']");
            if (submitButton) submitButton.click();
        });
        document.addEventListener("click", function (event) {
            var link = event.target.closest("a[href]");
            if (!link || link.closest(".course-tabs") || link.classList.contains("mc-preview-action") ||
                link.target === "_blank" || link.hasAttribute("download")) return;
            var target;
            try { target = new URL(link.href, document.baseURI); } catch (_) { return; }
            if (target.origin !== window.location.origin ||
                (target.pathname === window.location.pathname && target.search === window.location.search && !link.classList.contains("mc-back"))) return;
            event.preventDefault();
            destination = target.pathname + target.search + target.hash;
            leaveDestination.value = destination;
            if (!reviewDialog.open) reviewDialog.showModal();
            stayButton.focus();
        }, true);
    }
}());
