using Bing;
using MesLectures.API;
using MesLectures.DataModel;
using MesLectures.ISBN;
using Microsoft.OneDrive.Sdk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MesLectures
{
    public static class Settings
    {
        public static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        private const string DataFileName = "data.txt";
        private const string ZipFileName = "pictures.zip";

        // boolean used for old users who could personalized on which folder on OneDrive data were saved
        private static bool isCustomOnedriveFolderUsed = false;

        private static IOneDriveClient _client;

        public static List<BookDataItem> BookList { get; set; }

        public static bool ComingFromSearch { get; set; }

        public static string CurrentSearch { get; set; }

        public static BookDataItem CurrentBook { get; set; }

        public static ImageSource CurrentImage { get; set; }

        public static long IsbnSearched { get; set; }

        public static IsbnBook IsbnBook { get; set; }

        public static bool IsbnUpdate { get; set; }

        public static string UserName { get; set; }

        public static string UserPictureUrl { get; set; }

        public static InfosClass UserDataInfos { get; set; }

        public static void SetLocalSettings(LocalSettingsValue settings, object value)
        {
            LocalSettings.Values[settings.ToString()] = value;
        }

        public static object GetLocalSettings(LocalSettingsValue settings)
        {
            return DoesLocalSettingsExists(settings) ? LocalSettings.Values[settings.ToString()] : null;
        }

        private static bool DoesLocalSettingsExists(LocalSettingsValue settings)
        {
            if (LocalSettings.Values[settings.ToString()] == null)
            {
                return false;
            }

            return true;
        }
        public static async Task SaveBookListToXml()
        {
            var xmlBook = CreateBookXml(BookList.OrderBy(i => i.Id).ToList());
            await CreateXmlFile(DataFileName, xmlBook);
        }

        public static bool CanLogout()
        {
            return _client.IsAuthenticated;
        }

        public static string GetRessource(string ressourceName)
        {
            return Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(ressourceName);
        }

        public static XDocument CreateUriXml(Dictionary<string, List<Uri>> uriDico)
        {
            var xDoc = new XDocument();
            xDoc.Declaration = new XDeclaration("1.0", "utf-8", null);
            var root = new XElement("searchItems");
            xDoc.Add(root);
            foreach (var entry in uriDico)
            {
                var parentNode = new XElement("item");
                root.Add(parentNode);

                int urlCount = entry.Value.Count;
                var nodeList = new List<XElement>();
                var nodeTextList = new List<XText>();
                nodeList.Add(new XElement("SearchItem"));
                nodeTextList.Add(new XText(entry.Key));

                for (int i = 0; i < urlCount; i++)
                {
                    nodeList.Add(new XElement("uri"));
                    nodeTextList.Add(new XText(entry.Value[i].ToString()));
                }

                for (int i = 0; i < nodeList.Count; i++)
                {
                    parentNode.Add(nodeList[i]);
                    nodeList[i].Add(nodeTextList[i]);
                }
            }

            return xDoc;
        }

        public static async Task CreateXmlFile(string fileName, XDocument xDoc)
        {
            StorageFile sampleFile = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            string xml = xDoc.ToString();
            await FileIO.WriteTextAsync(sampleFile, xml);
        }

        public static async Task<List<BookDataItem>> ReadFileForBookList(StorageFile storageFile = null)
        {
            try
            {
                if (storageFile == null)
                {
                    storageFile = await LocalFolder.GetFileAsync(DataFileName);
                }

                string xmlString = await FileIO.ReadTextAsync(storageFile);
                return GetBookListXdoc(XDocument.Parse(xmlString));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<InfosClass> ReadFileForInfos(StorageFile storageFile = null)
        {
            try
            {
                if (storageFile == null)
                {
                    storageFile = await LocalFolder.GetFileAsync(DataFileName);
                }

                var infosData = XDocument.Parse(await FileIO.ReadTextAsync(storageFile)).Descendants("Infos").First();
                return new InfosClass()
                {
                    Number = int.Parse(infosData.Descendants("Number").ToList()[0].Value),
                    LastUpdateDate = DateTime.Parse(infosData.Descendants("Date").ToList()[0].Value, new CultureInfo("en-US"))
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task<Dictionary<string, List<Uri>>> GetDicoFromFilename(string fileName)
        {
            try
            {
                StorageFile sampleFile = await LocalFolder.GetFileAsync(fileName);
                string xmlString = await FileIO.ReadTextAsync(sampleFile);
                return GetDicoFromXdoc(XDocument.Parse(xmlString));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task SaveImage(Uri adress, string fileName)
        {
            var response = await WebRequest.Create(adress).GetResponseAsync();
            var allBytes = new List<byte>();
            using (Stream imageStream = response.GetResponseStream())
            {
                var buffer = new byte[4000];
                int bytesRead = 0;
                while ((bytesRead = await imageStream.ReadAsync(buffer, 0, 4000)) > 0)
                {
                    allBytes.AddRange(buffer.Take(bytesRead));
                }
            }

            var file = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, allBytes.ToArray());
        }

        public static async Task Signout()
        {
            await _client.SignOutAsync();
        }

        public static async Task SaveImage(StorageFile imageFile, string fileName)
        {
            var allBytes = new List<byte>();
            using (Stream imageStream = await imageFile.OpenStreamForWriteAsync())
            {
                var buffer = new byte[4000];
                int bytesRead = 0;
                while ((bytesRead = await imageStream.ReadAsync(buffer, 0, 4000)) > 0)
                {
                    allBytes.AddRange(buffer.Take(bytesRead));
                }

                var file = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, allBytes.ToArray());
            }
        }

        public static async Task DeleteImage(string fileName)
        {
            StorageFile file = await LocalFolder.GetFileAsync(fileName);
            await file.DeleteAsync();
        }

        public static async Task<BitmapImage> GetImageFromLocalFolder(string fileName)
        {
            StorageFile file = await LocalFolder.GetFileAsync(fileName);
            if (file != null)
            {
                var fileStream = await file.OpenAsync(FileAccessMode.Read);
                var image = new BitmapImage();
                image.SetSource(fileStream);
                return image;
            }

            return null;
        }

        public static async Task<List<Uri>> GetBingResults(string search, int resultsNumber)
        {
            var bingImageUriList = new List<Uri>();
            var bingContainer = new BingSearchContainer(new Uri("https://api.datamarket.azure.com/Bing/Search/"));
            const string AccountKey = "5UU52xWmxVX5oL+Dn62rbFJHPQPN+kOZ/ueIuVPb3Qk=";
            bingContainer.Credentials = new NetworkCredential(AccountKey, AccountKey);
            var imageQuery = bingContainer.Image(search, Settings.GetRessource("Locale"), null, null, null, null);
            var results = await imageQuery.QueryAsync();
            var imageResults = results.ToList();

            if (imageResults != null)
            {
                foreach (var result in imageResults)
                {
                    bingImageUriList.Add(new Uri(result.MediaUrl));
                    if (bingImageUriList.Count == resultsNumber) { break; }
                }
            }

            return bingImageUriList;
        }

        public static string GetRandomNumber()
        {
            var random = new Random();
            return random.Next().ToString();
        }

        public static bool IsUserSignIn()
        {
            return _client != null && _client.IsAuthenticated;
        }

        private static async Task<string> GetToken()
        {
            // Initialize Graph
            var scopes = new[]
                {
                      "onedrive.readwrite",
                      "onedrive.appfolder",
                      "wl.signin"
                    };

            // Sign in
            _client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes);
            var session = await _client.AuthenticateAsync();
            return session.AccessToken;
        }

        private static async Task<XDocument> GetXMLFile()
        {
            Stream contentStream;
            try
            {
                IItemRequestBuilder builder = _client.Drive.Special.AppRoot.ItemWithPath($"{DataFileName}");
                contentStream = await builder.Content.Request().GetAsync();
                isCustomOnedriveFolderUsed = false;
            }
            catch
            {
                try
                {
                    IItemRequestBuilder builder = _client.Drive.Root.ItemWithPath($"{getOneDriveFolderName()}/{DataFileName}");
                    contentStream = await builder.Content.Request().GetAsync();
                    isCustomOnedriveFolderUsed = true;
                }
                catch
                {
                    return null;
                }
            }

            using (var reader = new StreamReader(contentStream))
            {
                var result = reader.ReadToEnd();
                return XDocument.Parse(result);
            }
        }

        public static async Task<bool> GetOnedriveUserInfo()
        {
            try
            {
                string accessToken = await GetToken();

                // Get the profile info of the user.
                var uri = new Uri($"https://apis.live.net/v5.0/me?access_token={accessToken}");
                var httpClient = new System.Net.Http.HttpClient();
                var result = await httpClient.GetAsync(uri);
                string jsonUserInfo = await result.Content.ReadAsStringAsync();
                if (jsonUserInfo != null)
                {
                    var json = Newtonsoft.Json.Linq.JObject.Parse(jsonUserInfo);
                    UserName = json["name"].ToString();
                }


                // Get proficle picture
                string imgDataURL = "";
                if (!String.IsNullOrEmpty(imgDataURL))
                {
                    // UserPictureUrl = imgDataURL;
                }

                // Get the infos
                await GetOneDriveDataUserInfos();


                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }



        public static async Task UploadToOnedrive(bool uploadPictures, TextBlock uploadDownloadTextBlock)
        {
            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Preparing");

            StorageFile dataTextFile = await LocalFolder.GetFileAsync(DataFileName);
            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Upload_Ongoing");

            using (var stream = await dataTextFile.OpenStreamForReadAsync())
            {
                // Upload the data file
                await _client.Drive.Special.AppRoot
                  .ItemWithPath(DataFileName)
                  .Content.Request()
                  .PutAsync<Item>(stream);
                isCustomOnedriveFolderUsed = false;
            }
            if (uploadPictures)
            {
                // Copy all the images
                var tempFolder = await LocalFolder.CreateFolderAsync("temp", CreationCollisionOption.ReplaceExisting);
                foreach (var storageFile in await LocalFolder.GetFilesAsync())
                {
                    if (storageFile.FileType == ".jpg" || storageFile.FileType == ".png" || storageFile.FileType == ".gif")
                    {
                        await storageFile.CopyAsync(tempFolder);
                    }
                }

                if (await LocalFolder.TryGetItemAsync(ZipFileName) != null)
                {
                    var oldZipFile = await LocalFolder.GetFileAsync(ZipFileName);
                    await oldZipFile.DeleteAsync();
                }

                ZipFile.CreateFromDirectory(tempFolder.Path, LocalFolder.Path + "\\" + ZipFileName);
                StorageFile zipFile = await LocalFolder.GetFileAsync(ZipFileName);

                using (var zipStream = await zipFile.OpenStreamForReadAsync())
                {
                    // Upload files
                    await _client.Drive.Special.AppRoot
                      .ItemWithPath(ZipFileName)
                      .Content.Request()
                      .PutAsync<Item>(zipStream);
                }

                // Delete temp items
                await tempFolder.DeleteAsync();
                await zipFile.DeleteAsync();
            }

            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Finishing");

            // Update the user infos
            await GetOneDriveDataUserInfos();
            uploadDownloadTextBlock.Text = "";
        }

        public static async Task DownloadFromOnedrive(bool uploadPictures, TextBlock uploadDownloadTextBlock)
        {
            uploadDownloadTextBlock.Text = GetRessource("OneDrivePage_Transfer_Preparing");

            IItemRequestBuilder builder = isCustomOnedriveFolderUsed ?
                _client.Drive.Root.ItemWithPath($"{getOneDriveFolderName()}/{DataFileName}") :
                _client.Drive.Special.AppRoot.ItemWithPath(DataFileName);

            uploadDownloadTextBlock.Text = GetRessource("OneDrivePage_Download_Ongoing");
            var dataFile = await LocalFolder.CreateFileAsync(DataFileName, CreationCollisionOption.ReplaceExisting);
            using (var downloadStream = await builder.Content.Request().GetAsync())
            {
                using (var downloadMemoryStream = new MemoryStream())
                {
                    // Get the data file
                    await downloadStream.CopyToAsync(downloadMemoryStream);
                    var fileBytes = downloadMemoryStream.ToArray();
                    await FileIO.WriteBytesAsync(dataFile, fileBytes);
                }
            }

            await ReadFileForBookList(dataFile);

            if (uploadPictures)
            {
                // Download pictures
                var zipFile = await LocalFolder.CreateFileAsync(ZipFileName, CreationCollisionOption.ReplaceExisting);
                IItemRequestBuilder zipFileBuilder = isCustomOnedriveFolderUsed ?
                    _client.Drive.Root.ItemWithPath($"{getOneDriveFolderName()}/{ZipFileName}") :
                    _client.Drive.Special.AppRoot.ItemWithPath(ZipFileName);

                using (var downloadStream = await zipFileBuilder.Content.Request().GetAsync())
                {
                    using (var downloadMemoryStream = new MemoryStream())
                    {
                        await downloadStream.CopyToAsync(downloadMemoryStream);
                        var fileBytes = downloadMemoryStream.ToArray();
                        await FileIO.WriteBytesAsync(zipFile, fileBytes);
                    }
                }

                var tempFolder = await LocalFolder.CreateFolderAsync("temp", CreationCollisionOption.ReplaceExisting);
                ZipFile.ExtractToDirectory(LocalFolder.Path + "\\" + ZipFileName, tempFolder.Path);
                // Copy all the images
                foreach (var storageFile in await tempFolder.GetFilesAsync())
                {
                    if (storageFile.FileType == ".jpg" || storageFile.FileType == ".png" || storageFile.FileType == ".gif")
                    {
                        await storageFile.CopyAsync(LocalFolder, storageFile.DisplayName + storageFile.FileType, NameCollisionOption.ReplaceExisting);
                    }
                }

                // Delete temp items
                await tempFolder.DeleteAsync();
                await zipFile.DeleteAsync();
            }

            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Finishing");
            uploadDownloadTextBlock.Text = "";
        }

        private static List<BookDataItem> GetBookListXdoc(XDocument xDoc)
        {
            // Check if the data.xml file contains "Infos"
            var oldVersion = !xDoc.Descendants("Infos").Any();

            var bookList = new List<BookDataItem>();
            foreach (var book in xDoc.Descendants("Book"))
            {
                try
                {
                    DateTime date;
                    if (oldVersion)
                    {
                        var success = DateTime.TryParse(book.Descendants("Date").ToList()[0].Value, new CultureInfo(GetRessource("Locale")), DateTimeStyles.None, out date);
                        if (!success)
                        {
                            success = DateTime.TryParse(book.Descendants("Date").ToList()[0].Value, new CultureInfo("en-US"), DateTimeStyles.None, out date);
                        }
                        if (!success)
                        {
                            date = DateTime.Now;
                        }
                    }
                    else
                    {
                        var success = DateTime.TryParse(book.Descendants("Date").ToList()[0].Value, new CultureInfo("en-US"), DateTimeStyles.None, out date);
                        if (!success)
                        {
                            date = DateTime.Now;
                        }
                    }
                    bookList.Add(
                                 new BookDataItem()
                                 {
                                     Title = book.Descendants("Title").ToList()[0].Value,
                                     Author = book.Descendants("Author").ToList()[0].Value,
                                     Editor = book.Descendants("Editor").ToList()[0].Value,
                                     LikeStars = book.Descendants("Like").ToList()[0].Value,
                                     Date = date,
                                     Summary = book.Descendants("Summary").ToList()[0].Value,
                                     Story = book.Descendants("Story").ToList()[0].Value,
                                     Id = int.Parse(book.Descendants("Id").ToList()[0].Value),
                                     MyOpinion = book.Descendants("MyOpinion").ToList()[0].Value,
                                     ImagePath = book.Descendants("ImagePath").ToList()[0].Value,
                                 });
                }
                catch (Exception)
                {
                }
            }

            return bookList;
        }

        private static Dictionary<string, List<Uri>> GetDicoFromXdoc(XDocument xDoc)
        {
            var dico = new Dictionary<string, List<Uri>>();
            foreach (XElement item in xDoc.Descendants("item"))
            {
                try
                {
                    string key = item.Descendants("SearchItem").ToList()[0].Value;
                    var uriList = item.Descendants("uri").Select(i => new Uri(i.Value)).ToList();
                    dico.Add(key, uriList);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return dico;
        }

        private static XDocument CreateBookXml(List<BookDataItem> bookList)
        {
            var xDoc = new XDocument();
            xDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

            var root = new XElement("BookList");
            xDoc.Add(root);

            // Add Infos 
            var infos = new XElement("Infos");
            root.Add(infos);
            var number = new XElement("Number");
            infos.Add(number);
            number.Add(new XText(BookList.Count.ToString()));
            var date = new XElement("Date");
            infos.Add(date);
            date.Add(new XText(DateTime.Now.ToString(new CultureInfo("en-US"))));

            foreach (BookDataItem book in bookList)
            {
                var parentNode = new XElement("Book");

                root.Add(parentNode);

                var nodeList = new List<XElement>()
                                        {
                                            new XElement("Title"),
                                            new XElement("Author"),
                                            new XElement("Editor"),
                                            new XElement("Like"),
                                            new XElement("Date"),
                                            new XElement("Summary"),
                                            new XElement("MyOpinion"),
                                            new XElement("Story"),
                                            new XElement("Id"),
                                            new XElement("ImagePath"),
                                        };

                var nodeTextList = new List<XText>()
                                        {
                                            new XText(book.Title ?? string.Empty),
                                            new XText(book.Author ?? string.Empty),
                                            new XText(book.Editor ?? string.Empty),
                                            new XText(book.LikeStars),
                                            new XText(book.Date.ToString(new CultureInfo("en-US"))),
                                            new XText(book.Summary ?? string.Empty),
                                            new XText(book.MyOpinion ?? string.Empty),
                                            new XText(book.Story ?? string.Empty),
                                            new XText(book.Id.ToString()),
                                            new XText(book.ImagePath ?? string.Empty),
                                        };
                for (int i = 0; i < nodeList.Count; i++)
                {
                    parentNode.Add(nodeList[i]);
                    nodeList[i].Add(nodeTextList[i]);
                }
            }

            return xDoc;
        }

        public static int GetNewId()
        {
            if (Settings.BookList == null || Settings.BookList.Count == 0)
            {
                Settings.BookList = new List<BookDataItem>();
                return 1;
            }

            return Settings.BookList.Select(book => book.Id).Max() + 1;
        }

        private static async Task GetOneDriveDataUserInfos()
        {
            // Reset OneDrive user infos
            UserDataInfos = null;
            string accessToken = await GetToken();

            if (_client != null && _client.IsAuthenticated && accessToken != null && accessToken != "")
            {
                //User info
                XDocument xmlFile = await GetXMLFile();
                if (xmlFile != null)
                {
                    var infosXml = xmlFile.Descendants("Infos").First();
                    if (infosXml != null)
                    {
                        UserDataInfos = new InfosClass()
                        {
                            Number = int.Parse(infosXml.Descendants("Number").First().Value),
                            LastUpdateDate = DateTime.Parse(infosXml.Descendants("Date").First().Value, new CultureInfo("en-US"))
                        };
                    }
                }
            }
        }

        private static string getOneDriveFolderName()
        {
            return GetLocalSettings(LocalSettingsValue.onedriveFolderName) == null ? GetRessource("OneDrive_FolderName") : (string)GetLocalSettings(LocalSettingsValue.onedriveFolderName);

        }
    }
}
