(function () {
    "use strict";

    var feedback = document.querySelector(".submission-chef-review textarea");
    var resubmitButton = document.querySelector(".submission-resubmit-action");

    if (!feedback || !resubmitButton) {
        return;
    }

    resubmitButton.addEventListener("click", function (event) {
        if (feedback.value.trim().length > 0) {
            return;
        }

        feedback.setCustomValidity("Please provide feedback before requesting resubmission.");
        feedback.reportValidity();
        event.preventDefault();
    });

    feedback.addEventListener("input", function () {
        feedback.setCustomValidity("");
    });
}());
