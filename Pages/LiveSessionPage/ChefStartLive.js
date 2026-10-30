(function () {
    "use strict";
    var tabs = document.querySelectorAll("[data-tab]");
    var input = document.getElementById("fuThumbnail");
    var image = document.getElementById("thumbnailPreviewImage");
    var placeholder = document.getElementById("thumbnailPreviewPlaceholder");
    var previewUrl = "";

    tabs.forEach(function (tab) {
        tab.addEventListener("click", function () {
            tabs.forEach(function (item) {
                var selected = item === tab;
                item.classList.toggle("chef-live-tab--active", selected);
                item.setAttribute("aria-selected", String(selected));
                document.getElementById(item.getAttribute("data-tab")).hidden = !selected;
            });
        });
    });

    if (!input) {
        return;
    }

    input.addEventListener("change", function () {
        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
        }
        var file = input.files && input.files[0];
        if (!file) {
            image.removeAttribute("src");
            image.hidden = true;
            placeholder.hidden = false;
            previewUrl = "";
            return;
        }
        previewUrl = URL.createObjectURL(file);
        image.src = previewUrl;
        image.hidden = false;
        placeholder.hidden = true;
    });
}());
