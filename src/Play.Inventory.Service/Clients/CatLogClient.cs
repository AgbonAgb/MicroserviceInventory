using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service.Clients
{
    public class CatLogClient
    {
        private readonly HttpClient _httpclient;
        public CatLogClient(HttpClient httpclient)
        {
            _httpclient = httpclient;
        }

        //method of fuction that will retreive the Catlog Value
        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCataLogItemAsync()
        {
            var items = await _httpclient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/items");
            return items;
        }
    }
}