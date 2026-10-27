(function () {
    "use strict";
    var form = document.querySelector(".recipe-form");
    if (!form) return;
    var upload = form.querySelector("input[type=file]");
    var image = form.querySelector(".recipe-image");
    var savedPreview = image && image.getAttribute("src");
    var previewUrl;
    if (!upload || !image) return;
    upload.addEventListener("change", function () {
        if (previewUrl) URL.revokeObjectURL(previewUrl);
        var file = upload.files[0];
        if (!file) {
            if (savedPreview) image.src = savedPreview;
            else image.removeAttribute("src");
            image.style.display = savedPreview ? "block" : "none";
            return;
        }
        if (!file || !/^image\/(jpeg|png)$/.test(file.type) || file.size > 3 * 1024 * 1024) return;
        previewUrl = URL.createObjectURL(file);
        image.src = previewUrl;
        image.style.display = "block";
    });
}());
