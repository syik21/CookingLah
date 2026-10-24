(function () {
    "use strict";
    // Native progress elements render without JavaScript; this adds useful tooltips.
    // When data binding is connected, use the same clamped value in the adjacent label.
    document.querySelectorAll(".learner-dashboard progress.course-progress").forEach(function (progress) {
        var percentage = Math.round(progress.value / progress.max * 100);
        progress.title = percentage + "% complete";
    });
}());
