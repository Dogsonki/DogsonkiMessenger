using Client.Networking;
using Client.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Client.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchPage : ContentPage
    {
        protected static SearchPage CachedSearchView;

        public SearchPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            SearchPageView.ClearList();
            InitializeComponent();
        }

        private void FindClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SearchInput.Text))
                return;

            SearchPageView.ClearList();
            if (!SocketCore.SendR(SearchPageView.ParseQuery, $"{SearchInput.Text}", Token.SEARCH_USER))
            {
                //TODO: make error level
            }
        }

        public static void RedirectAndSearch(string search)
        {
            if (CachedSearchView == null)
                CachedSearchView = new SearchPage();

            CachedSearchView.SearchInput.Text = search;

            StaticNavigator.Push(CachedSearchView);
            SearchPageView.ClearList();
            if (!SocketCore.SendR(SearchPageView.ParseQuery, $"{search}", Token.SEARCH_USER))
            {
                //TODO: make error level
            }
        }

        private void PeopleFound_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var _temp = (ListView)sender;
            _temp.SelectedItem = null;
        }
    }
}