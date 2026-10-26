(function () {
    'use strict';

    const curriculum = document.getElementById('courseCurriculum');
    const toggleButton = document.getElementById('btnToggleCurriculum');
    const mobileLayout = window.matchMedia('(max-width: 720px)');

    if (!curriculum || !toggleButton) return;

    function updateCurriculumForViewport() {
        if (!mobileLayout.matches) {
            curriculum.classList.remove('is-open');
            toggleButton.setAttribute('aria-expanded', 'true');
            return;
        }

        toggleButton.setAttribute('aria-expanded', String(curriculum.classList.contains('is-open')));
    }

    toggleButton.addEventListener('click', () => {
        const isOpen = curriculum.classList.toggle('is-open');
        toggleButton.setAttribute('aria-expanded', String(isOpen));
    });

    if (mobileLayout.addEventListener) {
        mobileLayout.addEventListener('change', updateCurriculumForViewport);
    } else {
        mobileLayout.addListener(updateCurriculumForViewport);
    }

    updateCurriculumForViewport();
}());
