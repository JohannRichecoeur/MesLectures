using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using MesLectures.Common;
using MesLectures.DataModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private static double leftColumWidth;

        private readonly NavigationHelper navigationHelper;

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper => this.navigationHelper;

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel { get; } = new ObservableDictionary();

        public MainPage()
        {
            this.InitializeComponent();
            this.AppBarInfosButton.Label = Settings.GetRessource("AppBar_Infos");
            this.AppBarAddButton.Label = Settings.GetRessource("AppBar_Add");
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
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
                this.DefaultViewModel["Items"] = BookDataSource.GetGroups("AllGroups").First().Items;
            }

            this.FakeButtonForFocus.Focus(FocusState.Programmatic);
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

        private void OneDriveButtonClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OnedrivePage));
        }
    }
}