using System.Collections.Generic;
using System.Data.Services.Client;
using System.Threading.Tasks;

namespace MesLectures.API
{
    public static class QueryExtensions
    {
        public static Task<IEnumerable<TResult>> QueryAsync<TResult>(this DataServiceQuery<TResult> query)
        {
            return Task<IEnumerable<TResult>>.Factory.FromAsync(query.BeginExecute, query.EndExecute, null);
        }
    }
}