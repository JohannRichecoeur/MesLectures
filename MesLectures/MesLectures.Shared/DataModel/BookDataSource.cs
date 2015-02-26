using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using ShareClass;

namespace MesLectures.DataModel
{
    public sealed class BookDataSource
    {
        private static BookDataSource bookDataSource = new BookDataSource();

        private ObservableCollection<BookDataGroup> allGroups = new ObservableCollection<BookDataGroup>();

        public static BookDataGroup SearchDataGroup { get; private set; }

        public ObservableCollection<BookDataGroup> AllGroups
        {
            get { return this.allGroups; }
        }

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
                Settings.BookList = await Settings.ReadFileForBookList();

                if (Settings.BookList == null)
                {
                    Settings.BookList = new List<Book>();
                }
            }

            if (Settings.BookList.Count != 0)
            {
                var bookListByDate = Settings.BookList.OrderByDescending(x => x.Date).ToList();
                var bookListByLike = Settings.BookList.OrderByDescending(x => x.Like).ToList();
                var bookListByTitle = Settings.BookList.OrderBy(x => x.Title).ToList();
                var bookListByAuthor = Settings.BookList.OrderBy(x => x.Author).ToList();

                // 1.Last Readings
                var group1 = new BookDataGroup(
                    1,
                    Settings.GetRessource("Title_LastReadings"));

                foreach (Book b in bookListByDate)
                {
                    if (b != bookListByDate[0])
                    {
                        group1.Items.Add(new BookDataItem(b.Id, b.Title, b.Author, (await GetBookImage(b)).Path, b.Summary, "Assets/" + b.Like + "etoiles.jpg", b.Editor, b.MyOpinion, b.Story, b.Date, group1));
                    }
                }

                bookDataSource.AllGroups.Add(group1);

                // 50.Last Book
                var group50 = new BookDataGroup(
                    50,
                    Settings.GetRessource("Title_LastReadings"));

                group50.Items.Add(new BookDataItem(bookListByDate[0].Id, bookListByDate[0].Title, bookListByDate[0].Author, (await GetBookImage(bookListByDate[0])).Path, bookListByDate[0].Summary, "Assets/" + bookListByDate[0].Like + "etoiles.jpg", bookListByDate[0].Editor, bookListByDate[0].MyOpinion, bookListByDate[0].Story, bookListByDate[0].Date, group50));
                bookDataSource.AllGroups.Add(group50);

                // 2.Top Rated
                var group2 = new BookDataGroup(
                    2,
                    Settings.GetRessource("Title_TopRated"));
                foreach (Book b in bookListByLike)
                {
                    group2.Items.Add(new BookDataItem(b.Id, b.Title, b.Author, (await GetBookImage(b)).Path, b.Summary, "Assets/" + b.Like + "etoiles.jpg", b.Editor, b.MyOpinion, b.Story, b.Date, group2));
                }

                bookDataSource.AllGroups.Add(group2);

                // 3. Sorted by title A > Z
                var group3 = new BookDataGroup(
                    3,
                    Settings.GetRessource("Title_SortedByTitle"));
                foreach (Book b in bookListByTitle)
                {
                    group3.Items.Add(new BookDataItem(b.Id, b.Title, b.Author, (await GetBookImage(b)).Path, b.Summary, "Assets/" + b.Like + "etoiles.jpg", b.Editor, b.MyOpinion, b.Story, b.Date, group3));
                }

                bookDataSource.AllGroups.Add(group3);

                // 4. Sorted by author A > Z
                var group4 = new BookDataGroup(
                    4,
                    Settings.GetRessource("Title_SortedByAuthor"));
                foreach (Book b in bookListByAuthor)
                {
                    group4.Items.Add(new BookDataItem(b.Id, b.Title, b.Author, (await GetBookImage(b)).Path, b.Summary, "Assets/" + b.Like + "etoiles.jpg", b.Editor, b.MyOpinion, b.Story, b.Date, group4));
                }

                bookDataSource.AllGroups.Add(group4);
            }
        }

        private static async Task<StorageFile> GetBookImage(Book b)
        {
            StorageFile imageDebutFile = null;
            
            // Set the default image to each book
            try
            {
                // imageDebutFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/default.jpg"));
            
                // Search for a specific picture for each book
                imageDebutFile = await Settings.LocalFolder.GetFileAsync(b.ImagePath);
            }
            catch (Exception)
            {
            }

            return imageDebutFile;
        }
    }
}