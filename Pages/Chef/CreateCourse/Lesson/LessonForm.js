window.CookingLessons = {
    confirmDelete: function (button) {
        return window.confirm("Delete Lesson " + button.dataset.order + "? This action cannot be undone after saving the draft.");
    }
};
