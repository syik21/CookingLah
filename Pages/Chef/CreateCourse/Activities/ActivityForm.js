(function () {
    "use strict";
    var instructions = document.querySelector(".activity-form textarea");
    if (!instructions) return;
    var count = document.createElement("span");
    count.className = "activity-count";
    instructions.parentNode.insertAdjacentElement("afterend", count);

    function updateCount() {
        var words = instructions.value.trim();
        count.textContent = (words ? words.split(/\s+/).length : 0) + " words";
    }
    instructions.addEventListener("input", updateCount);
    updateCount();
}());
