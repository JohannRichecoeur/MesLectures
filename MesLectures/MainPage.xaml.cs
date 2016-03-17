using System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace MesLectures
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            this.MainFrame.Navigate(typeof (Books));
        }

        private void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(EditionPage));
        }

        private void OneDriveButtonClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(OnedrivePage));
        }

        private async void MenuSettingsClick(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog(Settings.GetRessource("Infos_dev") + " = Jean-Eric Hourchon (jean-eric.hourchon@live.fr) \nVersion = 1.3 ", Settings.GetRessource("AppTitle"));
            md.Commands.Add(new UICommand(Settings.GetRessource("Infos_sendMail")));
            md.Commands.Add(new UICommand(Settings.GetRessource("Infos_close")));
            var response = await md.ShowAsync();

            if (response.Label == Settings.GetRessource("Infos_sendMail"))
            {
                var mailto = new Uri("mailto:?to=jean-eric.hourchon@live.fr&subject=" + Settings.GetRessource("AppTitle") + ", Windows 8");
                await Windows.System.Launcher.LaunchUriAsync(mailto);
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            this.MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void ButtonBooksClick(object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(typeof(Books));
        }
    }
}