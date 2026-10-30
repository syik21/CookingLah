(function () {
    "use strict";

    var modeNow = document.getElementById("btnModeNow");
    var modeSchedule = document.getElementById("btnModeSchedule");
    var modeField = document.getElementById("hfLiveMode");
    var schedule = document.getElementById("liveSchedule");
    var actionNow = document.getElementById("actionNow");
    var actionSchedule = document.getElementById("actionSchedule");
    var thumbnailInput = document.getElementById("fuThumbnail");
    var thumbnailImage = document.getElementById("thumbnailPreviewImage");
    var thumbnailPlaceholder = document.getElementById("thumbnailPreviewPlaceholder");
    var previewUrl = "";

    function setMode(mode) {
        var isSchedule = mode === "Schedule";
        modeField.value = isSchedule ? "Schedule" : "Now";
        modeNow.classList.toggle("live-mode-option--active", !isSchedule);
        modeSchedule.classList.toggle("live-mode-option--active", isSchedule);
        modeNow.setAttribute("aria-pressed", String(!isSchedule));
        modeSchedule.setAttribute("aria-pressed", String(isSchedule));
        schedule.hidden = !isSchedule;
        actionNow.hidden = isSchedule;
        actionSchedule.hidden = !isSchedule;
    }

    modeNow.addEventListener("click", function () { setMode("Now"); });
    modeSchedule.addEventListener("click", function () { setMode("Schedule"); });
    thumbnailInput.addEventListener("change", function () {
        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
            previewUrl = "";
        }
        var file = thumbnailInput.files && thumbnailInput.files[0];
        if (!file) {
            thumbnailImage.removeAttribute("src");
            thumbnailImage.hidden = true;
            thumbnailPlaceholder.hidden = false;
            return;
        }
        previewUrl = URL.createObjectURL(file);
        thumbnailImage.src = previewUrl;
        thumbnailImage.hidden = false;
        thumbnailPlaceholder.hidden = true;
    });

    setMode(modeField.value);
}());
