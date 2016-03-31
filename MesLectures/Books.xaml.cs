using System;
using System.Linq;
using System.Threading.Tasks;
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
            //this.FakeButtonForFocus.Focus(FocusState.Programmatic);
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
                this.SortComboBox.Visibility = Visibility.Collapsed;
                var md = new MessageDialog(Settings.GetRessource("Welcome_Title") + "\n" + Settings.GetRessource("Welcome_Text"), Settings.GetRessource("AppTitle"));
                md.Commands.Add(new UICommand("OK"));
                await md.ShowAsync();
            }
            else
            {
                await BookDataSource.FillData();

                this.SortComboBox.Visibility = Visibility.Visible;
                this.SortComboBox.Items.Add(Settings.GetRessource("MyBooks_SortByDate"));   //0
                this.SortComboBox.Items.Add(Settings.GetRessource("MyBooks_SortByTitle"));  //1
                this.SortComboBox.Items.Add(Settings.GetRessource("MyBooks_SortByAuthor")); //2
                this.SortComboBox.Items.Add(Settings.GetRessource("MyBooks_SortByLikes"));  //3
                this.SortComboBox.SelectedIndex = Settings.GetLocalSettings(LocalSettingsValue.sortOption) == null ? 0 : (int)Settings.GetLocalSettings(LocalSettingsValue.sortOption);

                this.DefaultViewModel["Items"] = BookDataSource.GetGroup(this.SortComboBox.SelectedIndex).Items;
            }
        }

        private void BookClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((BookDataItem)e.ClickedItem).Id;
            var groupId = ((BookDataItem)e.ClickedItem).Group.GroupId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId + "-" + groupId);
        }

        private void SortComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.SetLocalSettings(LocalSettingsValue.sortOption, this.SortComboBox.SelectedIndex);
            this.DefaultViewModel["Items"] = BookDataSource.GetGroup(this.SortComboBox.SelectedIndex).Items;
        }

        private async void SearchIcon_OnClick(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Width = Window.Current.Bounds.Width - 80;
            this.SearchBox.Visibility = Visibility.Visible;
            await Task.Delay(200);
            this.SearchBox.Focus(FocusState.Pointer);
        }

        private void SearchBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            this.Frame.Navigate(typeof(SectionPage), "searchValue=" + args.QueryText);
        }
    }
}