using System.Linq;
using Windows.UI.Core;
using MesLectures.Common;
using MesLectures.DataModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class SectionPage
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private bool searchContext = false;
        private BookDataGroup dataGroup;

        public SectionPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelperLoadState;

            // Handle back navigation
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            args.Handled = true;
            this.Frame.Navigate(typeof(Books));
        }

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            await BookDataSource.FillData();

            if (e.NavigationParameter.ToString().StartsWith("searchValue="))
            {
                // Search page context
                this.searchContext = true;
                string query = e.NavigationParameter.ToString().Replace("searchValue=", "");
                Settings.CurrentSearch = query;
                this.dataGroup = await BookDataSource.GetSearchResults(query);
                this.PageTitle.Text = string.Format(Settings.GetRessource("Search_Results"), query);
                this.PageSubTitle.Text = " / " + this.dataGroup.Items.Count + " " + (this.dataGroup.Items.Count == 1 ? Settings.GetRessource("Settings_Book_1") : Settings.GetRessource("Settings_Book_MoreThanOne"));
            }
            else
            {
                // All books context
                this.searchContext = false;
                this.PageTitle.Text = Settings.GetRessource("Title_HeaderSection");
                this.dataGroup = BookDataSource.GetGroups("AllGroups").First();
            }

            this.DefaultViewModel["Groups"] = this.dataGroup;
            this.DefaultViewModel["Items"] = this.dataGroup.Items;
        }

        private void ItemViewClick(object sender, ItemClickEventArgs e)
        {
            if (this.searchContext)
            {
                Settings.ComingFromSearch = true;
            }

            var itemId = ((BookDataItem)e.ClickedItem).Id;
            var groupId = ((BookDataItem)e.ClickedItem).Group.GroupId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId + "-" + groupId);
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}