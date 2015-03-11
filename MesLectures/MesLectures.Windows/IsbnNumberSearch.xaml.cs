using System;
using System.Threading.Tasks;
using ShareClass.ISBN;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace MesLectures
{
    public sealed partial class IsbnNumberSearch
    {
        public IsbnNumberSearch()
        {
            this.InitializeComponent();

            var bounds = Window.Current.Bounds;
            this.RootPanel.Width = bounds.Width;
            this.RootPanel.Height = bounds.Height;

            Settings.IsbnSearched = 0;
            Settings.IsbnBook = null;
            Settings.IsbnUpdate = false;

            this.HeaderTextBlock.Text = Settings.GetRessource("ISBNSearch_HeaderTitle") + ":";
            this.SearchButton.Content = Settings.GetRessource("SearchButton");
            this.CancelButton.Content = Settings.GetRessource("CancelButton");

            if (ISBNBox.Text != "")
            {
                try
                {
                    string isbn = ISBNBox.Text.Replace("-", "").Replace(" ", "");
                    Settings.IsbnSearched = long.Parse(isbn);
                }
                catch (Exception)
                {
                }
            }
        }

        public event EventHandler CloseIsbnRequested;

        private void AnnulerButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.CloseIsbnRequested != null)
            {
                this.CloseIsbnRequested(this, EventArgs.Empty);
            }
        }

        private async void RechercherButtonClick(object sender, RoutedEventArgs e)
        {
            errorMessageBox.Text = "";
            await this.Search();
            if (Settings.IsbnSearched != 0)
            {
                if (this.CloseIsbnRequested != null)
                {
                    this.CloseIsbnRequested(this, EventArgs.Empty);
                }
            }
        }

        private async Task Search()
        {
            Settings.IsbnBook = null;
            Settings.IsbnUpdate = false;
            try
            {
                string isbn = ISBNBox.Text.Replace("-", "").Replace(" ", "");
                Settings.IsbnSearched = long.Parse(isbn);
            }
            catch (Exception)
            {
                errorMessageBox.Foreground = new SolidColorBrush(Colors.Red);
                errorMessageBox.Text = Settings.GetRessource("ISBNSearch_InvalidISBN");
            }

            if (Settings.IsbnSearched != 0)
            {
                long isbn = Settings.IsbnSearched;
                errorMessageBox.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageBox.Text = Settings.GetRessource("ISBNSearch_Searching");

                try
                {
                    Settings.IsbnBook = await IsbnSearch.GetBookFromIsbn(isbn);
                }
                catch (Exception e)
                { }

                try
                {
                    if (Settings.IsbnBook != null)
                    {
                        Settings.IsbnBook.ImageLink = await GoogleResult.GetGoogleCover(isbn);
                    }
                }
                catch (Exception)
                { }

                if (Settings.IsbnBook == null || Settings.IsbnBook.Author == null)
                {
                    errorMessageBox.Foreground = new SolidColorBrush(Colors.Black);
                    errorMessageBox.Text = Settings.GetRessource("ISBNSearch_NoResults");
                    Settings.IsbnSearched = 0;
                }
            }
        }
    }
}