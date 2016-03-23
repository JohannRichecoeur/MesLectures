using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Bing;
using MesLectures.API;
using MesLectures.ISBN;
using Microsoft.Live;
using Newtonsoft.Json.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MesLectures.DataModel;

namespace MesLectures
{
    public static class Settings
    {
        public static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        private const string DataFileName = "data.txt";
        private const string ZipFileName = "pictures.zip";
        private static readonly string OneDriveFolderName = Settings.GetRessource("OneDrive_FolderName");

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

        private static LiveConnectClient LiveConnectClient { get; set; }

        private static LiveAuthClient LiveAuthClient { get; set;}

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
            var x = CreateBookXml(BookList.OrderBy(i => i.Id).ToList());
            await CreateXmlFile(DataFileName, x);
        }

        public static bool CanLogout()
        {
            return LiveAuthClient.CanLogout;
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
                    LastUpdateDate = DateTime.Parse(infosData.Descendants("Date").ToList()[0].Value, new CultureInfo("fr-fr"))
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

        public static void Signout()
        {
            LiveAuthClient.Logout();
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

        public static async Task<bool> GetOnedriveUserInfo()
        {
            try
            {
                // Open Live Connect SDK client.
                LiveAuthClient = new LiveAuthClient();
                try
                {
                    var loginResult = await LiveAuthClient.LoginAsync(new string[] { "wl.basic", "wl.offline_access", "wl.skydrive_update", "wl.skydrive" });

                    if (loginResult.Status == LiveConnectSessionStatus.Connected)
                    {
                        // Create a client session to get the profile data.
                        LiveConnectClient = new LiveConnectClient(LiveAuthClient.Session);
                        
                        // Get the profile info of the user.
                        LiveOperationResult operationResultName = await LiveConnectClient.GetAsync("me");
                        LiveOperationResult operationResultUserPicture = await LiveConnectClient.GetAsync("me/picture");
                        dynamic resultName = operationResultName.Result;
                        if (resultName != null)
                        {
                            // Update the text of the object passed in to the method. 
                            UserName = resultName.name;
                        }

                        dynamic resultUserPicture = operationResultUserPicture.Result;
                        if (resultUserPicture != null)
                        {
                            UserPictureUrl = resultUserPicture.location;
                        }

                        // Get the infos
                        await GetOneDriveDataUserInfos();

                        return true;
                    }

                    return false;
                }
                catch (LiveAuthException)
                {
                    return false;
                }
            }
            catch (LiveAuthException)
            {
                return false;
            }
            catch (LiveConnectException)
            {
                return false;
            }
        }

        public static async Task UploadToOnedrive(bool uploadPictures, TextBlock uploadDownloadTextBlock)
        {
            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Preparing");
            // Get the list of folders
            var rootFolders = JObject.Parse((await LiveConnectClient.GetAsync("/me/skydrive/files?filter=folders,albums")).RawResult);
            var mesLecturesFolder = rootFolders["data"].FirstOrDefault(f => f.Value<string>("name").Equals(OneDriveFolderName, StringComparison.OrdinalIgnoreCase));
            if (mesLecturesFolder == null)
            {
                // Create the folder
                var folderData = new Dictionary<string, object>();
                folderData.Add("name", OneDriveFolderName);
                await LiveConnectClient.PostAsync("me/skydrive", folderData);
                rootFolders = JObject.Parse((await LiveConnectClient.GetAsync("/me/skydrive/files?filter=folders,albums")).RawResult);
                mesLecturesFolder = rootFolders["data"].FirstOrDefault(f => f.Value<string>("name").Equals(OneDriveFolderName, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!uploadPictures)
            {
                // Upload the data file
                var folderId = mesLecturesFolder.SelectToken("id").ToString();
                var dataTextFile = await LocalFolder.GetFileAsync(DataFileName);
                uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Upload_Ongoing");

                var uploadDataProgress = new Progress<LiveOperationProgress>((p) =>
                {
                    var percentage = p.ProgressPercentage.ToString("0,0") + "%";
                    uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Upload_Ongoing") + percentage;
                });

                await LiveConnectClient.BackgroundUploadAsync(folderId, DataFileName, dataTextFile, OverwriteOption.Overwrite, new CancellationToken(), uploadDataProgress);
            }
            else
            {
                var folderId = mesLecturesFolder.SelectToken("id").ToString();

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

                var uploadFullProgress = new Progress<LiveOperationProgress>((p) =>
                    {
                        var percentage = p.ProgressPercentage.ToString("0,0") + "%";
                        uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Upload_Ongoing") + percentage;
                    });


                ZipFile.CreateFromDirectory(tempFolder.Path, LocalFolder.Path + "\\" + ZipFileName);
                StorageFile zipFile = await LocalFolder.GetFileAsync(ZipFileName);
                var dataTextFile = await LocalFolder.GetFileAsync(DataFileName);

                // Upload files
                await LiveConnectClient.BackgroundUploadAsync(folderId, ZipFileName, zipFile, OverwriteOption.Overwrite, new CancellationToken(), uploadFullProgress);
                await LiveConnectClient.BackgroundUploadAsync(folderId, DataFileName, dataTextFile, OverwriteOption.Overwrite);

                // Delete temp items
                uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Finishing");
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
            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Preparing");
            var rootFolders = JObject.Parse((await LiveConnectClient.GetAsync("/me/skydrive/files?filter=folders,albums")).RawResult);
            var mesLecturesFolder =
                rootFolders["data"].FirstOrDefault(
                    f => f.Value<string>("name").Equals(OneDriveFolderName, StringComparison.OrdinalIgnoreCase));
            var folderId = mesLecturesFolder.SelectToken("id").ToString();

            // Get the data file
            var mesLecturesFile = await LiveConnectClient.GetAsync(folderId + "/files");
            var files = mesLecturesFile.Result["data"] as List<object>;
            if (files != null && files.Count != 0)
            {
                var selectedDataFile =
                    files.Select(item => item as IDictionary<string, object>)
                        .Where(file => file["name"].ToString() == DataFileName);
                if (selectedDataFile.Count() != 0)
                {
                    var dataFileId = selectedDataFile.First()["id"].ToString();
                    var dataFile = await LocalFolder.CreateFileAsync(DataFileName, CreationCollisionOption.ReplaceExisting);
                    uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Download_Ongoing");
                    await LiveConnectClient.BackgroundDownloadAsync(dataFileId + "/content", dataFile);
                    await ReadFileForBookList(dataFile);
                }
            }

            if (uploadPictures)
            {
                // Download pictures
                var picturesFileId = (files.Select(item => item as IDictionary<string, object>).First(file => file["name"].ToString() == ZipFileName))["id"].ToString();
                var picturesFile = await LocalFolder.CreateFileAsync(ZipFileName, CreationCollisionOption.ReplaceExisting);

                var downloadProgress = new Progress<LiveOperationProgress>((p) =>
                {
                    var percentage = p.ProgressPercentage.ToString("0,0") + "%";
                    uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Download_Ongoing") + percentage;
                });

                await LiveConnectClient.BackgroundDownloadAsync(picturesFileId + "/content", picturesFile, new CancellationToken(), downloadProgress);

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

                uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Finishing");
                // Delete temp items
                await tempFolder.DeleteAsync();
                await picturesFile.DeleteAsync();
            }

            uploadDownloadTextBlock.Text = Settings.GetRessource("OneDrivePage_Transfer_Finishing");
            uploadDownloadTextBlock.Text = "";
        }

        private static List<BookDataItem> GetBookListXdoc(XDocument xDoc)
        {
            var bookList = new List<BookDataItem>();
            foreach (XElement book in xDoc.Descendants("Book"))
            {
                try
                {
                    bookList.Add(
                                 new BookDataItem()
                                 {
                                     Title = book.Descendants("Title").ToList()[0].Value,
                                     Author = book.Descendants("Author").ToList()[0].Value,
                                     Editor = book.Descendants("Editor").ToList()[0].Value,
                                     LikeStars = book.Descendants("Like").ToList()[0].Value,
                                     Date = DateTime.Parse(book.Descendants("Date").ToList()[0].Value, new CultureInfo("fr-FR")),
                                     Summary = book.Descendants("Summary").ToList()[0].Value,
                                     Story = book.Descendants("Story").ToList()[0].Value,
                                     Id = int.Parse(book.Descendants("Id").ToList()[0].Value),
                                     MyOpinion = book.Descendants("MyOpinion").ToList()[0].Value,
                                     ImagePath = book.Descendants("ImagePath").ToList()[0].Value,
                                 });
                }
                catch (Exception e)
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
            date.Add(new XText(DateTime.Now.ToString(new CultureInfo("fr-FR"))));

            IFormatProvider culture = new CultureInfo(Settings.GetRessource("Locale"));
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
                                            new XText(book.Date.ToString(culture)),
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
            
            var rootFolders = JObject.Parse((await LiveConnectClient.GetAsync("/me/skydrive/files?filter=folders,albums")).RawResult);
            var mesLecturesFolder =
                rootFolders["data"].FirstOrDefault(
                    f => f.Value<string>("name").Equals(OneDriveFolderName, StringComparison.OrdinalIgnoreCase));
            if (mesLecturesFolder != null)
            {
                var folderId = mesLecturesFolder.SelectToken("id").ToString();
                var mesLecturesFile = await LiveConnectClient.GetAsync(folderId + "/files");
                var files = mesLecturesFile.Result["data"] as List<object>;
                if (files != null && files.Count != 0)
                {
                    var selectedDataFile = files.Select(item => item as IDictionary<string, object>).Where(file => file["name"].ToString() == DataFileName);
                    if (selectedDataFile.Count() != 0)
                    {
                        var dataFileId = selectedDataFile.First()["id"].ToString();
                        var tempFile =
                            await LocalFolder.CreateFileAsync("dataTemp.txt", CreationCollisionOption.ReplaceExisting);
                        await LiveConnectClient.BackgroundDownloadAsync(dataFileId + "/content", tempFile);

                        UserDataInfos = await ReadFileForInfos(tempFile);

                        // Delete temp file
                        await tempFile.DeleteAsync();
                    }
                }
            }
        }
    }
}
