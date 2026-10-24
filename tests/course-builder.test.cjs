const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');

function setup(unsaved = false) {
    const events = {}, pageEvents = {}, formEvents = {};
    const note = { textContent: '' };
    const builder = {
        dataset: { unsaved: String(unsaved), courseId: '42', step: '0' },
        addEventListener(name, handler) { events[name] = handler; },
        closest() { return { addEventListener(name, handler) { formEvents[name] = handler; } }; },
        querySelector(selector) { return selector === '.builder-save-note' ? note : null; }
    };
    let resumeUrl;
    vm.runInNewContext(fs.readFileSync('Pages/Chef/CreateCourse/CreateCoursePage.js', 'utf8'), {
        document: { querySelector() { return builder; } }, URL,
        window: { addEventListener(name, handler) { pageEvents[name] = handler; },
            location: { href: 'https://example.test/CreateCoursePage' },
            history: { replaceState(state, title, url) { resumeUrl = String(url); } } }
    });
    function wouldWarn() {
        let warned = false;
        pageEvents.beforeunload({ preventDefault() { warned = true; } });
        return warned;
    }
    return { events, formEvents, note, wouldWarn, resumeUrl };
}

test('saved draft does not warn; a field edit does', () => {
    const ui = setup();
    assert.equal(ui.wouldWarn(), false);
    ui.events.input();
    assert.equal(ui.wouldWarn(), true);
    assert.match(ui.note.textContent, /Unsaved changes/);
});

test('unsaved paging state survives the postback', () => {
    assert.equal(setup(true).wouldWarn(), true);
});

test('actual save submission suppresses the leave warning', () => {
    const ui = setup(true);
    ui.formEvents.submit();
    assert.equal(ui.wouldWarn(), false);
});

test('cancelled actions keep the unsaved warning', () => {
    const ui = setup(true);
    ui.events.click({ defaultPrevented: true, target: { closest() { return {}; } } });
    assert.equal(ui.wouldWarn(), true);
});

test('the current URL carries the saved draft ID', () => {
    assert.equal(setup().resumeUrl, 'https://example.test/CreateCoursePage?courseId=42');
});

test('lesson and question deletion respects cancelled confirmation', () => {
    const context = { window: { confirm() { return false; } } };
    vm.runInNewContext(fs.readFileSync('Pages/Chef/CreateCourse/Lesson/LessonForm.js', 'utf8'), context);
    vm.runInNewContext(fs.readFileSync('Pages/Chef/CreateCourse/Quiz/QuizForm.js', 'utf8'), context);
    assert.equal(context.window.CookingLessons.confirmDelete({ dataset: { order: '3' } }), false);
    assert.equal(context.window.CookingQuiz.confirmQuestion({ dataset: { order: '2' } }), false);
    assert.equal(context.window.CookingQuiz.confirmOption(), false);
});
