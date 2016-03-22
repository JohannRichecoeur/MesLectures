using System;
using Windows.UI.Xaml.Documents;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MesLectures
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            var mailLink = new Hyperlink
            {
                NavigateUri = new Uri("mailto:jean-eric.hourchon@live.fr?subject=" + Settings.GetRessource("AppTitle/Text"))
            };
            var hyperlinkText = new Run { Text = "jean-eric.hourchon@live.fr" };
            mailLink.Inlines.Add(hyperlinkText);
            
            this.Developer.Text = Settings.GetRessource("Infos_dev") + " = Jean-Eric Hourchon ( ";
            this.Developer.Inlines.Add(mailLink);
            this.Developer.Inlines.Add(new Run() {Text = " )."});

            this.Version.Text = "Version = " + Settings.GetRessource("AppVersion");
        }
    }
}