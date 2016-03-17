using System;
using System.Linq;
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
                this.DefaultViewModel["Items"] = BookDataSource.GetGroups("AllGroups").First().Items;
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
    }
}