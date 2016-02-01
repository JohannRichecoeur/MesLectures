using System;
using Windows.UI.Xaml;

namespace MesLectures
{
    public sealed partial class IsbnDialog
    {
        public IsbnDialog()
        {
            this.InitializeComponent();

            var bounds = Window.Current.Bounds;
            this.RootPanel.Width = bounds.Width;
            this.RootPanel.Height = bounds.Height;
            this.DisplayData();
        }

        public event EventHandler CloseRequested;

        private void AnnulerButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.CloseRequested != null)
            {
                this.CloseRequested(this, EventArgs.Empty);
            }
        }

        private void UtiliserButtonClick(object sender, RoutedEventArgs e)
        {
            Settings.IsbnUpdate = true;
            if (this.CloseRequested != null)
            {
                this.CloseRequested(this, EventArgs.Empty);
            }
        }

        private void DisplayData()
        {
            this.HeaderTextBlock.Text = Settings.GetRessource("ISBNResult_HeaderTitle") + ":";
            this.UseButton.Content = Settings.GetRessource("ISBNResult_UseButton");
            this.CancelButton.Content = Settings.GetRessource("CancelButton");

            if (Settings.IsbnBook != null)
            {
                if (Settings.IsbnBook.ImageLink != null)
                {
                    coverPicture.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(Settings.IsbnBook.ImageLink));
                }

                TitleTextBlock.Text = Settings.IsbnBook.Title;
                AuthorTextBlock.Text = Settings.IsbnBook.Author;
            }
        }
    }
}