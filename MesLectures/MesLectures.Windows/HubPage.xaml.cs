using System;
using System.Collections.Generic;
using System.Linq;
using MesLectures.Common;
using MesLectures.DataModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage
    {
        private IEnumerable<BookDataGroup> dataGroups;
        private static double leftColumWidth;

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

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

        public HubPage()
        {
            this.InitializeComponent();
            this.AppBarInfosButton.Label = Settings.GetRessource("AppBar_Infos");
            this.AppBarAddButton.Label = Settings.GetRessource("AppBar_Add");
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.PageTitle.Text = Settings.GetRessource("AppTitle");

            // Retrieve book list from Local folder
            Settings.BookList = await Settings.ReadFileForBookList();

            // Otherwise create an xml from settings
            if (Settings.BookList == null || Settings.BookList.Count == 0)
            {
                var md = new MessageDialog(Settings.GetRessource("Welcome_Title") + "\n" + Settings.GetRessource("Welcome_Text"), Settings.GetRessource("AppTitle"));
                md.Commands.Add(new UICommand("OK"));
                await md.ShowAsync();
                MyAppBar.IsOpen = true;
            }
            else
            {
                await BookDataSource.FillData();
                var bookDataGroups = BookDataSource.GetGroups("AllGroups");
                this.dataGroups = bookDataGroups;
                this.DefaultViewModel["Groups"] = bookDataGroups.First();
            }

            this.FakeButtonForFocus.Focus(FocusState.Programmatic);

            // ReSharper disable once CSharpWarnings::CS4014 => we don't want to wauit this method to unblock the navigation
            Settings.GetUserName(this);
        }

        /// <summary>
        /// Invoked when a HubSection header is clicked.
        /// </summary>
        /// <param name="sender">The Hub that contains the HubSection whose header was clicked.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            HubSection section = e.Section;
            var group = section.DataContext;
            this.Frame.Navigate(typeof(SectionPage), ((BookDataGroup)group).GroupId);

        }

        private async void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width > 500)
            {
                this.Frame.Navigate(typeof(EditionPage));
            }
            else
            {
                var md = new MessageDialog(Settings.GetRessource("Windows_IncreaseSize"));
                md.Commands.Add(new UICommand("OK"));
                await md.ShowAsync();
            }
        }

        private async void ButtonInfosClick(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog(Settings.GetRessource("Infos_dev") + " = Jean-Eric Hourchon (jean-eric.hourchon@live.fr) \nVersion = 1.3 ", Settings.GetRessource("AppTitle"));
            md.Commands.Add(new UICommand(Settings.GetRessource("Infos_sendMail")));
            md.Commands.Add(new UICommand(Settings.GetRessource("Infos_close")));
            var response = await md.ShowAsync();

            if (response.Label == Settings.GetRessource("Infos_sendMail"))
            {
                var mailto = new Uri("mailto:?to=jean-eric.hourchon@live.fr&subject=" + Settings.GetRessource("AppTitle") + ", Windows 8");
                await Windows.System.Launcher.LaunchUriAsync(mailto);
            }
        }

        private void BookClick(object sender, ItemClickEventArgs e)
        {
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
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            leftColumWidth = ((Image)sender).ActualWidth;
        }

        private void SearchBoxQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            this.Frame.Navigate(typeof(SectionPage), "searchValue=" + args.QueryText);
        }
    }
}
