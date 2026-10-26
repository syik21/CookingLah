const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');

class Element {
    constructor() {
        this.children = [];
        this.dataset = {};
        this.listeners = {};
        this.style = { setProperty() {} };
        this.classList = { add() {} };
        this.value = '';
    }
    appendChild(child) {
        if (child.fragment) this.children.push(...child.children);
        else this.children.push(child);
    }
    replaceChildren(child) { this.children = []; this.appendChild(child); }
    addEventListener(name, callback) { this.listeners[name] = callback; }
    fire(name, event = {}) { this.listeners[name](event); }
    getBoundingClientRect() { return { bottom: 100 }; }
}
const ids = {};
function el(id) { return ids[id] ||= new Element(); }
const shell = el('foodMapShell');
shell.dataset = { selectedCountry: 'Japan', appRoot: '/CookingLah/', placeholder: '/CookingLah/F_Assets/placeholder-food.svg' };
shell.dataset.courseSearchUrl = '/CookingLah/Pages/Guest/SearchCourse/SearchCoursePage.aspx';
shell.querySelector = () => el('results');
const foods = Array.from({ length: 9 }, (_, i) => ({ foodCultureId: i + 1, country: 'Malaysia', dishName: 'Nasi ' + i, description: '<img onerror=attack()>', dishImage: i ? '' : 'javascript:attack()' }));
foods.push({ foodCultureId: 10, country: 'Japan', dishName: 'Sushi', description: 'Rice', dishImage: '~/F_Assets/sushi.png' });
foods.push({ foodCultureId: 11, country: 'United States', dishName: 'Burger', description: '', dishImage: null });
el('foodCultureData').textContent = JSON.stringify(foods);
const document = {
    readyState: 'complete', body: new Element(), listeners: {},
    getElementById: el, querySelector: () => el('header'),
    createElement: () => new Element(),
    createDocumentFragment: () => Object.assign(new Element(), { fragment: true }),
    addEventListener(name, callback) { this.listeners[name] = callback; }
};
let mapSelection;
let selectCalls = 0;
const map = {
    getCountries: () => ['Malaysia', 'Japan', 'Italy', 'United States of America'],
    selectCountry(country) {
        assert.ok(++selectCalls < 10, 'selection must not loop');
        mapSelection = country;
        document.listeners['worldmap:countrychange']({ detail: { country } });
    }
};
vm.runInNewContext(fs.readFileSync('Pages/Shared/FoodMap/FoodMapView.js', 'utf8'), {
    document, URL, ResizeObserver: class { observe() {} },
    window: { CookingLahWorldMap: map, location: { origin: 'https://example.test' }, addEventListener() {} }
});
assert.equal(mapSelection, 'Japan');
assert.equal(el('foodDishList').children.length, 1);
assert.equal(el('foodDishList').children[0].children[0].children[0].src, 'https://example.test/CookingLah/F_Assets/sushi.png');
function select(country) {
    el('foodCountrySelector').value = country;
    el('foodCountrySelector').fire('change');
}
select('Malaysia');
assert.equal(mapSelection, 'Malaysia');
assert.equal(el('foodDishList').children.length, 4);
assert.equal(el('foodPageInfo').textContent, '1 / 3');
const first = el('foodDishList').children[0];
assert.equal(first.children[1].children[1].textContent, '<img onerror=attack()>');
assert.equal(first.children[1].children[2].href, shell.dataset.courseSearchUrl + '?foodCultureId=1');
assert.equal(first.children[1].children[2].ariaLabel, 'Explore courses for Nasi 0');
assert.equal(first.children[0].children[0].src, shell.dataset.placeholder);
el('foodNext').fire('click');
assert.equal(el('foodDishList').children[0].dataset.foodCultureId, 5);
el('foodNext').fire('click');
assert.equal(el('foodDishList').children.length, 1);
assert.equal(el('foodNext').disabled, true);
el('foodPrevious').fire('click');
assert.equal(el('foodPageInfo').textContent, '2 / 3');
el('foodDishSearch').value = 'NASI 0';
el('foodDishSearch').fire('input');
assert.equal(el('foodDishList').children.length, 1);
assert.equal(el('foodPageInfo').textContent, '1 / 1');
el('foodDishSearch').value = 'Sushi';
el('foodDishSearch').fire('input');
assert.equal(el('foodNoMatches').hidden, false);
assert.equal(el('foodComingSoon').hidden, true);
document.listeners['worldmap:countrychange']({ detail: { country: 'Japan' } });
assert.equal(el('foodCountrySelector').value, 'Japan');
assert.equal(el('foodDishSearch').value, '');
assert.equal(el('foodDishList').children.length, 1);
select('Italy');
assert.equal(el('foodComingSoon').hidden, false);
assert.equal(el('foodNoMatches').hidden, true);
select('United States of America');
assert.equal(el('foodDishList').children.length, 1);
let prevented = false;
el('foodDishSearch').fire('keydown', { key: 'Enter', preventDefault() { prevented = true; } });
assert.equal(prevented, true);
console.log('Food map interaction checks passed.');
