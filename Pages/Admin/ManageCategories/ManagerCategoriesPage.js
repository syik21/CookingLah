(function () {
    "use strict";
    var description = document.getElementById("txtCategoryDescription");
    var counter = document.getElementById("categoryDescriptionCount");
    if (description && counter) {
        description.maxLength = 500;
        function updateCount() { counter.textContent = description.value.length + "/500"; }
        description.addEventListener("input", updateCount);
        updateCount();
    }
    // Keep the panel inside the Web Forms form so its controls still post back.
    // Native dialogs provide keyboard focus trapping and make the background inert.
    var panel = document.getElementById("pnlCategoryForm") || document.getElementById("pnlDeleteCategory");
    var feedback = document.getElementById("pnlMessage");
    var cancelDelete = document.querySelector("input[id$='btnCancelDelete']");
    var name = document.getElementById("txtCategoryName");
    if (panel) {
        var dialog = document.createElement("dialog");
        if (typeof dialog.showModal === "function") {
            dialog.className = "category-dialog";
            dialog.setAttribute("aria-labelledby", name ? "categoryFormHeading" : "deleteHeading");
            panel.parentNode.insertBefore(dialog, panel);
            dialog.appendChild(panel);
            // Validation and database errors must remain visible inside the popup.
            if (feedback) panel.querySelector("h2").insertAdjacentElement("afterend", feedback);
            dialog.showModal();
            document.documentElement.classList.add("category-dialog-open");
            dialog.addEventListener("cancel", function (event) {
                event.preventDefault();
                // Use the existing Cancel postback to clear the server-side form state.
                panel.querySelector("input[id$='btnCancelCategory'], input[id$='btnCancelDelete']").click();
            });
        }
    }
    var target = feedback || cancelDelete || name;
    if (target) target.focus();
}());

