using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ShareClass.ISBN
{
    public class IsbnSearch
    {
        public static async Task<IsbnBook> GetBookFromIsbn(long isbn)
        {
            var url = "https://www.googleapis.com/books/v1/volumes?q=isbn:" + isbn + "&key=AIzaSyAmADeI4L7Ca_EVVe1vBnaZhQSl2bgviW4";
            var client = new HttpClient();
            var response = (await client.GetAsync(url)).Content.ReadAsStringAsync();
            var jsonObject = JsonConvert.DeserializeObject<JObject>(response.Result);

            return new IsbnBook
                               {
                                   Title = jsonObject["items"][0]["volumeInfo"]["title"].ToString(),
                                   Author = string.Join(",", jsonObject["items"][0]["volumeInfo"]["authors"]),
                               };
        }
    }
}