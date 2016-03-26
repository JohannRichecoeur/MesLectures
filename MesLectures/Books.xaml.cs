using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MesLectures.Common;
using MesLectures.DataModel;

namespace MesLectures
{
    public sealed partial class Books
    {
        public ObservableDictionary DefaultViewModel { get; } = new ObservableDictionary();

        public Books()
        {
            this.InitializeComponent();
            this.FakeButtonForFocus.Focus(FocusState.Programmatic);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            Application.Current.Resources["PhoneItemTemplateWidth"] = Window.Current.Bounds.Width - 60;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Retrieve book list from Local folder
            Settings.BookList = await Settings.ReadFileForBookList();

            // Otherwise create an xml from settings
            if (Settings.BookList == null || Settings.BookList.Count == 0)
            {
                var md = new MessageDialog(Settings.GetRessource("Welcome_Title") + "\n" + Settings.GetRessource("Welcome_Text"), Settings.GetRessource("AppTitle"));
                md.Commands.Add(new UICommand("OK"));
                await md.ShowAsync();
            }
            else
            {
                await BookDataSource.FillData();

                // 0 : Sort by added date
                // 1 : Sort by Title
                // 2 : Sort by Author
                // 3 : Sort by Likes

                this.DefaultViewModel["Items"] = BookDataSource.GetGroup(0).Items;
            }
        }

        private void SearchBoxQuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            this.Frame.Navigate(typeof(SectionPage), "searchValue=" + args.QueryText);
        }

        private void BookClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((BookDataItem)e.ClickedItem).Id;
            var groupId = ((BookDataItem)e.ClickedItem).Group.GroupId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId + "-" + groupId);
        }

        private void ButtonA_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Items"] = BookDataSource.GetGroup(0).Items;
        }

        private void ButtonB_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Items"] = BookDataSource.GetGroup(1).Items;
        }

        private void ButtonC_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Items"] = BookDataSource.GetGroup(2).Items;
        }
        private void ButtonD_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Items"] = BookDataSource.GetGroup(3).Items;
        }
    }
}