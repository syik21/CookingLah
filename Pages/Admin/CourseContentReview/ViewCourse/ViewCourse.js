(function () {
    "use strict";
    document.querySelectorAll(".course-review").forEach(function (root) {
        var activeField = root.querySelector("input[id$='hfActiveTab']");
        if (!activeField) return;
        var tabs = Array.from(root.querySelectorAll("[data-tab]"));
        var sections = Array.from(root.querySelectorAll("[data-section]"));
        function selectTab(name, focus) {
            if (!tabs.some(function (tab) { return tab.dataset.tab === name; })) name = "overview";
            activeField.value = name;
            tabs.forEach(function (tab, index) {
                var section = sections.find(function (item) { return item.dataset.section === tab.dataset.tab; });
                var active = tab.dataset.tab === name;
                tab.id = activeField.id + "_tab_" + index;
                tab.setAttribute("aria-controls", section.id);
                tab.setAttribute("aria-selected", String(active));
                tab.tabIndex = active ? 0 : -1;
                section.hidden = !active;
                section.setAttribute("aria-labelledby", tab.id);
                section.tabIndex = 0;
                if (active && focus) tab.focus();
            });
        }
        tabs.forEach(function (tab, index) {
            tab.addEventListener("click", function () { selectTab(tab.dataset.tab, false); });
            tab.addEventListener("keydown", function (event) {
                var next = index;
                if (event.key === "ArrowRight") next = (index + 1) % tabs.length;
                else if (event.key === "ArrowLeft") next = (index + tabs.length - 1) % tabs.length;
                else if (event.key === "Home") next = 0;
                else if (event.key === "End") next = tabs.length - 1;
                else return;
                event.preventDefault(); selectTab(tabs[next].dataset.tab, true);
            });
        });
        selectTab(activeField.value, false);
        var dialogField = root.querySelector("input[id$='hfDialog']");
        var reason = root.querySelector("textarea[id$='txtRejectionReason']");
        if (!dialogField) return;
        function openDialog(name) {
            var dialog = root.querySelector('[data-dialog="' + name + '"]');
            if (!dialog) return;
            dialogField.value = name;
            dialog.showModal();
            if (name === "reject" && reason) reason.focus();
        }
        root.querySelectorAll("[data-open-dialog]").forEach(function (button) {
            button.addEventListener("click", function () { openDialog(button.dataset.openDialog); });
        });
        root.querySelectorAll("dialog").forEach(function (dialog) {
            dialog.querySelector("[data-close-dialog]").addEventListener("click", function () { dialog.close(); });
            dialog.addEventListener("close", function () { dialogField.value = ""; });
        });
        if (reason) {
            reason.maxLength = 500;
            var count = root.querySelector("[data-character-count]");
            var updateCount = function () { count.textContent = reason.value.length; };
            reason.addEventListener("input", updateCount); updateCount();
        }
        if (dialogField.value === "reject") openDialog("reject");
    });
}());
