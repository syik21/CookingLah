(function () {
    "use strict";
    var button = document.querySelector(".preview-expand");
    var lessons = document.querySelectorAll(".preview-lesson");
    if (!button || !lessons.length) return;
    button.hidden = false;
    button.addEventListener("click", function () {
        var expand = Array.from(lessons).some(function (lesson) { return !lesson.open; });
        lessons.forEach(function (lesson) { lesson.open = expand; });
        button.textContent = expand ? "Collapse lesson details" : "Expand lesson details";
    });
}());
