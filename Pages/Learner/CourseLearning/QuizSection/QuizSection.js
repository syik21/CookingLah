(function () {
    'use strict';

    const questionCards = Array.from(document.querySelectorAll('.quiz-question-card'));
    const previousButton = document.getElementById('btnPreviousQuestion');
    const submitButton = document.querySelector('.quiz-question-navigation input[type="submit"]');
    const progressText = document.getElementById('quizQuestionProgressText');
    const progressBar = document.getElementById('quizQuestionProgressBar');

    if (questionCards.length === 0 || !previousButton || !submitButton) return;

    let currentQuestion = 0;

    function showQuestion(index) {
        currentQuestion = Math.max(0, Math.min(index, questionCards.length - 1));

        questionCards.forEach((card, cardIndex) => {
            card.hidden = cardIndex !== currentQuestion;
        });

        previousButton.hidden = currentQuestion === 0;
        submitButton.value = currentQuestion === questionCards.length - 1
            ? 'Submit quiz'
            : 'Next question';

        const questionNumber = currentQuestion + 1;
        const percentage = Math.round(questionNumber * 100 / questionCards.length);
        progressText.textContent = 'Question ' + questionNumber + ' of ' + questionCards.length;
        progressBar.setAttribute('aria-valuemax', String(questionCards.length));
        progressBar.setAttribute('aria-valuenow', String(questionNumber));
        progressBar.querySelector('span').style.width = percentage + '%';
    }

    previousButton.addEventListener('click', () => showQuestion(currentQuestion - 1));

    window.CookingLahQuiz = {
        handleAction: function () {
            if (currentQuestion < questionCards.length - 1) {
                showQuestion(currentQuestion + 1);
                return false;
            }

            return window.confirm('Submit your quiz now? Make sure you have answered every question.');
        }
    };

    showQuestion(0);
}());
