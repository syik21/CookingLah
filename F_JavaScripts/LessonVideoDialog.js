(function () {
    "use strict";

    document.querySelectorAll("[data-lesson-video-dialog]").forEach(function (dialog) {
        var player = dialog.querySelector("video");
        var heading = dialog.querySelector("h2");
        var status = dialog.querySelector("[role='status']");
        var opener = null;

        document.querySelectorAll("[data-video-open]").forEach(function (button) {
            button.addEventListener("click", function () {
                var url;
                try {
                    url = new URL(button.dataset.videoUrl, document.baseURI);
                } catch (_) {
                    status.textContent = "This lesson video has an invalid address.";
                    return;
                }

                // Course video media is stored locally. Reject unexpected URL schemes or hosts.
                if (url.origin !== window.location.origin || !url.pathname.includes("/F_Assets/CourseMedia/")) {
                    status.textContent = "This lesson video is unavailable.";
                    return;
                }

                opener = button;
                heading.textContent = button.dataset.videoTitle || "Lesson video";
                status.textContent = "";
                player.src = url.href;
                player.load();
                dialog.showModal();
                dialog.querySelector("[data-video-close]").focus();
            });
        });

        dialog.querySelector("[data-video-close]").addEventListener("click", function () {
            dialog.close();
        });

        dialog.addEventListener("click", function (event) {
            if (event.target === dialog) dialog.close();
        });

        player.addEventListener("error", function () {
            if (dialog.open) status.textContent = "This video could not be played. Try again or check the video file.";
        });

        dialog.addEventListener("close", function () {
            player.pause();
            player.removeAttribute("src");
            player.load();
            status.textContent = "";
            if (opener && opener.isConnected) opener.focus();
            opener = null;
        });
    });
}());
