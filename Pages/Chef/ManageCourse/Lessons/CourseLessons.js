(function () {
    "use strict";
    var builder = document.querySelector('[data-lesson-builder]');
    if (!builder) return;
    var editing = builder.dataset.editing === 'true';
    var state = builder.querySelector('.lb-state');
    var sidebar = builder.querySelector('.lb-sidebar');
    if (window.matchMedia('(max-width:700px)').matches) sidebar.open = false;
    if (!editing) return;
    var dirty = false;
    function markDirty() { dirty = true; state.textContent = 'Unsaved changes'; }
    builder.addEventListener('input', markDirty);
    builder.addEventListener('change', markDirty);
    function showError(input, message) {
        var field = input.closest('.lb-field');
        var error = field.querySelector('.lb-client-error');
        if (!error) {
            error = document.createElement('span');
            error.className = 'lb-error lb-client-error';
            error.id = input.id + '-error';
            field.appendChild(error);
        }
        input.setAttribute('aria-describedby', error.id);
        input.setAttribute('aria-invalid', message ? 'true' : 'false');
        error.textContent = message;
        return !message;
    }
    function validateInput(input) {
        var message = '';
        if (input.dataset.required && !input.value.trim()) message = input.dataset.required;
        if (input.dataset.maxlength && input.value.length > Number(input.dataset.maxlength)) message = 'Keep this field within ' + input.dataset.maxlength + ' characters.';
        if (input.matches('[data-lb-upload]')) {
            var image = input.closest('[data-content-type]').dataset.contentType === 'Image';
            var file = input.files[0];
            if (!file && !input.dataset.existing) message = image ? 'Please select a valid image.' : 'Please provide a valid video.';
            if (file && (!file.size || file.size > (image ? 3 : 20) * 1024 * 1024 || !(image ? /\.(jpg|jpeg|png)$/i : /\.mp4$/i).test(file.name)))
                message = image ? 'Choose a non-empty JPG or PNG, up to 3 MB.' : 'Choose a non-empty MP4, up to 20 MB.';
        }
        return showError(input, message);
    }
    builder.querySelectorAll('[data-maxlength]').forEach(function (input) { input.maxLength = Number(input.dataset.maxlength); });
    builder.querySelectorAll('[data-required], [data-maxlength], [data-lb-upload]').forEach(function (input) {
        input.addEventListener('blur', function () { validateInput(input); });
    });
    builder.querySelectorAll('[data-lb-upload]').forEach(function (input) {
        var objectUrl;
        input.addEventListener('change', function () {
            var preview = input.closest('.lb-block').querySelector('[data-lb-preview]');
            if (objectUrl) URL.revokeObjectURL(objectUrl);
            if (!validateInput(input)) return;
            var file = input.files[0];
            preview.src = file ? (objectUrl = URL.createObjectURL(file)) : input.dataset.existing;
            preview.hidden = !preview.src;
        });
    });
    function validateLesson() {
        var firstInvalid;
        builder.querySelectorAll('[data-required], [data-maxlength], [data-lb-upload]').forEach(function (input) {
            if (!validateInput(input) && !firstInvalid) firstInvalid = input;
        });
        var empty = !builder.querySelector('.lb-block');
        var contentError = builder.querySelector('[id$="errContent"]');
        if (contentError) contentError.textContent = empty ? 'Add at least one Article, Image, or Video to this Lesson.' : '';
        if (firstInvalid) firstInvalid.focus();
        if (firstInvalid || empty) state.textContent = 'Unable to save Lesson. Check the marked fields.';
        return !firstInvalid && !empty;
    }
    var save = builder.querySelector('[data-save-lesson]');
    if (save) save.addEventListener('click', function (event) { if (!validateLesson()) event.preventDefault(); });
    builder.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' && event.target.matches('input[type=text]') && save) { event.preventDefault(); save.click(); }
    });
    builder.querySelectorAll('[data-lesson-order]').forEach(function (group) {
        var dragged;
        function saveOrder() {
            builder.querySelector('[id$="hfOrderKind"]').value = group.dataset.lessonOrder;
            builder.querySelector('[id$="hfOrderIds"]').value = Array.from(group.children).map(function (row) { return row.dataset.lessonId; }).join(',');
            builder.querySelector('[id$="btnSaveOrder"]').click();
        }
        group.addEventListener('dragstart', function (event) {
            if (!event.target.matches('.lb-drag')) { event.preventDefault(); return; }
            dragged = event.target.closest('[data-lesson-id]');
            dragged.classList.add('lb-dragging');
            event.dataTransfer.setData('text/plain', dragged.dataset.lessonId);
            event.dataTransfer.effectAllowed = 'move';
        });
        group.addEventListener('dragover', function (event) { if (dragged) event.preventDefault(); });
        group.addEventListener('drop', function (event) {
            var target = event.target.closest('[data-lesson-id]');
            if (!dragged || !target || target.parentNode !== group) return;
            event.preventDefault();
            if (target !== dragged) {
                var below = event.clientY > target.getBoundingClientRect().top + target.offsetHeight / 2;
                group.insertBefore(dragged, below ? target.nextSibling : target);
                saveOrder();
            }
        });
        group.addEventListener('dragend', function () { if (dragged) dragged.classList.remove('lb-dragging'); dragged = null; });
        group.addEventListener('click', function (event) {
            var button = event.target.closest('[data-lb-move]');
            if (!button) return;
            var row = button.closest('[data-lesson-id]');
            var previous = button.dataset.lbMove === '-1';
            var next = previous ? row.previousElementSibling : row.nextElementSibling;
            if (!next) return;
            group.insertBefore(row, previous ? next : next.nextSibling);
            saveOrder();
        });
    });
    var validation = builder.querySelector('.lb-validation');
    builder.querySelectorAll('.lb-error:not(.lb-client-error)').forEach(function (error) {
        if (!error.textContent.trim()) return;
        var field = error.closest('.lb-field') || error.closest('.lb-block');
        if (!field) return;
        field.querySelectorAll('input:not([type=hidden]), textarea').forEach(function (input) {
            input.setAttribute('aria-invalid', 'true');
            input.setAttribute('aria-describedby', error.id);
        });
    });
    if (validation) validation.focus();
    // Web Forms LinkButtons submit programmatically, without a submit event.
    builder.addEventListener('click', function (event) {
        if (event.target.closest('a[href^="javascript:__doPostBack"]')) dirty = false;
    });
    document.querySelector('form').addEventListener('submit', function () { dirty = false; });
    window.addEventListener('beforeunload', function (event) { if (dirty) { event.preventDefault(); event.returnValue = ''; } });
}());
