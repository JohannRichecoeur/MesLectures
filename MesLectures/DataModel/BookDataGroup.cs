using System.Collections.ObjectModel;
using System.Collections.Specialized;


namespace MesLectures.DataModel
{
    public class BookDataGroup
    {
        private readonly ObservableCollection<BookDataItem> items = new ObservableCollection<BookDataItem>();
        private readonly ObservableCollection<BookDataItem> topItems = new ObservableCollection<BookDataItem>();

        public BookDataGroup(int groupId, string title)
        {
            this.GroupId = groupId;
            this.Title = title;
            this.Items.CollectionChanged += this.ItemsCollectionChanged;
        }

        public int GroupId { get; private set; }

        public string Title { get; private set; }

        public ObservableCollection<BookDataItem> Items
        {
            get { return this.items; }
        }

        // ReSharper disable once MemberCanBePrivate.Global: must be PUBLIC as it is used in "GroupedItemsPage.xaml"
        public ObservableCollection<BookDataItem> TopItems
        {
            get { return this.topItems; }
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        this.TopItems.Insert(e.NewStartingIndex, this.Items[e.NewStartingIndex]);
                        if (this.TopItems.Count > 12)
                        {
                            this.TopItems.RemoveAt(12);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        this.TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        this.TopItems.RemoveAt(e.OldStartingIndex);
                        if (this.Items.Count >= 12)
                        {
                            this.TopItems.Add(this.Items[11]);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        this.TopItems[e.OldStartingIndex] = this.Items[e.OldStartingIndex];
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.TopItems.Clear();
                    while (this.TopItems.Count < this.Items.Count && this.TopItems.Count < 12)
                    {
                        this.TopItems.Add(this.Items[this.TopItems.Count]);
                    }

                    break;
            }
        }
    }
}