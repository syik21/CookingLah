(function () {
    "use strict";
    document.querySelectorAll("[data-ingredient-editor]").forEach(function (editor) {
        var source = editor.querySelector("[data-ingredient-json]");
        var list = editor.querySelector("[data-ingredient-rows]");
        var add = editor.querySelector("[data-ingredient-add]");
        var values;
        try {
            values = JSON.parse(source.value || "[]");
        } catch (_) {
            values = source.value.split(/\r?\n/).filter(Boolean).map(function (name) {
                return {
                    name: name,
                    amount: ""
                };
            });
        }
        if (!Array.isArray(values)) values = [];
        source.hidden = true;

        function sync() {
            var rows = Array.from(list.children);
            source.value = JSON.stringify(rows.map(function (row, index) {
                var inputs = row.querySelectorAll("input");
                var remove = row.querySelector("button");
                remove.disabled = rows.length === 1;
                remove.setAttribute("aria-label", "Remove ingredient " + (index + 1));
                return {
                    name: inputs[0].value,
                    amount: inputs[1].value
                };
            }));
            add.disabled = rows.length >= 100;
        }

        function append(value, focus) {
            var row = document.createElement("div");
            row.className = "ingredient-row";
            ["name", "amount"].forEach(function (key) {
                var label = document.createElement("label");
                label.textContent = key === "name" ? "Ingredient name" : "Amount";
                var input = document.createElement("input");
                input.type = "text";
                input.maxLength = key === "name" ? 150 : 100;
                input.placeholder = key === "name" ? "e.g. Flour" : "e.g. 200 g";
                input.value = value && typeof value[key] === "string" ? value[key] : "";
                input.setAttribute("aria-required", "true");
                input.addEventListener("input", sync);
                label.appendChild(input);
                row.appendChild(label);
            });
            var remove = document.createElement("button");
            remove.type = "button";
            remove.className = "ingredient-remove";
            remove.textContent = "Remove";
            remove.addEventListener("click", function () {
                if (list.children.length <= 1) return;
                var next = row.nextElementSibling || row.previousElementSibling;
                row.remove();
                sync();
                next.querySelector("input").focus();
            });
            row.appendChild(remove);
            list.appendChild(row);
            sync();
            if (focus) row.querySelector("input").focus();
        }
        (values.length ? values : [{}]).forEach(function (value) {
            append(value, false);
        });
        add.addEventListener("click", function () {
            if (list.children.length < 100) append({}, true);
        });
    });
}());
