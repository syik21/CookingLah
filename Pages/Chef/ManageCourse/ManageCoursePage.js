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
    var tabField = document.getElementById("hfActiveTab");
    var tabs = Array.from(document.querySelectorAll("[data-course-tab]"));
    var sections = Array.from(document.querySelectorAll("[data-course-section]"));
    function selectTab(name, moveFocus) {
        if (!tabs.some(function (tab) { return tab.dataset.courseTab === name; })) return;
        tabField.value = name;
        tabs.forEach(function (tab) {
            var selected = tab.dataset.courseTab === name;
            tab.setAttribute("aria-selected", String(selected));
            tab.tabIndex = selected ? 0 : -1;
            if (selected && moveFocus) tab.focus();
        });
        sections.forEach(function (section) {
            section.hidden = section.dataset.courseSection !== name;
            if (section.hidden) section.querySelectorAll("video").forEach(function (video) { video.pause(); });
        });
        document.dispatchEvent(new CustomEvent("course:tabchange", { detail: name }));
    }
    tabs.forEach(function (tab, index) {
        tab.addEventListener("click", function () { selectTab(tab.dataset.courseTab, false); });
        tab.addEventListener("keydown", function (event) {
            var destination;
            if (event.key === "ArrowRight") destination = (index + 1) % tabs.length;
            else if (event.key === "ArrowLeft") destination = (index + tabs.length - 1) % tabs.length;
            else if (event.key === "Home") destination = 0;
            else if (event.key === "End") destination = tabs.length - 1;
            else return;
            event.preventDefault();
            selectTab(tabs[destination].dataset.courseTab, true);
        });
    });
    if (tabField) selectTab(tabField.value || "overview", false);
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

    var preview = document.getElementById("btnPreviewLearner");
    if (preview) preview.addEventListener("click", function () {
        document.getElementById("builderFeedback").textContent = "Learner preview will be available later.";
    });
    document.querySelectorAll("[data-order-group]").forEach(function (group) {
        var dragged;
        function save() {
            document.getElementById("hfOrderKind").value = group.dataset.orderGroup;
            document.getElementById("hfOrderParent").value = group.dataset.orderParent || "0";
            document.getElementById("hfOrderIds").value = Array.from(group.querySelectorAll(":scope > [data-order-id]")).map(function (row) { return row.dataset.orderId; }).join(",");
            document.getElementById("btnReorder").click();
        }
        group.addEventListener("dragstart", function (event) {
            if (!event.target.matches(".drag-handle") || event.target.hidden) { event.preventDefault(); return; }
            dragged = event.target.closest("[data-order-id]");
            event.dataTransfer.setData("text/plain", dragged.dataset.orderId);
            event.dataTransfer.effectAllowed = "move";
        });
        group.addEventListener("dragover", function (event) { if (dragged) event.preventDefault(); });
        group.addEventListener("drop", function (event) {
            var target = event.target.closest("[data-order-id]");
            if (!dragged || !target || target.parentNode !== group) return;
            event.preventDefault();
            if (target !== dragged) {
                var after = event.clientY > target.getBoundingClientRect().top + target.offsetHeight / 2;
                group.insertBefore(dragged, after ? target.nextSibling : target);
                save();
            }
            dragged = null;
        });
        group.addEventListener("dragend", function () { dragged = null; });
        group.addEventListener("click", function (event) {
            var button = event.target.closest("[data-move]");
            if (!button) return;
            var row = button.closest("[data-order-id]");
            var next = button.dataset.move === "-1" ? row.previousElementSibling : row.nextElementSibling;
            if (!next) return;
            group.insertBefore(row, button.dataset.move === "-1" ? next : next.nextSibling);
            save();
        });
    });
    var dirty = false;
    document.querySelectorAll(".mc-modal input, .mc-modal textarea, .mc-modal select").forEach(function (input) {
        input.addEventListener("change", function () { dirty = true; document.getElementById("builderFeedback").textContent = "Unsaved changes"; });
    });
    document.querySelector("form").addEventListener("submit", function () { dirty = false; });
    window.addEventListener("beforeunload", function (event) { if (dirty) { event.preventDefault(); event.returnValue = ""; } });
}());
