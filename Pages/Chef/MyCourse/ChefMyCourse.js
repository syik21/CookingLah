(() => {
    const courseList = document.querySelector('[data-course-list]');
    const viewButtons = document.querySelectorAll('[data-course-view]');
    if (!courseList || !viewButtons.length) return;

    const setView = view => {
        const listView = view === 'list';
        courseList.classList.toggle('is-list-view', listView);
        viewButtons.forEach(button => {
            const selected = button.dataset.courseView === view;
            button.classList.toggle('is-selected', selected);
            button.setAttribute('aria-pressed', String(selected));
        });
    };

    let savedView = 'cards';
    try { savedView = localStorage.getItem('chefCourseView') || 'cards'; } catch (_) { }
    setView(savedView === 'list' ? 'list' : 'cards');

    viewButtons.forEach(button => button.addEventListener('click', () => {
        const view = button.dataset.courseView;
        setView(view);
        try { localStorage.setItem('chefCourseView', view); } catch (_) { }
    }));
})();
