const test = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');

function setup() {
    const elements = {};
    const form = { children: [], submissions: 0, appendChild(el) { this.children.push(el); } };
    function element(id) {
        return elements[id] ||= {
            id, value: '', type: 'password', disabled: false, dataset: {}, handlers: {},
            addEventListener(event, handler) { this.handlers[event] = handler; },
            click() { this.handlers.click(); }, focus() {},
            setAttribute(key, value) { this[key] = value; },
            setCustomValidity(message) { this.invalid = message; },
            reportValidity() { return !this.invalid; }, closest() { return form; },
            showModal() { this.open = true; }, close() { this.open = false; this.handlers.close(); }
        };
    }
    element('fullName').value = 'Original Name';
    element('username').value = 'original';
    element('email').value = 'original@example.com';
    element('personalDetailsFields').disabled = true;
    element('passwordFields').disabled = true;
    element('profileAvatar').src = 'https://example.com/F_Assets/ProfilePicture/photo.png';
    element('profileAvatar').dataset.defaultAvatar = '/F_Assets/mascot.png';
    const toggles = ['currentPassword', 'newPassword', 'confirmPassword'].map(id => {
        const button = element(id + 'Toggle'); button.dataset.password = id; return button;
    });
    const document = { getElementById: element, baseURI: 'https://example.com/',
        createElement: () => ({}), querySelectorAll: () => toggles,
        querySelector: () => ({ textContent: 'Password' }) };
    function HTMLFormElement() {}
    HTMLFormElement.prototype.submit = function () { this.submissions++; };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../Pages/Shared/Profile/Profile.js'), 'utf8'),
        { document, URL, TextEncoder, HTMLFormElement });
    return { element, form };
}

test('cancel restores details without submitting', () => {
    const { element: el, form } = setup();
    el('editDetails').click();
    el('fullName').value = 'Changed';
    el('resetDetails').click();
    assert.equal(el('fullName').value, 'Original Name');
    assert.equal(el('personalDetailsFields').disabled, true);
    assert.equal(form.submissions, 0);
});

test('details save submits once and excludes passwords', () => {
    const { element: el, form } = setup();
    el('editDetails').click();
    el('fullName').value = ' Updated Name ';
    el('previewDetails').click();
    el('previewDetails').click();
    assert.equal(el('fullName').value, 'Updated Name');
    assert.equal(form.submissions, 1);
    assert.equal(form.children[0].value, 'details');
    assert.equal(el('passwordFields').disabled, true);
});

test('password mismatch blocks submission; matching passwords submit', () => {
    const { element: el, form } = setup();
    el('editPassword').click();
    el('currentPassword').value = 'old-password';
    el('newPassword').value = 'new-password';
    el('confirmPassword').value = 'different';
    el('checkPassword').click();
    assert.equal(form.submissions, 0);
    el('confirmPassword').value = 'new-password';
    el('checkPassword').click();
    assert.equal(form.submissions, 1);
    assert.equal(form.children[0].value, 'password');
    assert.equal(el('personalDetailsFields').disabled, true);
});

test('password visibility and cancel reset sensitive fields', () => {
    const { element: el, form } = setup();
    el('editPassword').click();
    el('currentPassword').value = 'secret';
    el('currentPasswordToggle').click();
    assert.equal(el('currentPassword').type, 'text');
    el('cancelPassword').click();
    assert.equal(el('currentPassword').value, '');
    assert.equal(el('currentPassword').type, 'password');
    assert.equal(form.submissions, 0);
});

test('photo removal saves only after confirmation and closing cancels', () => {
    const { element: el, form } = setup();
    el('editProfilePhoto').click();
    el('removePhoto').click();
    assert.equal(form.submissions, 0);
    el('closePhotoDialog').click();
    assert.equal(el('savePhoto').disabled, true);
    el('editProfilePhoto').click();
    el('removePhoto').click();
    el('savePhoto').click();
    assert.equal(form.children[0].value, 'remove');
    assert.equal(form.submissions, 1);
});

test('delete profile requires password and explicit confirmation', () => {
    const { element: el, form } = setup();
    el('openDeleteProfile').click();
    assert.equal(el('deleteProfileDialog').open, true);
    el('confirmDeleteButton').click();
    assert.equal(form.submissions, 0);
    el('deletePassword').value = 'current-password';
    el('confirmDeleteButton').click();
    assert.equal(form.submissions, 0);
    el('confirmDeleteProfile').checked = true;
    el('confirmDeleteButton').click();
    el('confirmDeleteButton').click();
    assert.equal(form.submissions, 1);
    assert.equal(form.children[0].value, 'delete');
    assert.equal(el('personalDetailsFields').disabled, true);
    assert.equal(el('passwordFields').disabled, true);
});

test('cancel delete clears confirmation and password without submitting', () => {
    const { element: el, form } = setup();
    el('openDeleteProfile').click();
    el('deletePassword').value = 'secret';
    el('confirmDeleteProfile').checked = true;
    el('cancelDeleteProfile').click();
    assert.equal(el('deleteProfileDialog').open, false);
    assert.equal(el('deletePassword').value, '');
    assert.equal(el('confirmDeleteProfile').checked, false);
    assert.equal(form.submissions, 0);
});
