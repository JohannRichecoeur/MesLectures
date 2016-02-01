using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MesLectures.DataModel
{
    public class BookDataItem
    {
        private static readonly Uri baseUri = new Uri("ms-appx:///");

        private ImageSource imageSource;

        public BookDataItem(int id, string title, string author, string imagePath, string summary, string likeStars, string editor, string myOpinion, string story, DateTime date, BookDataGroup group)
        {
            this.Id = id;
            this.Title = title;
            this.Author = author;
            this.Summary = summary;
            this.MyOpinion = myOpinion;
            this.ImagePath = imagePath;
            this.Group = group;
            this.LikeStars = likeStars;
            this.Editor = editor;
            this.Story = story;
            this.Date = date;
        }

        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Author { get; private set; }

        public string MyOpinion { get; private set; }

        public string Summary { get; private set; }

        public string LikeStars { get; private set; }

        public string Story { get; private set; }

        public DateTime Date { get; private set; }

        public BookDataGroup Group { get; private set; }

        public string Editor { get; private set; }

        public ImageSource Image
        {
            get
            {
                if (this.imageSource == null && this.ImagePath != null)
                {
                    this.imageSource = new BitmapImage(new Uri(baseUri, this.ImagePath));
                }

                return this.imageSource;
            }
        }

        private string ImagePath { get; set; }
    }
}