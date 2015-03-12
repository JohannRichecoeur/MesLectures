using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MesLectures.Common;
using MesLectures.DataModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    public sealed partial class EditionPage
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private static StorageFile localePictureFile;
        private Popup catalogPopup;
        private Popup isbnSearchPopup;
        private bool isNewBook;

        public EditionPage()
        {
            this.InitializeComponent();

            this.AppBarSaveButton.Label = Settings.GetRessource("AppBar_Save");
            this.AppBarCancelButton.Label = Settings.GetRessource("AppBar_Cancel");
            this.AppBarSearchButton.Label = Settings.GetRessource("AppBar_Search");

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelperLoadState;

            Window.Current.SizeChanged += this.WindowSizeChanged;
            this.SetDisplay();
        }

        public static Uri BingUri { get; set; }

        public static bool FromBing { get; set; }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }
        
        private void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            DatePickerForUser.Date = new DateTimeOffset(DateTime.Now);
            
            AppBar.IsOpen = true;
            AppBar.IsSticky = true;

            if (!FromBing)
            {
                Settings.CurrentBook = new Book() { Title = "", Author = "", Editor = "", Like = 1, MyOpinion = "", Summary = "", Story = "", Id = this.GetNewId() };
                this.isNewBook = true;

                if (e.NavigationParameter != null)
                {

                    foreach (Book b in Settings.BookList.Where(b => b.Id == (int)e.NavigationParameter))
                    {
                        Settings.CurrentBook = b;
                        break;
                    }

                    if (Settings.CurrentBook != null)
                    {
                        this.FillContent();
                        this.isNewBook = false;
                    }
                }
            }
            else
            {
                FromBing = false;
                this.FillBackBingContent();
                if (BingUri != null)
                {
                    ImageBox.Source = new BitmapImage(BingUri);
                    BingUri = null;
                }
                else
                {
                    ImageBox.Source = Settings.CurrentImage;
                }
            }

            this.TitleBox.FontStyle = this.TitleBox.Text == Settings.GetRessource("DetailBook_Title") ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
            this.AuthorBox.FontStyle = this.AuthorBox.Text == Settings.GetRessource("DetailBook_Author") ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
            this.EditorBox.FontStyle = this.EditorBox.Text == Settings.GetRessource("DetailBook_Editor") ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
            this.SummaryBox.FontStyle = this.SummaryBox.Text == Settings.GetRessource("DetailBook_Summary") ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
            this.MyOpinionBox.FontStyle = this.MyOpinionBox.Text == Settings.GetRessource("DetailBook_MyOpinion") ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
            this.StoryBox.FontStyle = this.StoryBox.Text == Settings.GetRessource("DetailBook_Story") ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;
        }

        private void SetDisplay()
        {
            this.LocalButton.Content = Settings.GetRessource("DetailBook_LocalImageBook");
            this.BingButton.Content = Settings.GetRessource("DetailBook_BingImageBook");
            this.TitleBox.Text = Settings.GetRessource("DetailBook_Title");
            this.AuthorBox.Text = Settings.GetRessource("DetailBook_Author");
            this.EditorBox.Text = Settings.GetRessource("DetailBook_Editor");
            this.SummaryBox.Text = Settings.GetRessource("DetailBook_Summary");
            this.MyOpinionBox.Text = Settings.GetRessource("DetailBook_MyOpinion");
            this.StoryBox.Text = Settings.GetRessource("DetailBook_Story");
            
            var pageHeight = Window.Current.Bounds.Height;
            var pageWidth = Window.Current.Bounds.Width;

            ImageBox.MaxHeight = pageHeight - 100;
            ImageBox.MaxWidth = pageWidth / 4;
            TitleBox.Margin = new Thickness(ImageBox.MaxWidth + 80, 80, 0, 0);
            AuthorBox.Margin = new Thickness(ImageBox.MaxWidth + 80, 120, 0, 0);
            EditorBox.Margin = new Thickness(ImageBox.MaxWidth + 80, 160, 0, 0);
            if (pageWidth - ImageBox.MaxWidth > 580)
            {
                TitleBox.Width = pageWidth - ImageBox.MaxWidth - 80 - 500;
                AuthorBox.Width = pageWidth - ImageBox.MaxWidth - 80 - 500;
                EditorBox.Width = pageWidth - ImageBox.MaxWidth - 80 - 500;
            }

            ImageEtoile1.Margin = new Thickness(0, 140, 220, 0);
            ImageEtoile2.Margin = new Thickness(0, 140, 180, 0);
            ImageEtoile3.Margin = new Thickness(0, 140, 140, 0);
            ImageEtoile4.Margin = new Thickness(0, 140, 100, 0);
            ImageEtoile5.Margin = new Thickness(0, 140, 60, 0);
            DatePickerForUser.Margin = new Thickness(0, 80, 60, 0);

            var h = pageHeight - 200;
            SummaryBox.Margin = new Thickness(ImageBox.MaxWidth + 80, 200, 0, 0);
            SummaryBox.Width = pageWidth - ImageBox.MaxWidth - 80 - 60;
            SummaryBox.Height = h / 6;

            MyOpinionBox.Margin = new Thickness(ImageBox.MaxWidth + 80, 200 + (h * 1 / 6) + 10, 0, 0);
            MyOpinionBox.Width = pageWidth - ImageBox.MaxWidth - 80 - 60;
            MyOpinionBox.Height = ((h / 6) * 2) - 10;

            StoryBox.Margin = new Thickness(ImageBox.MaxWidth + 80, 200 + (h * 1 / 2) + 10, 0, 0);
            StoryBox.Width = pageWidth - ImageBox.MaxWidth - 80 - 60;
            StoryBox.Height = (h * 0.5) - 50;
        }

        private void WindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // Obtain view state by explicitly querying for it
            var windowWidth = Window.Current.Bounds.Width;
            if (windowWidth < 500)
            {
                this.Frame.GoBack();
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.SavePicture();
                await this.SaveBook();

                // Reset page data
                BingUri = null;
                Settings.CurrentImage = null;
                Settings.CurrentBook = null;
                localePictureFile = null;
                this.Frame.GoBack();
            }
            catch (Exception)
            {
            }
        }

        private async void FillContent()
        {
            AppBar.IsOpen = true;
            AppBar.IsSticky = true;

            TitleBox.Text = Settings.CurrentBook.Title;
            AuthorBox.Text = Settings.CurrentBook.Author;
            EditorBox.Text = Settings.CurrentBook.Editor;
            this.ModifyStar(Settings.CurrentBook.Like.ToString());
            DatePickerForUser.Date = Settings.CurrentBook.Date;
            SummaryBox.Text = Settings.CurrentBook.Summary;
            MyOpinionBox.Text = Settings.CurrentBook.MyOpinion;
            StoryBox.Text = Settings.CurrentBook.Story;

            try
            {
                ImageBox.Source = await Settings.GetImageFromLocalFolder(Settings.CurrentBook.ImagePath);
            }
            catch (Exception)
            { }
        }

        private void FillBackBingContent()
        {
            AppBar.IsOpen = true;
            AppBar.IsSticky = true;

            if (Settings.CurrentBook != null)
            {
                TitleBox.Text = Settings.CurrentBook.Title;
                AuthorBox.Text = Settings.CurrentBook.Author;
                EditorBox.Text = Settings.CurrentBook.Editor;
                this.ModifyStar(Settings.CurrentBook.Like.ToString());
                DatePickerForUser.Date = Settings.CurrentBook.Date;
                SummaryBox.Text = Settings.CurrentBook.Summary;
                MyOpinionBox.Text = Settings.CurrentBook.MyOpinion;
                StoryBox.Text = Settings.CurrentBook.Story;
            }
        }

        private async void LocalButtonClick(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            localePictureFile = await openPicker.PickSingleFileAsync();

            if (localePictureFile != null)
            {
                IRandomAccessStream fileStream = await localePictureFile.OpenAsync(FileAccessMode.Read);
                var image = new BitmapImage();
                image.SetSource(fileStream);
                ImageBox.Source = image;
            }
        }

        private async void BingButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.CurrentBook.Title = TitleBox.Text;
            Settings.CurrentBook.Author = AuthorBox.Text;
            Settings.CurrentBook.Editor = EditorBox.Text;
            Settings.CurrentBook.Date = new DateTime(this.DatePickerForUser.Date.Year, this.DatePickerForUser.Date.Month, this.DatePickerForUser.Date.Day);
            Settings.CurrentBook.Summary = SummaryBox.Text;
            Settings.CurrentBook.MyOpinion = MyOpinionBox.Text;
            Settings.CurrentBook.Story = StoryBox.Text;
            Settings.CurrentImage = ImageBox.Source;
            string search = TitleBox.Text + " " + AuthorBox.Text + " " + EditorBox.Text;
            this.Frame.Navigate(typeof(BingImages), search);
        }

        private async Task SaveBook()
        {
            Settings.CurrentBook.Title = TitleBox.Text;
            Settings.CurrentBook.Author = AuthorBox.Text;
            Settings.CurrentBook.Editor = EditorBox.Text;
            Settings.CurrentBook.Like = Settings.CurrentBook.Like;
            Settings.CurrentBook.Date = new DateTime(DatePickerForUser.Date.Year, DatePickerForUser.Date.Month, DatePickerForUser.Date.Day);
            Settings.CurrentBook.Summary = SummaryBox.Text;
            Settings.CurrentBook.MyOpinion = MyOpinionBox.Text;
            Settings.CurrentBook.Story = StoryBox.Text;

            // Existing book => remove the existing one and add the new one
            if (!this.isNewBook && Settings.BookList != null)
            {
                Book toDelete;
                try
                {
                    toDelete = Settings.BookList.First(x => x.Id == Settings.CurrentBook.Id);
                }
                catch
                {
                    toDelete = null;
                }

                if (toDelete != null)
                {
                    Settings.BookList.Remove(toDelete);
                }

                // Add the book to the list and save it
                Settings.BookList.Add(Settings.CurrentBook);
                await Settings.SaveBookListToXml();
            }
            else
            {
                // Check the new book doesn't already exists
                if (Settings.BookList == null || Settings.BookList.All(b => b.Id != Settings.CurrentBook.Id))
                {
                    // Add the book to the list and save it
                    if (Settings.BookList == null)
                    {
                        Settings.BookList = new List<Book>();
                    }

                    Settings.BookList.Add(Settings.CurrentBook);
                    await Settings.SaveBookListToXml();
                }
                else
                {
                    var md = new MessageDialog(Settings.GetRessource("CreateBook_Duplicate"), Settings.GetRessource("AppTitle"));
                    md.Commands.Add(new UICommand("OK"));
                    await md.ShowAsync();
                }
            }
        }

        private int GetNewId()
        {
            if (Settings.BookList == null || Settings.BookList.Count == 0)
            {
                Settings.BookList = new List<Book>();
                return 1;
            }

            return Settings.BookList.Select(book => book.Id).Max() + 1;
        }

        private async Task SavePicture()
        {
            var image = (BitmapImage)this.ImageBox.Source;
            var source = image.UriSource;

            string initialImagePath = Settings.CurrentBook.ImagePath;
            string newImagePath = Settings.CurrentBook.ImagePath;

            // Save local image
            if (source == null && localePictureFile != null && Settings.CurrentBook != null)
            {
                // Save the new one
                string extension = localePictureFile.FileType;
                string number = Settings.GetRandomNumber();
                string imageName = Settings.CurrentBook.Id + "-" + number + extension;
                Settings.CurrentBook.ImagePath = imageName;
                await Settings.SaveImage(localePictureFile, imageName);
                newImagePath = Settings.CurrentBook.ImagePath;
            }
            else if (source != null && source.ToString().StartsWith("http://") && Settings.CurrentBook != null)
            {
                // Save the BING image
                string uri = source.ToString();
                string extension = uri.Remove(0, uri.Length - 4);
                string number = Settings.GetRandomNumber();
                string imageName = Settings.CurrentBook.Id + "-" + number + extension;
                Settings.CurrentBook.ImagePath = imageName;
                await Settings.SaveImage(source, imageName);
                newImagePath = Settings.CurrentBook.ImagePath;
            }
            else if (Settings.CurrentBook != null && Settings.CurrentBook.ImagePath == null)
            {
                Settings.CurrentBook.ImagePath = "";
            }

            // Delete old image
            if (initialImagePath != newImagePath)
            {
                try
                {
                    await Settings.DeleteImage(initialImagePath);
                }
                catch (Exception)
                {
                }
            }

            await BookDataSource.FillData();
        }

        private void EtoileTapped(object sender, TappedRoutedEventArgs e)
        {
            this.ModifyStar(((Image)e.OriginalSource).Name.Replace("ImageEtoile", ""));
        }

        private void ModifyStar(string number)
        {
            var full = int.Parse(number);
            var imageList = new List<Image> { ImageEtoile1, ImageEtoile2, ImageEtoile3, ImageEtoile4, ImageEtoile5 };

            int counter = 0;
            foreach (Image image in imageList)
            {
                image.Source = counter < full
                    ? new BitmapImage(new Uri(this.BaseUri, @"Assets/etoile.jpg"))
                    : new BitmapImage(new Uri(this.BaseUri, @"Assets/etoileContour.jpg"));
                counter++;
            }

            Settings.CurrentBook.Like = full;
        }

        private void TitleBoxFocusChange(object sender, RoutedEventArgs e)
        {
            this.FocusChange(TitleBox, Settings.GetRessource("DetailBook_Title"));
        }

        private void AuthorBoxFocusChange(object sender, RoutedEventArgs e)
        {
            this.FocusChange(this.AuthorBox, Settings.GetRessource("DetailBook_Author"));
        }

        private void EditorBoxFocusChange(object sender, RoutedEventArgs e)
        {
            this.FocusChange(EditorBox, Settings.GetRessource("DetailBook_Editor"));
        }

        private void SummaryBoxFocusChange(object sender, RoutedEventArgs e)
        {
            this.FocusChange(SummaryBox, Settings.GetRessource("DetailBook_Summary"));
        }

        private void MyOpinionBoxFocusChange(object sender, RoutedEventArgs e)
        {
            this.FocusChange(MyOpinionBox, Settings.GetRessource("DetailBook_MyOpinion"));
        }

        private void StoryBoxFocusChange(object sender, RoutedEventArgs e)
        {
            this.FocusChange(StoryBox, Settings.GetRessource("DetailBook_Story"));
        }

        private void FocusChange(TextBox textBox, string value)
        {
            if (textBox.Text == value)
            {
                textBox.Text = "";
                textBox.FontStyle = Windows.UI.Text.FontStyle.Normal;
            }
            else if (textBox.Text == "")
            {
                textBox.Text = value;
                textBox.FontStyle = Windows.UI.Text.FontStyle.Normal;
            }
        }

        private void DialogCloseRequested(object sender, EventArgs e)
        {
            this.catalogPopup.IsOpen = false;
            if (Settings.IsbnUpdate)
            {
                Settings.CurrentBook.Editor = Settings.IsbnBook.Editor;

                TitleBox.Text = Settings.IsbnBook.Title;
                TitleBox.FontStyle = Windows.UI.Text.FontStyle.Normal;

                AuthorBox.Text = Settings.IsbnBook.Author;
                AuthorBox.FontStyle = Windows.UI.Text.FontStyle.Normal;

                if (Settings.IsbnBook.ImageLink != null)
                {
                    ImageBox.Source = new BitmapImage(new Uri(Settings.IsbnBook.ImageLink));
                }
            }
        }

        private async void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            var isbnNumberSearch = new IsbnNumberSearch();
                isbnNumberSearch.CloseIsbnRequested += this.IsbnSearchCloseRequested;
                this.isbnSearchPopup = new Popup
                {
                    Child = isbnNumberSearch,
                    IsOpen = true
                };
        }

        private void IsbnSearchCloseRequested(object sender, EventArgs e)
        {
            this.isbnSearchPopup.IsOpen = false;
            if (Settings.IsbnSearched != 0)
            {
                var dialog = new IsbnDialog();
                dialog.CloseRequested += this.DialogCloseRequested;
                this.catalogPopup = new Popup
                                        {
                                            Child = dialog,
                                            IsOpen = true
                                        };
            }
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