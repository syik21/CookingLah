/* Run only from the disposable IIS test harness. Uses installed Edge and Node's CDP client. */
const fs = require("node:fs");
const path = require("node:path");
const os = require("node:os");
const { spawn } = require("node:child_process");
const assert = require("node:assert/strict");
const base = process.argv[2];
const profile = fs.mkdtempSync(path.join(os.tmpdir(), "CookingLahBrowserTest-"));
const edge = spawn("C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe", [
    "--headless=new", "--disable-gpu", "--no-first-run", "--no-default-browser-check",
    "--remote-debugging-port=0", "--user-data-dir=" + profile, "about:blank"
], { windowsHide: true, stdio: "ignore" });
let socket;
const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
async function until(check, label) {
    for (let attempt = 0; attempt < 120; attempt++) {
        try { const result = await check(); if (result) return result; } catch (_) {}
        await sleep(250);
    }
    throw new Error("Timed out: " + label);
}
(async () => {
    const port = await until(() => {
        const file = path.join(profile, "DevToolsActivePort");
        return fs.existsSync(file) && fs.readFileSync(file, "utf8").split("\n")[0];
    }, "Edge debugging port");
    const tabs = await (await fetch("http://127.0.0.1:" + port + "/json/list")).json();
    socket = new WebSocket(tabs.find(tab => tab.type === "page").webSocketDebuggerUrl);
    await new Promise(resolve => socket.addEventListener("open", resolve, { once: true }));
    let sequence = 0;
    const pending = new Map();
    socket.addEventListener("message", event => {
        const response = JSON.parse(event.data);
        if (pending.has(response.id)) {
            const { resolve, reject } = pending.get(response.id);
            pending.delete(response.id);
            response.error ? reject(response.error) : resolve(response.result);
        }
    });
    function send(method, params = {}) {
        return new Promise((resolve, reject) => {
            const id = ++sequence; pending.set(id, { resolve, reject });
            socket.send(JSON.stringify({ id, method, params }));
        });
    }
    async function evaluate(expression) {
        const result = await send("Runtime.evaluate", { expression, returnByValue: true, awaitPromise: true });
        if (result.exceptionDetails) throw new Error(JSON.stringify(result.exceptionDetails));
        return result.result.value;
    }
    async function ready(expression) { try { return await until(() => evaluate(expression), expression); } catch (error) { console.error(await evaluate("document.body.innerText")); throw error; } }
    await send("Page.enable");
    await send("Runtime.enable");
    await send("Emulation.setDeviceMetricsOverride", { width: 1440, height: 1000, deviceScaleFactor: 1, mobile: false });
    await send("Page.navigate", { url: base + "/Fixture.aspx" });
    await ready(`document.body && document.body.textContent.includes('fixture ready')`);

    const page = base + "/Pages/Admin/ConrseContentReview/ReviewCoursesPage.aspx";
    await send("Page.navigate", { url: page });
    await ready(`document.querySelector('[id$="btnPending"]')`);
    assert.equal(await evaluate(`document.querySelectorAll('.review-course-card').length`), 8);
    assert.equal(await evaluate(`document.querySelector('.review-toolbar').getBoundingClientRect().top < document.querySelector('.review-status-filters').getBoundingClientRect().top`), true);
    await evaluate(`document.querySelector('[id$="btnNext"]').click()`);
    await ready(`document.body.textContent.includes('Page 2 of 2')`);
    await evaluate(`document.querySelector('[id$="btnPending"]').click()`);
    await ready(`document.querySelector('[id$="btnPending"]').classList.contains('active') && document.body.textContent.includes('Page 1 of 2')`);
    await evaluate(`document.querySelector('[id$="txtSearchCourses"]').value='Ikan Bakar'; document.querySelector('[id$="btnSearch"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 1`);
    await ready(`document.querySelector('[id*="btnViewCourse"]')`);
    await evaluate(`document.querySelector('[id*="btnViewCourse"]').click()`);
    await ready(`document.querySelector('[data-tab="recipe"]')`);
    assert.equal(await evaluate(`location.pathname.endsWith('ReviewCoursesPage.aspx')`), true);
    assert.equal(await evaluate(`document.querySelectorAll('[data-section]:not([hidden])').length`), 1);
    await evaluate(`document.querySelector('[data-tab="recipe"]').click()`);
    assert.equal(await evaluate(`document.querySelector('[data-section="recipe"]').textContent.includes('Fish')`), true);
    await evaluate(`document.querySelector('[data-tab="lessons"]').click(); document.querySelector('[id$="btnLessonNext"]').click()`);
    await ready(`document.querySelector('[data-section="lessons"]:not([hidden])') && document.querySelector('[data-section="lessons"]').textContent.includes('Page 2 of 2')`);
    assert.equal(await evaluate(`document.querySelectorAll('[data-section="lessons"] .review-content-item').length`), 2);
    await evaluate(`document.querySelector('[data-tab="quiz"]').click(); document.querySelector('[id$="btnQuestionNext"]').click()`);
    await ready(`document.querySelector('[data-section="quiz"]:not([hidden])') && document.querySelector('[data-section="quiz"]').textContent.includes('Page 2 of 2')`);
    assert.equal(await evaluate(`document.querySelectorAll('.review-options .correct').length`), 2);
    await evaluate(`document.querySelector('[data-open-dialog="reject"]').click()`);
    assert.equal(await evaluate(`document.activeElement.matches('textarea')`), true);
    await evaluate(`document.querySelector('[id$="btnRejectCourse"]').click()`);
    await ready(`document.querySelector('dialog[open]') && document.body.textContent.includes('Please provide a reason for rejecting this course.')`);
    await evaluate(`document.querySelector('[id$="txtRejectionReason"]').maxLength=1000; document.querySelector('[id$="txtRejectionReason"]').value='a'.repeat(501); document.querySelector('[id$="btnRejectCourse"]').click()`);
    await ready(`document.querySelector('dialog[open]') && document.body.textContent.includes('Please keep the rejection reason to 500 characters or fewer.')`);
    await evaluate(`document.querySelector('[id$="txtRejectionReason"]').value='Lesson 3 needs clearer instructions.'; document.querySelector('[id$="btnRejectCourse"]').click()`);
    await ready(`document.body.textContent.includes('Course rejected successfully.')`);
    assert.equal(await evaluate(`document.querySelector('[id$="btnPending"]').classList.contains('active')`), true);
    assert.equal(await evaluate(`document.querySelector('[id$="txtSearchCourses"]').value`), 'Ikan Bakar');
    await evaluate(`document.querySelector('[id$="btnRejected"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 1`);
    await ready(`document.querySelector('[id*="btnViewCourse"]')`);
    await evaluate(`document.querySelector('[id*="btnViewCourse"]').click()`);
    await ready(`document.querySelector('[data-tab="overview"]')`);
    assert.equal(await evaluate(`document.body.textContent.includes('Lesson 3 needs clearer instructions.') && !document.querySelector('[data-open-dialog]')`), true);
    await evaluate(`document.querySelector('[id$="btnBackToCourses"]').click()`);
    await ready(`document.querySelector('[id$="btnPending"]')`);
    await evaluate(`document.querySelector('[id$="txtSearchCourses"]').value='Approve Me'; document.querySelector('[id$="btnPending"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 1 && document.querySelector('.review-course-card').textContent.includes('Approve Me')`);
    await ready(`document.querySelector('[id*="btnViewCourse"]')`);
    await evaluate(`document.querySelector('[id*="btnViewCourse"]').click()`);
    await ready(`document.querySelector('[data-open-dialog="approve"]')`);
    await evaluate(`document.querySelector('[data-open-dialog="approve"]').click()`);
    assert.equal(await evaluate(`document.activeElement.textContent`), 'Cancel');
    await evaluate(`document.querySelector('[id$="btnApproveCourse"]').click()`);
    await ready(`document.body.textContent.includes('Course approved successfully.')`);
    const state = await (await fetch(base + '/Fixture.aspx?inspect=1')).json();
    assert.equal(state.approvedStatus, 'Published'); assert.equal(state.approvedReason, null);
    assert.equal(state.rejectedStatus, 'Rejected'); assert.equal(state.rejectedReason, 'Lesson 3 needs clearer instructions.');
    await evaluate(`document.querySelector('[id$="btnApproved"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 1`);
    await ready(`document.querySelector('[id*="btnViewCourse"]')`);
    await evaluate(`document.querySelector('[id*="btnViewCourse"]').click()`);
    await ready(`document.body.textContent.includes('This course has already been approved and published.')`);
    assert.equal(await evaluate(`!document.querySelector('[data-open-dialog]')`), true);
    for (const width of [375,768,1024,1440]) {
        await send('Emulation.setDeviceMetricsOverride', {width, height:900, deviceScaleFactor:1,mobile:false});
        assert.equal(await evaluate(`document.documentElement.scrollWidth <= innerWidth`), true, 'Detail overflow at '+width);
    }
    await evaluate(`document.querySelector('[id$="btnBackToCourses"]').click()`);
    await ready(`document.querySelector('[id$="btnPending"]')`);
    await evaluate(`document.querySelector('[id$="txtSearchCourses"]').value=''; document.querySelector('[id$="btnAllCourses"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 8`);
    for (const width of [375,768,1024,1440]) {
        await send('Emulation.setDeviceMetricsOverride', {width, height:900, deviceScaleFactor:1,mobile:false});
        assert.equal(await evaluate(`document.documentElement.scrollWidth <= innerWidth`), true, 'List overflow at '+width);
    }
    await evaluate(`document.querySelector('[id$="ddlChefFilter"]').value='2'; document.querySelector('[id$="ddlChefFilter"]').dispatchEvent(new Event('change',{bubbles:true}))`);
    await ready(`document.querySelectorAll('.review-course-card').length === 1`);
    assert.equal(await evaluate(`document.querySelector('.review-course-card').textContent.includes('Other Chef')`), true);
    await evaluate(`document.querySelector('[id$="txtSearchCourses"]').value='no-such-course'; document.querySelector('[id$="btnSearch"]').click()`);
    await ready(`document.body.textContent.includes('No courses match your search.')`);
    await evaluate(`document.querySelector('[id$="btnClearSearch"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 8`);
    // Recheck a pending decision after another reviewer publishes the same course.
    await evaluate(`document.querySelector('[id$="txtSearchCourses"]').value='Race Course'; document.querySelector('[id$="btnSearch"]').click()`);
    await ready(`document.querySelectorAll('.review-course-card').length === 1`);
    await ready(`document.querySelector('[id*="btnViewCourse"]')`);
    await evaluate(`document.querySelector('[id*="btnViewCourse"]').click()`);
    await ready(`document.querySelector('[data-open-dialog="reject"]')`);
    await fetch(base + '/Fixture.aspx?race=1');
    await evaluate(`document.querySelector('[data-open-dialog="reject"]').click(); document.querySelector('[id$="txtRejectionReason"]').value='Stale decision'; document.querySelector('[id$="btnRejectCourse"]').click()`);
    await ready(`document.body.textContent.includes('This course is no longer pending')`);
    assert.equal(await evaluate(`!document.querySelector('[data-open-dialog]')`), true);
    await send('Page.navigate', {url:base+'/Fixture.aspx?learner=1'});
    await ready(`document.body.textContent.includes('fixture ready')`);
    const denied = await evaluate(`fetch(${JSON.stringify(page)}).then(r=>r.status)`);
    assert.equal(denied,403);
    console.log('PASS: SQL-backed list, filters, pagination, five tabs, nested answers, validation, approve/reject, stale decisions, role checks, responsive widths.');
    await send("Browser.close").catch(() => {});
})().catch(error => { console.error(error); process.exitCode = 1; }).finally(async () => {
    if (socket) socket.close();
    if (edge.exitCode === null) edge.kill();
    await sleep(1000);
    // This path is generated above under the OS temporary directory for this run only.
    if (path.resolve(profile).startsWith(path.resolve(os.tmpdir()) + path.sep) && path.basename(profile).startsWith("CookingLahBrowserTest-")) {
        try { fs.rmSync(profile, { recursive: true, force: true, maxRetries: 3, retryDelay: 250 }); } catch (_) {}
    }
});
