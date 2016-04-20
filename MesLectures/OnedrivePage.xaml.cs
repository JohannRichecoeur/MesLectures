using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Popups;
using MesLectures.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace MesLectures
{
    public sealed partial class OnedrivePage
    {
        private readonly NavigationHelper navigationHelper;
        private DateTime lastUpdateLocalTime;
        private DateTime lastUpdateOneDriveTime;

        public OnedrivePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelperLoadState;
            this.UploadPicturesToggle.IsOn = Settings.GetLocalSettings(LocalSettingsValue.uploadPicturesToOneDrive) != null && (bool) Settings.GetLocalSettings(LocalSettingsValue.uploadPicturesToOneDrive);
            this.OneDrivePage_OneDriveFolderNameTextBox.Text = Settings.GetLocalSettings(LocalSettingsValue.onedriveFolderName) == null ? Settings.GetRessource("OneDrive_FolderName") : (string) Settings.GetLocalSettings(LocalSettingsValue.onedriveFolderName);
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile != null &&
                connectionProfile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
            {
                var messageDialogError = new MessageDialog(Settings.GetRessource("OneDriveNoInternetAccess"), Settings.GetRessource("AppTitle/Text"));
                messageDialogError.Commands.Add(new UICommand("OK"));
                await messageDialogError.ShowAsync();
                this.Frame.Navigate(typeof (Books));
            }
            else
            {
                this.OneDriveProgressRing.IsActive = true;
                this.OneDriveGetOneDriveInfos.Visibility = Visibility.Visible;
                this.UserName.Visibility = Visibility.Collapsed;
                this.LocalInfosTextBlock.Visibility = Visibility.Collapsed;
                this.OnedriveInfosTextBlock.Visibility = Visibility.Collapsed;
                this.SignOutButton.Visibility = Visibility.Collapsed;
                this.UserPicture.Opacity = 0;

                var localInfosTask = Settings.ReadFileForInfos();
                var getOnedriveUserInfoTask = Settings.GetOnedriveUserInfo();

                await Task.WhenAll(localInfosTask, getOnedriveUserInfoTask);

                this.DisplayLocalUserInfos(localInfosTask.Result);

                if (getOnedriveUserInfoTask.Result)
                {
                    this.DisplayOnedriveUserInfos();

                    // Delay to display the image at the same time than the name
                    await Task.Delay(1000);

                    this.OneDriveProgressRing.IsActive = false;
                    this.OneDriveGetOneDriveInfos.Visibility = Visibility.Collapsed;
                    this.UserPicture.Opacity = 1;
                    this.UserName.Visibility = Visibility.Visible;
                    this.LocalInfosTextBlock.Visibility = Visibility.Visible;
                    this.OnedriveInfosTextBlock.Visibility = Visibility.Visible;
                    this.UploadSection.Visibility = Visibility.Visible;
                    this.OneDriveFolderNameSection.Visibility = Visibility.Visible;
                    this.SignOutButton.Visibility = Settings.CanLogout() ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    // No OneDrive credentials
                    this.Frame.Navigate(typeof (Books));
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        private void UploadPicturesToggle_OnToggled(object toggleSwitch, RoutedEventArgs e)
        {
            Settings.SetLocalSettings(LocalSettingsValue.uploadPicturesToOneDrive, ((ToggleSwitch) toggleSwitch).IsOn);
        }

        private void DisplayOnedriveUserInfos()
        {
            if (Settings.UserName != null)
            {
                this.UserName.Text = Settings.UserName;
            }

            if (Settings.UserPictureUrl != null)
            {
                this.UserPicture.ImageSource = new BitmapImage(new Uri(Settings.UserPictureUrl));
            }

            if (Settings.UserDataInfos != null)
            {
                OnedriveInfosTextBlock.Text = string.Format(Settings.GetRessource("OneDrivePage_OneDriveCountFormat"),
                    Settings.UserDataInfos.Number, Settings.UserDataInfos.LastUpdateDate.ToString("g"));
                lastUpdateOneDriveTime = Settings.UserDataInfos.LastUpdateDate;
            }
            else
            {
                OnedriveInfosTextBlock.Text = Settings.GetRessource("OneDrivePage_NoOneDriveFolderFound");
                this.DownloadFromOnedriveButton.IsEnabled = false;
            }
        }

        private void DisplayLocalUserInfos(InfosClass infos)
        {
            if (infos != null)
            {
                LocalInfosTextBlock.Text = string.Format(Settings.GetRessource("OneDrivePage_LocalCountFormat"),
                    infos.Number, infos.LastUpdateDate.ToString("g"));
                lastUpdateLocalTime = infos.LastUpdateDate;
            }
            else
            {
                LocalInfosTextBlock.Text = Settings.GetRessource("OneDrivePage_NoLocalBooksFound");
                this.UploadToOnedriveButton.IsEnabled = false;
                lastUpdateLocalTime = DateTime.Now;
            }
        }

        private void SignOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Signout();
            this.Frame.Navigate(typeof (Books));
        }

        private async void UploadToOnedriveClick(object sender, RoutedEventArgs e)
        {
            this.UploadPicturesToggle.IsEnabled = false;
            this.UploadToOnedriveButton.IsEnabled = false;
            this.DownloadFromOnedriveButton.IsEnabled = false;
            this.UploadToOnedriveProgressRing.IsActive = true;

            await Settings.UploadToOnedrive(this.UploadPicturesToggle.IsOn, this.UploadDownloadTextBlock);
            this.DisplayOnedriveUserInfos();

            this.UploadPicturesToggle.IsEnabled = true;
            this.UploadToOnedriveButton.IsEnabled = true;
            this.DownloadFromOnedriveButton.IsEnabled = true;
            this.UploadToOnedriveProgressRing.IsActive = false;
        }

        private async void DownloadFromOnedriveClick(object sender, RoutedEventArgs e)
        {
            this.UploadPicturesToggle.IsEnabled = false;
            this.UploadToOnedriveButton.IsEnabled = false;
            this.DownloadFromOnedriveButton.IsEnabled = false;
            this.UploadToOnedriveProgressRing.IsActive = true;

            await Settings.DownloadFromOnedrive(this.UploadPicturesToggle.IsOn, this.UploadDownloadTextBlock);
            var localInfos = await Settings.ReadFileForInfos();
            this.DisplayOnedriveUserInfos();
            this.DisplayLocalUserInfos(localInfos);

            this.UploadPicturesToggle.IsEnabled = true;
            this.UploadToOnedriveButton.IsEnabled = true;
            this.DownloadFromOnedriveButton.IsEnabled = true;
            this.UploadToOnedriveProgressRing.IsActive = false;
        }

        private void OneDrivePage_OneDriveFolderNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.OneDrivePage_OneDriveFolderNameTextBox.Text.Any())
            {
                Settings.SetLocalSettings(LocalSettingsValue.onedriveFolderName, this.OneDrivePage_OneDriveFolderNameTextBox.Text);
                this.UploadToOnedriveButton.IsEnabled = true;
                this.DownloadFromOnedriveButton.IsEnabled = true;
            }
            else
            {
                this.UploadToOnedriveButton.IsEnabled = false;
                this.DownloadFromOnedriveButton.IsEnabled = false;
            }
        }
    }
}