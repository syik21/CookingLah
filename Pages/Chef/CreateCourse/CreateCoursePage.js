(function () {
    "use strict";
    var builder = document.querySelector(".course-builder");
    if (!builder) return;
    var dirty = builder.dataset.unsaved === "true";
    var posting = false;
    var saveNote = builder.querySelector(".builder-save-note");
    builder.addEventListener("input", function () {
        dirty = true;
        if (saveNote) saveNote.textContent = "Unsaved changes - select Save draft to keep them.";
    });
    builder.addEventListener("change", function () { dirty = true; });
    builder.addEventListener("click", function (event) {
        var action = event.target.closest("a[href^='javascript:__doPostBack']");
        if (action && !event.defaultPrevented) posting = true;
    });
    builder.closest("form").addEventListener("submit", function () { posting = true; });
    window.addEventListener("beforeunload", function (event) {
        if (dirty && !posting) { event.preventDefault(); event.returnValue = ""; }
    });
    var courseId = Number(builder.dataset.courseId);
    if (courseId > 0 && window.history.replaceState) {
        var url = new URL(window.location.href);
        url.searchParams.set("courseId", courseId);
        window.history.replaceState(null, "", url);
    }
    var feedback = builder.querySelector(".builder-errors, .builder-feedback");
    if (feedback) feedback.focus();
    else if (Number(builder.dataset.step) > 0) {
        var heading = document.getElementById("builderStepTitle");
        if (heading) heading.focus({ preventScroll: true });
    }
    var thumbnail = builder.querySelector(".builder-thumbnail input[type=file]");
    var preview = builder.querySelector(".builder-thumbnail img");
    var previewUrl;
    if (thumbnail && preview) thumbnail.addEventListener("change", function () {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        var file = thumbnail.files[0];
        if (!file || !/^image\/(jpeg|png)$/.test(file.type) || file.size > 3 * 1024 * 1024) return;
        previewUrl = URL.createObjectURL(file);
        preview.src = previewUrl;
        preview.style.display = "block";
    });
}());
