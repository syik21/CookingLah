(function () {
    "use strict";
    var quiz = document.querySelector(".quiz-form");
    if (!quiz) return;
    var dialog = quiz.querySelector("[data-question-delete-dialog]");
    var cancel = dialog.querySelector("[data-question-delete-cancel]");
    var confirm = dialog.querySelector("[data-question-delete-confirm]");
    var pendingButton = null;
    var confirmedButton = null;

    quiz.addEventListener("click", function (event) {
        var button = event.target.closest("[data-delete-question]");
        if (!button) return;
        if (button === confirmedButton) {
            confirmedButton = null;
            return;
        }
        event.preventDefault();
        event.stopPropagation();
        pendingButton = button;
        dialog.querySelector("[data-question-delete-name]").textContent = "Question " + button.dataset.order;
        dialog.showModal();
        cancel.focus();
    }, true);

    function dismiss() {
        var button = pendingButton;
        pendingButton = null;
        dialog.close();
        if (button) button.focus();
    }
    cancel.addEventListener("click", dismiss);
    dialog.addEventListener("cancel", function (event) {
        event.preventDefault();
        dismiss();
    });
    confirm.addEventListener("click", function () {
        if (!pendingButton) return;
        var button = pendingButton;
        dismiss();
        // Replay the original Web Forms button so its question command is preserved.
        confirmedButton = button;
        button.click();
        confirmedButton = null;
    });
}());
