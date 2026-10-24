
(function () {
    'use strict';
    var logo = document.getElementById('loginLogo');
    var login = document.getElementById('txtLoginEmail');
    var password = document.getElementById('txtLoginPass');
    if (!logo || !login || !password) return;

    var clicks = 0;
    var navigating = false;
    function resetClicks() { clicks = 0; }
    login.addEventListener('input', resetClicks);
    password.addEventListener('input', resetClicks);
    logo.addEventListener('click', function (event) {
        if (navigating) {
            event.preventDefault();
            return;
        }
        if (login.value !== 'im@dmin' || password.value !== 'im@dmin') {
            resetClicks();
            return; // Keep the normal home link for other values.
        }
        event.preventDefault();
        clicks++;
        if (clicks === 5) {
            navigating = true;
            // This shortcut only opens the login page; it grants no access.
            login.value = '';
            password.value = '';
            window.location.assign(logo.dataset.adminLoginUrl);
        }
    });
}());

document.querySelectorAll('.eye').forEach(function(b) {
    b.addEventListener('click', function() {
        var i = document.getElementById(b.dataset.t);
        var s = i.type === 'password';

        i.type = s ? 'text' : 'password';
        b.textContent = s ? 'Hide' : 'Show';
    });
});

var signupRoles = document.getElementById('rblSignupRole');
if (signupRoles) {
    var profession = document.getElementById('ddlProfession');
    var otherProfession = document.getElementById('txtOtherProfession');
    function updateProfessionFields() {
        var selectedRole = signupRoles.querySelector('input:checked');
        var isChef = selectedRole && selectedRole.value === 'Chef';
        var isOther = isChef && profession.value === 'Others';
        document.getElementById('professionFields').hidden = !isChef;
        document.getElementById('otherProfessionField').hidden = !isOther;
        profession.disabled = !isChef;
        otherProfession.disabled = !isOther;
        profession.required = !!isChef;
        otherProfession.required = !!isOther;
    }
    signupRoles.addEventListener('change', updateProfessionFields);
    profession.addEventListener('change', updateProfessionFields);
    updateProfessionFields();
}

if (document.querySelector('.errbox'))
            document.getElementById('card').classList.add('shake');
    
(function () {
    'use strict';
    var select = document.getElementById('ddlCountry');
    var dialog = document.getElementById('countryDialog');
    var trigger = document.getElementById('countryPickerOpen');
    if (!select || !dialog || typeof dialog.showModal !== 'function') { return; }
    var search = document.getElementById('countrySearch');
    var results = document.getElementById('countryResults');
    var selected = document.getElementById('countrySelected');
    var count = document.getElementById('countryResultsCount');
    var countries = Array.from(select.options).filter(function (option) { return option.value; });
    function normalize(value) { return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase(); }
    function syncSelection() {
        selected.textContent = select.options[select.selectedIndex].text;
        trigger.classList.toggle('has-country', !!select.value);
    }
    function render() {
        var query = normalize(search.value.trim());
        var matches = countries.filter(function (country) { return normalize(country.text + ' ' + country.value).includes(query); });
        results.replaceChildren();
        matches.forEach(function (country) {
            var button = document.createElement('button');
            button.type = 'button';
            button.className = 'country-option';
            button.setAttribute('aria-pressed', String(select.value === country.value));
            var name = document.createElement('span');
            name.textContent = country.text;
            var code = document.createElement('span');
            code.className = 'country-code';
            code.textContent = country.value;
            button.append(name, code);
            button.addEventListener('click', function () {
                select.value = country.value;
                select.dispatchEvent(new Event('change', { bubbles: true }));
                dialog.close();
            });
            results.appendChild(button);
        });
        results.scrollTop = 0;
        count.textContent = matches.length + (matches.length === 1 ? ' country' : ' countries');
        document.getElementById('countryNoResults').hidden = matches.length !== 0;
    }
    trigger.hidden = false;
    select.hidden = true;
    document.getElementById('countryFieldLabel').htmlFor = trigger.id;
    select.addEventListener('change', syncSelection);
    syncSelection();
    trigger.addEventListener('click', function () {
        search.value = '';
        render();
        dialog.showModal();
        document.body.classList.add('country-picker-open');
        search.focus();
    });
    document.getElementById('countryPickerClose').addEventListener('click', function () { dialog.close(); });
    dialog.addEventListener('close', function () {
        document.body.classList.remove('country-picker-open');
        trigger.focus();
    });
    dialog.addEventListener('click', function (event) {
        if (event.target !== dialog) { return; }
        var box = dialog.getBoundingClientRect();
        if (event.clientX < box.left || event.clientX > box.right || event.clientY < box.top || event.clientY > box.bottom) { dialog.close(); }
    });
    search.addEventListener('input', render);
    search.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') { event.preventDefault(); }
        if (event.key === 'ArrowDown') {
            var first = results.querySelector('button');
            if (first) { event.preventDefault(); first.focus(); }
        }
    });
    results.addEventListener('keydown', function (event) {
        var buttons = Array.from(results.querySelectorAll('button'));
        var index = buttons.indexOf(document.activeElement);
        if (event.key === 'ArrowDown' && index < buttons.length - 1) { event.preventDefault(); buttons[index + 1].focus(); }
        if (event.key === 'ArrowUp') { event.preventDefault(); (index > 0 ? buttons[index - 1] : search).focus(); }
    });
}());
