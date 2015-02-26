using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using ZXing;

namespace ShareClass.ISBN
{
    public class IsbnSearch
    {
        public static async Task<long> GetIsbnFromPicture()
        {
            var deviceInformation = (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)).FirstOrDefault();
            var mediaCapture = new MediaCapture();

            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
                {
                    AudioDeviceId = "",
                    VideoDeviceId = deviceInformation.Id,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    PhotoCaptureSource = PhotoCaptureSource.VideoPreview,
                });

            var imgFormat = ImageEncodingProperties.CreateJpeg();
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("preview.jpg", CreationCollisionOption.ReplaceExisting);
            await mediaCapture.CapturePhotoToStorageFileAsync(imgFormat, file);

            var localFiles = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            var photo = localFiles.FirstOrDefault(x => x.Name == "preview.jpg");
            
            if (photo != null)
            {
                var reader = new BarcodeReader();
                var wrb = new WriteableBitmap(1, 1);
                using (IRandomAccessStream fileStream = await photo.OpenAsync(FileAccessMode.Read))
                {
                    wrb.SetSource(fileStream);
                }

                try
                {
                    return long.Parse(reader.Decode(wrb).Text);
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            return 0;
        }

        public static async Task<IsbnBook> GetBookFromIsbn(long isbn)
        {
            string url = "http://isbndb.com/api/books.xml?access_key=ORHLN5OK&index1=isbn&value1=" + isbn;
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            var xDoc = XDocument.Parse(await response.Content.ReadAsStringAsync(), LoadOptions.None);
            var isbnBook = new IsbnBook();
            foreach (XElement book in xDoc.Descendants("BookData"))
            {
                isbnBook = new IsbnBook()
                {
                    Title = book.Descendants("Title").ToList()[0].Value,
                    Author = book.Descendants("AuthorsText").ToList()[0].Value,
                    Editor = book.Descendants("PublisherText").ToList()[0].Value,
                };

                isbnBook.Title = isbnBook.Title[0].ToString().ToUpper() + isbnBook.Title.Substring(1).ToLower();
                isbnBook.Author = ToTitleCase(isbnBook.Author);
                isbnBook.Editor = isbnBook.Editor.ToUpper();
                break;
            }

            return isbnBook;
        }

        private static string ToTitleCase(string value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length == 0)
            {
                return value;
            }

            var result = new StringBuilder(value);
            result[0] = char.ToUpper(result[0]);
            for (int i = 1; i < result.Length; ++i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                {
                    result[i] = char.ToUpper(result[i]);
                }
                else
                {
                    result[i] = char.ToLower(result[i]);
                }
            }

            return result.ToString();
        }
    }
}