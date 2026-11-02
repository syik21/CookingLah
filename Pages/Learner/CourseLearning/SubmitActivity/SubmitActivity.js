(function () {
    'use strict';

    const fileInput = document.querySelector('.activity-file-input');
    const previewPanel = document.querySelector('.activity-image-preview');
    const previewImage = document.getElementById('imgActivityPreview');
    const clearButton = document.getElementById('btnClearActivityImage');
    const uploadFeedback = document.getElementById('activityUploadFeedback');

    if (!fileInput || !previewPanel || !previewImage || !clearButton) return;

    let previewUrl = null;

    function clearPreview() {
        if (previewUrl) {
            URL.revokeObjectURL(previewUrl);
            previewUrl = null;
        }

        previewImage.removeAttribute('src');
        previewPanel.hidden = true;
    }

    function showUploadFeedback(message) {
        if (!uploadFeedback) return;

        uploadFeedback.textContent = message;
        uploadFeedback.hidden = message.length === 0;
    }

    fileInput.addEventListener('change', () => {
        clearPreview();
        showUploadFeedback('');

        const file = fileInput.files[0];
        if (!file) return;

        if (!['image/jpeg', 'image/png'].includes(file.type) || file.size > 3 * 1024 * 1024) {
            fileInput.value = '';
            showUploadFeedback(file.size > 3 * 1024 * 1024 ?
                'Choose an image smaller than 3 MB.' :
                'Choose a JPG or PNG image.');
            return;
        }

        previewUrl = URL.createObjectURL(file);
        previewImage.src = previewUrl;
        previewPanel.hidden = false;
    });

    clearButton.addEventListener('click', () => {
        fileInput.value = '';
        clearPreview();
        fileInput.focus();
    });

    window.addEventListener('beforeunload', clearPreview);
}());
