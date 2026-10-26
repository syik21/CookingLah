(function () {
    'use strict';

    var shell;
    var countrySelector;
    var searchInput;
    var allFoodCultures = [];
    var selectedCountry = 'Malaysia';
    var currentSearch = '';
    var currentPage = 1;
    var pageSize = 4;

    function displayCountryName(country) {
        return country === 'United States of America' ? 'United States' : country;
    }

    function getCountryFoods() {
        return allFoodCultures.filter(function (food) {
            // Preserve the existing database alias for the SVG's United States name.
            return food.country === selectedCountry ||
                (selectedCountry === 'United States of America' && food.country === 'United States');
        });
    }

    function getDishImage(path) {
        if (!path || !path.trim()) return shell.dataset.placeholder;
        path = path.trim();
        if (path.indexOf('~/') === 0) path = shell.dataset.appRoot + path.substring(2);
        try {
            var url = new URL(path, new URL(shell.dataset.appRoot, window.location.origin));
            return url.protocol === 'http:' || url.protocol === 'https:' ? url.href : shell.dataset.placeholder;
        } catch (error) {
            return shell.dataset.placeholder;
        }
    }

    function createElement(tag, className, text) {
        var element = document.createElement(tag);
        element.className = className;
        if (text !== undefined) element.textContent = text;
        return element;
    }

    function renderDishCards(foods) {
        var list = document.getElementById('foodDishList');
        var cards = document.createDocumentFragment();
        foods.forEach(function (food) {
            var card = createElement('article', 'food-dish-card');
            card.dataset.foodCultureId = food.foodCultureId;
            var imageWrap = createElement('div', 'food-dish-card__image-wrap');
            var image = createElement('img', 'food-dish-card__image');
            image.alt = food.dishName;
            image.addEventListener('error', function () {
                image.src = shell.dataset.placeholder;
            }, { once: true });
            image.src = getDishImage(food.dishImage);
            imageWrap.appendChild(image);
            var body = createElement('div', 'food-dish-card__body');
            body.appendChild(createElement('h2', 'food-dish-card__title', food.dishName));
            body.appendChild(createElement('p', 'food-dish-card__description',
                food.description || 'Traditional food waiting to be explored.'));
            var exploreLink = createElement('a', 'food-dish-card__explore', 'Explore courses \u2192');
            exploreLink.href = shell.dataset.courseSearchUrl + '?foodCultureId=' + encodeURIComponent(food.foodCultureId);
            exploreLink.ariaLabel = 'Explore courses for ' + food.dishName;
            body.appendChild(exploreLink);
            card.appendChild(imageWrap);
            card.appendChild(body);
            cards.appendChild(card);
        });
        list.replaceChildren(cards);
        shell.querySelector('.food-map-results').scrollTop = 0;
    }

    function renderPagination(totalPages) {
        document.getElementById('foodPagination').hidden = totalPages <= 1;
        document.getElementById('foodPageInfo').textContent = currentPage + ' / ' + totalPages;
        document.getElementById('foodPrevious').disabled = currentPage <= 1;
        document.getElementById('foodNext').disabled = currentPage >= totalPages;
    }

    function renderFoodCultures() {
        var countryFoods = getCountryFoods();
        var filteredFoods = countryFoods.filter(function (food) {
            return food.dishName.toLowerCase().startsWith(currentSearch.toLowerCase());
        });
        var totalPages = Math.max(1, Math.ceil(filteredFoods.length / pageSize));
        currentPage = Math.max(1, Math.min(currentPage, totalPages));
        var startIndex = (currentPage - 1) * pageSize;
        renderDishCards(filteredFoods.slice(startIndex, startIndex + pageSize));
        renderPagination(totalPages);
        document.getElementById('foodCountryName').textContent = displayCountryName(selectedCountry);
        document.getElementById('foodDishCount').textContent = currentSearch
            ? filteredFoods.length + ' of ' + countryFoods.length + ' dishes found'
            : countryFoods.length + (countryFoods.length === 1 ? ' dish' : ' dishes');
        document.getElementById('foodComingSoon').hidden = countryFoods.length !== 0;
        document.getElementById('foodNoMatches').hidden = countryFoods.length === 0 || filteredFoods.length !== 0;
    }

    function selectFoodCountry(country) {
        selectedCountry = country;
        currentSearch = '';
        currentPage = 1;
        countrySelector.value = country;
        searchInput.value = '';
        searchInput.placeholder = 'Search dishes in ' + displayCountryName(country) + '...';
        renderFoodCultures();
    }

    function initializeCountryInteraction() {
        var map = window.CookingLahWorldMap;
        var countries = map.getCountries();
        countries.forEach(function (country) {
            var option = document.createElement('option');
            option.value = country;
            option.textContent = displayCountryName(country);
            countrySelector.appendChild(option);
        });
        document.addEventListener('worldmap:countrychange', function (event) {
            if (event.detail && countries.includes(event.detail.country)) {
                selectFoodCountry(event.detail.country);
            }
        });
        countrySelector.addEventListener('change', function () {
            map.selectCountry(countrySelector.value, { focus: true });
        });
        var initialCountry = shell.dataset.selectedCountry;
        if (initialCountry === 'United States') initialCountry = 'United States of America';
        map.selectCountry(countries.includes(initialCountry) ? initialCountry : 'Malaysia', { focus: true });
    }

    function initialize() {
        shell = document.getElementById('foodMapShell');
        if (!shell) return;
        countrySelector = document.getElementById('foodCountrySelector');
        searchInput = document.getElementById('foodDishSearch');
        allFoodCultures = JSON.parse(document.getElementById('foodCultureData').textContent);
        document.body.classList.add('food-map-page-active');

        var panel = document.getElementById('foodExplorerPanel');
        var panelToggle = document.getElementById('foodPanelToggle');
        panelToggle.addEventListener('click', function () {
            panel.hidden = !panel.hidden;
            panelToggle.ariaExpanded = String(!panel.hidden);
            panelToggle.textContent = panel.hidden ? 'Show food explorer' : 'Hide food explorer';
        });

        var header = document.querySelector('.master-header, .public-header');
        function updateLayout() {
            shell.style.setProperty('--food-map-top', header.getBoundingClientRect().bottom + 'px');
        }
        updateLayout();
        new ResizeObserver(updateLayout).observe(header);
        window.addEventListener('resize', updateLayout);

        searchInput.addEventListener('input', function () {
            currentSearch = searchInput.value.trim();
            currentPage = 1;
            renderFoodCultures();
        });
        searchInput.addEventListener('keydown', function (event) {
            if (event.key === 'Enter') event.preventDefault();
        });
        document.getElementById('foodPrevious').addEventListener('click', function () {
            currentPage--;
            renderFoodCultures();
        });
        document.getElementById('foodNext').addEventListener('click', function () {
            currentPage++;
            renderFoodCultures();
        });
        if (window.CookingLahWorldMap) initializeCountryInteraction();
        else document.addEventListener('worldmap:ready', initializeCountryInteraction, { once: true });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initialize);
    else initialize();
}());
