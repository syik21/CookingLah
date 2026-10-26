(function () {
    'use strict';

    document.querySelectorAll('[data-country-picker]').forEach(function (picker) {
        var select = picker.querySelector('[data-country-select]');
        var trigger = picker.querySelector('[data-country-trigger]');
        var dialog = picker.querySelector('[data-country-dialog]');

        if (!select || !trigger || !dialog || typeof dialog.showModal !== 'function') return;

        var search = picker.querySelector('[data-country-search]');
        var results = picker.querySelector('[data-country-results]');
        var selected = picker.querySelector('[data-country-selected]');
        var count = picker.querySelector('[data-country-count]');
        var noResults = picker.querySelector('[data-country-empty]');
        var close = picker.querySelector('[data-country-close]');
        var label = picker.querySelector('label[for]');
        var countries = Array.from(select.options).filter(function (option) { return option.value; });

        function normalize(value) {
            return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
        }

        function syncSelection() {
            selected.textContent = select.options[select.selectedIndex].text;
            trigger.classList.toggle('has-country', !!select.value);
        }

        function renderResults() {
            var query = normalize(search.value.trim());
            var matches = countries.filter(function (country) {
                return normalize(country.text + ' ' + country.value).includes(query);
            });

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
            noResults.hidden = matches.length !== 0;
        }

        trigger.hidden = false;
        select.hidden = true;
        if (label) label.htmlFor = trigger.id;
        select.addEventListener('change', syncSelection);
        syncSelection();

        trigger.addEventListener('click', function () {
            search.value = '';
            renderResults();
            dialog.showModal();
            document.body.classList.add('country-picker-open');
            search.focus();
        });

        close.addEventListener('click', function () { dialog.close(); });
        dialog.addEventListener('close', function () {
            document.body.classList.remove('country-picker-open');
            trigger.focus();
        });
        dialog.addEventListener('click', function (event) {
            if (event.target !== dialog) return;

            var bounds = dialog.getBoundingClientRect();
            if (event.clientX < bounds.left || event.clientX > bounds.right ||
                event.clientY < bounds.top || event.clientY > bounds.bottom) {
                dialog.close();
            }
        });
        search.addEventListener('input', renderResults);
        search.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') event.preventDefault();
            if (event.key === 'ArrowDown') {
                var firstResult = results.querySelector('button');
                if (firstResult) {
                    event.preventDefault();
                    firstResult.focus();
                }
            }
        });
        results.addEventListener('keydown', function (event) {
            var buttons = Array.from(results.querySelectorAll('button'));
            var currentIndex = buttons.indexOf(document.activeElement);

            if (event.key === 'ArrowDown' && currentIndex < buttons.length - 1) {
                event.preventDefault();
                buttons[currentIndex + 1].focus();
            }
            if (event.key === 'ArrowUp') {
                event.preventDefault();
                (currentIndex > 0 ? buttons[currentIndex - 1] : search).focus();
            }
        });
    });
}());
