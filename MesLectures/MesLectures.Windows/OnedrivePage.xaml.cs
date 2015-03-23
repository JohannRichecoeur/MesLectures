using System;
using System.Threading.Tasks;
using MesLectures.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    public sealed partial class OnedrivePage
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        
        public OnedrivePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelperLoadState;
            this.navigationHelper.SaveState += this.NavigationHelperSaveState;
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            this.OneDriveProgressRing.IsActive = true;
            this.UserName.Visibility = Visibility.Collapsed;
            this.UserPicture.Opacity = 0;

            await Settings.GetOnedriveUserInfo();

            try
            {
                if (Settings.UserName != null)
                {
                    this.UserName.Text = Settings.UserName;
                }

                if (Settings.UserPictureUrl != null)
                {
                    this.UserPicture.ImageSource = new BitmapImage(new Uri(Settings.UserPictureUrl));
                }
            }
            catch (Exception)
            {
                this.Frame.GoBack();
            }

            await Task.Delay(1000);
            this.OneDriveProgressRing.IsActive = false;
            this.UserPicture.Opacity = 1;
            this.UserName.Visibility = Visibility.Visible;
        }

        private void NavigationHelperSaveState(object sender, SaveStateEventArgs e)
        {
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
