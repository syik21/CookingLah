(function () {
    "use strict";
    document.querySelectorAll(".reset-toggle").forEach(function (button) {
        button.addEventListener("click", function () {
            var input = document.getElementById(button.dataset.target);
            if (!input) return;
            var show = input.type === "password";
            input.type = show ? "text" : "password";
            button.textContent = show ? "Hide" : "Show";
            button.setAttribute("aria-label", (show ? "Hide" : "Show") + " password");
        });
    });

    var form = document.getElementById("form1");
    var password = document.getElementById("txtNewPassword");
    var confirmation = document.getElementById("txtConfirmPassword");
    var submit = document.getElementById("btnResetPassword");
    if (form && password && confirmation && submit) {
        confirmation.addEventListener("input", function () { confirmation.setCustomValidity(""); });
        form.addEventListener("submit", function (event) {
            if (password.value !== confirmation.value) {
                confirmation.setCustomValidity("The passwords do not match.");
                confirmation.reportValidity();
                event.preventDefault();
                return;
            }
            submit.value = "Resetting...";
            submit.setAttribute("aria-busy", "true");
        });
    }
}());
