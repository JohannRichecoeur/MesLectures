using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MesLectures.DataModel;

namespace MesLectures
{
    public sealed partial class Books
    {
        public Books()
        {
            this.InitializeComponent();
            this.FakeButtonForFocus.Focus(FocusState.Programmatic);
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