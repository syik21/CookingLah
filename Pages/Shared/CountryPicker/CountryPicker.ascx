<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CountryPicker.ascx.cs" Inherits="CookingLah.Pages.Shared.CountryPicker.CountryPicker" %>
<div class="country-field" data-country-picker>
    <asp:Label
        ID="lblCountry"
        runat="server"
        AssociatedControlID="ddlCountry"
        CssClass="country-field-label"
        Text="Country" />
    <asp:DropDownList
        ID="ddlCountry"
        runat="server"
        CssClass="form-select"
        data-country-select="" />
    <button
        type="button"
        id="<%= ClientID %>_trigger"
        class="country-trigger"
        data-country-trigger=""
        aria-haspopup="dialog"
        aria-controls="<%= ClientID %>_dialog"
        aria-labelledby="<%= lblCountry.ClientID %> <%= ClientID %>_selected"
        hidden>
        <span id="<%= ClientID %>_selected" data-country-selected="">Select your country</span>
        <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m7 10 5 5 5-5" /></svg>
    </button>
    <dialog
        id="<%= ClientID %>_dialog"
        class="country-dialog"
        aria-labelledby="<%= ClientID %>_title"
        aria-describedby="<%= ClientID %>_description">
        <div class="country-dialog-heading">
            <h2 id="<%= ClientID %>_title">Select your country</h2>
            <button
                type="button"
                class="country-close"
                data-country-close=""
                aria-label="Close country selection">&times;</button>
        </div>
        <p id="<%= ClientID %>_description">Choose the country associated with this Traditional Food.</p>
        <label class="country-search-label" for="<%= ClientID %>_search">Search countries</label>
        <input
            type="search"
            id="<%= ClientID %>_search"
            data-country-search=""
            placeholder="Search by country name or code"
            autocomplete="off" />
        <p
            class="country-result-count"
            data-country-count=""
            role="status"
            aria-live="polite">
        </p>
        <div
            class="country-results"
            data-country-results=""
            role="group"
            aria-label="Countries">
        </div>
        <p class="country-empty" data-country-empty="" hidden>No countries found. Try another name or country code.</p>
    </dialog>
</div>
