using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CookingLah.Pages.Shared.CountryPicker
{
    public partial class CountryPicker : UserControl
    {
        public string LabelText
        {
            get { return lblCountry.Text; }
            set { lblCountry.Text = value; }
        }

        public string SelectedCountryName
        {
            get
            {
                ListItem selectedCountry = ddlCountry.SelectedItem;
                return selectedCountry == null || string.IsNullOrEmpty(selectedCountry.Value)
                    ? ""
                    : selectedCountry.Text;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LoadCountries();
        }

        public void SelectCountry(string countryName)
        {
            ListItem country = ddlCountry.Items.Cast<ListItem>()
                .FirstOrDefault(item => string.Equals(
                    item.Text,
                    countryName,
                    StringComparison.OrdinalIgnoreCase));

            if (country != null)
            {
                ddlCountry.SelectedValue = country.Value;
            }
        }

        public void ClearSelection()
        {
            ddlCountry.ClearSelection();
            ddlCountry.SelectedIndex = 0;
        }

        public static bool IsSupportedCountryName(string countryName)
        {
            return GetCountries().Any(country => string.Equals(
                country.EnglishName,
                countryName,
                StringComparison.OrdinalIgnoreCase));
        }

        private void LoadCountries()
        {
            ddlCountry.Items.Clear();
            ddlCountry.Items.Add(new ListItem("Select your country", ""));

            foreach (RegionInfo country in GetCountries())
            {
                ddlCountry.Items.Add(new ListItem(
                    country.EnglishName,
                    country.TwoLetterISORegionName));
            }
        }

        private static IOrderedEnumerable<RegionInfo> GetCountries()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture => new RegionInfo(culture.Name))
                .Where(region => region.TwoLetterISORegionName.Length == 2)
                .GroupBy(region => region.TwoLetterISORegionName)
                .Select(group => group.First())
                .OrderBy(region => region.EnglishName, StringComparer.Ordinal);
        }
    }
}
