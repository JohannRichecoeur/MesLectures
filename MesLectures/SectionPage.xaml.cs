using System;
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

            this.AppBarAddButton.Label = Settings.GetRessource("AppBar_Add");
            this.AppBarEditButton.Label = Settings.GetRessource("AppBar_Edit");
            this.AppBarDiscardButton.Label = Settings.GetRessource("AppBar_Delete");

            // Handle back navigation
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested +=
                (sender, args) =>
                {
                    this.Frame.Navigate(typeof(Books));
                };
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

        private async void ButtonEditClick(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width > 500)
            {
                var itemSelected = (BookDataItem)this.ItemGridView.SelectedItem;
                if (itemSelected != null)
                {
                    this.Frame.Navigate(typeof(EditionPage), itemSelected.Id);
                }
            }
            else
            {
                var md = new MessageDialog(Settings.GetRessource("Windows_IncreaseSize"));
                md.Commands.Add(new UICommand("OK"));
                await md.ShowAsync();
            }
        }

        private async void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            IUICommand response = new UICommand("initialize");
            if (this.ItemGridView.SelectedItems.Count == 1)
            {
                var bookDataItem = (BookDataItem)this.ItemGridView.SelectedItem;
                if (bookDataItem != null)
                {
                    var md =
                        new MessageDialog(
                            string.Format(Settings.GetRessource("Delete-Book"), bookDataItem.Title, bookDataItem.Author),
                            Settings.GetRessource("AppTitle"));
                    md.Commands.Add(new UICommand(Settings.GetRessource("Yes")));
                    md.Commands.Add(new UICommand(Settings.GetRessource("No")));
                    response = await md.ShowAsync();
                }
            }
            else if (this.ItemGridView.SelectedItems.Count > 1)
            {
                var md =
                    new MessageDialog(
                        string.Format(Settings.GetRessource("Delete-Books"), this.ItemGridView.SelectedItems.Count),
                        Settings.GetRessource("AppTitle"));
                md.Commands.Add(new UICommand(Settings.GetRessource("Yes")));
                md.Commands.Add(new UICommand(Settings.GetRessource("No")));
                response = await md.ShowAsync();
            }

            if (response.Label == "Yes")
            {
                foreach (var selectedItem in this.ItemGridView.SelectedItems)
                {
                    var selectedBook = Settings.BookList.First(x => x.Id == ((BookDataItem)selectedItem).Id);
                    if (selectedBook != null)
                    {
                        Settings.BookList.Remove(selectedBook);

                        // Delete old image
                        if (selectedBook.ImagePath != null)
                        {
                            try
                            {
                                var deleteFile = selectedBook.ImagePath;
                                await Settings.DeleteImage(deleteFile);
                            }
                            catch (Exception)
                            {
                                // we don't care if the old image is not deleted
                            }
                        }
                    }
                }

                await Settings.SaveBookListToXml();
                await BookDataSource.FillData();
                var group = BookDataSource.GetGroup(this.dataGroup.GroupId);
                if (group != null)
                {
                    this.dataGroup = group;
                    this.DefaultViewModel["Group"] = group;
                    this.DefaultViewModel["Items"] = group.Items;
                    ItemGridView.SelectedIndex = -1;
                }
            }
            else
            {
                AppBar.IsOpen = true;
            }
        }

        private void SelectedItem(object sender, SelectionChangedEventArgs e)
        {
            if (ItemGridView.SelectedItems.Count == 1 && Window.Current.Bounds.Width > 500)
            {
                AppBarEditButton.Visibility = Visibility.Visible;
                AppBarDiscardButton.Visibility = Visibility.Visible;
                AppBar.IsSticky = true;
                AppBar.IsOpen = true;
            }
            else if (ItemGridView.SelectedItems.Count > 1 && Window.Current.Bounds.Width > 500)
            {
                AppBarEditButton.Visibility = Visibility.Collapsed;
                AppBarDiscardButton.Visibility = Visibility.Visible;
                AppBar.IsSticky = true;
                AppBar.IsOpen = true;
            }
            else
            {
                AppBarEditButton.Visibility = Visibility.Collapsed;
                AppBarDiscardButton.Visibility = Visibility.Collapsed;
                AppBar.IsSticky = false;
                AppBar.IsOpen = false;
            }
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
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}