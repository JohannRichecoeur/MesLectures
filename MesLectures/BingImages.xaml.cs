using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MesLectures.API;
using MesLectures.Common;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MesLectures
{
    public sealed partial class BingImages
    {
        private bool firstVisit = true;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public BingImages()
        {
            InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelperLoadState;

            WaitProgressRing.IsActive = true;
            WaitProgressRing.Visibility = Visibility.Visible;
            EditionPage.FromBing = true;
            this.PageTitle.Text = Settings.GetRessource("Bing_Search");
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelperLoadState(object sender, LoadStateEventArgs e)
        {
            var search = (string)e.NavigationParameter;
            var uriList = new List<Uri>();

            // Get the previous searched item in the file bingUri.txt
            Dictionary<string, List<Uri>> dico = await Settings.GetDicoFromFilename("bingUri.txt");
            if (dico != null)
            {
                foreach (KeyValuePair<string, List<Uri>> pair in dico.Where(pair => pair.Key == search))
                {
                    uriList = pair.Value;
                    break;
                }
            }
            else
            {
                dico = new Dictionary<string, List<Uri>>();
            }

            // If this is a new search, perform it and add the results to the file bingUri.txt
            if (uriList.Count == 0)
            {
                try
                {
                    uriList = await Settings.GetBingResults(search, 50);
                    if (dico.ContainsKey(search))
                    {
                        dico.Remove(search);
                    }

                    dico.Add(search, uriList);
                    await Settings.CreateXmlFile("bingUri.txt", Settings.CreateUriXml(dico));

                    // Assign values to binding data
                    var list = new ObservableCollection<BingImage>();
                    foreach (var y in uriList)
                    {
                        list.Add(new BingImage(new BitmapImage(y)));
                    }

                    this.defaultViewModel["Items"] = list;

                    ItemGridView.SelectedIndex = -1;

                    if (list.Count == 0)
                    {
                        WaitProgressRing.Visibility = Visibility.Collapsed;
                        var md = new MessageDialog("Aucun résultat n'a été trouvé", "Mes lectures");
                        md.Commands.Add(new UICommand("OK"));
                        await md.ShowAsync();
                        this.Frame.GoBack();
                    }

                    await Task.Delay(4000);
                    WaitProgressRing.Visibility = Visibility.Collapsed;
                }
                catch (Exception)
                {
                    WaitProgressRing.Visibility = Visibility.Collapsed;
                    var messageDialogError = new MessageDialog("Erreur de connexion, merci d'essayer ultérieurement.", "Mes lectures");
                    messageDialogError.Commands.Add(new UICommand("OK"));
                    messageDialogError.ShowAsync();
                    this.Frame.GoBack();
                }
            }
            else
            {
                // Assign values to binding data
                var list = new ObservableCollection<BingImage>();
                foreach (var y in uriList)
                {
                    list.Add(new BingImage(new BitmapImage(y)));
                }

                this.DefaultViewModel["Items"] = list;

                ItemGridView.SelectedIndex = -1;

                await Task.Delay(4000);
                WaitProgressRing.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectionTapped(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count == 1 && e.RemovedItems.Count == 0)
                {
                    if (!this.firstVisit)
                    {
                        var image = (BingImage)e.AddedItems[0];
                        EditionPage.BingUri = ((BitmapImage)image.Image).UriSource;

                        if (EditionPage.BingUri != null)
                        {
                            this.Frame.GoBack();
                        }
                    }
                    else
                    {
                        this.firstVisit = false;
                    }
                }
            }
            catch
            {
            }
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

        private void GoBack(object sender, RoutedEventArgs e)
        {
            this.navigationHelper.GoBack();
        }
    }
}