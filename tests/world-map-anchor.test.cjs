const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const source = fs.readFileSync('Pages/Shared/WorldMap/WorldMap.js', 'utf8');
const markup = fs.readFileSync('Pages/Shared/WorldMap/WorldMap.ascx', 'utf8');
const context = vm.createContext({ document: { addEventListener() {} } });
vm.runInContext(source, context);
const paths = [...markup.matchAll(/<path d="([^"]+)" data-n="([^"]+)"/g)];
assert.ok(paths.length > 150);
for (const [, d, name] of paths) {
    assert.match(d, /^[MLZ\d.,\-]+$/, `${name}: supported polygon commands`);
    const path = { getAttribute: () => d };
    const anchor = context.getCountryAnchor(path);
    assert.ok(Number.isFinite(anchor.x) && Number.isFinite(anchor.y), `${name}: finite anchor`);
    const inside = d.match(/M[^M]+/g).some(part => {
        const values = part.match(/-?\d+(?:\.\d+)?/g).map(Number);
        let contained = false;
        for (let i = 0, j = values.length - 2; i < values.length; j = i, i += 2) {
            const ax = values[i], ay = values[i + 1], bx = values[j], by = values[j + 1];
            if ((ay > anchor.y) !== (by > anchor.y) &&
                anchor.x < (bx - ax) * (anchor.y - ay) / (by - ay) + ax) contained = !contained;
        }
        return contained;
    });
    assert.ok(inside, `${name}: anchor stays on land`);
    assert.equal(context.getCountryAnchor(path), anchor, `${name}: cached anchor`);
}
const detached = context.getCountryAnchor({ getAttribute: () => 'M0,0L10,0L10,10L0,10ZM100,100L101,100L101,101L100,101Z' });
assert.equal(detached.x, 5);
assert.equal(detached.y, 5);
assert.ok(markup.indexOf('class="world-map"') < markup.indexOf('id="enlargeMap"'));
assert.ok(markup.indexOf('id="enlargeMap"') < markup.indexOf('class="map-viewport"'));
console.log(`Passed: ${paths.length} country anchors, detached-region centering, and controls inside map.`);

const handlers = {};
let captured = false;
const classes = new Set();
const surface = {
    scrollWidth: 2000, clientWidth: 1000, scrollHeight: 1000, clientHeight: 500,
    scrollLeft: 200, scrollTop: 100,
    addEventListener: (name, handler) => { handlers[name] = handler; },
    classList: { add: name => classes.add(name), remove: name => classes.delete(name) },
    setPointerCapture: () => { captured = true; },
    hasPointerCapture: () => captured,
    releasePointerCapture: () => { captured = false; }
};
context.hideMapTooltip = () => {};
context.initializeMapPanning(surface);
const pointer = { pointerId: 1, pointerType: 'mouse', isPrimary: true, button: 0, clientX: 100, clientY: 100, preventDefault() {} };
handlers.pointerdown(pointer);
handlers.pointermove({ ...pointer, clientX: 102 });
assert.equal(captured, false, 'Small movements preserve normal clicks');
handlers.pointermove({ ...pointer, clientX: 150, clientY: 130 });
assert.equal(surface.scrollLeft, 150);
assert.equal(surface.scrollTop, 70);
assert.equal(captured, true);
handlers.pointerup(pointer);
assert.equal(captured, false);
assert.equal(classes.size, 0);
let suppressed = false;
handlers.click({ detail: 1, preventDefault() {}, stopPropagation() { suppressed = true; } });
assert.equal(suppressed, true, 'Dragging must not select a country');
handlers.pointerdown({ ...pointer, pointerType: 'touch' });
handlers.pointermove({ ...pointer, clientX: 300 });
assert.equal(surface.scrollLeft, 150, 'Touch keeps native scrolling');
console.log('Passed: drag threshold, panning, pointer cleanup, click suppression, and native touch handling.');
