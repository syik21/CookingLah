<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FoodMapView.ascx.cs" Inherits="CookingLah.Pages.Shared.FoodMap.FoodMapView" %>
<%@ Register Src="~/Pages/Shared/WorldMap/WorldMap.ascx" TagPrefix="shared" TagName="WorldMap" %>

<link rel="stylesheet" href="<%= ResolveUrl("~/Pages/Shared/FoodMap/FoodMapView.css") %>" />
<section class="food-map-shell" id="foodMapShell"
         data-selected-country="<%= Server.HtmlEncode(SelectedCountry) %>"
         data-app-root="<%= ResolveUrl("~/") %>"
         data-course-search-url="<%= Server.HtmlEncode(CourseSearchUrl) %>"
         data-placeholder="<%= ResolveUrl("~/F_Assets/placeholder-food.svg") %>">
    <div class="food-map-canvas" aria-label="World food map">
        <shared:WorldMap ID="WorldMap1" runat="server" />
    </div>
    <button id="foodPanelToggle" type="button" class="btn btn-primary food-map-panel-toggle"
            aria-controls="foodExplorerPanel" aria-expanded="true">Hide food explorer</button>
    <aside id="foodExplorerPanel" class="food-map-panel" aria-label="Food explorer">
        <div class="food-map-panel__header">
            <p class="food-map-panel__eyebrow">Food Explorer</p>
            <h1 class="food-map-panel__title">Explore traditional food</h1>
        </div>
        <div class="food-map-panel__controls">
            <label class="food-map-label" for="foodCountrySelector">Country</label>
            <select id="foodCountrySelector" class="food-map-select"></select>
            <label class="food-map-label" for="foodDishSearch">Search dishes</label>
            <input id="foodDishSearch" type="search" class="food-map-search__input" maxlength="100" autocomplete="off" />
        </div>
        <div class="food-map-panel__summary" role="status" aria-atomic="true">
            <span id="foodCountryName" class="food-map-country-name"></span>
            <span id="foodDishCount" class="food-map-dish-count"></span>
        </div>
        <div class="food-map-results">
            <div id="foodDishList" class="food-map-dish-list"></div>
            <div id="foodComingSoon" class="food-map-empty" hidden>
                <h2>Coming Soon.</h2><p>The food is on the way.</p>
            </div>
            <div id="foodNoMatches" class="food-map-empty" hidden>
                <h2>No dishes found.</h2><p>Try another dish name.</p>
            </div>
        </div>
        <div id="foodPagination" class="food-map-pagination" hidden>
            <button id="foodPrevious" type="button" class="btn btn-secondary food-map-pagination__button">&#8249; Previous</button>
            <span id="foodPageInfo" class="food-map-pagination__info"></span>
            <button id="foodNext" type="button" class="btn btn-secondary food-map-pagination__button">Next &#8250;</button>
        </div>
    </aside>
    <script id="foodCultureData" type="application/json"><%= FoodCulturesJson %></script>
</section>
<script src="<%= ResolveUrl("~/Pages/Shared/FoodMap/FoodMapView.js") %>"></script>


