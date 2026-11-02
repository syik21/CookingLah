(function () {
    'use strict';
    var root = document.querySelector('.manage-users');
    if (!root) return;
    // Set modern attribute values at runtime for the legacy Web Forms HTML validator.
    root.querySelectorAll('[data-popup]').forEach(function (link) {
        link.setAttribute('aria-haspopup', link.getAttribute('data-popup'));
    });
    root.querySelectorAll('[data-autocomplete]').forEach(function (input) {
        input.setAttribute('autocomplete', input.getAttribute('data-autocomplete'));
    });
    var modal = root.querySelector('.mu-modal');
    if (modal) {
        // Keep valid section markup for the editor and no-JavaScript fallback,
        // then upgrade it to a native dialog for focus trapping and a backdrop.
        var dialog = document.createElement('dialog');
        if (typeof dialog.showModal === 'function') {
            Array.from(modal.attributes).forEach(function (attribute) {
                dialog.setAttribute(attribute.name, attribute.value);
            });
            while (modal.firstChild) dialog.appendChild(modal.firstChild);
            modal.parentNode.replaceChild(dialog, modal);
            modal = dialog;
        }
    }
    if (modal && typeof modal.showModal === 'function') {
        modal.showModal();
        document.documentElement.classList.add('mu-modal-open');
        modal.addEventListener('cancel', function (event) {
            event.preventDefault();
            window.location.assign(modal.querySelector('[data-close-modal]').href);
        });
    }
    var search = document.getElementById('userSearch');

    function filterUsers() {
        var url = new URL('ManageUserPage.aspx', window.location.href);
        [
            ['q', search.value.trim()],
            ['role', document.getElementById('roleFilter').value],
            ['status', document.getElementById('statusFilter').value]
        ].forEach(function (item) {
            if (item[1]) url.searchParams.set(item[0], item[1]);
        });
        window.location.assign(url.href);
    }
    document.getElementById('applyFilters').addEventListener('click', filterUsers);
    search.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            filterUsers();
        }
    });
    var role = document.getElementById('newRole');
    var profession = document.getElementById('profession');
    if (role && profession) {
        var updateProfession = function () {
            var instructor = role.value === 'Instructor';
            document.getElementById('professionField').hidden = !instructor;
            profession.required = instructor;
            profession.disabled = !instructor;
        };
        role.addEventListener('change', updateProfession);
        updateProfession();
    }
    var password = document.getElementById('password');
    var confirmPassword = document.getElementById('confirmPassword');
    if (password && confirmPassword) {
        var validatePassword = function () {
            confirmPassword.setCustomValidity(confirmPassword.value !== password.value ? 'The passwords do not match.' : '');
        };
        password.addEventListener('input', validatePassword);
        confirmPassword.addEventListener('input', validatePassword);
    }
    var form = root.closest('form');
    var editorError = document.getElementById('lblError');
    // Put browser validation messages in the same label as server validation errors.
    form.addEventListener('invalid', function (event) {
        if (!editorError || !modal || !modal.contains(event.target)) return;
        var invalid = modal.querySelector('input:invalid, select:invalid');
        if (event.target !== invalid) return;
        var label = modal.querySelector('label[for="' + event.target.id + '"]');
        editorError.textContent = (label ? label.textContent + ': ' : '') + event.target.validationMessage;
        editorError.hidden = false;
    }, true);
    // Native validation applies only to the editor, not the master page logout controls.
    form.addEventListener('click', function (event) {
        var button = event.target.closest('input[type=submit],button[type=submit]');
        if (button && !root.contains(button)) button.formNoValidate = true;
    });
    var submitted = false;
    form.addEventListener('submit', function (event) {
        var button = event.submitter;
        if (!button || !button.hasAttribute('data-save-user')) return;
        if (submitted) {
            event.preventDefault();
            return;
        }
        submitted = true;
        button.setAttribute('aria-disabled', 'true');
        button.textContent = button.value === 'remove' ? 'Removing...' : 'Saving...';
        // Keep the submit button enabled so its action name/value reaches the server.
    });
    var error = root.querySelector('.mu-error:not([hidden])');
    if (error) error.focus();
    else if (modal && modal.id === 'userEditor') document.getElementById('fullName').focus();
    else if (modal) {
        var initialFocus = modal.querySelector('[data-initial-focus]');
        if (initialFocus) initialFocus.focus();
    }
}());
