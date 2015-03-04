using System;
using System.Globalization;
using System.Linq;
using MesLectures.Common;
using MesLectures.DataModel;
using ShareClass;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    public sealed partial class ItemDetailPage
    {
        private static TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler;
        private static double leftColumWidth;

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemDetailPage()
        {
            this.InitializeComponent();
            this.AppBarAddButton.SetValue(AutomationProperties.NameProperty, Settings.GetRessource("AppBar_Add"));
            this.AppBarEditButton.SetValue(AutomationProperties.NameProperty, Settings.GetRessource("AppBar_Edit"));
            this.AppBarDiscardButton.SetValue(AutomationProperties.NameProperty, Settings.GetRessource("AppBar_Delete"));

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelperLoadState;

            // Sharing data
            //if (handler != null)
            //{
            //    DataTransferManager.GetForCurrentView().DataRequested -= handler;
            //}

            //handler = this.ShareItem;
            //DataTransferManager.GetForCurrentView().DataRequested += handler;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            await BookDataSource.FillData();

            if (Settings.ComingFromSearch == true)
            {
                Settings.ComingFromSearch = false;
                BookDataItem item = BookDataSource.GetItemFromSearch((int)e.NavigationParameter);
                this.DefaultViewModel["Group"] = BookDataSource.SearchDataGroup;
                this.DefaultViewModel["Items"] = BookDataSource.SearchDataGroup.Items;
                this.FlipView.SelectedItem = item;
            }
            else
            {
                var parameters = ((string)e.NavigationParameter).Split('-');
                var item = BookDataSource.GetItem(short.Parse(parameters[0]), short.Parse(parameters[1]));
                this.DefaultViewModel["Group"] = item.Group;
                this.DefaultViewModel["Items"] = item.Group.Items;
                this.FlipView.SelectedItem = item;
            }
        }

        private async void ButtonEditClick(object sender, RoutedEventArgs e)
        {
            var windowWidth = Window.Current.Bounds.Width;
            if (windowWidth > 500)
            {
                var selectedItem = (BookDataItem)this.FlipView.SelectedItem;
                if (selectedItem != null)
                {
                    this.Frame.Navigate(typeof(EditionPage), selectedItem.Id);
                }
            }
            else
            {
                var md = new MessageDialog(Settings.GetRessource("Windows_IncreaseSize"));
                md.Commands.Add(new UICommand("OK"));
                await md.ShowAsync();
            }
        }

        private void FlipViewSelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            // Set the title of the page
            if (FlipView.SelectedItem != null)
            {
                PageTitle.Text = ((BookDataItem)FlipView.SelectedItem).Title;
            }
        }

        private async void ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            var bookDataItem = (BookDataItem)this.FlipView.SelectedItem;
            if (bookDataItem != null)
            {
                var md = new MessageDialog(
                    string.Format(Settings.GetRessource("Delete-Book"), bookDataItem.Title, bookDataItem.Author),
                            Settings.GetRessource("AppTitle"));
                md.Commands.Add(new UICommand(Settings.GetRessource("Yes")));
                md.Commands.Add(new UICommand(Settings.GetRessource("No")));
                var response = await md.ShowAsync();

                if (response.Label == "Yes")
                {
                    var selectedBook = new Book();
                    foreach (Book b in Settings.BookList)
                    {
                        if (b.Author == bookDataItem.Author && b.Title == bookDataItem.Title && b.Editor == bookDataItem.Editor)
                        {
                            selectedBook = b;
                            break;
                        }
                    }

                    Settings.BookList.Remove(selectedBook);
                    await Settings.SaveBookListToXml();

                    // Delete old image
                    if (selectedBook.ImagePath != null)
                    {
                        try
                        {
                            string deleteFile = selectedBook.ImagePath;
                            await Settings.DeleteImage(deleteFile);
                        }
                        catch (Exception)
                        { }
                    }

                    this.Frame.GoBack();
                }
            }
        }

        private async void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            var windowWidth = Window.Current.Bounds.Width;
            if (windowWidth > 500)
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

        private void ShareItem(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            if (FlipView.SelectedItem != null)
            {
                request.Data.Properties.Title = ((BookDataItem)FlipView.SelectedItem).Title + " - " + Settings.GetRessource("AppTitle");
                request.Data.Properties.Description = ((BookDataItem)FlipView.SelectedItem).Author;
                var localImage = "ms-appdata:///local/"
                                    + ((BitmapImage)((BookDataItem)this.FlipView.SelectedItem).Image).UriSource.ToString().Split('/').Last();
                var htmlExample = "<strong>" + ((BookDataItem)FlipView.SelectedItem).Title + ", "
                                     + ((BookDataItem)FlipView.SelectedItem).Author
                                     + "</strong><BR><BR><em>" + Settings.GetRessource("SharedBook_Editor") + "</em>"
                                     + ((BookDataItem)FlipView.SelectedItem).Editor + "<BR><em>" + Settings.GetRessource("SharedBook_ReadingDate") + "</em>"
                                     + ((BookDataItem)FlipView.SelectedItem).Date.ToString("MMMM yyyy", new CultureInfo(Settings.GetRessource("Locale")))
                                     + "<BR><BR><em>" + Settings.GetRessource("SharedBook_Summary") + "</em>" + ((BookDataItem)FlipView.SelectedItem).Summary
                                     + "<BR><BR><em>" + Settings.GetRessource("SharedBook_MyOpinion") + "</em>" + ((BookDataItem)FlipView.SelectedItem).MyOpinion
                                     + "<BR><BR><em>" + Settings.GetRessource("SharedBook_Story") + "</em>" + ((BookDataItem)FlipView.SelectedItem).Story
                                     + "<BR><BR><img src=" + localImage.Replace("\"\"", "\"") + "><BR>";
                var htmlFormat = HtmlFormatHelper.CreateHtmlFormat(htmlExample);
                request.Data.SetHtmlFormat(htmlFormat);

                // Because the HTML contains a local image, we need to add it to the ResourceMap.
                RandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromUri(new Uri(localImage));
                request.Data.ResourceMap[localImage] = streamRef;
            }
        }

        private void OtherColumn_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((RichTextBlockOverflow)sender).Width = Window.Current.Bounds.Width - leftColumWidth - 280;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            leftColumWidth = ((Image)sender).ActualWidth;
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            this.navigationHelper.GoBack();
        }


        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    
    }
}