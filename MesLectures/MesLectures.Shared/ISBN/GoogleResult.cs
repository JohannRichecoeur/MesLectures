using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MesLectures.ISBN
{
    public class GoogleResult
    {
        private string link;
        private int byteSize;

        public static async Task<IsbnBook> GetBookFromIsbn(long isbn)
        {
            var url = "https://www.googleapis.com/books/v1/volumes?q=isbn:" + isbn + "&AIzaSyA2WQj4lT7s1y0jY4O6QkIinbCYLfUJcBQ";
            var client = new HttpClient();
            var response = (await client.GetAsync(url)).Content.ReadAsStringAsync();
            var jsonObject = JsonConvert.DeserializeObject<JObject>(response.Result);

            return new IsbnBook
            {
                Title = jsonObject["items"][0]["volumeInfo"]["title"].ToString(),
                Author = string.Join(",", jsonObject["items"][0]["volumeInfo"]["authors"]),
            };
        }

        public static async Task<string> GetGoogleImage(string value)
        {
            var url = "https://www.googleapis.com/customsearch/v1?key=AIzaSyA2WQj4lT7s1y0jY4O6QkIinbCYLfUJcBQ&cx=012473240362233858632:d1ddxwwhlq0&searchType=image&q=value&cr=country" + Settings.GetRessource("Locale").Split('-')[1];
            var googleList = await GetGoogleImageResults(url);
            googleList = googleList.OrderBy(x => x.byteSize).ToList();
            googleList.Reverse();

            return googleList[0].link;
        }

        private static async Task<List<GoogleResult>> GetGoogleImageResults(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var stream = await response.Content.ReadAsStreamAsync();

            var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            var json = JsonConvert.DeserializeObject<JObject>(text);

            var resultsList = new List<GoogleResult>();
            foreach (var jToken in json["items"])
            {
                var obj = (JObject)jToken;
                resultsList.Add(new GoogleResult()
                {
                    link = obj["link"].ToString() ?? string.Empty,
                    byteSize = int.Parse(obj["image"]["byteSize"].ToString()),
                });
            }

            return resultsList;
        }
    }
}