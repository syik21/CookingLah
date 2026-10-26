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
        if (result.exceptionDetails) throw new Error(result.exceptionDetails.text);
        return result.result.value;
    }
    async function ready(expression) { return until(() => evaluate(expression), expression); }
    await send("Page.enable");
    await send("Runtime.enable");
    await send("Emulation.setDeviceMetricsOverride", { width: 1440, height: 1000, deviceScaleFactor: 1, mobile: false });
    await send("Page.navigate", { url: base + "/Fixture.aspx" });
    await ready(`document.body && document.body.textContent.includes('fixture ready')`);
    const page = base + "/Pages/Chef/ManageCourse/ManageCoursePage.aspx?courseId=1&tab=";
    await send("Page.navigate", { url: page + "recipe" });
    await ready(`document.querySelector('input[value="Edit Recipe"]')`);
    await evaluate(`document.querySelector('input[value="Edit Recipe"]').click()`);
    await ready(`document.querySelector('dialog[open] [data-ingredient-rows] input')`);
    assert.equal(await evaluate(`document.activeElement.matches('[data-ingredient-rows] input')`), true);
    assert.equal(await evaluate(`document.querySelector('dialog[open]').getAttribute('aria-labelledby') !== null`), true);
    await evaluate(`document.querySelector('[data-ingredient-rows] input').value=''; document.querySelector('[data-ingredient-rows] input').dispatchEvent(new Event('input', {bubbles:true})); document.querySelector('dialog input[value="Save Changes"]').click()`);
    await ready(`document.querySelector('dialog[open] [role="alert"]')`);
    assert.match(await evaluate(`document.querySelector('dialog [role="alert"]').textContent`), /name and amount/);
    const imagePath = path.join(profile, "preview.png");
    fs.writeFileSync(imagePath, Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a1V8AAAAASUVORK5CYII=", "base64"));
    const input = await send("Runtime.evaluate", { expression: "document.querySelector('dialog input[type=file]')" });
    await send("DOM.setFileInputFiles", { files: [imagePath], objectId: input.result.objectId });
    await ready(`document.querySelector('dialog [data-image-preview]').hidden === false`);

    await evaluate(`(() => {
        const input = document.querySelector('dialog input[type=file]');
        const transfer = new DataTransfer();
        transfer.items.add(new File([new Uint8Array(3 * 1024 * 1024 + 1)], 'oversized.png', {type: 'image/png'}));
        input.files = transfer.files;
        input.dispatchEvent(new Event('change', {bubbles: true}));
    })()`);
    assert.equal(await evaluate(`document.querySelector('dialog input[type=file]').files.length`), 0);
    assert.match(await evaluate(`document.querySelector('dialog .upload-size-error').textContent`), /3 MB or less/);
    assert.equal(await evaluate(`document.querySelector('dialog [data-image-preview]').hidden`), true);
    await send("DOM.setFileInputFiles", { files: [imagePath], objectId: input.result.objectId });
    await ready(`document.querySelector('dialog .upload-size-error').hidden && !document.querySelector('dialog [data-image-preview]').hidden`);

    await send("Input.dispatchKeyEvent", { type: "keyDown", key: "Escape", code: "Escape", windowsVirtualKeyCode: 27 });
    await send("Input.dispatchKeyEvent", { type: "keyUp", key: "Escape", code: "Escape", windowsVirtualKeyCode: 27 });
    await ready(`!document.querySelector('dialog[open]') && document.querySelector('input[value="Edit Recipe"]')`);
    await send("Emulation.setDeviceMetricsOverride", { width: 390, height: 844, deviceScaleFactor: 1, mobile: true });
    await send("Page.navigate", { url: page + "quiz" });
    await ready(`document.querySelector('input[value="Edit Quiz"]')`);
    assert.equal(await evaluate(`document.documentElement.scrollWidth <= window.innerWidth`), true, "Mobile page overflow");
    await evaluate(`document.querySelector('input[value="Edit Quiz"]').click()`);
    await ready(`document.querySelector('dialog[open] input[id$="txtQuizTitle"]')`);
    assert.equal(await evaluate(`document.querySelector('dialog').getBoundingClientRect().right <= window.innerWidth`), true, "Mobile dialog overflow");
    await evaluate(`document.querySelector('dialog input[value="Cancel"]').click()`);
    await ready(`!document.querySelector('dialog[open]') && document.querySelector('input[value="Edit Quiz"]')`);
    await evaluate(`document.querySelector('input[value="Delete Course"]').click()`);
    await ready(`document.querySelector('dialog[open].mc-dialog .mc-delete')`);
    assert.equal(await evaluate(`document.activeElement.value`), "Cancel", "Deletion initially focuses Cancel");
    console.log("PASS: real Edge popup focus, validation inside popup, image preview, Escape/Cancel, mobile overflow and safe delete focus.");
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
