window.CookingQuiz = {
    confirmQuestion: function (button) {
        return window.confirm("Delete Question " + button.dataset.order + " and its options? This action cannot be undone after saving the draft.");
    },
    confirmOption: function () {
        return window.confirm("Remove this answer option?");
    }
};
