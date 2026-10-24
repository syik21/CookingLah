const test = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
function setup(mode, hasError = false) {
    const elements = {};
    function el(id) { return elements[id] ||= { value: '', handlers: {}, attrs: {},
        addEventListener(name, callback) { this.handlers[name] = callback; },
        setCustomValidity(value) { this.error = value; },
        setAttribute(name, value) { this.attrs[name] = value; },
        removeAttribute(name) { delete this.attrs[name]; },
        hasAttribute(name) { return name === 'data-save-user'; }, focus() { this.focused = true; }
    }; }
    const form = el('form');
    const modal = mode ? el(mode === 'remove' ? 'userRemove' : 'userEditor') : null;
    const section = modal ? {
        attributes: [{ name: 'id', value: mode === 'remove' ? 'userRemove' : 'userEditor' }],
        parentNode: { replaceChild(replacement, original) {
            assert.equal(replacement, modal);
            assert.equal(original, section);
        } }
    } : null;
    const error = el('lblError');
    error.hidden = !hasError;
    if (modal) {
        modal.id = mode === 'remove' ? 'userRemove' : 'userEditor';
        modal.showModal = () => { modal.shown = true; };
        modal.contains = () => true;
        modal.querySelector = selector => selector === '[data-close-modal]' ? { href: '/users?q=instructor&page=2' }
            : selector === '[data-initial-focus]' ? el('keepUser')
            : selector === 'input:invalid, select:invalid' ? el('email') : { textContent: 'Email' };
    }
    const root = { closest: () => form, contains: () => true,
        querySelectorAll: () => [],
        querySelector: selector => selector === '.mu-modal' ? section : hasError ? error : null };
    const location = { href: 'https://example.com/Pages/Admin/ManageUser/ManageUserPage.aspx?mode=edit&id=3', assign(url) { this.result = url; } };
    vm.runInNewContext(fs.readFileSync('Pages/Admin/ManageUser/ManageUserjs.js', 'utf8'), {
        document: { querySelector: () => root, createElement: () => modal, getElementById: el, documentElement: { classList: { add() {} } } }, window: { location }, URL, TextEncoder
    });
    return { el, form, location, modal };
}

test('editor opens as a modal and Escape returns to the filtered directory', () => {
    const { el, modal, location } = setup('edit');
    assert.equal(modal.shown, true);
    assert.equal(el('fullName').focused, true);
    let prevented = false;
    modal.handlers.cancel({ preventDefault() { prevented = true; } });
    assert.equal(prevented, true);
    assert.equal(location.result, '/users?q=instructor&page=2');
});

test('failed editor submission focuses the visible error label', () => {
    const { el, modal } = setup('create', true);
    assert.equal(modal.shown, true);
    assert.equal(el('lblError').focused, true);
    assert.notEqual(el('fullName').focused, true);
});

test('invalid editor fields show a validation message in lblError', () => {
    const { el, form } = setup('create');
    el('email').id = 'email';
    el('email').validationMessage = 'Enter a valid email address.';
    form.handlers.invalid({ target: el('email') });
    assert.equal(el('lblError').hidden, false);
    assert.equal(el('lblError').textContent, 'Email: Enter a valid email address.');
});

test('removal opens as a modal without focusing the destructive submit button', () => {
    const { el, modal } = setup('remove');
    assert.equal(modal.shown, true);
    assert.notEqual(el('fullName').focused, true);
    assert.equal(el('keepUser').focused, true);
});
test('filters encode search and clear editing and pagination', () => {
    const { el, location } = setup();
    el('userSearch').value = ' someone+test@example.com & ';
    el('roleFilter').value = 'Instructor';
    el('statusFilter').value = 'Inactive';
    el('applyFilters').handlers.click();
    const result = new URL(location.result);
    assert.equal(result.searchParams.get('q'), 'someone+test@example.com &');
    assert.equal(result.searchParams.get('role'), 'Instructor');
    assert.equal(result.searchParams.get('status'), 'Inactive');
    assert.equal(result.searchParams.has('mode'), false);
});
test('profession is required and submitted only for instructors', () => {
    const { el } = setup();
    el('newRole').value = 'Instructor';
    el('newRole').handlers.change();
    assert.equal(el('profession').required, true);
    assert.equal(el('profession').disabled, false);
    el('newRole').value = 'Admin';
    el('newRole').handlers.change();
    assert.equal(el('profession').required, false);
    assert.equal(el('profession').disabled, true);
    assert.equal(el('professionField').hidden, true);
});
test('password validation checks UTF-8 byte limit and matching confirmation', () => {
    const { el } = setup();
    el('password').value = 'é'.repeat(37);
    el('confirmPassword').value = 'different';
    el('password').handlers.input();
    assert.ok(el('password').error);
    assert.ok(el('confirmPassword').error);
    el('password').value = el('confirmPassword').value = 'validpassword123';
    el('confirmPassword').handlers.input();
    assert.equal(el('password').error, '');
    assert.equal(el('confirmPassword').error, '');
});
test('duplicate save is blocked while first submission preserves action button', () => {
    const { el, form } = setup();
    const button = el('save');
    let prevented = 0;
    const event = { submitter: button, preventDefault() { prevented++; } };
    form.handlers.submit(event);
    assert.equal(prevented, 0);
    assert.notEqual(button.disabled, true);
    form.handlers.submit(event);
    assert.equal(prevented, 1);
});
