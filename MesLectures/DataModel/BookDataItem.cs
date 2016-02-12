using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MesLectures.DataModel
{
    public class BookDataItem
    {
        private static readonly Uri baseUri = new Uri("ms-appx:///");

        private ImageSource imageSource;

        public BookDataItem()
        {
        }

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

        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string MyOpinion { get; set; }

        public string Summary { get; set; }

        public string LikeStars { get; set; }

        public string Story { get; set; }

        public DateTime Date { get; set; }

        public BookDataGroup Group { get; set; }

        public string Editor { get; set; }

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

        public string ImagePath { get; set; }
    }
}