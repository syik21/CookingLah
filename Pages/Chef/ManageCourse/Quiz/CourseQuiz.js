(function () {
    'use strict';
    var builder = document.querySelector('[data-quiz-builder]');
    if (!builder) return;
    if (window.matchMedia('(max-width:700px)').matches) builder.querySelector('.lb-sidebar').open = false;
    if (builder.dataset.editing !== 'true') return;
    var dirty = false;
    var state = builder.querySelector('.lb-state');
    function updateAnswers() {
        builder.querySelectorAll('.qb-option').forEach(function (option) {
            option.classList.toggle('is-correct', option.querySelector('input[type=radio]').checked);
        });
    }
    updateAnswers();
    function changed() { dirty = true; state.textContent = 'Unsaved changes'; updateAnswers(); }
    builder.addEventListener('input', changed);
    builder.addEventListener('change', changed);
    function validateInput(input) {
        var message = '';
        if (input.dataset.qbRequired && !input.value.trim()) message = input.dataset.qbRequired;
        if (input.dataset.qbMaxlength && input.value.length > Number(input.dataset.qbMaxlength)) message = 'Keep this field within ' + input.dataset.qbMaxlength + ' characters.';
        if (input.hasAttribute('data-qb-score') && (!/^\d{1,3}$/.test(input.value.trim()) || Number(input.value) > 100)) message = 'Enter a passing score between 0 and 100.';
        var field = input.closest('.lb-field');
        var error = field.querySelector('.qb-client-error');
        if (!error) {
            error = document.createElement('span'); error.className = 'lb-error qb-client-error';
            error.id = input.id + '-error'; field.appendChild(error);
        }
        error.textContent = message;
        input.setAttribute('aria-invalid', String(!!message));
        input.setAttribute('aria-describedby', error.id);
        return !message;
    }
    builder.querySelectorAll('[data-qb-required], [data-qb-score], [data-qb-maxlength]').forEach(function (input) {
        if (input.dataset.qbMaxlength) input.maxLength = Number(input.dataset.qbMaxlength);
        input.addEventListener('blur', function () { validateInput(input); });
    });
    var save = builder.querySelector('[data-save-quiz]');
    if (save) save.addEventListener('click', function (event) {
        var firstInvalid;
        builder.querySelectorAll('[data-qb-required], [data-qb-score], [data-qb-maxlength]').forEach(function (input) {
            if (!validateInput(input) && !firstInvalid) firstInvalid = input;
        });
        var options = builder.querySelector('.qb-options');
        var answerMissing = options && !options.querySelector('input[type=radio]:checked');
        if (options) builder.querySelector('[id$="errOptions"]').textContent = answerMissing ? 'Select one correct answer.' : '';
        if (firstInvalid || answerMissing) {
            event.preventDefault();
            state.textContent = 'Unable to save. Check the marked fields.';
            (firstInvalid || options.querySelector('input[type=radio]')).focus();
        }
    });
    builder.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' && event.target.matches('input[type=text]') && save) { event.preventDefault(); save.click(); }
    });
    var group = builder.querySelector('[data-question-order]');
    var dragged;
    function saveOrder() {
        builder.querySelector('[id$="hfOrderIds"]').value = Array.from(group.children).map(function (row) { return row.dataset.questionId; }).join(',');
        builder.querySelector('[id$="btnSaveOrder"]').click();
    }
    group.addEventListener('dragstart', function (event) {
        if (!event.target.matches('.qb-drag')) { event.preventDefault(); return; }
        dragged = event.target.closest('[data-question-id]'); dragged.classList.add('qb-dragging');
        event.dataTransfer.setData('text/plain', dragged.dataset.questionId); event.dataTransfer.effectAllowed = 'move';
    });
    group.addEventListener('dragover', function (event) { if (dragged) event.preventDefault(); });
    group.addEventListener('drop', function (event) {
        var target = event.target.closest('[data-question-id]');
        if (!dragged || !target || target.parentNode !== group) return;
        event.preventDefault();
        if (target !== dragged) {
            var after = event.clientY > target.getBoundingClientRect().top + target.offsetHeight / 2;
            group.insertBefore(dragged, after ? target.nextSibling : target); saveOrder();
        }
    });
    group.addEventListener('dragend', function () { if (dragged) dragged.classList.remove('qb-dragging'); dragged = null; });
    group.addEventListener('click', function (event) {
        var button = event.target.closest('[data-qb-move]');
        if (!button) return;
        var row = button.closest('[data-question-id]');
        var up = button.dataset.qbMove === '-1';
        var next = up ? row.previousElementSibling : row.nextElementSibling;
        if (!next) return;
        group.insertBefore(row, up ? next : next.nextSibling); saveOrder();
    });
    builder.querySelectorAll('.lb-error').forEach(function (error) {
        if (!error.textContent.trim()) return;
        var field = error.closest('.lb-field');
        var input = field && field.querySelector('input,textarea');
        if (input) { input.setAttribute('aria-invalid', 'true'); input.setAttribute('aria-describedby', error.id); }
    });
    var validation = builder.querySelector('.lb-validation');
    if (validation) validation.focus();
    builder.addEventListener('click', function (event) { if (event.target.closest('a[href^="javascript:__doPostBack"]')) dirty = false; });
    document.querySelector('form').addEventListener('submit', function () { dirty = false; });
    window.addEventListener('beforeunload', function (event) { if (dirty) { event.preventDefault(); event.returnValue = ''; } });
}());
