using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MesLectures.DataModel
{
    public sealed class BookDataSource
    {
        private static readonly BookDataSource bookDataSource = new BookDataSource();

        public static BookDataGroup SearchDataGroup { get; private set; }

        public ObservableCollection<BookDataGroup> AllGroups { get; } = new ObservableCollection<BookDataGroup>();

        public static IEnumerable<BookDataGroup> GetGroups(string uniqueId)
        {
            if (uniqueId != null)
            {
                if (!uniqueId.Equals("AllGroups"))
                {
                    throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
                }
            }

            return bookDataSource.AllGroups;
        }

        public static BookDataGroup GetGroup(int uniqueId)
        {
            var matches = bookDataSource.AllGroups.Where((group) => group.GroupId.Equals(uniqueId)).ToList();
            return matches.Count() == 1 ? matches.First() : null;
        }

        public static BookDataItem GetItem(int uniqueId, int groupId)
        {
            var bookDataGroup = bookDataSource.AllGroups.FirstOrDefault(group => groupId == group.GroupId);
            return bookDataGroup.Items.FirstOrDefault(item => item.Id == uniqueId);
        }

        public static async Task<BookDataGroup> GetSearchResults(string searchQuery)
        {
            await FillData();
            var search = searchQuery.ToLower();
            var list = new List<BookDataItem>();
            list.Clear();
            list.AddRange(bookDataSource.AllGroups[0].Items.Where(t => t.Title.ToLower().Contains(search)));

            foreach (var t in bookDataSource.AllGroups[0].Items.Where(t => t.Author.ToLower().Contains(search) && !list.Contains(t)))
            {
                list.Add(t);
            }

            foreach (var t in bookDataSource.AllGroups[0].Items.Where(t => t.Editor.ToLower().Contains(search) && !list.Contains(t)))
            {
                list.Add(t);
            }

            foreach (var t in bookDataSource.AllGroups[0].Items.Where(t => t.Summary.ToLower().Contains(search) && !list.Contains(t)))
            {
                list.Add(t);
            }

            foreach (var t in bookDataSource.AllGroups[0].Items.Where(t => t.MyOpinion.ToLower().Contains(search) && !list.Contains(t)))
            {
                list.Add(t);
            }

            foreach (var t in bookDataSource.AllGroups[0].Items.Where(t => t.Story.ToLower().Contains(search) && !list.Contains(t)))
            {
                list.Add(t);
            }

            var group = new BookDataGroup(0, "");
            foreach (var g in list)
            {
                group.Items.Add(g);
            }

            SearchDataGroup = group;
            return group;
        }

        public static BookDataItem GetItemFromSearch(int uniqueId)
        {
            return SearchDataGroup.Items.FirstOrDefault(sampleItem => sampleItem.Id == uniqueId);
        }

        public static async Task FillData()
        {
            bookDataSource.AllGroups.Clear();

            if (Settings.BookList == null)
            {
                Settings.BookList = new List<Book>();
            }

            if (Settings.BookList.Count == 0)
            {
                // Retrieve book list from user folder
                Settings.BookList = await Settings.ReadFileForBookList() ?? new List<Book>();
            }

            if (Settings.BookList.Count != 0)
            {
                var group1 = new BookDataGroup(
                    1,
                    Settings.GetRessource("Title_HeaderSection"));

                foreach (Book b in Settings.BookList.OrderByDescending(x => x.Date).ToList())
                {
                    group1.Items.Add(new BookDataItem(b.Id, b.Title, b.Author, (await GetBookImage(b)).Path, b.Summary, "Assets/" + b.Like + "etoiles.jpg", b.Editor, b.MyOpinion, b.Story, b.Date, group1));
                }

                bookDataSource.AllGroups.Add(group1);
            }
        }

        private static async Task<StorageFile> GetBookImage(Book b)
        {
            if (await Settings.LocalFolder.TryGetItemAsync(b.ImagePath) != null)
            {
                return await Settings.LocalFolder.GetFileAsync(b.ImagePath);
            }

            return await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/default.jpg"));
        }
    }
}