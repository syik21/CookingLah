(function () {
    'use strict';

    const courseList = document.getElementById('myCourseList');
    const viewButtons = document.querySelectorAll('[data-view-mode]');

    if (!courseList || viewButtons.length === 0) return;

    function setCourseView(viewMode) {
        courseList.classList.remove('four-column-view', 'single-column-view');
        courseList.classList.add(viewMode);

        viewButtons.forEach(button => {
            const isActive = button.dataset.viewMode === viewMode;
            button.classList.toggle('is-active', isActive);
            button.setAttribute('aria-pressed', String(isActive));
        });

        try {
            window.localStorage.setItem('learnerMyCourseView', viewMode);
        } catch (error) {
            // Keep the view toggle working when browser storage is unavailable.
        }
    }

    let savedView = 'four-column-view';
    try {
        const storedView = window.localStorage.getItem('learnerMyCourseView');
        if (storedView === 'single-column-view' || storedView === 'four-column-view') {
            savedView = storedView;
        }
    } catch (error) {
        // Use the default four-card view when browser storage is unavailable.
    }

    setCourseView(savedView);

    viewButtons.forEach(button => {
        button.addEventListener('click', () => {
            setCourseView(button.dataset.viewMode);
        });
    });
}());
