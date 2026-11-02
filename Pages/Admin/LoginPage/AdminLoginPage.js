(function () {
    'use strict';
    var password = document.getElementById('txtLoginPass');
    var toggle = document.getElementById('togglePassword');
    if (password && toggle) {
        toggle.hidden = false;
        toggle.addEventListener('click', function () {
            var show = password.type === 'password';
            password.type = show ? 'text' : 'password';
            toggle.textContent = show ? 'Hide' : 'Show';
            toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
            toggle.setAttribute('aria-pressed', String(show));
        });
    }
    var button = document.getElementById('btnLogin');
    if (button) {
        var form = button.form;
        var submitting = false;
        form.addEventListener('submit', function (event) {
            if (submitting) {
                event.preventDefault();
                return;
            }
            if (!form.checkValidity()) {
                event.preventDefault();
                form.reportValidity();
                return;
            }
            submitting = true;
            button.value = 'Signing in...';
            button.setAttribute('aria-disabled', 'true');
        });
        window.addEventListener('pageshow', function () {
            submitting = false;
            button.value = 'Log in as admin';
            button.removeAttribute('aria-disabled');
        });
    }
    var feedback = document.getElementById('lblLoginError') || document.getElementById('pnlSuccess');
    if (feedback) feedback.focus();
    var success = document.getElementById('pnlSuccess');
    if (success) document.querySelector('.admin-login-card').setAttribute('aria-labelledby', 'successTitle');
}());
