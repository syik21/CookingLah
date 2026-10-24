/* Profile editing, photo previews, and authenticated account saves. */
(function () {
    'use strict';
    const byId = id => document.getElementById(id);
    const profile = byId('profileMain');
    let submitting = false;
    function submitAction(action, button) {
        if (submitting) return;
        const form = profile.closest('form');
        const actionField = document.createElement('input');
        actionField.type = 'hidden';
        actionField.name = 'profileAction';
        actionField.value = action;
        form.appendChild(actionField);
        // Submit only the fields belonging to this action.
        byId('personalDetailsFields').disabled = action !== 'details';
        byId('passwordFields').disabled = action !== 'password';
        if (action !== 'upload') byId('profilePhoto').value = '';
        submitting = true;
        button.disabled = true;
        button.textContent = 'Saving...';
        HTMLFormElement.prototype.submit.call(form);
    }
    const fields = ['fullName', 'username', 'email'].map(byId);
    let initial = fields.map(field => field.value);
    function setDetailsEditing(editing) {
        byId('personalDetailsFields').disabled = !editing;
        byId('editDetails').hidden = editing;
        byId('resetDetails').hidden = !editing;
        byId('previewDetails').hidden = !editing;
        (editing ? fields[0] : byId('editDetails')).focus();
    }
    byId('editDetails').addEventListener('click', () => {
        setDetailsEditing(true);
        byId('detailsState').textContent = 'Edit your details, then save your changes.';
        byId('detailsFeedback').textContent = '';
    });
    const avatar = byId('profileAvatar');
    const defaultAvatar = new URL(avatar.dataset.defaultAvatar, document.baseURI).href;
    let photoVersion = 0;
    let pendingPhotoUrl = null;
    const photoDialog = byId('profilePhotoDialog');
    const photoPreview = byId('photoPreview');
    const savePhoto = byId('savePhoto');

    function discardPendingPhoto() {
        photoVersion++;
        if (pendingPhotoUrl) URL.revokeObjectURL(pendingPhotoUrl);
        pendingPhotoUrl = null;
        byId('profilePhoto').value = '';
        savePhoto.disabled = true;
    }
    byId('editProfilePhoto').addEventListener('click', () => {
        discardPendingPhoto();
        photoPreview.src = avatar.src;
        byId('removePhoto').hidden = avatar.src === defaultAvatar;
        byId('photoFeedback').textContent = '';
        photoDialog.showModal();
    });
    byId('closePhotoDialog').addEventListener('click', () => photoDialog.close());
    photoDialog.addEventListener('close', () => {
        discardPendingPhoto();
        photoPreview.src = avatar.src;
        byId('editProfilePhoto').focus();
    });
    savePhoto.addEventListener('click', () => {
        if (savePhoto.disabled) return;
        submitAction(photoPreview.src === defaultAvatar ? 'remove' : 'upload', savePhoto);
    });

    // Keep Enter in editable inputs from submitting the master's form; leave logout buttons working.
    profile.addEventListener('keydown', event => {
        if (event.key === 'Enter' && event.target.matches('input:not([type="file"])')) {
            event.preventDefault();
        }
    });
    fields.forEach(field => field.addEventListener('input', () => {
        field.setCustomValidity('');
        byId('detailsState').textContent = 'Changes have not been saved.';
        byId('detailsFeedback').textContent = '';
    }));
    byId('previewDetails').addEventListener('click', () => {
        if (byId('personalDetailsFields').disabled) return;
        for (const field of fields) {
            field.value = field.value.trim();
            if (!field.reportValidity()) return;
        }
        submitAction('details', byId('previewDetails'));
    });
    byId('resetDetails').addEventListener('click', () => {
        fields.forEach((field, index) => { field.value = initial[index]; field.setCustomValidity(''); });
        byId('profileName').textContent = initial[0] || 'Your name';
        byId('profileHandle').textContent = '@' + (initial[1] || 'yourusername');
        setDetailsEditing(false);
        byId('detailsState').textContent = 'Select Edit profile to update your details.';
        byId('detailsFeedback').textContent = 'Edits cancelled.';
    });
    byId('profilePhoto').addEventListener('change', () => {
        const version = ++photoVersion;
        const file = byId('profilePhoto').files[0];
        savePhoto.disabled = true;
        if (!file) return;
        const feedback = byId('photoFeedback');
        if (!['image/jpeg', 'image/png'].includes(file.type) || file.size > 2 * 1024 * 1024) {
            feedback.textContent = 'Choose a JPG or PNG image up to 2 MB.';
            byId('profilePhoto').value = '';
            return;
        }
        const candidate = URL.createObjectURL(file);
        feedback.textContent = 'Loading photo preview...';
        const probe = new Image();
        probe.onload = () => {
            if (version !== photoVersion) { URL.revokeObjectURL(candidate); return; }
            if (probe.width > 4096 || probe.height > 4096 || probe.width * probe.height > 16000000) {
                URL.revokeObjectURL(candidate);
                byId('profilePhoto').value = '';
                feedback.textContent = 'Choose an image no larger than 4096 pixels per side and 16 megapixels.';
                return;
            }
            if (pendingPhotoUrl) URL.revokeObjectURL(pendingPhotoUrl);
            pendingPhotoUrl = candidate;
            photoPreview.src = candidate;
            savePhoto.disabled = false;
            byId('removePhoto').hidden = false;
            feedback.textContent = 'Photo ready. Select Save to apply it.';
        };
        probe.onerror = () => {
            URL.revokeObjectURL(candidate);
            if (version !== photoVersion) return;
            byId('profilePhoto').value = '';
            feedback.textContent = 'This image could not be opened. Choose another JPG or PNG.';
        };
        probe.src = candidate;
    });
    byId('removePhoto').addEventListener('click', () => {
        discardPendingPhoto();
        photoPreview.src = defaultAvatar;
        savePhoto.disabled = false;
        byId('removePhoto').hidden = true;
        byId('photoFeedback').textContent = 'Select Save to remove your photo.';
        byId('profilePhoto').focus();
    });
    document.querySelectorAll('[data-password]').forEach(button => {
        button.addEventListener('click', () => {
            const input = byId(button.dataset.password);
            const show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            button.textContent = show ? 'Hide' : 'Show';
            button.setAttribute('aria-pressed', String(show));
            const label = document.querySelector('label[for="' + input.id + '"]').textContent;
            button.setAttribute('aria-label', (show ? 'Hide ' : 'Show ') + label.toLowerCase());
        });
    });
    const passwords = ['currentPassword', 'newPassword', 'confirmPassword'].map(byId);
    function setPasswordEditing(editing) {
        byId('passwordFields').disabled = !editing;
        byId('editPassword').hidden = editing;
        byId('cancelPassword').hidden = !editing;
        byId('checkPassword').hidden = !editing;
        passwords.forEach(input => {
            input.value = '';
            input.type = 'password';
            input.setCustomValidity('');
        });
        document.querySelectorAll('[data-password]').forEach(button => {
            button.textContent = 'Show';
            button.setAttribute('aria-pressed', 'false');
            const label = document.querySelector('label[for="' + button.dataset.password + '"]').textContent;
            button.setAttribute('aria-label', 'Show ' + label.toLowerCase());
        });
        (editing ? passwords[0] : byId('editPassword')).focus();
    }
    byId('editPassword').addEventListener('click', () => {
        setPasswordEditing(true);
        byId('passwordFeedback').textContent = '';
    });
    byId('cancelPassword').addEventListener('click', () => {
        setPasswordEditing(false);
        byId('passwordFeedback').textContent = 'Password edits cancelled.';
    });
    passwords.forEach(input => input.addEventListener('input', () => {
        passwords.forEach(field => field.setCustomValidity(''));
        byId('passwordFeedback').textContent = '';
    }));
    byId('checkPassword').addEventListener('click', () => {
        if (byId('passwordFields').disabled) return;
        passwords.forEach(input => input.setCustomValidity(input.value ? '' : 'Please fill in this password field.'));
        if (new TextEncoder().encode(passwords[1].value).length > 72) passwords[1].setCustomValidity('Use at most 72 UTF-8 bytes.');
        if (passwords[1].value && passwords[1].value.length < 8) passwords[1].setCustomValidity('Use at least 8 characters.');
        if (passwords[1].value && passwords[1].value === passwords[0].value) passwords[1].setCustomValidity('Choose a different new password.');
        if (passwords[2].value && passwords[2].value !== passwords[1].value) passwords[2].setCustomValidity('The new passwords do not match.');
        for (const input of passwords) { if (!input.reportValidity()) return; }
        submitAction('password', byId('checkPassword'));
    });
}());
