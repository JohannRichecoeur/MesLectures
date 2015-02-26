using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ShareClass.API
{
    public class BingImage : Common.BindableBase
    {
        private readonly BitmapImage imageBitmapImage;
        private ImageSource imageSource;

        public BingImage(BitmapImage bitmapimage)
        {
            this.imageBitmapImage = bitmapimage;
        }

        public ImageSource Image
        {
            get
            {
                if (this.imageSource == null && this.imageBitmapImage != null)
                {
                    this.imageSource = this.imageBitmapImage;
                }

                return this.imageSource;
            }
        }
    }
}