(function () {
    "use strict";

    var page = document.querySelector("[data-create-course]");
    if (!page) return;

    var categorySelect = page.querySelector("select[id$='ddlCategory']");
    var foodSection = page.querySelector("[data-category-food-section]");
    var foodModeInputs = page.querySelectorAll("input[name$='rblFoodCultureMode']");
    var existingFields = page.querySelector("[data-food-existing-fields]");
    var newFields = page.querySelector("[data-food-new-fields]");

    if (existingFields) {
        var foodSelect = existingFields.querySelector("select");
        var foodPicker = existingFields.querySelector("[data-food-picker]");
        var foodSearch = existingFields.querySelector("[data-food-search]");
        var foodResults = existingFields.querySelector("[data-food-results]");
        var foodOptions = existingFields.querySelector("[data-food-options]");
        var foodSearchStatus = existingFields.querySelector("[data-food-search-status]");
        var foodEmpty = existingFields.querySelector("[data-food-empty]");
        var foodLabel = existingFields.querySelector("label");

        if (foodSelect && foodPicker && foodSearch && foodResults && foodOptions) {
            var foods = Array.from(foodSelect.options).filter(function (option) { return option.value; });
            var searchId = foodSelect.id + "_search";
            var resultsId = foodSelect.id + "_results";

            foodSearch.id = searchId;
            foodOptions.id = resultsId;
            foodSearch.setAttribute("aria-controls", resultsId);
            foodSearch.setAttribute("aria-expanded", "false");
            foodPicker.hidden = false;
            foodSelect.hidden = true;
            if (foodLabel) foodLabel.htmlFor = searchId;

            function normalize(value) {
                return value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
            }

            function closeFoodResults() {
                foodResults.hidden = true;
                foodSearch.setAttribute("aria-expanded", "false");
            }

            function selectFood(food) {
                foodSelect.value = food.value;
                foodSearch.value = food.text;
                foodSearch.removeAttribute("aria-invalid");
                foodSelect.dispatchEvent(new Event("change", { bubbles: true }));
                closeFoodResults();
                foodSearch.focus();
            }

            function renderFoodResults() {
                var query = normalize(foodSearch.value.trim());
                var matches = foods.filter(function (food) { return normalize(food.text).includes(query); });

                foodOptions.replaceChildren();
                matches.forEach(function (food) {
                    var option = document.createElement("button");
                    option.type = "button";
                    option.className = "traditional-food-option";
                    option.setAttribute("role", "option");
                    option.setAttribute("aria-selected", String(foodSelect.value === food.value));
                    option.textContent = food.text;
                    option.addEventListener("click", function () { selectFood(food); });
                    foodOptions.appendChild(option);
                });

                foodSearchStatus.textContent = matches.length + (matches.length === 1 ? " dish found" : " dishes found");
                foodEmpty.hidden = matches.length !== 0;
                foodResults.hidden = false;
                foodSearch.setAttribute("aria-expanded", "true");
            }

            function syncTypedFood() {
                var query = normalize(foodSearch.value.trim());
                var match = foods.find(function (food) { return normalize(food.text.trim()) === query; });
                foodSelect.value = match ? match.value : "";
                foodSearch.removeAttribute("aria-invalid");
                renderFoodResults();
            }

            if (foodSelect.value) foodSearch.value = foodSelect.options[foodSelect.selectedIndex].text;
            foodSearch.addEventListener("focus", renderFoodResults);
            foodSearch.addEventListener("input", syncTypedFood);
            foodSearch.addEventListener("blur", function () {
                if (foodSearch.value.trim() && !foodSelect.value) foodSearch.setAttribute("aria-invalid", "true");
            });
            foodSearch.addEventListener("keydown", function (event) {
                if (event.key === "Escape") closeFoodResults();
                if (event.key === "ArrowDown") {
                    var firstOption = foodOptions.querySelector("button");
                    if (firstOption) { event.preventDefault(); firstOption.focus(); }
                }
            });
            foodOptions.addEventListener("keydown", function (event) {
                var options = Array.from(foodOptions.querySelectorAll("button"));
                var current = options.indexOf(document.activeElement);
                if (event.key === "ArrowDown" && current < options.length - 1) {
                    event.preventDefault(); options[current + 1].focus();
                }
                if (event.key === "ArrowUp") {
                    event.preventDefault(); (current > 0 ? options[current - 1] : foodSearch).focus();
                }
                if (event.key === "Escape") { closeFoodResults(); foodSearch.focus(); }
            });
            document.addEventListener("click", function (event) {
                if (!foodPicker.contains(event.target)) closeFoodResults();
            });
        }
    }

    function updateFoodFields() {
        var categoryName = categorySelect && categorySelect.selectedIndex >= 0
            ? categorySelect.options[categorySelect.selectedIndex].text.trim().toLowerCase()
            : "";
        var isTraditional = categoryName === "traditional dishes";
        var addNew = Array.prototype.some.call(foodModeInputs, function (input) {
            return input.checked && input.value === "New";
        });

        if (foodSection) foodSection.hidden = !isTraditional;
        if (existingFields) existingFields.hidden = !isTraditional || addNew;
        if (newFields) newFields.hidden = !isTraditional || !addNew;
    }

    if (categorySelect) categorySelect.addEventListener("change", updateFoodFields);
    foodModeInputs.forEach(function (input) { input.addEventListener("change", updateFoodFields); });
    updateFoodFields();

    var description = page.querySelector("textarea[id$='txtDescription']");
    var descriptionCount = page.querySelector("[data-description-count]");
    function updateDescriptionCount() {
        if (descriptionCount && description) descriptionCount.textContent = description.value.length;
    }
    if (description) description.addEventListener("input", updateDescriptionCount);
    updateDescriptionCount();

    var fileInput = page.querySelector("input[type=file]");
    var fileName = page.querySelector("[data-file-name]");
    var preview = page.querySelector(".create-course-thumbnail img");
    var previewUrl;
    if (fileInput) fileInput.addEventListener("change", function () {
        var file = fileInput.files[0];
        if (fileName) fileName.textContent = file ? file.name : "No file selected";
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        if (!file || !preview || !/^image\/(jpeg|png)$/.test(file.type) || file.size > 3 * 1024 * 1024) {
            if (preview) { preview.removeAttribute("src"); preview.style.display = "none"; }
            return;
        }
        previewUrl = URL.createObjectURL(file);
        preview.src = previewUrl;
        preview.style.display = "block";
    });

    var dirty = false;
    var submitting = false;
    page.addEventListener("input", function () { dirty = true; });
    page.addEventListener("change", function () { dirty = true; });
    page.closest("form").addEventListener("submit", function () { submitting = true; });
    window.addEventListener("beforeunload", function (event) {
        if (dirty && !submitting) { event.preventDefault(); event.returnValue = ""; }
    });

    var errors = page.querySelector(".create-course-errors");
    if (errors) errors.focus();
}());
