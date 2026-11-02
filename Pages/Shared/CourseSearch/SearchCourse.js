(function () {
    "use strict";

    var searchInput = document.querySelector(".course-search-toolbar input[type='search']");
    var searchButton = document.querySelector(".course-search-toolbar .search-submit");

    if (!searchInput || !searchButton) return;

    searchInput.addEventListener("keydown", function (event) {
        if (event.key !== "Escape" || searchInput.value.length === 0) return;

        event.preventDefault();
        searchInput.value = "";
        searchButton.click();
    });
}());
